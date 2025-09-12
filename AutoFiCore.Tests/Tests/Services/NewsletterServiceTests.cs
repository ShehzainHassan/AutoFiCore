using AutoFiCore.Data;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Services
{
    public class NewsLetterServiceTests
    {
        private readonly Mock<INewsLetterRepository> _repositoryMock;
        private readonly Mock<ILogger<NewsLetterService>> _loggerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<INewsLetterRepository> _unitOfWorkNewsLetterMock;
        private readonly NewsLetterService _service;

        public NewsLetterServiceTests()
        {
            _repositoryMock = new Mock<INewsLetterRepository>();
            _loggerMock = new Mock<ILogger<NewsLetterService>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkNewsLetterMock = new Mock<INewsLetterRepository>();
            _unitOfWorkMock.SetupGet(u => u.NewsLetter).Returns(_unitOfWorkNewsLetterMock.Object);
            _service = new NewsLetterService(
                _repositoryMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task SubscribeToNewsLetterAsync_ReturnsFailure_WhenAlreadySubscribed()
        {
            var newsletter = new Newsletter { Email = "test@example.com" };
            _repositoryMock.Setup(r => r.IsAlreadySubscribed(newsletter.Email)).ReturnsAsync(true);

            var result = await _service.SubscribeToNewsLetterAsync(newsletter);

            Assert.False(result.IsSuccess);
            Assert.Equal("Email is already subscribed", result.Error);
        }

        [Fact]
        public async Task SubscribeToNewsLetterAsync_ReturnsSuccess_WhenNotSubscribed()
        {
            var newsletter = new Newsletter { Email = "test@example.com" };
            _repositoryMock.Setup(r => r.IsAlreadySubscribed(newsletter.Email)).ReturnsAsync(false);
            _unitOfWorkNewsLetterMock.Setup(r => r.SubscribeToNewsletter(newsletter)).ReturnsAsync(newsletter);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.SubscribeToNewsLetterAsync(newsletter);

            Assert.True(result.IsSuccess);
            Assert.Equal(newsletter, result.Value);
        }
    }
}