using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SampleProject.Configurations;
using SampleProject.Data;
using SampleProject.Models;

namespace SampleProject.Services
{
	public class DbIpLookupService : IIpLookupService
	{
		protected readonly ApplicationDbContext _context;
		protected readonly IMemoryCache _cache;
		protected readonly MemoryCacheEntryOptions _cacheEntryOptions;
		protected readonly IIpLookupService _externalService;
		protected readonly IpLookupServiceConfiguration _lookupServiceConfig;
		protected readonly ILogger<DbIpLookupService> _logger;

		public DbIpLookupService(
			ApplicationDbContext context,
			IMemoryCache cache,
			IIpLookupService externalService,
			IOptions<IpLookupServiceConfiguration> lookupServiceConfigOptions,
			ILogger<DbIpLookupService> logger
		)
		{
			_context = context;
			_cache = cache;
			_externalService = externalService;
			_lookupServiceConfig = lookupServiceConfigOptions.Value;
			_cacheEntryOptions = new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_lookupServiceConfig.CacheExpirationMinutes),
				SlidingExpiration = _lookupServiceConfig.CacheSlidingEnabled ? TimeSpan.FromMinutes(_lookupServiceConfig.CacheSlidingExpirationMinutes) : null
			};
			_logger = logger;
		}

		public async Task<IpLookupResult?> LookupIp(string ip)
		{
			var cacheKey = $"IpAddress_{ip}";

            //Try to fetch result from memory cache
			if (!_cache.TryGetValue<IpLookupResult>(cacheKey, out var lookupResult))
			{
				_logger.LogInformation("Cache miss for IP lookup: {}; querying the DB.", ip);
				lookupResult = await ReadIpAddress(ip);

				if (lookupResult == null)
				{
					_logger.LogInformation("DB IP lookup found no results for: {}; performing remote IP lookup.", ip);
					lookupResult = await _externalService.LookupIp(ip);
					
					if (lookupResult != null)
					{
						using (var transaction = await _context.Database.BeginTransactionAsync())
						{
							try
							{
								//Store the new IP in DB (Country as well if new)
								await CreateIpAddress(lookupResult);
								await _context.SaveChangesAsync();
								await transaction.CommitAsync();
							}
					        catch (Exception)
							{
								await transaction.RollbackAsync();
							}
						}
					}
				}

				_cache.Set(cacheKey, lookupResult, _cacheEntryOptions);
			}

			//Could be null
			return lookupResult;
		}

		protected async Task<IpAddress> CreateIpAddress(IpLookupResult lookupResult)
		{
			var country = await _context.Countries.FirstOrDefaultAsync(c => c.TwoLetterCode == lookupResult.TwoLetterCode);
			if (country == null)
			{
				country = CreateCountry(lookupResult);
			}

			var ipAddress = new IpAddress
			{
				Ip = lookupResult.Ip,
				Country = country,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			_context.IpAddresses.Add(ipAddress);

			//NOTE: delegate commit to the calling function

			return ipAddress;
		}

		protected async Task<IpLookupResult?> externalLookupIp(string ip)
		{
			return await _externalService.LookupIp(ip);
		}

		protected async Task<IpLookupResult?> ReadIpAddress(string ip)
		{
			var lookupResult = await _context.IpAddresses
                .Include(i => i.Country)
                .Where(i => i.Ip == ip)
                .Select( i => new IpLookupResult 
                    {
                        Ip = i.Ip,
                        CountryName = i.Country.Name,
                        TwoLetterCode =  i.Country.TwoLetterCode,
                        ThreeLetterCode =  i.Country.ThreeLetterCode
                    }
                )
                .Distinct()
                .FirstOrDefaultAsync();

			return lookupResult;
		}

		protected Country CreateCountry(IpLookupResult lookupResult)
		{
			var country = new Country
			{
				Name = lookupResult.CountryName,
				TwoLetterCode = lookupResult.TwoLetterCode,
				ThreeLetterCode = lookupResult.ThreeLetterCode,
				CreatedAt = DateTime.UtcNow
			};

			_context.Countries.Add(country);

			//NOTE: delegate commit to the calling function

			return country;
		}
	}
}