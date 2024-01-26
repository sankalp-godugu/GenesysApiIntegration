using GenesysContactsProcessJob.Model.Common;

namespace GenesysContactsProcessJob.Model.Response
{
    public class GetContactListResponse
    {
        public string Id { get; set; }
        public Data Data { get; set; }
        public bool ContactCallable { get; set; }
        public ContactableBy ContactableBy { get; set; }
        public string ZipCodeAutomaticTimeZone { get; set; }
        public CallRecords CallRecords { get; set; }
        public Sms Sms { get; set; }
        public string AutomaticTimeZone_phoneNumber { get; set; }
    }
}
