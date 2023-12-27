using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenesysContactsProcessJob.Model.Request;

namespace GenesysContactsProcessJob.Model.Response
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class ConfigurationOverrides
    {
        public bool Priority { get; set; }
    }

    public class ContactableStatus
    {
        public Email Email { get; set; }
    }

    public class Email
    {
        public bool Contactable => true;
    }

    public class AddContactsResponse
    {
        public string Id { get; set; }
        public string contactListId = Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId");
        public Data Data { get; set; }
        public bool Callable => true;
        public PhoneNumberStatus PhoneNumberStatus { get; set; }
        public ContactableStatus ContactableStatus { get; set; }
        public ConfigurationOverrides ConfigurationOverrides { get; set; }
        public string DateCreated { get; set; }
        public string SelfUri { get; set; }
    }
}
