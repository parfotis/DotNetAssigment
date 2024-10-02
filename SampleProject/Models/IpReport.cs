namespace SampleProject.Models
{
	public class IpReport
	{
		public required string CountryName { get; set; }
		public required int AddressesCount { get; set; }
		public required DateTime LastAddressUpdated { get; set; }
    }
}