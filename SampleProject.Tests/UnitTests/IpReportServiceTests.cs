using Microsoft.AspNetCore.Mvc;
using Moq;
using SampleProject.Controllers;
using SampleProject.Models;
using SampleProject.Services;

namespace SampleProject.Tests.UnitTests
{
	public class IpReportControllerTests
	{
		private readonly Mock<IIpReportService> _ipReportServiceMock;
		private readonly IpReportController _controller;

		public IpReportControllerTests()
		{
			_ipReportServiceMock = new Mock<IIpReportService>();
			_controller = new IpReportController(_ipReportServiceMock.Object);
		}

		[Fact]
		public async Task GetIpReports_ReturnsOk_WithIpReports()
		{
			var reports = new List<IpReport>
			{
				new IpReport { CountryName = "USA", AddressesCount = 5, LastAddressUpdated = System.DateTime.UtcNow },
				new IpReport { CountryName = "Canada", AddressesCount = 3, LastAddressUpdated = System.DateTime.UtcNow }
			};

			_ipReportServiceMock.Setup(s => s.ReportIpAddresses((string)null)).ReturnsAsync(reports);

			var result = await _controller.GetIpReports();

			var okResult = Assert.IsType<OkObjectResult>(result);
			var returnedReports = Assert.IsType<List<IpReport>>(okResult.Value);
			Assert.Equal(2, returnedReports.Count);
		}

		[Fact]
		public async Task GetIpReports_ReturnsOk_WithEmptyList_WhenNoReports()
		{
			var reports = new List<IpReport>();
			_ipReportServiceMock.Setup(s => s.ReportIpAddresses((string)null)).ReturnsAsync(reports);

			var result = await _controller.GetIpReports();

			var okResult = Assert.IsType<OkObjectResult>(result);
			var returnedReports = Assert.IsType<List<IpReport>>(okResult.Value);
			Assert.Empty(returnedReports);
		}

		[Fact]
		public async Task GetIpReportByCountryName_ReturnsOk_WhenReportsFound()
		{
			var countryName = "Greece";
			var reports = new List<IpReport>
			{
				new IpReport { CountryName = countryName, AddressesCount = 2, LastAddressUpdated = System.DateTime.UtcNow }
			};

			_ipReportServiceMock.Setup(s => s.ReportIpAddresses(countryName)).ReturnsAsync(reports);

			var result = await _controller.GetIpReportByCountryName(countryName);

			var okResult = Assert.IsType<OkObjectResult>(result);
			var returnedReports = Assert.IsType<List<IpReport>>(okResult.Value);
			Assert.Single(returnedReports);
			Assert.Equal(countryName, returnedReports.First().CountryName);
		}

		[Fact]
		public async Task GetIpReportByCountryName_ReturnsNotFound_WhenNoReportsFound()
		{
			var countryName = "NonExistentCountry";
			_ipReportServiceMock.Setup(s => s.ReportIpAddresses(countryName)).ReturnsAsync(new List<IpReport>());

			var result = await _controller.GetIpReportByCountryName(countryName);

			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal($"No data found for country '{countryName}'.", notFoundResult.Value);
		}

		[Fact]
		public async Task GetIpReportByCountryName_ReturnsNotFound_WhenServiceReturnsNull()
		{
			var countryName = "NonExistentCountry";
			_ipReportServiceMock.Setup(s => s.ReportIpAddresses(countryName))
				.ReturnsAsync((List<IpReport>?)null); 
			var result = await _controller.GetIpReportByCountryName(countryName);

			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
			Assert.Equal($"No data found for country '{countryName}'.", notFoundResult.Value);
		}
	}
}