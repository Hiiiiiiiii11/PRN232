using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository.Admins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository.Admins
{
    public class AdminRepository : GenericRepository<User>, IAdminRepository
    {
        private readonly DrugUsePreventionDBContext _context;

        public AdminRepository(DrugUsePreventionDBContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await Entities
                .Include(u => u.ConsultantProfile)
                .FirstOrDefaultAsync(u => u.UserID == userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await Entities
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await Entities
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await Entities
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersByStatusAsync(string status)
        {
            return await Entities
                .Where(u => u.Status == status)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            return await Entities
                .Where(u => u.Role == role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                user.CreatedAt = DateTime.Now;
                user.Status = "Active";
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BanUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Status = "Banned";
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnbanUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Status = "Active";
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(int userId, string newRole)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Role = newRole;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyUserEmailAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsEmailVerified = true;
                    user.VerificationToken = null;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetUserPasswordAsync(int userId, string newPasswordHash)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.PasswordHash = newPasswordHash;
                    user.ResetPasswordToken = null;
                    user.ResetPasswordExpiry = null;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<User>> SearchUsersAsync(string searchTerm)
        {
            return await Entities
                .Where(u => u.FullName.Contains(searchTerm) ||
                           u.Email.Contains(searchTerm) ||
                           u.Username.Contains(searchTerm))
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await Entities.CountAsync();
        }

        public async Task<List<User>> GetUsersWithPaginationAsync(int page, int pageSize)
        {
            return await Entities
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> ExistsUserByEmailAsync(string email)
        {
            return await Entities.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsUserByUsernameAsync(string username)
        {
            return await Entities.AnyAsync(u => u.Username == username);
        }
    }
}