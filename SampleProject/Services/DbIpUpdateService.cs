using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SampleProject.Configurations;
using SampleProject.Data;
using SampleProject.Models;

namespace SampleProject.Services
{
	public class DbIpUpdateService : DbIpLookupService
	{
		private readonly IpUpdateJobConfiguration _updateJobConfig;

		public DbIpUpdateService(
			ApplicationDbContext context,
			IMemoryCache cache,
			IIpLookupService externalService,
			IOptions<IpLookupServiceConfiguration> lookupServiceConfigOptions,
			IOptions<IpUpdateJobConfiguration> updateJobConfigOptions,
			ILogger<DbIpLookupService> logger
		) : base(context, cache, externalService, lookupServiceConfigOptions, logger)
		{
			_updateJobConfig = updateJobConfigOptions.Value;
		}

		public async Task UpdateIpInfo()
		{
			int batchSize = _updateJobConfig.BatchSize;
			int totalRecords;
			int processedRecords = 0;
			int currentBatch = 0;

			_logger.LogInformation("Initiating IP update.");
		
			do
			{
				var ipAddresses = await ReadIpAddressPage(processedRecords, batchSize);

				totalRecords = ipAddresses.Count;
				currentBatch++;
				if (totalRecords > 0)
				{
					_logger.LogInformation("Processing batch {}: {}-{}...",currentBatch, processedRecords + 1, processedRecords + totalRecords);
				}

				using (var transaction = await _context.Database.BeginTransactionAsync())
				{
					try
					{
						foreach (var ipAddress in ipAddresses)
						{
							var lookupResult = await _externalService.LookupIp(ipAddress.Ip);
							if (lookupResult != null)
							{
								//Invalidate cache if Country info has changed
								if (
									ipAddress.Country.Name != lookupResult.CountryName ||
									ipAddress.Country.TwoLetterCode != lookupResult.TwoLetterCode ||
									ipAddress.Country.ThreeLetterCode != lookupResult.ThreeLetterCode
									)
									{
										_cache.Remove($"IpAddress_{ipAddress.Ip}");
									}

								await UpdateIpAddress(ipAddress, lookupResult);
							}
							//Don't update if remote lookup was unsuccessful
						}
						//Commit once for each batch
						await _context.SaveChangesAsync();
						await transaction.CommitAsync();
					}
					catch (Exception)
					{
						await transaction.RollbackAsync();
					}
				}
				
				processedRecords += totalRecords;
				
			} while (totalRecords == batchSize);

			_logger.LogInformation("IP update finished.");
		}

		protected async Task<List<IpAddress>> ReadIpAddressPage(int processedRecords, int batchSize)
		{
			var ipAddresses = await _context.IpAddresses
				.OrderBy(i => i.Id)
				.Skip(processedRecords)
				.Take(batchSize)
				.Include(i => i.Country)
				.ToListAsync();
			
			return ipAddresses;
		}

		protected async Task UpdateIpAddress(IpAddress ipAddress, IpLookupResult lookupResult)
		{
			var country = await _context.Countries.FirstOrDefaultAsync(c => c.TwoLetterCode == lookupResult.TwoLetterCode);
			if (country == null)
			{
				country = CreateCountry(lookupResult);
			}

			ipAddress.Country = country;
			ipAddress.UpdatedAt = DateTime.UtcNow;
			_context.IpAddresses.Update(ipAddress);

			//NOTE: delegate commit to the calling function
		}
	}
}