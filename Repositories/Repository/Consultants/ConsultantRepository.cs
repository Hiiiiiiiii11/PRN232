using BussinessObjects;
using Repositories.IRepository.Consultants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository.Consultants
{
    public class ConsultantRepository : GenericRepository<Consultant>, IConsultantRepository
    {
        public ConsultantRepository(DrugUsePreventionDBContext context)
            : base(context) { }
    }
}
