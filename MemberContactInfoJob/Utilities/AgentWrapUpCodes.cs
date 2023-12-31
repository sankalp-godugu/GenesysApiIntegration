﻿using System.Collections.Generic;

namespace GenesysContactsProcessJob.Utilities
{
    public class AgentWrapUpCodes
    {
        public static readonly List<string> WrapUpCodesForDeletion = new()
        {
            "Disconnected Number",
            "DNC – Do Not Call",
            "IN Hospital",
            "Benefits Exhausted",
            "Not Interested",
            "SOLD",
            "Wrong Number",
            "Already SOLD"
        };
    }
}
