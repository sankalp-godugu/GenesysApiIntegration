﻿namespace GenesysContactsProcessJob.Model.Response
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class GetContactsResponse
    {
        public string Id { get; set; }
        public string contactListId = "8518a928-6c33-491e-b43a-bf98ec790f7b";
        public Data Data { get; set; }
        public bool Callable => true;
        public PhoneNumberStatus PhoneNumberStatus { get; set; }
        public ContactableStatus ContactableStatus { get; set; }
        public ConfigurationOverrides ConfigurationOverrides { get; set; }
        public string DateCreated { get; set; }
        public string SelfUri { get; set; }
    }
}