using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemberContactInfoJob.Model.Request
{
    public class GenesysRequest
    {
        public long PostDischargeId { get; set; }
        public string NHMemberId { get; set; }
        public string MemberName { get; set; }
        public string CarrierName { get; set; }
        public string Region { get; set; }
        public string Language { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string PhoneNbr { get; set; }
        public string AlternatePhone { get; set; }
        public DateTime DischargeDate { get; set; }
        public DateTime LoadDate = DateTime.UtcNow.Date;
        public string DayCount { get; set; }
        public string AttemptCountToday { get; set; }
        public string AttemptCountTotal { get; set; }
        public int ShouldAddToContactList { get; set; }
        public int ShouldRemoveFromContactList { get; set; }
        public int ShouldUpdateInContactList { get; set; }
        public int IsDeletedFromContactList { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
    }
}
