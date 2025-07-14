using AutoFiCore.Controllers;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoFiCore.Tests.Tests.Controllers
{
    public class NewsletterControllerTests
    {
        private readonly Mock<INewsLetterService> _mockService;
        private readonly NewsLetterController _controller;

        public NewsletterControllerTests()
        {
            _mockService = new Mock<INewsLetterService>();
            _controller = new NewsLetterController(_mockService.Object);
        }
        [Fact]
        public async Task AddEmailToSubscribe_ReturnsOk_WhenServiceSucceeds()
        {
            // Arrange
            var newsletter = new Newsletter { Email = "test@example.com" };

            var success = Result<Newsletter>.Success(newsletter);

            _mockService
                .Setup(s => s.SubscribeToNewsLetterAsync(newsletter))
                .ReturnsAsync(success);

            // Act
            var actionResult = await _controller.AddEmailToSubscribe(newsletter);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returned = Assert.IsType<Newsletter>(ok.Value);
            Assert.Equal(newsletter.Email, returned.Email);

            _mockService.Verify(
                s => s.SubscribeToNewsLetterAsync(newsletter),
                Times.Once);
        }
    }
}
