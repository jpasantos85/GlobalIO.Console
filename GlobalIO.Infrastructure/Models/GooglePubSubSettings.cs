namespace GlobalIO.Infrastructure.Models
{
    public class GooglePubSubSettings
    {
        public const string SectionName = "GooglePubSub";
        public string ProjectId { get; set; }
        public string EmulatorHost { get; set; }
        public bool UseEmulator { get; set; }
        public int TimeoutSeconds { get; set; }
        public int MaxMessages { get; set; }
    }
}
