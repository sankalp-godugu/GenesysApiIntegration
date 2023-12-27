using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesysContactsProcessJob.Model.Response
{
    public class PostDischargeInfo
    {
        public long PostDischargeId { get; set; }
        public string NHMemberId { get; set; }
        public int? InsCarrierId { get; set; }

        // from PD layout #2
        public string AuthId { get; set; }
        public string SubscriberId { get; set; }
        public string MedicareNbr { get; set; }
        public string MedicaidNbr { get; set; }
        public DateTime DOB { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string PhoneNbr { get; set; }
        public string AlternatePhone { get; set; }
        public string ContractPBP { get; set; }
        public string FacilityName { get; set; }
        public DateTime AdmitDate { get; set; }
        public DateTime DischargeDate { get; set; }
        public string DiagnosisCode { get; set; }
        public string ProcedureCode { get; set; }
        public string CaseManagerAgency { get; set; }
        public string CaseManagerFirstName { get; set; }
        public string CaseManagerLastName { get; set; }
        public string CaseManagerEmail { get; set; }
        public string CaseManagerPhone { get; set; }
        public string AdmittingPhysician { get; set; }
        public string CaseManagerFax { get; set; }
        public string DischargeDispositionCode { get; set; }
        public int? LengthOfStay { get; set; }
        public string Comments { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
    }
}
