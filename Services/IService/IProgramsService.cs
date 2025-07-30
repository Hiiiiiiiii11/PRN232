using BussinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IProgramService
    {
        Task<IEnumerable<Program>> GetAllProgramsAsync();
        Task<Program> GetProgramByIdAsync(int id);
        Task<IEnumerable<Program>> GetProgramsByUserIdAsync(int userId);
        Task AddProgramAsync(Program program);
        Task UpdateProgramAsync(Program program);
        Task DeleteProgramAsync(int id);
    }
}
