using System;

namespace GenesysContactsProcessJob.Model.DTO
{
    public class GenesysMemberContactInfo
    {
        public long GenesysMemberContactInfoId { get; set; }
        public long PostDischargeId { get; set; }
        public string MemberName { get; set; }
        public string CarrierName { get; set; }
        public string Region { get; set; }
        public string Language { get; set; }
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
