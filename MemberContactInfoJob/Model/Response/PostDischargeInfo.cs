using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemberContactInfoJob.Model.Response
{
    public class PostDischargeInfo
    {
        public string NhMemberId { get; set; }
        public string MemberName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string CarrierName { get; set; }
        public string LoadDate { get; set; }
        public string DischargeDate { get; set; }
        public string DayCount { get; set; }
        public string AttemptCountToday = "0";
        public string AttemptCountTotal = "0 (will be derived from Genesys)";
        
    }
}
