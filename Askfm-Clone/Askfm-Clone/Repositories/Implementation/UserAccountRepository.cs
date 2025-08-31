using Askfm_Clone.Data;
using Askfm_Clone.Helpers;
using Askfm_Clone.Repositories.Contracs;
using Base_Library.DTOs;
using Base_Library.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Askfm_Clone.Repositories.Implementation
{
    public class UserAccountRepository(IOptions<JwtSection> config, AppDbContext appDpContext) : IUserAccountRepository
    {
        public async Task<GeneralResponse> RegisterAsync(Register user)
        {
            if (user is null)
            {
                return new GeneralResponse(false, "User cannot be empty.");
            }
            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return new GeneralResponse(false, "All fields are required.");
            }
            var emailExists = await FindUserByEmail(user.Email);
            if (emailExists != null)
            {
                return new GeneralResponse(false, "User already exists.");
            }

            var appicationUser = await AddToDatabase(new AppUser
            {
                Name = user.Name,
                Email = user.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password)
            });
            return new GeneralResponse(true, "User created successfully.");

        }
        public async Task<LoginReponse> SignInAsync(Login user)
        {
            if (user is null)
            {
                return new LoginReponse(false, "User cannot be empty.");
            }
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return new LoginReponse(false, "All fields are required.");
            }
            var userExists = await FindUserByEmail(user.Email);
            if (userExists is null)
            {
                return new LoginReponse(false, "User does not exist.");
            }
            if (!BCrypt.Net.BCrypt.Verify(user.Password, userExists.PasswordHash!))
            {
                return new LoginReponse(false, "Invalid Email or Password.");
            }
            
            var token = GenerateJwtToken(userExists);
            var refreshToken = GenerateRefreshToken();
            //save the refresh token to the database
            var findUser = await appDpContext.RefreshTokenInfos.FirstOrDefaultAsync(u => u.UserId == userExists.Id);
            if (findUser is not null)
            {
                findUser.Token = refreshToken;
                await appDpContext.SaveChangesAsync();
            }
            else
            {
                await AddToDatabase(new RefreshTokenInfo()
                {
                    Token = refreshToken,
                    UserId = userExists.Id
                });
            }
            return new LoginReponse(true, "Login successful.", token, refreshToken);

        }
        private string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        public async Task<LoginReponse> RefreshTokenAsync(RefreshToken refreshToken)
        {
            if (refreshToken is null || string.IsNullOrEmpty(refreshToken.Token))
            {
                return new LoginReponse(false, "Refresh token cannot be empty.");
            }
            var findToken = appDpContext.RefreshTokenInfos.FirstOrDefault(rt => rt.Token!.Equals(refreshToken.Token));
            if (findToken is null)
            {
                return new LoginReponse(false, "Refresh token does not exist.");
            }

            //get user details
            var user = appDpContext.Users.FirstOrDefault(u => u.Id == findToken.UserId);
            if (user is null)
            {
                return new LoginReponse(false, "User does not exist.");
            }
           
            //generate new token and refresh token
            var token = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            //update the refresh token in the database

            var updateRefreshToken = await appDpContext.RefreshTokenInfos.FirstOrDefaultAsync(u => u.UserId == user.Id);
            if (updateRefreshToken is null)
            {
                return new LoginReponse(false, "Refresh token does not exist for this user.");
            }
            updateRefreshToken.Token = newRefreshToken;
            await appDpContext.SaveChangesAsync();
            return new LoginReponse(true, "Token refreshed successfully.", token, newRefreshToken);

        }
        private string GenerateJwtToken(AppUser userExists)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userExists.Id.ToString()),
                new Claim(ClaimTypes.Name, userExists.Name!),
                new Claim(ClaimTypes.Email, userExists.Email!),
            };
            var token = new JwtSecurityToken(
                issuer: config.Value.Issuer,
                audience: config.Value.Audience,
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task<T> AddToDatabase<T>(T model)
        {

            if (model is null)
            {
                throw new ArgumentNullException(nameof(model), "Model cannot be null.");
            }
            var result = await appDpContext.AddAsync(model!);

            await appDpContext.SaveChangesAsync();
            return (T)result.Entity;
        }


        private async Task<AppUser> FindUserByEmail(string email)
        {

            return await appDpContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(email.ToLower()));

        }

    }
}
