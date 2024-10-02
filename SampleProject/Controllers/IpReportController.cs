using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleProject.Data;
using SampleProject.Models;
using SampleProject.Services;

namespace SampleProject.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class IpReportController : ControllerBase
	{
		private readonly IIpReportService _ipReportService;

		public IpReportController(IIpReportService ipReportService)
		{
			_ipReportService = ipReportService;
		}

        // GET: api/ipreport
		[HttpGet]
		public async Task<IActionResult> GetIpReports()
		{
			var result = await _ipReportService.ReportIpAddresses();

			return Ok(result);
		}

        // GET: api/ipreport/{countryName}
		[HttpGet("{countryName}")]
		public async Task<IActionResult> GetIpReportByCountryName(string countryName)
		{
			var result = await _ipReportService.ReportIpAddresses(countryName);

			if (result == null || !result.Any())
			{
				return NotFound($"No data found for country '{countryName}'.");
			}

			return Ok(result);
		}
	}
}