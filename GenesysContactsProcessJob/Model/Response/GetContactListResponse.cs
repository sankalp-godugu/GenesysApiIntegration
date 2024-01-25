using GenesysContactsProcessJob.Model.Common;
using System;

namespace GenesysContactsProcessJob.Model.Response
{
    public class GetContactListResponse
    {
        public string Id { get; set; }
        public Data Data { get; set; }
        public bool Callable { get; set; }
        public PhoneNumberStatus PhoneNumberStatus => new();
        public ContactableStatus ContactableStatus => new();
        public string WrapUpCode { get; set; }
        public string Region { get; set; }
    }
}
