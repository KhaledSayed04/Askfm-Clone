using Base_Library.DTOs;
using Base_Library.Responses;
using Microsoft.Win32;

namespace Askfm_Clone.Repositories.Contracs
{
    public interface IUserAccountRepository
    {
        Task<GeneralResponse> RegisterAsync(Register user);
        Task<LoginReponse> SignInAsync(Login user);
        Task<LoginReponse> RefreshTokenAsync(RefreshToken refreshToken);

    }
}
