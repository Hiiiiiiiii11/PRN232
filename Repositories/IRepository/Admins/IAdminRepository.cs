using BussinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepository.Admins
{
    public interface IAdminRepository : IGenericRepository<User>
    {
        // User CRUD operations for Admin
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersByStatusAsync(string status);
        Task<List<User>> GetUsersByRoleAsync(string role);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> BanUserAsync(int userId);
        Task<bool> UnbanUserAsync(int userId);
        Task<bool> ChangeUserRoleAsync(int userId, string newRole);
        Task<bool> VerifyUserEmailAsync(int userId);
        Task<bool> ResetUserPasswordAsync(int userId, string newPasswordHash);
        Task<List<User>> SearchUsersAsync(string searchTerm);
        Task<int> GetTotalUsersCountAsync();
        Task<List<User>> GetUsersWithPaginationAsync(int page, int pageSize);
        Task<bool> ExistsUserByEmailAsync(string email);
        Task<bool> ExistsUserByUsernameAsync(string username);
    }
}