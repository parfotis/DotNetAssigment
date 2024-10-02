namespace SampleProject.Services
{
	public class IpLookupServiceFactory : IIpLookupServiceFactory
	{
		private readonly IIpLookupService _dbIpUpdateService;
		private readonly IIpLookupService _externalIpLookupService;

		public IpLookupServiceFactory(DbIpUpdateService dbIpUpdateService, ExternalIpLookupService externalIpLookupService)
		{
			_dbIpUpdateService = dbIpUpdateService;
			_externalIpLookupService = externalIpLookupService;
		}

		public IIpLookupService CreateIpLookupService(string serviceType)
		{
			if (serviceType == "external")
			{
				return _externalIpLookupService;
			}
			else
			{
				return _dbIpUpdateService;
			}
		}
	}
}