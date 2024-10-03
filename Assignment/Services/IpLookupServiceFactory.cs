namespace Assignment.Services
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

		public IIpLookupService CreateIpLookupService(IpLookupServiceType serviceType)
		{
			if (serviceType == IpLookupServiceType.External)
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