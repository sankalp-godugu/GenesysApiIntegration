using GenesysContactsProcessJob.Model.Common;

namespace GenesysContactsProcessJob.Model.Response
{
    public class GetContactsExportDataFromGenesysResponse
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
