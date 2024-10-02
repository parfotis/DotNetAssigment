namespace SampleProject.Models
{
    public class Country
    {
        public int Id { get; set; }

		//Automatically truncate the Name to 50 chars due to DB constraints
		private string _name = "";
		public required string Name
		{
			get => _name;
			set => _name = value.Length > 50 ? value.Substring(0, 50) : value;
		}

        public required string TwoLetterCode { get; set; }
        public required string ThreeLetterCode { get; set; }
        public required DateTime CreatedAt { get; set; }

        public ICollection<IpAddress> IpAddresses { get; set; } = new List<IpAddress>();
	}
}