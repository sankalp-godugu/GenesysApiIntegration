using System.Collections.Generic;

namespace GenesysContactsProcessJob.Utilities
{
    public class AgentWrapUpCodes
    {
        public static readonly List<string> WrapUpCodes = new()
        {
            "Disconnected Number",
            "DNC - Do Not Call",
            "IN Hospital",
            "Benefits Exhausted",
            "Not Interested",
            "SOLD",
            "Wrong Number",
            "Callback Set",
            "Already SOLD"
        };
    }

    public enum ProcessingType
    {
        Add = 0,
        Update = 1,
        Remove = 2
    }
}
