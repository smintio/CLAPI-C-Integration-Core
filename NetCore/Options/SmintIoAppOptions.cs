namespace Client.Options
{
    public class SmintIoAppOptions
    {
        public SmintIoAppOptions() { }

        public string TenantId { get; set; }

        public int ChannelId { get; set; }

        public string[] ImportLanguages { get; set; }
    }
}
