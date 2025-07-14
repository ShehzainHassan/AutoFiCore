using AutoFiCore.Data;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AutoFiCore.Tests.Services
{
    public class NewsLetterServiceTests
    {
        private readonly Mock<INewsLetterRepository> _repo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<NewsLetterService>> _logger;
        private readonly NewsLetterService _service;

        public NewsLetterServiceTests()
        {
            _repo = new Mock<INewsLetterRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _logger = new Mock<ILogger<NewsLetterService>>();

            _mockUnitOfWork.Setup(u => u.NewsLetter).Returns(_repo.Object);

            _service = new NewsLetterService(_repo.Object, _logger.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public async Task SubscribeToNewsletterAsync_InvalidEmail_ReturnsFailureWithErrors()
        {
            // Arrange 
            var newsletter = new Newsletter { Email = "not-an-email" };

            // Act
            var result = await _service.SubscribeToNewsLetterAsync(newsletter);

            // Assert
            Assert.False(result.IsSuccess);

            Assert.True(!string.IsNullOrEmpty(result.Error) || result.Errors.Any());

            _repo.Verify(r => r.IsAlreadySubscribed(It.IsAny<string>()), Times.Never);
            _repo.Verify(r => r.SubscribeToNewsletter(It.IsAny<Newsletter>()), Times.Never);
        }


        [Fact]
        public async Task SubscribeToNewsletterAsync_EmailAlreadySubscribed_ReturnsFailureWithMessage()
        {
            // Arrange
            var newsletter = new Newsletter { Email = "duplicate@example.com" };
            _repo.Setup(r => r.IsAlreadySubscribed(newsletter.Email))
                 .ReturnsAsync(true);

            // Act
            var result = await _service.SubscribeToNewsLetterAsync(newsletter);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Email is already subscribed", result.Error);
            Assert.Empty(result.Errors);
            _repo.Verify(r => r.IsAlreadySubscribed(newsletter.Email), Times.Once);
            _repo.Verify(r => r.SubscribeToNewsletter(It.IsAny<Newsletter>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task SubscribeToNewsletterAsync_ValidEmail_ReturnsSuccessAndPersists()
        {
            // Arrange
            var newsletter = new Newsletter { Email = "john@example.com" };
            _repo.Setup(r => r.IsAlreadySubscribed(newsletter.Email))
                 .ReturnsAsync(false);
            _repo.Setup(r => r.SubscribeToNewsletter(newsletter))
                 .ReturnsAsync(newsletter);

            // Act
            var result = await _service.SubscribeToNewsLetterAsync(newsletter);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newsletter, result.Value);
            _repo.Verify(r => r.IsAlreadySubscribed(newsletter.Email), Times.Once);
            _repo.Verify(r => r.SubscribeToNewsletter(newsletter), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
