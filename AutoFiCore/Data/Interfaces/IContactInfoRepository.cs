using AutoFiCore.Models;

namespace AutoFiCore.Data.Interfaces
{
    public interface IContactInfoRepository
    {
        Task<ContactInfo> AddContactInfoAsync(ContactInfo contactInfo);
    }
}
