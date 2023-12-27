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
        public static string GetAllPDOrders = "[meals].[GetAllPDOrders]";

        /// <summary>
        /// Refreshs genesys tbl.
        /// </summary>
        public static string RefreshGenesysContactCountersAndStatuses = "[meals].[RefreshGenesysContactCountersAndStatuses]";

        /// <summary>
        /// Updates the Genesys reference for member contacts.
        /// </summary>
        public static string UpdateGenesysContactStatus = "[meals].[UpdateGenesysContactStatus]";
    }
}
