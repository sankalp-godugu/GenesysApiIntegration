﻿using GenesysContactsProcessJob.Model.Common;

namespace GenesysContactsProcessJob.Model.Response
{
    public class PostContactListResponse
    {
        public string Id { get; set; }
        public string ContactListId { get; set; }
        public Data Data { get; set; }
        public bool Callable => true;
        public PhoneNumberStatus PhoneNumberStatus { get; set; }
        public ContactableBy ContactableStatus { get; set; }
        public ConfigurationOverrides ConfigurationOverrides { get; set; }
        public string DateCreated { get; set; }
        public string SelfUri { get; set; }
    }
}
