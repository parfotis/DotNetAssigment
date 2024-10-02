using SampleProject.Models;

namespace SampleProject.Services
{
	public interface IIpReportService
	{
		Task<List<IpReport>> ReportIpAddresses(string? countryName = null);
	}
}