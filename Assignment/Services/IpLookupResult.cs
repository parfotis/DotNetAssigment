using System.Text.Json.Serialization;

namespace Assignment.Services
{
	public class IpLookupResult
	{
		[JsonIgnore]
		public string? Ip { get; set; }
		public required string CountryName { get; set; }
		public required string TwoLetterCode { get; set; }
		public required string ThreeLetterCode { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj is IpLookupResult other)
			{
				return Ip == other.Ip &&
					CountryName == other.CountryName &&
					TwoLetterCode == other.TwoLetterCode &&
					ThreeLetterCode == other.ThreeLetterCode;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Ip, CountryName, TwoLetterCode, ThreeLetterCode);
		}
    }
}