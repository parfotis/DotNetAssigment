namespace Assignment.Services
{
	public interface IIpLookupService
	{
		Task<IpLookupResult?> LookupIp(string ip);
	}
}