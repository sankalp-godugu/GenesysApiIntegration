using GenesysContactsProcessJob.Model.Common;
using System;

namespace GenesysContactsProcessJob.Model.Request
{
    public class UpdateContactsRequest
    {
        public string Id { get; set; }
        public string ContactListId => Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId");
        public Data Data { get; set; }
        public bool Callable { get; set; }
        public PhoneNumberStatus PhoneNumberStatus => new();
    }
}
