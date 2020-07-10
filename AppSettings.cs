namespace AuthServer
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string Url { get; set; }
        public string[] WhitelistedIps { get; set; }
    }
}
