using Microsoft.EntityFrameworkCore;
using Assignment.Data;
using Assignment.Models;

namespace Assignment.Services
{
	public class IpReportService : IIpReportService
	{
		private readonly ApplicationDbContext _context;

		public IpReportService(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<List<IpReport>> ReportIpAddresses(string? countryName = null)
		{
			//Automatically truncate the Country Name to 50 chars due to DB constraints
			if (!string.IsNullOrEmpty(countryName))
			{
				countryName = countryName.Length > 50 ? countryName.Substring(0, 50) : countryName;
			}

			var result = await FetchIpReport(countryName);
			return result;
		}

		private async Task<List<IpReport>> FetchIpReport(string? countryName = null)
		{
			var query = BuildIpReportQuery(countryName);
			var result = await _context.IpReports.FromSqlRaw(query).ToListAsync();
			return result;
		}

		private string BuildIpReportQuery(string? countryName)
		{
			var whereClause = string.IsNullOrEmpty(countryName) ? "" : $"WHERE c.Name = '{countryName}'";

			var query = $@"
				SELECT 
					c.Name AS CountryName, 
					COUNT(i.Id) AS AddressesCount, 
					MAX(i.UpdatedAt) AS LastAddressUpdated 
				FROM 
					IpAddresses i
				JOIN 
					Countries c ON i.CountryId = c.Id
				{whereClause}
				GROUP BY 
					c.Name
				";

			return query;
		}
	}
}