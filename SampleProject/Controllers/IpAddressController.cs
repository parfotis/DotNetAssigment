using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using SampleProject.Models;
using SampleProject.Services;

namespace SampleProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IpAddressController : ControllerBase
    {
        private readonly IIpLookupService _ipLookupService;

        public IpAddressController(IIpLookupServiceFactory ipLookupServiceFactory)
        {
            _ipLookupService = ipLookupServiceFactory.CreateIpLookupService("lookup");
        }

        // GET: api/ipaddress/{ip}
        [HttpGet("{ip}")]
        public async Task<ActionResult<IpAddress>> GetIpAddressByIp(string ip)
        {
            var cacheKey = $"IpAddress_{ip}";
            
            if (!IsValidIpAddress(ip))
            {
                return BadRequest("Invalid IP address format.");
            }

            var lookupResult = await _ipLookupService.LookupIp(ip);

            if (lookupResult == null)
            {
                return NotFound($"Information could not be found for IP Address {ip}.");
            }

            return Ok(lookupResult);
        }

        private bool IsValidIpAddress(string ip)
        {
            string ipv4Pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                                @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                                @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                                @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

            return Regex.IsMatch(ip, ipv4Pattern);
        }
    }
}