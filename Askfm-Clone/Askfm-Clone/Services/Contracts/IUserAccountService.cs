using Askfm_Clone.DTOs;
using Base_Library.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;

namespace Askfm_Clone.Repositories.Contracs
{
    public interface IUserAccountService
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto user);
        Task<AuthResultDto> LoginAsync(LoginDto user);
        Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
        Task<AuthResultDto> LogoutAsync(int userId ,string deviceId);
        Task<AuthResultDto> LogoutAllAsync(int userId);
    }
}
