using Microsoft.EntityFrameworkCore.Storage;

namespace AutoFiCore.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        public IVehicleRepository Vehicles { get; }
        public IUserRepository Users { get; }
        public IContactInfoRepository ContactInfo { get; }

        public INewsLetterRepository NewsLetter { get; }
        public UnitOfWork(ApplicationDbContext context, IVehicleRepository vehicleRepository, IUserRepository userRepository, IContactInfoRepository contactInfoRepository, INewsLetterRepository newsLetterRepository)
        {
            _context = context;
            Vehicles = vehicleRepository;
            Users = userRepository;
            ContactInfo = contactInfoRepository;
            NewsLetter = newsLetterRepository;
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
