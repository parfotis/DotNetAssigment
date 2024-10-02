namespace SampleProject.Services
{
	public interface IIpLookupServiceFactory
{
    IIpLookupService CreateIpLookupService(string serviceType);
}
}