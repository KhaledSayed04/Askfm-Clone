using Askfm_Clone.DTOs;
using Base_Library.DTOs;
using Base_Library.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;

namespace Askfm_Clone.Repositories.Contracs
{
    public interface IUserAccountRepository
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto user);
        Task<AuthResultDto> LoginAsync(LoginDto user);
        Task<AuthResultDto> RefreshTokenAsync(string refreshToken);

    }
}
