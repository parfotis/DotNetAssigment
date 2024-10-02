using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SampleProject.Configurations;
using SampleProject.Models;
using SampleProject.Services;

namespace SampleProject.Tests.UnitTests
{
	public class DbIpUpdateServiceTests : TestWithInMemoryDb
	{
		protected readonly IMemoryCache _cache;
		protected readonly Mock<IIpLookupService> _mockExternalService;
		protected readonly Mock<ILogger<DbIpLookupService>> _mockLogger;
		private readonly Mock<IOptions<IpUpdateJobConfiguration>> _mockUpdateJobConfig;
		private readonly DbIpUpdateService _service;

		protected readonly IpLookupServiceConfiguration _serviceConfig = new IpLookupServiceConfiguration
		{
			CacheExpirationMinutes = 2,
			CacheSlidingEnabled = false,
			CacheSlidingExpirationMinutes = 2
		};

		public DbIpUpdateServiceTests() : base()
		{

			_mockUpdateJobConfig = new Mock<IOptions<IpUpdateJobConfiguration>>();

			var updateJobConfig = new IpUpdateJobConfiguration { BatchSize = 5 };
			_mockUpdateJobConfig.Setup(x => x.Value).Returns(updateJobConfig);

			_cache = new MemoryCache(new MemoryCacheOptions());
			_mockExternalService = new Mock<IIpLookupService>();
			_mockLogger = new Mock<ILogger<DbIpLookupService>>();

			_service = new DbIpUpdateService(
				_context,
				_cache,
				_mockExternalService.Object,
				Options.Create(_serviceConfig),
				_mockUpdateJobConfig.Object,
				_mockLogger.Object
			);

		}

		[Fact]
		public async Task UpdateIpInfo_UpdatesCountry_WhenExternalServiceReturnsNewCountry()
		{
			var ipAddress = "37.6.160.243";
			var externalResult = new IpLookupResult
			{
				Ip = ipAddress,
				CountryName = "Canada",
				TwoLetterCode = "CA",
				ThreeLetterCode = "CAN"
			};

			_mockExternalService.Setup(es => es.LookupIp(ipAddress)).ReturnsAsync(externalResult);

			await _service.UpdateIpInfo();

			var updatedDbResult = await _context.IpAddresses.Include(i => i.Country)
				.FirstOrDefaultAsync(i => i.Ip == ipAddress);
			
			Assert.NotNull(updatedDbResult);
			Assert.Equal(externalResult.CountryName, updatedDbResult.Country.Name);
			Assert.Equal(externalResult.TwoLetterCode, updatedDbResult.Country.TwoLetterCode);
		}

		[Fact]
		public async Task UpdateIpInfo_DoesNotUpdate_WhenExternalServiceReturnsSameCountry()
		{
			var ipAddress = "37.6.160.244";
			var existingCountry = new Country { Name = "Greece", TwoLetterCode = "GR", ThreeLetterCode = "GRC", CreatedAt = DateTime.UtcNow };
			
			_context.Countries.Add(existingCountry);
			_context.IpAddresses.Add(new IpAddress { Ip = ipAddress, Country = existingCountry, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
			await _context.SaveChangesAsync();

			_mockExternalService.Setup(es => es.LookupIp(ipAddress)).ReturnsAsync(new IpLookupResult
			{
				Ip = ipAddress,
				CountryName = "Greece",
				TwoLetterCode = "GR",
				ThreeLetterCode = "GRC"
			});

			await _service.UpdateIpInfo();

			var updatedDbResult = await _context.IpAddresses.Include(i => i.Country)
				.FirstOrDefaultAsync(i => i.Ip == ipAddress);
			
			Assert.NotNull(updatedDbResult);
			Assert.Equal(existingCountry.Name, updatedDbResult.Country.Name);
		}
	}
}