using BussinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepository.Users
{
    public interface IUserRepository : IGenericRepository<User>
    {
        // User specific queries for Course module
        Task<User?> GetUserWithBasicInfoAsync(int userId);
        Task<bool> ExistsUserAsync(int userId);
        Task<string> GetUserFullNameAsync(int userId);
        Task<List<User>> GetUsersByRoleAsync(string role);
    }
}
