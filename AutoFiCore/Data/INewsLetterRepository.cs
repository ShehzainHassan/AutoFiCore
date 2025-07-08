using AutoFiCore.Models;

namespace AutoFiCore.Data
{
    public interface INewsLetterRepository
    {
        Task<Newsletter> SubscribeToNewsletter(Newsletter newsletter);
        Task<bool> IsAlreadySubscribed(string email);
    }
}
