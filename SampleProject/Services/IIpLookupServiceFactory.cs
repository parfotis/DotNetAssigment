namespace SampleProject.Services
{
	public interface IIpLookupServiceFactory
{
    IIpLookupService CreateIpLookupService(IpLookupServiceType serviceType);
}
}