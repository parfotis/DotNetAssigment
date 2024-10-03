using Assignment.Models;

namespace Assignment.Services
{
	public interface IIpReportService
	{
		Task<List<IpReport>> ReportIpAddresses(string? countryName = null);
	}
}