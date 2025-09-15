using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IContactInfoService
    {
        Task<Result<ContactInfo>> AddContactInfoAsync(ContactInfo contactInfo);
    }
}
