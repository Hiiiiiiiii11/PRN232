using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.IRepository;

namespace Repositories.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository Access
        IGenericRepository<T> GetRepository<T>()
            where T : class;

        // Save Changes
        void Save(); // Synchronous
        Task<int> SaveAsync(); // Asynchronous

        // Transaction Management (Both Sync & Async)
        void BeginTransaction();
        Task BeginTransactionAsync();
        void CommitTransaction();
        Task CommitTransactionAsync();
        void RollBack();
        Task RollbackTransactionAsync();
    }
}
