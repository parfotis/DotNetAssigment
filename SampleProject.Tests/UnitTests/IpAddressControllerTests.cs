using Microsoft.AspNetCore.Mvc;
using Moq;
using SampleProject.Controllers;
using SampleProject.Services;


namespace SampleProject.Tests.UnitTests
{
	public class IpAddressControllerTests
	{
		private readonly Mock<IIpLookupServiceFactory> _ipLookupServiceFactoryMock;
		private readonly Mock<IIpLookupService> _ipLookupServiceMock;
		private readonly IpAddressController _controller;

		public IpAddressControllerTests()
		{
			_ipLookupServiceFactoryMock = new Mock<IIpLookupServiceFactory>();
			_ipLookupServiceMock = new Mock<IIpLookupService>();
			_ipLookupServiceFactoryMock.Setup(f => f.CreateIpLookupService(It.IsAny<IpLookupServiceType>())).Returns(_ipLookupServiceMock.Object);
			_controller = new IpAddressController(_ipLookupServiceFactoryMock.Object);
		}

		[Fact]
		public async Task GetIpAddressByIp_ReturnsBadRequest_WhenInvalidIp()
		{
			var invalidIp = "999.999.999.999";

			var result = await _controller.GetIpAddressByIp(invalidIp);

			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
			Assert.Equal("Invalid IP address format.", badRequestResult.Value);
		}

		[Fact]
		public async Task GetIpAddressByIp_ReturnsNotFound_WhenIpNotFound()
		{
			var ip = "37.6.160.245";
			_ipLookupServiceMock.Setup(s => s.LookupIp(ip)).ReturnsAsync((IpLookupResult)null);

			var result = await _controller.GetIpAddressByIp(ip);

			var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
			Assert.Equal($"Information could not be found for IP Address {ip}.", notFoundResult.Value);
		}

		[Fact]
		public async Task GetIpAddressByIp_ReturnsOk_WhenIpFound()
		{
			var ip = "37.6.160.243";
			var lookupResult = new IpLookupResult { Ip = ip, CountryName = "Greece", TwoLetterCode = "GR", ThreeLetterCode = "GRC"	};

			_ipLookupServiceMock.Setup(s => s.LookupIp(ip)).ReturnsAsync(lookupResult);

			var result = await _controller.GetIpAddressByIp(ip);

			var okResult = Assert.IsType<OkObjectResult>(result.Result);
			var returnedResult = Assert.IsType<IpLookupResult>(okResult.Value);
			Assert.Equal(lookupResult, returnedResult);
		}
	}
}