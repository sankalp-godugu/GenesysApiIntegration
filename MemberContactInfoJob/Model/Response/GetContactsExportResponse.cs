using System;

namespace GenesysContactsProcessJob.Model.Response
{
    public class GetContactsExportResponse
    {
        public string Uri { get; set; }
        public DateTime ExportTimestamp { get; set; }
    }
}
