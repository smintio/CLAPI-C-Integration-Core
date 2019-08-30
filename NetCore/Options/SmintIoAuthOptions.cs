namespace Client.Options
{
    public class SmintIoAuthOptions
    {
        public SmintIoAuthOptions() { }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string RedirectUri { get; set; }
    }
}
