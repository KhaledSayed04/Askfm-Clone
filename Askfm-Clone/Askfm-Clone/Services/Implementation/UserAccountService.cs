using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Auth;
using Askfm_Clone.Helpers;
using Askfm_Clone.Repositories.Contracs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UAParser;

namespace Askfm_Clone.Repositories.Implementation
{
    public class UserAccountService : IUserAccountService
    {
        private readonly AppDbContext _context;
        private readonly IOptions<JwtSection> _config;

        public UserAccountService(IOptions<JwtSection> config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterDto user)
        {
            user.Email = user.Email!.Trim().ToLower();
            var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
            if (emailExists)
                return AuthResultDto.Fail("User already exists");

            var newUser = new AppUser
            {
                Name = user.Name!,
                Email = user.Email!.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password)
            };

            try
            {
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();
                return AuthResultDto.Success("User registered successfully");
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                return AuthResultDto.Fail("Email already in use.");
            }
            catch (DbUpdateException ex)
            {
                // Handles any rare race condition case
                return AuthResultDto.Fail("Registration failed due to concurrency conflict");
            }
        }

        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            // Look for SQL Server error numbers 2627 or 2601 (unique constraint/index violations)
            if (ex.InnerException is SqlException sqlEx &&
                (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                return true;
            }
            return false;
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto login)
        {
            login.Email = login.Email!.ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                return AuthResultDto.Fail("Invalid email or password");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenHash = HashToken(refreshToken);

            var deviceId = string.IsNullOrWhiteSpace(login.DeviceId)
                ? Guid.NewGuid().ToString()
                : login.DeviceId;

            var existingToken = await _context.RefreshTokensInfo
                .FirstOrDefaultAsync(rt => rt.UserId == user.Id && rt.DeviceId == deviceId);

            if (existingToken != null)
            {
                existingToken.Token = refreshTokenHash;
                existingToken.ExpiresAt = DateTime.UtcNow.AddDays(_config.Value.RefreshTokenLifetimeDays);
                existingToken.Revoked = false;
                existingToken.LastUsedAt = DateTime.UtcNow;
            }
            else
            {
                var refreshTokenInfo = new RefreshTokenInfo
                {
                    Token = refreshTokenHash,
                    DeviceId = deviceId,
                    ExpiresAt = DateTime.UtcNow.AddDays(_config.Value.RefreshTokenLifetimeDays),
                    UserId = user.Id,
                    LastUsedAt = DateTime.UtcNow
                };

                await _context.RefreshTokensInfo.AddAsync(refreshTokenInfo);
                await _context.SaveChangesAsync();
            }


            return AuthResultDto.Success("Login successful", new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                DeviceId = deviceId
            });
        }

        public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return AuthResultDto.Fail("Refresh token cannot be empty");

            var hashToken = HashToken(refreshToken);

            var refreshRow = await _context.RefreshTokensInfo
                .Include(rt => rt.User) // include user so we can issue JWT
                .FirstOrDefaultAsync(rt => rt.Token == hashToken); // TODO: check UserId

            if (refreshRow == null)
                return AuthResultDto.Fail("Invalid refresh token");

            if(refreshRow.Revoked)
            {
                // Immediately revoke all tokens for this user if token reuse is detected
                await LogoutAllAsync(refreshRow.UserId);
                return AuthResultDto.Fail("Security violation detected");
            }

            if (refreshRow.ExpiresAt <= DateTime.UtcNow)
                return AuthResultDto.Fail("Refresh token expired or revoked");

            var user = refreshRow.User;
            //if (user == null) // NOTE: Will not happen due to the cascade delete
            //    return AuthResultDto.Fail("User not found for this refresh token");

            var newRefreshToken = GenerateRefreshToken();
            var newRefreshTokenHash = HashToken(newRefreshToken);


            refreshRow.Revoked = true;
            refreshRow.LastUsedAt = DateTime.UtcNow;

            var newRefreshRow = new RefreshTokenInfo
            {
                Token = newRefreshTokenHash,
                DeviceId = refreshRow.DeviceId,
                ExpiresAt = DateTime.UtcNow.AddDays(_config.Value.RefreshTokenLifetimeDays),
                UserId = user.Id,
                LastUsedAt = DateTime.UtcNow
            };

            await _context.RefreshTokensInfo.AddAsync(newRefreshRow);
            await _context.SaveChangesAsync();

            var newAccessToken = GenerateJwtToken(user);

            return AuthResultDto.Success("Token refreshed successfully", new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                DeviceId = refreshRow.DeviceId
            });
        }

        private string GenerateJwtToken(AppUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Value.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _config.Value.Issuer,
                audience: _config.Value.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_config.Value.AccessTokenLifetimeMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        private string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<AuthResultDto> LogoutAsync(int userId, string deviceId)
        {
            var refreshToken = await _context.RefreshTokensInfo
                .FirstOrDefaultAsync(rt => (rt.DeviceId == deviceId && rt.UserId == userId && !rt.Revoked));

            if (refreshToken == null)
                return AuthResultDto.Fail("Not signed in.");

            refreshToken.Revoked = true;

            await _context.SaveChangesAsync();

            return AuthResultDto.Success("Logged out from this device.");
        }


        public async Task<AuthResultDto> LogoutAllAsync(int userId)
        {
            var tokens = await _context.RefreshTokensInfo
                .Where(rt => rt.UserId == userId && !rt.Revoked)
                .ToListAsync();

            if (!tokens.Any())
                return AuthResultDto.Fail("Not signed in.");

            foreach (var token in tokens)
            {
                token.Revoked = true;
            }

            await _context.SaveChangesAsync();

            return AuthResultDto.Success("Logged out from all devices.");
        }

        public async Task CleanExpiredTokensAsync()
        {
            var expiredTokens = _context.RefreshTokensInfo
                .Where(rt => rt.ExpiresAt <= DateTime.UtcNow || rt.Revoked);

            _context.RefreshTokensInfo.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }
}
