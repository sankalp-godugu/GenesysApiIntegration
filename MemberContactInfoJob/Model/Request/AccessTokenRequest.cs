using System;

namespace GenesysContactsProcessJob.Model.Request
{
    public class AccessTokenRequest
    {
        public string Grant_Type => Environment.GetEnvironmentVariable("GrantType");
        public string client_id => Environment.GetEnvironmentVariable("ClientId");
        public string client_secret => Environment.GetEnvironmentVariable("ClientSecret");
    }
}
