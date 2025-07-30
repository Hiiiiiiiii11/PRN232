using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository.IProgramPaticipate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository.ProgramPaticipate
{
    public class ProgramParticipationRepository : IProgramParticipationRepository
    {
        private readonly DrugUsePreventionDBContext _context;

        public ProgramParticipationRepository(DrugUsePreventionDBContext context)
        {
            _context = context;
        }

        public async Task<List<ProgramParticipation>> GetAllAsync()
        {
            return await _context.ProgramParticipations
                .Include(p => p.Program)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<List<ProgramParticipation>> GetByUserIdAsync(int userId)
        {
            return await _context.ProgramParticipations
                .Include(p => p.Program)
                .Where(p => p.UserID == userId)
                .ToListAsync();
        }

        public async Task<ProgramParticipation> GetByIdAsync(int id)
        {
            return await _context.ProgramParticipations
                .Include(p => p.Program)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ParticipationID == id);
        }

        public async Task<ProgramParticipation> AddAsync(ProgramParticipation participation)
        {
            _context.ProgramParticipations.Add(participation);
            await _context.SaveChangesAsync();
            return participation;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var participation = await _context.ProgramParticipations.FindAsync(id);
            if (participation == null) return false;

            _context.ProgramParticipations.Remove(participation);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
