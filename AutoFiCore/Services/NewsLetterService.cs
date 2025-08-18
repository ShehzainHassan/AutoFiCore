using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Services
{
    public interface INewsLetterService
    {
        Task<Result<Newsletter>> SubscribeToNewsLetterAsync(Newsletter newsletter);
    }
    public class NewsLetterService : INewsLetterService
    {
        private readonly INewsLetterRepository _repository;
        private readonly ILogger<NewsLetterService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public NewsLetterService(INewsLetterRepository repository, ILogger<NewsLetterService> logger, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<Newsletter>> SubscribeToNewsLetterAsync(Newsletter newsletter)
        {
            var isAlreadySubcribed = await _repository.IsAlreadySubscribed(newsletter.Email);
            if (isAlreadySubcribed)
                return Result<Newsletter>.Failure("Email is already subscribed");

            var subscribed = await _unitOfWork.NewsLetter.SubscribeToNewsletter(newsletter);
            await _unitOfWork.SaveChangesAsync();
            return Result<Newsletter>.Success(subscribed);
        }
    }
}
