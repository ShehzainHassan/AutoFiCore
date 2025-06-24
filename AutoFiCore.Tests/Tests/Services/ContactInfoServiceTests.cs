using AutoFiCore.Data;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Threading.Tasks;

public class ContactInfoServiceTests
{
    private readonly Mock<IContactInfoRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<ContactInfoService>> _mockLogger;
    private readonly ContactInfoService _service;

    public ContactInfoServiceTests()
    {
        _mockRepository = new Mock<IContactInfoRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<ContactInfoService>>();

        _mockUnitOfWork.Setup(u => u.ContactInfo).Returns(_mockRepository.Object);

        _service = new ContactInfoService(_mockRepository.Object, _mockLogger.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task AddContactInfoAsync_ReturnsAddedContactInfo()
    {
        // Arrange
        var contact = new ContactInfo
        {
            FirstName = "John",
            LastName = "Doe",
            SelectedOption = "Test Drive",
            VehicleName = "BMW X5",
            PostCode = "12345",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            PreferredContactMethod = "Email",
            Comment = "Looking forward to it.",
            EmailMeNewResults = true
        };

        _mockRepository.Setup(r => r.AddContactInfoAsync(contact))
            .ReturnsAsync(contact);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.AddContactInfoAsync(contact);

        // Assert
        Assert.Equal(contact, result);
        _mockRepository.Verify(r => r.AddContactInfoAsync(contact), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
