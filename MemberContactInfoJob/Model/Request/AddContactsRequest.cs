using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesysContactsProcessJob.Model.Request
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class Data
    {
        // TODO: remove later
        public string TransactionId => Guid.NewGuid().ToString();
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
        public string ContactListId = "8518a928-6c33-491e-b43a-bf98ec790f7b";
        public Data Data { get; set; }
        public bool Callable => true;
        public PhoneNumberStatus PhoneNumberStatus => new();
    }
}
