using BussinessObjects;
using Repositories.IRepository.Admins;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.Service
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        // User retrieval operations
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _adminRepository.GetUserByIdAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _adminRepository.GetUserByEmailAsync(email);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _adminRepository.GetUserByUsernameAsync(username);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _adminRepository.GetAllUsersAsync();
        }

        public async Task<List<User>> GetUsersByStatusAsync(string status)
        {
            return await _adminRepository.GetUsersByStatusAsync(status);
        }

        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            return await _adminRepository.GetUsersByRoleAsync(role);
        }

        public async Task<List<User>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<User>();

            return await _adminRepository.SearchUsersAsync(searchTerm.Trim());
        }

        public async Task<(List<User> Users, int TotalCount, int TotalPages)> GetUsersWithPaginationAsync(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var users = await _adminRepository.GetUsersWithPaginationAsync(page, pageSize);
            var totalCount = await _adminRepository.GetTotalUsersCountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return (users, totalCount, totalPages);
        }

        // User creation and modification
        public async Task<(bool Success, string Message)> CreateUserAsync(User user)
        {
            try
            {
                // Validate user data
                var validationResult = await ValidateUserForCreationAsync(user);
                if (!validationResult.IsValid)
                    return (false, validationResult.Message);

                // Hash password if provided
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    user.PasswordHash = HashPassword(user.PasswordHash);
                }

                // Set default values
                user.CreatedAt = DateTime.Now;
                user.Status = "Active";
                user.IsEmailVerified = true;

                var result = await _adminRepository.CreateUserAsync(user);
                return result
                    ? (true, "Người dùng đã được tạo thành công.")
                    : (false, "Có lỗi xảy ra khi tạo người dùng.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(User user)
        {
            try
            {
                var existingUser = await _adminRepository.GetUserByIdAsync(user.UserID);
                if (existingUser == null)
                    return (false, "Không tìm thấy người dùng.");

                // Validate email uniqueness (if changed)
                if (existingUser.Email != user.Email)
                {
                    var emailExists = await _adminRepository.ExistsUserByEmailAsync(user.Email);
                    if (emailExists)
                        return (false, "Email đã được sử dụng bởi người dùng khác.");
                }

                // Validate username uniqueness (if changed)
                if (existingUser.Username != user.Username)
                {
                    var usernameExists = await _adminRepository.ExistsUserByUsernameAsync(user.Username);
                    if (usernameExists)
                        return (false, "Tên đăng nhập đã được sử dụng bởi người dùng khác.");
                }

                // Preserve sensitive fields
                user.PasswordHash = existingUser.PasswordHash;
                user.CreatedAt = existingUser.CreatedAt;
                user.ResetPasswordToken = existingUser.ResetPasswordToken;
                user.ResetPasswordExpiry = existingUser.ResetPasswordExpiry;

                var result = await _adminRepository.UpdateUserAsync(user);
                return result
                    ? (true, "Thông tin người dùng đã được cập nhật thành công.")
                    : (false, "Có lỗi xảy ra khi cập nhật thông tin người dùng.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _adminRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "Không tìm thấy người dùng.");

                var result = await _adminRepository.DeleteUserAsync(userId);
                return result
                    ? (true, "Người dùng đã được xóa thành công.")
                    : (false, "Có lỗi xảy ra khi xóa người dùng.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        // User status management
        public async Task<(bool Success, string Message)> BanUserAsync(int userId)
        {
            try
            {
                var user = await _adminRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "Không tìm thấy người dùng.");

                if (user.Status == "Banned")
                    return (false, "Người dùng đã bị cấm trước đó.");

                var result = await _adminRepository.BanUserAsync(userId);
                return result
                    ? (true, "Người dùng đã bị cấm thành công.")
                    : (false, "Có lỗi xảy ra khi cấm người dùng.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UnbanUserAsync(int userId)
        {
            try
            {
                var user = await _adminRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "Không tìm thấy người dùng.");

                if (user.Status != "Banned")
                    return (false, "Người dùng chưa bị cấm.");

                var result = await _adminRepository.UnbanUserAsync(userId);
                return result
                    ? (true, "Người dùng đã được bỏ cấm thành công.")
                    : (false, "Có lỗi xảy ra khi bỏ cấm người dùng.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ChangeUserRoleAsync(int userId, string newRole)
        {
            try
            {
                var user = await _adminRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "Không tìm thấy người dùng.");

                var validRoles = new[] { "Admin", "Manager", "Consultant", "User", "Guest" };
                if (!validRoles.Contains(newRole))
                    return (false, "Vai trò không hợp lệ.");

                if (user.Role == newRole)
                    return (false, "Người dùng đã có vai trò này.");

                var result = await _adminRepository.ChangeUserRoleAsync(userId, newRole);
                return result
                    ? (true, $"Vai trò người dùng đã được thay đổi thành {newRole}.")
                    : (false, "Có lỗi xảy ra khi thay đổi vai trò người dùng.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> VerifyUserEmailAsync(int userId)
        {
            try
            {
                var user = await _adminRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "Không tìm thấy người dùng.");

                if (user.IsEmailVerified)
                    return (false, "Email đã được xác thực trước đó.");

                var result = await _adminRepository.VerifyUserEmailAsync(userId);
                return result
                    ? (true, "Email người dùng đã được xác thực thành công.")
                    : (false, "Có lỗi xảy ra khi xác thực email.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        // Password management
        public async Task<(bool Success, string Message)> ResetUserPasswordAsync(int userId, string newPassword)
        {
            try
            {
                var user = await _adminRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "Không tìm thấy người dùng.");

                if (!IsValidPassword(newPassword))
                    return (false, "Mật khẩu phải có ít nhất 6 ký tự và chứa ít nhất một chữ cái và một số.");

                var hashedPassword = HashPassword(newPassword);
                var result = await _adminRepository.ResetUserPasswordAsync(userId, hashedPassword);

                return result
                    ? (true, "Mật khẩu đã được đặt lại thành công.")
                    : (false, "Có lỗi xảy ra khi đặt lại mật khẩu.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ChangeUserPasswordAsync(int userId, string oldPassword, string newPassword)
        {
            try
            {
                var user = await _adminRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "Không tìm thấy người dùng.");

                if (!VerifyPassword(oldPassword, user.PasswordHash))
                    return (false, "Mật khẩu cũ không chính xác.");

                if (!IsValidPassword(newPassword))
                    return (false, "Mật khẩu mới phải có ít nhất 6 ký tự và chứa ít nhất một chữ cái và một số.");

                var hashedPassword = HashPassword(newPassword);
                var result = await _adminRepository.ResetUserPasswordAsync(userId, hashedPassword);

                return result
                    ? (true, "Mật khẩu đã được thay đổi thành công.")
                    : (false, "Có lỗi xảy ra khi thay đổi mật khẩu.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        // Validation operations
        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return !await _adminRepository.ExistsUserByEmailAsync(email);
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _adminRepository.ExistsUserByUsernameAsync(username);
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            var user = await _adminRepository.GetUserByIdAsync(userId);
            return user != null;
        }

        // Statistics and reporting
        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _adminRepository.GetTotalUsersCountAsync();
        }

        public async Task<Dictionary<string, int>> GetUsersByRoleStatisticsAsync()
        {
            var allUsers = await _adminRepository.GetAllUsersAsync();
            return allUsers
                .GroupBy(u => u.Role)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetUsersByStatusStatisticsAsync()
        {
            var allUsers = await _adminRepository.GetAllUsersAsync();
            return allUsers
                .GroupBy(u => u.Status)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<List<User>> GetRecentlyRegisteredUsersAsync(int count = 10)
        {
            var allUsers = await _adminRepository.GetAllUsersAsync();
            return allUsers
                .OrderByDescending(u => u.CreatedAt)
                .Take(count)
                .ToList();
        }

        // Private helper methods
        private async Task<(bool IsValid, string Message)> ValidateUserForCreationAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.FullName))
                return (false, "Họ tên không được để trống.");

            if (string.IsNullOrWhiteSpace(user.Email))
                return (false, "Email không được để trống.");

            if (!IsValidEmail(user.Email))
                return (false, "Định dạng email không hợp lệ.");

            if (string.IsNullOrWhiteSpace(user.Username))
                return (false, "Tên đăng nhập không được để trống.");

            if (user.Username.Length < 3)
                return (false, "Tên đăng nhập phải có ít nhất 3 ký tự.");

            var emailExists = await _adminRepository.ExistsUserByEmailAsync(user.Email);
            if (emailExists)
                return (false, "Email đã được sử dụng.");

            var usernameExists = await _adminRepository.ExistsUserByUsernameAsync(user.Username);
            if (usernameExists)
                return (false, "Tên đăng nhập đã được sử dụng.");

            return (true, string.Empty);
        }

        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        private bool IsValidPassword(string password)
        {
            return password.Length >= 6 &&
                   password.Any(char.IsLetter) &&
                   password.Any(char.IsDigit);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }
}