using BussinessObjects;
using Repositories.IRepository.IProgramPaticipate;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class ProgramParticipationService : IProgramParticipationService
    {
        private readonly IProgramParticipationRepository _repository;

        public ProgramParticipationService(IProgramParticipationRepository repository)
        {
            _repository = repository;
        }

        public Task<List<ProgramParticipation>> GetAllAsync() => _repository.GetAllAsync();

        public Task<List<ProgramParticipation>> GetByUserIdAsync(int userId) => _repository.GetByUserIdAsync(userId);

        public Task<ProgramParticipation> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public Task<ProgramParticipation> AddAsync(ProgramParticipation participation) => _repository.AddAsync(participation);

        public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
    }

}
