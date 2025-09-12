using AutoFiCore.Models;

namespace AutoFiCore.Data.Interfaces
{
    public interface INewsLetterRepository
    {
        Task<Newsletter> SubscribeToNewsletter(Newsletter newsletter);
        Task<bool> IsAlreadySubscribed(string email);
    }
}
