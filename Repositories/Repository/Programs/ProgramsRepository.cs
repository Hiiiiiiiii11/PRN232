using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository.Programs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository.Programs
{
    public class ProgramRepository : IProgramRepository
    {
        private readonly DrugUsePreventionDBContext _context;

        public ProgramRepository(DrugUsePreventionDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BussinessObjects.Program>> GetAllAsync()
        {
            return await _context.Programs.Include(p => p.Creator).ToListAsync();
        }

        public async Task<BussinessObjects.Program> GetByIdAsync(int id)
        {
            return await _context.Programs.Include(p => p.Creator)
                                          .FirstOrDefaultAsync(p => p.ProgramID == id);
        }

        public async Task<IEnumerable<BussinessObjects.Program>> GetByUserIdAsync(int userId)
        {
            return await _context.Programs.Where(p => p.CreatedBy == userId).ToListAsync();
        }

        public async Task AddAsync(BussinessObjects.Program program)
        {
            _context.Programs.Add(program);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BussinessObjects.Program program)
        {
            _context.Programs.Update(program);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var program = await _context.Programs.FindAsync(id);
            if (program != null)
            {
                _context.Programs.Remove(program);
                await _context.SaveChangesAsync();
            }
        }
    }
}
