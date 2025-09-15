using AutoFiCore.Data.Interfaces;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Services
{
    public class ContactInfoService : IContactInfoService
    {
        private readonly IContactInfoRepository _repository;
        private readonly ILogger<ContactInfoService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ContactInfoService(IContactInfoRepository repository, ILogger<ContactInfoService> logger, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ContactInfo>> AddContactInfoAsync(ContactInfo contactInfo)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        await _unitOfWork.ContactInfo.AddContactInfoAsync(contactInfo);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        return Result<ContactInfo>.Success(contactInfo);
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<ContactInfo>.Failure("Unexpected error occurred while adding contact info.");
                    }
                });
            }
            catch (Exception ex)
            {
                return Result<ContactInfo>.Failure("Execution strategy failed.");
            }
        }
    }
}