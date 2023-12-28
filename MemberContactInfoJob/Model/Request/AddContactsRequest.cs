using GenesysContactsProcessJob.Model.Common;
using System;

namespace GenesysContactsProcessJob.Model.Request
{
    public class AddContactsRequest
    {
        public string Id { get; set; }
        public string ContactListId => Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId");
        public Data Data { get; set; }
        public bool Callable => true;
        public PhoneNumberStatus PhoneNumberStatus => new();
    }
}
