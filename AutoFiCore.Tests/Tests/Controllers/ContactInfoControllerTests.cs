using AutoFiCore.Controllers;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Controllers
{
    public class ContactControllerTests
    {
        private readonly Mock<IContactInfoService> _contactInfoServiceMock;
        private readonly Mock<ILogger<ContactController>> _loggerMock;
        private readonly TestContactController _controller;

        public ContactControllerTests()
        {
            _contactInfoServiceMock = new Mock<IContactInfoService>();
            _loggerMock = new Mock<ILogger<ContactController>>();
            _controller = new TestContactController(_contactInfoServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task AddContactInfo_ReturnsOk_WhenUserContextValid()
        {
            // Arrange
            var contactInfo = new ContactInfo
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                SelectedOption = "Test Drive",
                VehicleName = "Test car",
                PostCode = "12345",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                PreferredContactMethod = "Email",
                Comment = "Interested",
                EmailMeNewResults = true
            };
            _controller.SetUserContextValid(true, 42);
            _contactInfoServiceMock.Setup(s => s.AddContactInfoAsync(contactInfo))
                .ReturnsAsync(contactInfo);

            // Act
            var result = await _controller.AddContactInfo(contactInfo);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(contactInfo, okResult.Value);
        }

        [Fact]
        public async Task AddContactInfo_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            // Arrange
            var contactInfo = new ContactInfo();
            _controller.SetUserContextValid(false, 0);

            // Act
            var result = await _controller.AddContactInfo(contactInfo);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Contains("Invalid token", unauthorized.Value!.ToString());
        }

        private class TestContactController : ContactController
        {
            private bool _isUserContextValid;
            private int _userId;

            public TestContactController(
                IContactInfoService contactInfoService,
                ILogger<ContactController> logger)
                : base(contactInfoService, logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };
            }

            public void SetUserContextValid(bool valid, int userId)
            {
                _isUserContextValid = valid;
                _userId = userId;
            }

            protected override bool IsUserContextValid(out int userId)
            {
                userId = _userId;
                return _isUserContextValid;
            }

        }
    }
}