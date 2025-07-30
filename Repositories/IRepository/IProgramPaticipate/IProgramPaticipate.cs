using BussinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepository.IProgramPaticipate
{
    public interface IProgramParticipationRepository
    {
        Task<List<ProgramParticipation>> GetAllAsync();
        Task<List<ProgramParticipation>> GetByUserIdAsync(int userId);
        Task<ProgramParticipation> GetByIdAsync(int id);
        Task<ProgramParticipation> AddAsync(ProgramParticipation participation);
        Task<bool> DeleteAsync(int id);
    }

}
