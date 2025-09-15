using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Services
{
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
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        var isAlreadySubscribed = await _repository.IsAlreadySubscribed(newsletter.Email);
                        if (isAlreadySubscribed)
                        {
                            _logger.LogWarning("Subscription attempt failed: {Email} is already subscribed", newsletter.Email);
                            return Result<Newsletter>.Failure("Email is already subscribed.");
                        }

                        var subscribed = await _unitOfWork.NewsLetter.SubscribeToNewsletter(newsletter);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        _logger.LogInformation("Newsletter subscription successful for {Email}", newsletter.Email);
                        return Result<Newsletter>.Success(subscribed);
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError(ex, "Failed to subscribe {Email} to newsletter", newsletter.Email);
                        return Result<Newsletter>.Failure("Unexpected error occurred while subscribing to newsletter.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution strategy failed during newsletter subscription for {Email}", newsletter.Email);
                return Result<Newsletter>.Failure("Execution strategy failed.");
            }
        }
    }
}
