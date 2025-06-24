namespace AutoFiCore.Data
{
    public interface IUnitOfWork:IDisposable
    {
        IVehicleRepository Vehicles { get; }
        IUserRepository Users { get; }
        IContactInfoRepository ContactInfo { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
