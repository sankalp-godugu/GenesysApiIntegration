using System;

namespace GenesysContactsProcessJob.Model.DTO
{
    public class GenesysContactInfo
    {
        public long GenesysContactInfoId { get; set; }
        public long PostDischargeId { get; set; }
        public string MemberName { get; set; }
        public string CarrierName { get; set; }
        public string Region { get; set; }
        public string Language { get; set; }
        public DateTime LoadDate = DateTime.UtcNow.Date;
        public string DayCount { get; set; }
        public string AttemptCountToday { get; set; }
        public string AttemptCountTotal { get; set; }
        public bool ShouldAddToContactList { get; set; }
        public bool ShouldRemoveFromContactList { get; set; }
        public bool ShouldUpdateInContactList { get; set; }
        public bool IsDeletedFromContactList { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
    }
}
