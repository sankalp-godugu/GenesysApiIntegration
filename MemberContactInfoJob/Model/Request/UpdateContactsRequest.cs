using GenesysContactsProcessJob.Model.Common;

namespace GenesysContactsProcessJob.Model.Request
{
    public class UpdateContactsRequest
    {
        public string Id { get; set; }
        public string ContactListId { get; set; }
        public Data Data { get; set; }
        public bool Callable { get; set; }
        public PhoneNumberStatus PhoneNumberStatus => new();
    }
}
