﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesysContactsProcessJob.Model.Response
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class DeleteContactsResponse
    {
        public string Id { get; set; }
        public string ContactListId { get; set; }
        public Data Data { get; set; }
        public CallRecords CallRecords { get; set; }
        public bool Callable { get; set; }
        public PhoneNumberStatus PhoneNumberStatus { get; set; }
        public ContactableStatus ContactableStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public string SelfUri { get; set; }
    }
}