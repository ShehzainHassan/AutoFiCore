using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AutoFiCore.Data
{
    public class DbContactInfoRepository:IContactInfoRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<DbContactInfoRepository> _logger;
        public DbContactInfoRepository(ApplicationDbContext dbContext, ILogger<DbContactInfoRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public Task<ContactInfo> AddContactInfoAsync(ContactInfo contactInfo)
        {
            _dbContext.ContactInfos.Add(contactInfo);
            return Task.FromResult(contactInfo);
        }
    }
}

