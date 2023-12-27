using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesysContactsProcessJob.Model.Request
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class UpdateContactsRequest
    {
        public string Id { get; set; }
        public string ContactListId { get; set; }
        public Data Data { get; set; }
        public bool Callable { get; set; }
        public PhoneNumberStatus PhoneNumberStatus => new();
    }
}
