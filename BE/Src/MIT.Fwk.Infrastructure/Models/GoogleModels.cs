namespace MIT.Fwk.Infrastructure.Models
{
    public class GoogleAuthModel
    {
        public string access_token { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
        public string id_token { get; set; }

    }
    public class GoogleProfileModel
    {
        public string iss { get; set; }
        public string azp { get; set; }
        public string aud { get; set; }
        public string sub { get; set; }
        public string hd { get; set; }
        public string email { get; set; }
        public string email_verified { get; set; }
        public string at_hash { get; set; }
        public string iat { get; set; }
        public string exp { get; set; }
        public string alg { get; set; }
        public string kid { get; set; }
        public string typ { get; set; }

    }
}
