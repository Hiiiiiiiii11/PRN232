using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Services.DTOs.User;
using Services.IService;

namespace Services.Service
{
    public class AuthService : IAuthService
    {
        private readonly DrugUsePreventionDBContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(DrugUsePreventionDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User?> ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return null;
            }

            user.ResetPasswordToken = Guid.NewGuid().ToString(); // Tạo token đặt lại mật khẩu
            user.ResetPasswordExpiry = DateTime.UtcNow.AddHours(1); // Token hết hạn sau 1 giờ
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.ResetPasswordToken == token && u.ResetPasswordExpiry > DateTime.UtcNow
            );

            if (user == null)
            {
                return false;
            }

            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, newPassword);
            user.ResetPasswordToken = null; // Xóa token sau khi đặt lại mật khẩu
            user.ResetPasswordExpiry = null;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<User?> RegisterAsync(RegisterUserRequest request)
        {
            if (await _context.Users.AnyAsync(x => x.Username == request.Username))
            {
                return null;
            }

            var user = new User
            {
                FullName = request.FullName,
                CreatedAt = DateTime.UtcNow,
                Status = "Active", // Trạng thái người dùng mới là Active
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Role = request.Role ?? "Member", // Mặc định là Guest nếu không có role
                Username = request.Username,
                Email = request.Email, // Assuming username is the email
                PasswordHash = new PasswordHasher<User>().HashPassword(null, request.Password),
                VerificationToken = Guid.NewGuid().ToString(), // Tạo token xác thực
                IsEmailVerified =
                    false // Đặt trạng thái email chưa được xác thực
                ,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null)
            {
                return false;
            }

            user.IsEmailVerified = true;
            user.VerificationToken = null; // Xóa token sau khi xác thực
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<TokenResponseDto?> LoginAsync(UserLoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.Username == request.Username
            );
            if (user is null)
            {
                return null;
            }

            if (
                new PasswordHasher<User>().VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password
                ) == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed
            )
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto { AccessToken = CreateToken(user) };
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!)
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["AppSettings:Issuer"],
                audience: _configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
