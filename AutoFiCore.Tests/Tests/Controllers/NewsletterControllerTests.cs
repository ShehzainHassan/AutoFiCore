using AutoFiCore.Controllers;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Controllers
{
    public class NewsLetterControllerTests
    {
        private readonly Mock<INewsLetterService> _serviceMock;
        private readonly NewsLetterController _controller;

        public NewsLetterControllerTests()
        {
            _serviceMock = new Mock<INewsLetterService>();
            _controller = new NewsLetterController(_serviceMock.Object);
        }

        [Fact]
        public async Task AddEmailToSubscribe_ReturnsOk_WhenSuccess()
        {
            var newsletter = new Newsletter { Id = 1, Email = "test@example.com" };
            var result = Result<Newsletter>.Success(newsletter);
            _serviceMock.Setup(s => s.SubscribeToNewsLetterAsync(newsletter)).ReturnsAsync(result);

            var response = await _controller.AddEmailToSubscribe(newsletter);

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal(newsletter, ok.Value);
        }

        [Fact]
        public async Task AddEmailToSubscribe_ReturnsBadRequest_WhenErrors()
        {
            var newsletter = new Newsletter { Id = 1, Email = "test@example.com" };
            var errors = new List<string> { "Invalid email" };
            var result = Result<Newsletter>.Failure(errors);
            _serviceMock.Setup(s => s.SubscribeToNewsLetterAsync(newsletter)).ReturnsAsync(result);

            var response = await _controller.AddEmailToSubscribe(newsletter);

            var bad = Assert.IsType<BadRequestObjectResult>(response.Result);
            Assert.Contains("errors", bad.Value!.ToString());
        }

        [Fact]
        public async Task AddEmailToSubscribe_ReturnsConflict_WhenError()
        {
            var newsletter = new Newsletter { Id = 1, Email = "test@example.com" };
            var result = Result<Newsletter>.Failure("Already subscribed");
            _serviceMock.Setup(s => s.SubscribeToNewsLetterAsync(newsletter)).ReturnsAsync(result);

            var response = await _controller.AddEmailToSubscribe(newsletter);

            var conflict = Assert.IsType<ConflictObjectResult>(response.Result);
            Assert.Contains("message", conflict.Value!.ToString());
        }
    }
}