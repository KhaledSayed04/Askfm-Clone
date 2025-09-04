using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.Helpers;
using Askfm_Clone.Repositories.Contracs;
using Base_Library.DTOs;
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
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly AppDbContext _context;
        private readonly IOptions<JwtSection> _config;

        public UserAccountRepository(IOptions<JwtSection> config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterDto user)
        {
            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == user.Email!.ToLower());
            if (emailExists)
                return AuthResultDto.Fail("User already exists");

            var newUser = new AppUser
            {
                Name = user.Name!,
                Email = user.Email!.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password)
            };

            await AddToDatabase(newUser);

            return AuthResultDto.Success("User registered successfully");
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email!.ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                return AuthResultDto.Fail("Invalid email or password");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenHash = HashToken(refreshToken);

            var deviceId = string.IsNullOrWhiteSpace(login.DeviceId)
                ? Guid.NewGuid().ToString() 
                : login.DeviceId;
            var existingToken = await _context.RefreshTokenInfos
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
                //await _context.RefreshTokenInfos.AddAsync(refreshTokenInfo);
                await AddToDatabase(refreshTokenInfo);
            }

            await _context.SaveChangesAsync();

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

            var refreshRow = await _context.RefreshTokenInfos
                .Include(rt => rt.User) // include user so we can issue JWT
                .FirstOrDefaultAsync(rt => rt.Token == hashToken);

            if (refreshRow == null)
                return AuthResultDto.Fail("Invalid refresh token");

            if (refreshRow.Revoked || refreshRow.ExpiresAt <= DateTime.UtcNow)
                return AuthResultDto.Fail("Refresh token expired or revoked");

            var user = refreshRow.User;
            if (user == null)
                return AuthResultDto.Fail("User not found for this refresh token");

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

            //await _context.RefreshTokenInfos.AddAsync(newRefreshRow);
            await AddToDatabase(newRefreshRow);

        
            var newAccessToken = GenerateJwtToken(user);

            await _context.SaveChangesAsync();

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


        private async Task<T> AddToDatabase<T>(T model)
        {

            if (model is null)
            {
                throw new ArgumentNullException(nameof(model), "Model cannot be null.");
            }
            var result = await _context.AddAsync(model!);

            await _context.SaveChangesAsync();
            return (T)result.Entity;
        }

        public async Task<AuthResultDto> LogoutAsync(int userId, string deviceId)
        {
            var refreshToken = await _context.RefreshTokenInfos
                .FirstOrDefaultAsync(rt => (rt.DeviceId == deviceId && rt.UserId == userId && !rt.Revoked));

            if (refreshToken == null)
                return AuthResultDto.Fail("No active session found for this device.");

            refreshToken.Revoked = true;
            refreshToken.LastUsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return AuthResultDto.Success("Logged out from this device.");
        }


        public async Task<AuthResultDto> LogoutAllAsync(int userId)
        {
            var tokens = await _context.RefreshTokenInfos
                .Where(rt => rt.UserId == userId && !rt.Revoked)
                .ToListAsync();

            if (!tokens.Any())
                return AuthResultDto.Fail("No active sessions found.");

            foreach (var token in tokens)
            {
                token.Revoked = true;
                token.LastUsedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return AuthResultDto.Success("Logged out from all devices.");
        }
    }
}
