namespace EndpointX.Models
{
    public class AppSettings
    {
        public string JWTSecretKey { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
    }
}
