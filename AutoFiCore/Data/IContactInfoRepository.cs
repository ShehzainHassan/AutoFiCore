using AutoFiCore.Models;

namespace AutoFiCore.Data
{
    public interface IContactInfoRepository
    {
        Task<ContactInfo> AddContactInfoAsync(ContactInfo contactInfo);
    }
}
