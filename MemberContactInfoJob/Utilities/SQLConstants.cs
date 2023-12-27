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
        public static string GetAllPDOrders =
            "[meals].[GetAllPDOrders]";

        /// <summary>
        /// Refreshs genesys tbl.
        /// </summary>
        public static string RefreshGenesysTable = "[meals].[RefreshDayCountAndAttemptCountToday]";
    }
}
