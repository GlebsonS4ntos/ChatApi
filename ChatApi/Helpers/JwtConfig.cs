namespace ChatApi.Helpers
{
    public class JwtConfig
    {
        public string AccessTokenKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AcessTokenExpirationMinutes { get; set; }
    }
}
