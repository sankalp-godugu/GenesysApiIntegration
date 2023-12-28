namespace GenesysContactsProcessJob.Model.Request
{
    public class Data
    {
        public string NhMemberId { get; set; }
        public string MemberName { get; set; }
        public string Language { get; set; }
        public string Address { get; set; }
        public string Region { get; set; }
        public string PhoneNumber { get; set; }
        public string CarrierName { get; set; }
        public string LoadDate { get; set; }
        public string DischargeDate { get; set; }
        public string DayCount { get; set; }
        public string AttemptCountToday { get; set; }
        public string AttemptCountTotal { get; set; }
    }

    public class PhoneNumber
    {
        public bool Callable => true;
    }

    public class PhoneNumberStatus
    {
        public PhoneNumber PhoneNumber => new();
    }

    public class AddContactsRequest
    {
        public string Id { get; set; }
        public string ContactListId { get; set; }
        public Data Data { get; set; }
        public bool Callable => true;
        public PhoneNumberStatus PhoneNumberStatus => new();
    }
}
