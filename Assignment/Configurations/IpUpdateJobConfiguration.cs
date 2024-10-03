namespace Assignment.Configurations
{
	public class IpUpdateJobConfiguration
	{
		public TimeSpan CommandBatchMaxTimeout { get; set; }
		public TimeSpan SlidingInvisibilityTimeout { get; set; }
		public TimeSpan QueuePollInterval { get; set; }
		public bool UseRecommendedIsolationLevel { get; set; }
		public bool DisableGlobalLocks { get; set; }
		public string ExecutionInterval { get; set; } = "0 * * * *";
		public int BatchSize { get; set; } = 100;
	}
}