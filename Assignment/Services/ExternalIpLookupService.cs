using Microsoft.Extensions.Options;
using Assignment.Configurations;

namespace Assignment.Services
{
	public class ExternalIpLookupService : IIpLookupService
	{
		private readonly HttpClient _httpClient;
		private readonly IpLookupServiceConfiguration _config;
		private readonly ILogger<ExternalIpLookupService> _logger;

		public ExternalIpLookupService(
			HttpClient httpClient, 
			IOptions<IpLookupServiceConfiguration> configOptions, 
			ILogger<ExternalIpLookupService> logger
		)
		{
			_httpClient = httpClient;
			_config = configOptions.Value;
			_logger = logger;
		}

		public async Task<IpLookupResult?> LookupIp(string ip)
		{
			try
			{
				var response = await _httpClient.GetStringAsync($"{_config.ExternalServiceUrl}/{ip}");
				if (string.IsNullOrEmpty(response))
				{
					_logger.LogInformation("No result found for remote IP lookup: {}.", ip);
					return null;
				}

				var lookupResult = parseResponse(response, ip);
				if (lookupResult == null)
				{
					_logger.LogInformation("Invalid response received for remote IP lookup: {}.\nResponse:\n{}", ip, response);
					return null;
				}
				
				_logger.LogInformation("Successfully received response for remote IP lookup: {}.\nResponse:{}\n", ip, response);
				return lookupResult;

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while looking up IP: {}.", ip);
				return null;
			}
		}

		private IpLookupResult? parseResponse(string response, string ip){
			var responseFields = response.Split(';');
			if (responseFields.Length < 4 || responseFields[0] != "1")
			{
				return null;
			}

			return new IpLookupResult
			{
				Ip = ip,
				CountryName = responseFields[3],
				TwoLetterCode = responseFields[1],
				ThreeLetterCode = responseFields[2]
			};
		}
	}
}