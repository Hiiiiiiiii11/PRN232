using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository.Users
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(DrugUsePreventionDBContext context)
            : base(context) { }

        public async Task<User?> GetUserWithBasicInfoAsync(int userId)
        {
            return await Entities
                .Select(u => new User
                {
                    UserID = u.UserID,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role
                })
                .FirstOrDefaultAsync(u => u.UserID == userId);
        }

        public async Task<bool> ExistsUserAsync(int userId)
        {
            return await Entities.AnyAsync(u => u.UserID == userId);
        }

        public async Task<string> GetUserFullNameAsync(int userId)
        {
            var user = await Entities
                .Where(u => u.UserID == userId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync();

            return user ?? "Không xác định";
        }

        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            return await Entities
                .Where(u => u.Role == role)
                .ToListAsync();
        }
    }
}