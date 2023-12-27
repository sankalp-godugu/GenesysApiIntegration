using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesysContactsProcessJob.Model.DTO
{
    public class GenesysIntegrationInfo
    {
        public long GenesysIntegrationId { get; set; }
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
        [JsonProperty("CallRecordLastResult-phoneNumber")]
        public string CallRecordLastResultPhoneNumber { get; set; }
    }
}
