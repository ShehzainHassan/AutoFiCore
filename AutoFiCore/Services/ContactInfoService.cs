using AutoFiCore.Data;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Services
{
    public interface IContactInfoService
    {
        Task<ContactInfo> AddContactInfoAsync(ContactInfo contactInfo);
    }

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

        public async Task<ContactInfo> AddContactInfoAsync(ContactInfo contactInfo)
        {
            await _unitOfWork.ContactInfo.AddContactInfoAsync(contactInfo);
            await _unitOfWork.SaveChangesAsync();
            return contactInfo;
        }
    }
}
