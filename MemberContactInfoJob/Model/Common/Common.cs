using System;

namespace GenesysContactsProcessJob.Model.Common
{
    public class Common
    {
    }

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
        public DateTime LastAttempt { get; set; }
        public string LastResult { get; set; }
    }

    public class PhoneNumberStatus
    {
        public PhoneNumber PhoneNumber => new();
    }

    public class ContactableStatus
    {
        public Email Email => new();
        public Sms Sms => new();
    }

    public class Email
    {
        public bool Contactable => true;
    }

    public class Sms
    {
        public bool Contactable => true;
    }

    public class CallRecords
    {
        public PhoneNumber PhoneNumber { get; set; }
    }

    public class ConfigurationOverrides
    {
        public bool Priority { get; set; }
    }
}
