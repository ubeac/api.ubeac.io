using System.Collections.Generic;

namespace uBeac
{
    public class AppConfig
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public List<IdentityServerClient> IdentityServerClients { get; set; }
        public List<string> CorsOrigins { get; set; }

        public class IdentityServerClient
        {
            public string AuthenticationScheme { get; set; }
            public string Authority { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string ResponseType { get; set; }
            public string Scopes { get; set; }
            public string ApiName { get; set; }
        }
    }

}
