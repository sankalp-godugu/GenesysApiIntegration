using System;

namespace GenesysContactsProcessJob.Model.Response
{
    public class GetContactListExportUriResponse
    {
        public string Uri { get; set; }
        public DateTime ExportTimestamp { get; set; }
    }
}
