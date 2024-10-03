namespace Assignment.Configurations
{
	public class IpLookupServiceConfiguration
	{
		public int CacheExpirationMinutes { get; set; } = 5;
		public Boolean CacheSlidingEnabled { get; set; } = false;
		public int CacheSlidingExpirationMinutes { get; set; } = 5;
		public string ExternalServiceUrl { get; set; } = "";
	}
}