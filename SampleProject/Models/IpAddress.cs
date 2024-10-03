namespace SampleProject.Models
{
    public class IpAddress
    {
        public int Id { get; set; }

        public int CountryId { get; set; }
	    public required Country Country { get; set; }

        public required string Ip { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
	}
}