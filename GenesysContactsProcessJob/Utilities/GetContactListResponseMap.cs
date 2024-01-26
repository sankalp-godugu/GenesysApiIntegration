using CsvHelper.Configuration;
using GenesysContactsProcessJob.Model.Response;

namespace GenesysContactsProcessJob.Utilities
{
    public class GetContactListResponseMap : ClassMap<GetContactListResponse>
    {
        public GetContactListResponseMap()
        {
            _ = Map(r => r.Id).Index(0);
            _ = Map(r => r.Data.NhMemberId).Index(1);
            _ = Map(r => r.Data.MemberName).Index(2);
            _ = Map(r => r.Data.Address).Index(3);
            _ = Map(r => r.Data.Region).Index(4);
            _ = Map(r => r.Data.PhoneNumber).Index(5);
            _ = Map(r => r.Data.CarrierName).Index(6);
            _ = Map(r => r.Data.LoadDate).Index(7);
            _ = Map(r => r.Data.DischargeDate).Index(8);
            _ = Map(r => r.Data.DayCount).Index(9);
            _ = Map(r => r.Data.AttemptCountToday).Index(10);
            _ = Map(r => r.Data.AttemptCountTotal).Index(11);
            _ = Map(r => r.ContactCallable).Index(12);
            _ = Map(r => r.ContactableBy.Voice.Callable).Index(13);
            _ = Map(r => r.ContactableBy.Sms.Callable).Index(14);
            _ = Map(r => r.ContactableBy.Email.Callable).Index(15);
            _ = Map(r => r.ZipCodeAutomaticTimeZone).Index(16);
            _ = Map(r => r.CallRecords.LastAttempt_PhoneNumber).Index(17);
            _ = Map(r => r.CallRecords.LastResult_PhoneNumber).Index(18);
            _ = Map(r => r.CallRecords.LastAgentWrapup_PhoneNumber).Index(19);
            _ = Map(r => r.Sms.LastAttempt_PhoneNumber).Index(20);
            _ = Map(r => r.Sms.LastResult_PhoneNumber).Index(21);
            _ = Map(r => r.ContactCallable).Index(22);
            _ = Map(r => r.ContactableBy.Voice.PhoneNumber).Index(23);
            _ = Map(r => r.ContactableBy.Sms.PhoneNumber).Index(24);
            _ = Map(r => r.AutomaticTimeZone_phoneNumber).Index(25);

        }
    }
}
