namespace GenesysContactsProcessJob.Utilities
{
    /// <summary>
    /// SQL constants.
    /// </summary>
    public class SQLConstants
    {
        /// <summary>
        /// Get all PD orders.
        /// </summary>
        public static readonly string GetPDAndGenesysInfo = "[meals].[GetPDAndGenesysInfo]";

        /// <summary>
        /// Refreshs genesys tbl.
        /// </summary>
        public static readonly string RefreshGenesysMemberContactInfo = "[meals].[RefreshGenesysMemberContactInfo]";

        /// <summary>
        /// Updates the Genesys reference for member contacts.
        /// </summary>
        public static readonly string UpdateGenesysMemberContactInfo = "[meals].[UpdateGenesysMemberContactInfo]";
    }
}
