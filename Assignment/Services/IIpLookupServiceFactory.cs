namespace Assignment.Services
{
	public interface IIpLookupServiceFactory
{
    IIpLookupService CreateIpLookupService(IpLookupServiceType serviceType);
}
}