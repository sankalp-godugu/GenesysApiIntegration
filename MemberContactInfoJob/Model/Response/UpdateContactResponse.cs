using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemberContactInfoJob.Model.Response
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class CallRecords
    {
        public PhoneNumber PhoneNumber { get; set; }
    }

    public class Data
    {
        public string TransactionId { get; set; }
        public string NhMemberId { get; set; }
        public string MemberName { get; set; }
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
        public DateTime LastAttempt { get; set; }
        public string LastResult { get; set; }
        public bool Callable { get; set; }
    }

    public class PhoneNumberStatus
    {
        public PhoneNumber PhoneNumber { get; set; }
    }

    public class UpdateContactResponse
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
