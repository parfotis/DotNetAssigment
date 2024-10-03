using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Assignment.Configurations;
using Assignment.Services;

namespace Assignment.Tests.UnitTests
{
    public class DbIpLookupServiceTests : TestWithInMemoryDb
    {
        protected readonly IMemoryCache _cache;
        protected readonly Mock<IIpLookupService> _mockExternalService;
        protected readonly Mock<ILogger<DbIpLookupService>> _mockLogger;
        protected readonly DbIpLookupService _service;

        protected readonly IpLookupServiceConfiguration _serviceConfig = new IpLookupServiceConfiguration
        {
            CacheExpirationMinutes = 2,
            CacheSlidingEnabled = false,
            CacheSlidingExpirationMinutes = 2
        };

        public DbIpLookupServiceTests() : base()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _mockExternalService = new Mock<IIpLookupService>();
            _mockLogger = new Mock<ILogger<DbIpLookupService>>();

            var optionsAccessor = Options.Create(_serviceConfig);
            
            _service = new DbIpLookupService(
                _context,
                _cache,
                _mockExternalService.Object,
                optionsAccessor,
                _mockLogger.Object);
        }
        
        [Fact]
        public async Task LookupIp_ReturnsCachedResult_WhenCacheHit()
        {
            var ip = "37.6.160.243";
            var expectedResult = new IpLookupResult { Ip = ip, CountryName = "Greece", TwoLetterCode = "GR", ThreeLetterCode = "GRC" };

            _cache.Set($"IpAddress_{ip}", expectedResult);

            var result = await _service.LookupIp(ip);

            Assert.Equal(expectedResult, result);
            _mockExternalService.Verify(s => s.LookupIp(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LookupIp_QueriesDatabase_WhenCacheMiss()
        {
            var ip = "37.6.160.244";
            var expectedResult = new IpLookupResult { Ip = ip, CountryName = "Greece", TwoLetterCode = "GR", ThreeLetterCode = "GRC" };

            var result = await _service.LookupIp(ip);

            Assert.Equal(expectedResult, result);
            _mockExternalService.Verify(s => s.LookupIp(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LookupIp_ReturnsNull_WhenIpNotFound()
        {
            var ipAddress = "37.6.160.245";
            _mockExternalService.Setup(es => es.LookupIp(ipAddress)).ReturnsAsync((IpLookupResult?)null);

            var result = await _service.LookupIp(ipAddress);

            Assert.Null(result);
        }

        [Fact]
        public async Task LookupIp_AddsNewIpAddress_WhenExternalServiceReturnsResult()
        {
            var ipAddress = "1.1.1.1";
            var externalResult = new IpLookupResult
            {
                Ip = ipAddress,
                CountryName = "Australia",
                TwoLetterCode = "AU",
                ThreeLetterCode = "AUS"
            };

            _mockExternalService.Setup(es => es.LookupIp(ipAddress)).ReturnsAsync(externalResult);

            var result = await _service.LookupIp(ipAddress);

            Assert.NotNull(result);
            Assert.Equal(externalResult, result);

            var dbResult = await _context.IpAddresses.Include(i => i.Country)
                .FirstOrDefaultAsync(i => i.Ip == ipAddress);

            Assert.NotNull(dbResult);
            Assert.Equal(externalResult.CountryName, dbResult.Country.Name);
        }
    }
}