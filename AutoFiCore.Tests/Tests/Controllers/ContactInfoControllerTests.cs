using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFiCore.Tests.Tests.Controllers
{
    using AutoFiCore.Controllers;
    using AutoFiCore.Models;
    using AutoFiCore.Services;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class ContactControllerTests
    {
        private readonly Mock<IContactInfoService> _mockService;
        private readonly ContactController _controller;

        public ContactControllerTests()
        {
            _mockService = new Mock<IContactInfoService>();
            _controller = new ContactController(_mockService.Object);
        }

        [Fact]
        public async Task AddContactInfo_ReturnsOkResult_WhenValid()
        {
            // Arrange
            var contact = new ContactInfo
            {
                FirstName = "Jane",
                LastName = "Smith",
                SelectedOption = "Inquiry",
                VehicleName = "Audi A4",
                PostCode = "54321",
                Email = "jane@example.com",
                PhoneNumber = "0987654321",
                PreferredContactMethod = "Call",
                Comment = "",
                EmailMeNewResults = false
            };

            _mockService.Setup(s => s.AddContactInfoAsync(contact))
                .ReturnsAsync(contact);

            // Act
            var result = await _controller.AddContactInfo(contact);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedContact = Assert.IsType<ContactInfo>(okResult.Value);

            Assert.Equal(contact.Email, returnedContact.Email);
            Assert.Equal(contact.FirstName, returnedContact.FirstName);

            _mockService.Verify(s => s.AddContactInfoAsync(contact), Times.Once);
        }

        [Fact]
        public async Task AddContactInfo_ReturnsBadRequest_WhenInvalid()
        {
            // Arrange
            var invalidContact = new ContactInfo();

            // Act
            var result = await _controller.AddContactInfo(invalidContact);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequest.Value);
        }
    }

}
