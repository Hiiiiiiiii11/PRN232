using BussinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepository.Programs
{
    public interface IProgramRepository
    {
        Task<IEnumerable<BussinessObjects.Program>> GetAllAsync();
        Task<BussinessObjects.Program> GetByIdAsync(int id);
        Task<IEnumerable<BussinessObjects.Program>> GetByUserIdAsync(int userId);
        Task AddAsync(BussinessObjects.Program program);
        Task UpdateAsync(BussinessObjects.Program program);
        Task DeleteAsync(int id);
    }
}
