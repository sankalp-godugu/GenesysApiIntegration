using GenesysContactsProcessJob.Model.Common;
using System;

namespace GenesysContactsProcessJob.Model.Response
{
    public class DeleteContactsResponse
    {
        public string Id { get; set; }
        public string ContactListId { get; set; }
        public Data Data { get; set; }
        public CallRecords CallRecords { get; set; }
        public bool Callable { get; set; }
        public PhoneNumberStatus PhoneNumberStatus { get; set; }
        public ContactableStatus ContactableStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public string SelfUri { get; set; }
    }
}
