using GenesysContactsProcessJob.Model.Common;

namespace GenesysContactsProcessJob.Model.Request
{
    public class PostContactsRequest
    {
        public long Id { get; set; }
        public string ContactListId { get; set; }
        public Data Data { get; set; }
        public bool Callable => true;
        public PhoneNumberStatus PhoneNumberStatus => new();
    }
}
