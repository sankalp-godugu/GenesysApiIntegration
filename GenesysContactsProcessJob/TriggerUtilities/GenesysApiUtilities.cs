using CsvHelper;
using GenesysContactsProcessJob.DataLayer.Interfaces;
using GenesysContactsProcessJob.GenesysLayer.Interfaces;
using GenesysContactsProcessJob.Model.DTO;
using GenesysContactsProcessJob.Model.Response;
using GenesysContactsProcessJob.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GenesysContactsProcessJob.TriggerUtilities
{
    /// <summary>
    /// Genesys utilities.
    /// </summary>
    public static class GenesysApiUtilities
    {
        /// <summary>
        /// Processes Genesys contacts, performing operations such as logging, configuration retrieval,
        /// data layer interaction, and Genesys API service calls.
        /// </summary>
        /// <param name="_logger">An instance of the <see cref="ILogger"/> interface for logging.</param>
        /// <param name="_configuration">An instance of the <see cref="IConfiguration"/> interface for accessing configuration settings.</param>
        /// <param name="_dataLayer">An instance of the <see cref="IDataLayer"/> interface or class for interacting with the data layer.</param>
        /// <param name="_genesysClientService">An instance of the <see cref="IGenesysClientService"/> interface or class for Genesys API service calls.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the Genesys contacts processing.</returns>
        public static async Task<IActionResult> ProcessGenesysContacts(ILogger _logger, IConfiguration _configuration, IDataLayer _dataLayer, IGenesysClientService _genesysClientService, string lang)
        {
            try
            {
                await Task.Run(async () =>
                {
                    string appConnectionString = _configuration["DataBase:APPConnectionString"] ?? Environment.GetEnvironmentVariable("DataBase:ConnectionString");

                    // BEGIN TESTING --------------------------------------------------

                    //// 1) read original list
                    //IEnumerable<GetContactsExportDataFromGenesysResponse> listFromDialer = await _genesysClientService?.GetListFromCsv(@"C:\Temp\ListFromDialer.csv");

                    //// 2) compare filtered and original list, removing duplicates from original list ONLY
                    //IEnumerable<GetContactsExportDataFromGenesysResponse> listFromDialerNoDuplicates = listFromDialer
                    //    .GroupBy(c => new { c.Data.NhMemberId, c.Data.DischargeDate })
                    //    .Select(c2 => c2.First());

                    //IEnumerable<GetContactsExportDataFromGenesysResponse> temp = listFromDialer
                    //.GroupBy(c => new { c.Data.NhMemberId, c.Data.DischargeDate })
                    //.SelectMany(c2 => c2);

                    //IEnumerable<GetContactsExportDataFromGenesysResponse> temp2 = listFromDialer
                    //.GroupBy(c => new { c.Data.NhMemberId, c.Data.DischargeDate })
                    //.Where(g => g.Count() > 1)
                    //.SelectMany(c2 => c2);

                    //List<GetContactsExportDataFromGenesysResponse> duplicates = listFromDialer.Except(listFromDialerNoDuplicates).Where(c => c.Data.NhMemberId != "0").ToList();

                    //using (StreamWriter writer = new(@"C:\Temp\DuplicatesToRemove.csv", false, System.Text.Encoding.UTF8))
                    //using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    //{
                    //    csv.WriteRecords(duplicates); // where values implements IEnumerable
                    //}

                    //List<string> duplicatesToDelete = duplicates.Select(c => c.Id.ToString()).ToList();

                    //long result = await _genesysClientService?.DeleteContactsFromContactList(duplicatesToDelete, lang, _logger);

                    // END TESTING ----------------------------------------------------------

                    _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
                    _logger?.LogInformation("********* Member PD Orders => Genesys Contact List Execution Started **********");

                    string contactListIdKey = lang == Languages.English ? ConfigConstants.ContactListIdAetnaEnglishKey : ConfigConstants.ContactListIdAetnaSpanishKey;

                    string contactListId = _configuration[contactListIdKey] ?? Environment.GetEnvironmentVariable(contactListIdKey);

                    // ------------------ 6pm: INITIATE EXPORT OF CONTACT LIST ----------------------

                    _logger?.LogInformation($"Started initiating contact list export via Genesys API for contact list with id:{contactListId}");
                    InitiateContactListExportResponse initiateContactListExportResponse = await _genesysClientService?.InitiateContactListExport(lang, _logger);
                    _logger?.LogInformation($"Finished fetching contacts via Genesys API for contact list id: {contactListId}");
                    await Task.Delay(5000);

                    if (DateTime.Now < new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 17, 0, 0))
                    {

                    }

                    // ----------------- GET CONTACTS FROM GENESYS -------------------

                    _logger?.LogInformation($"Started fetching contacts via Genesys API for contact list id: {contactListId}");
                    IEnumerable<GetContactsExportDataFromGenesysResponse> contactsToProcess = await _genesysClientService?.GetContactsFromContactListExport(lang, _logger);
                    _logger?.LogInformation($"Finished fetching contacts via Genesys API for contact list id: {contactListId}");

                    // write list of contacts from dialer
                    StreamWriter writer = new(@$"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Genesys\PROD\{DateTime.Now.Month}-{DateTime.Now.Day}\{DateTime.Now.Month}-{DateTime.Now.Day}_Aetna_{lang}_GET.csv", false, System.Text.Encoding.UTF8);
                    CsvWriter csv = new(writer, CultureInfo.InvariantCulture);
                    csv.WriteRecords(contactsToProcess);

                    // ---------------- REMOVE CONTACTS FROM GENESYS ------------------

                    IEnumerable<GetContactsExportDataFromGenesysResponse> contactsWithoutDupes = contactsToProcess.ToList()
                    .GroupBy(c => new { c.Data.NhMemberId, c.Data.DischargeDate })
                    .Select(c2 => c2.First());

                    IEnumerable<GetContactsExportDataFromGenesysResponse> contactsDupes = contactsToProcess.Except(contactsWithoutDupes);

                    IEnumerable<GetContactsExportDataFromGenesysResponse> contactsWithDNCCodesOrDayCountExceeded = contactsToProcess.ToList()
                    .Where(c => LastResultAndWrapUpCodes.WrapUpCodesForDeletion.Contains(c.WrapUpCode)
                                || LastResultAndWrapUpCodes.WrapUpCodesForDeletion.Contains(c.PhoneNumberStatus.PhoneNumber.LastResult)
                                || int.Parse(c.Data.DayCount) > 50
                                || int.Parse(c.Data.AttemptCountTotal) > 45);

                    IEnumerable<GetContactsExportDataFromGenesysResponse> contactsToRemove = contactsWithDNCCodesOrDayCountExceeded.Union(contactsDupes);

                    // write duplicates to be scrubbed to file
                    writer = new(@$"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Genesys\PROD\{DateTime.Now.Month}-{DateTime.Now.Day}\{DateTime.Now.Month}-{DateTime.Now.Day}_Aetna_{lang}_Duplicates.csv", false, System.Text.Encoding.UTF8);
                    csv = new(writer, CultureInfo.InvariantCulture);
                    csv.WriteRecords(contactsDupes);

                    // write DNC records to be scrubbed to file
                    writer = new(@$"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Genesys\PROD\{DateTime.Now.Month}-{DateTime.Now.Day}\{DateTime.Now.Month}-{DateTime.Now.Day}_Aetna_{lang}_DNC.csv", false, System.Text.Encoding.UTF8);
                    csv = new(writer, CultureInfo.InvariantCulture);
                    csv.WriteRecords(contactsWithDNCCodesOrDayCountExceeded);

                    // write all records to be scrubbed to file
                    writer = new(@$"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Genesys\PROD\{DateTime.Now.Month}-{DateTime.Now.Day}\{DateTime.Now.Month}-{DateTime.Now.Day}_Aetna_{lang}_AllScrubbed.csv", false, System.Text.Encoding.UTF8);
                    csv = new(writer, CultureInfo.InvariantCulture);
                    csv.WriteRecords(contactsWithDNCCodesOrDayCountExceeded);

                    Environment.Exit(Environment.ExitCode);
                    //if (contactsToRemove.Any())
                    //{
                    //    _logger?.LogInformation($"Started removing contacts from contact list with id: {contactListId}");
                    //    IEnumerable<string> contactIds = contactsToRemove.Select(c => c.Id);
                    //    removeContactsFromGenesysResponse = await _genesysClientService?.DeleteContactsFromContactList(contactIds, lang, _logger);
                    //    _logger?.LogInformation($"Finished removing contacts from contact list with id: {contactListId}");
                    //}
                    //else
                    //{
                    //    _logger?.LogInformation($"No contacts to remove from contact list with id: {contactListId}");
                    //}
                    //contactsToProcess = contactsToProcess.Except(contactsToRemove);

                    //// ------------------- GET MEMBERS BY LANGUAGE ------------------

                    //Dictionary<string, object> sqlParams = new()
                    //{
                    //    { "@lang", lang },
                    //};

                    //_logger?.LogInformation("Started fetching PD orders for all members");
                    //IEnumerable<GetContactsExportDataFromGenesysResponse> newContactsToProcess = await _dataLayer.ExecuteReader<GetContactsExportDataFromGenesysResponse>(SQLConstants.GetPDAndGenesysInfo, sqlParams, appConnectionString, _logger);
                    //_logger?.LogInformation($"Ended fetching PD orders with count: {contactsToProcess?.Count()}");

                    //// --------------------- ADD CONTACTS TO GENESYS ---------------------

                    //IEnumerable<PostContactsToGenesysResponse> contactsAdded = new List<PostContactsToGenesysResponse>();
                    //List<PostContactsRequest> contactsToAdd = new();

                    //if (contactsToAdd.Any())
                    //{
                    //    _logger?.LogInformation($"Started adding contacts via Genesys API to contact list with id: {contactListId}");
                    //    contactsAdded = await _genesysClientService?.AddContactsToContactList(contactsToAdd, lang, _logger);
                    //    _logger?.LogInformation($"Finished adding contacts via Genesys API to contact list with id: {contactListId}");
                    //}
                    //newContactsToProcess = newContactsToProcess.Where(c => !contactsAdded.Any(c2 => c2.Id == c.Id));

                    //// -------------------- UPDATE CONTACTS TO BE DIALED IN GENESYS ----------------------

                    //IEnumerable<PostContactsToGenesysResponse> contactsUpdatedAndDialed = new List<PostContactsToGenesysResponse>();
                    //List<PostContactsRequest> contactsToUpdateAndDial = new();
                    //if (contactsToUpdateAndDial.Any())
                    //{
                    //    _logger?.LogInformation($"Started updating contacts TO be dialed via Genesys API in contact list with id: {contactListId}");
                    //    contactsUpdatedAndDialed = await _genesysClientService?.UpdateContactsInContactList(contactsToUpdateAndDial, lang, _logger);
                    //    _logger?.LogInformation($"Finished updating contacts TO be dialed via Genesys API in contact list with id: {contactListId}");
                    //}
                    //newContactsToProcess = newContactsToProcess.Where(c => !contactsUpdatedAndDialed.Any(c2 => c2.Id == c.Id));

                    //// --------------------- UPDATE CONTACTS NOT TO BE DIALED IN GENESYS -------------------

                    //IEnumerable<PostContactsRequest> contactsToUpdateOnly = (IEnumerable<PostContactsRequest>)newContactsToProcess.Where(c => int.Parse(c.Data.DayCount) > 20 && int.Parse(c.Data.DayCount) % 2 == 1);

                    //if (contactsToUpdateOnly.Any())
                    //{
                    //    _logger?.LogInformation($"Started updating contacts NOT to be dialed via Genesys API in contact list with id: {contactListId}");
                    //    foreach (PostContactsRequest contactToUpdate in contactsToUpdateOnly)
                    //    {
                    //        PostContactsToGenesysResponse contactsUpdatedOnly = await _genesysClientService?.UpdateContactInContactList(contactToUpdate, lang, _logger);
                    //    }
                    //    _logger?.LogInformation($"Finished updating contacts NOT to be dialed via Genesys API in contact list with id: {contactListId}");
                    //}
                    //newContactsToProcess = newContactsToProcess.Where(c => !contactsToUpdateOnly.Any(c2 => c2.Id == c.Id));

                    //// --------------- REFRESH GENESYS CONTACT STATUS TABLE -------------------

                    //_logger?.LogInformation("Started refreshing Genesys Contact Info Reference table");
                    //IEnumerable<string> refreshGenesysContactInfoResponse = await _dataLayer.ExecuteReader<string>(SQLConstants.RefreshGenesysContactInfo, new(), appConnectionString, _logger);
                    //_logger?.LogInformation($"Ended refreshing Genesys Contact Info Reference table with result: {refreshGenesysContactInfoResponse}");
                    //_logger?.LogInformation("********* Member PD Orders => Genesys Contact List Execution Ended *********");
                });

                return new OkObjectResult("Task of processing PD Orders in Genesys has been allocated to azure function and see logs for more information about its progress...");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed with an exception with message: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Updates the Genesys contact reference and is processed status.
        /// </summary>
        /// <param name="_logger">Logger.</param>
        /// <param name="brConnectionString">APP connection string.</param>
        /// <param name="genesysContact">Genesys contact.</param>
        /// <param name="currentProcessId">Current process identifier.</param>
        /// <param name="genesysContactReference">Genesys contact reference.</param>
        private static async Task UpdateGenesysContactStatus(ILogger logger, string appConnectionString, PostDischargeInfo_GenesysContactInfo genesysContact, long genesysContactReference, string action, long currentProcessId, IDataLayer dataLayer)
        {
            logger?.LogInformation($"Updating Genesys contact details with contact id :{genesysContact?.PostDischargeId}");

            int result = await dataLayer.ExecuteNonQueryForGenesys(SQLConstants.UpdateGenesysContactInfo, genesysContact.PostDischargeId, action, currentProcessId, appConnectionString, logger);

            if (result == 1)
            {
                logger?.LogInformation($"Updated the Genesys contact status: {genesysContactReference}.");
            }
            else
            {
                logger?.LogInformation($"Failed to update the Genesys contact status: {genesysContactReference}.");
            }
        }
    }
}