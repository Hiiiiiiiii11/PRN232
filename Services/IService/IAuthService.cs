using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Services.DTOs.User;

namespace Services.IService
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterUserRequest request);
        Task<TokenResponseDto?> LoginAsync(UserLoginRequest request);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<User?> ForgotPasswordAsync(string email);
    }
}
