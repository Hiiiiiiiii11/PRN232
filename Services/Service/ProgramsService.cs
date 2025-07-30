using BussinessObjects;
using Repositories.IRepository.Programs;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class ProgramService : IProgramService
    {
        private readonly IProgramRepository _repository;

        public ProgramService(IProgramRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Program>> GetAllProgramsAsync() => _repository.GetAllAsync();
        public Task<Program> GetProgramByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<IEnumerable<Program>> GetProgramsByUserIdAsync(int userId) => _repository.GetByUserIdAsync(userId);
        public Task AddProgramAsync(Program program) => _repository.AddAsync(program);
        public Task UpdateProgramAsync(Program program) => _repository.UpdateAsync(program);
        public Task DeleteProgramAsync(int id) => _repository.DeleteAsync(id);
    }
}
