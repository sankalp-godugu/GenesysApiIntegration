using CsvHelper.Configuration;
using GenesysContactsProcessJob.Model.Response;

namespace GenesysContactsProcessJob.Utilities
{
    public class GetContactsExportDataFromGenesysResponseMap_Upload : ClassMap<GetContactsExportDataFromGenesysResponse>
    {
        public GetContactsExportDataFromGenesysResponseMap_Upload()
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
        }
    }
}
