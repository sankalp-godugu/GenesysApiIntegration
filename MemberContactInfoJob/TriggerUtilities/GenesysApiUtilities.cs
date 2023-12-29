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
                    _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
                    /*if (myTimer.IsPastDue)
                    {
                        _logger.LogInformation("Timer is running late!");
                    }*/

                    _logger?.LogInformation("********* Member PD Orders => Genesys Contact List Execution Started **********");

                    // TODO: swap the following before deploying code
                    string appConnectionString = _configuration["DataBase:APPConnectionString"] ?? Environment.GetEnvironmentVariable("ConnectionStrings:Test2Conn");

                    // TODO: remove for PROD deployment
                    IEnumerable<int> insertPostDischargeInfoTestDataResponse = await _dataLayer.ExecuteReader<int>(SQLConstants.InsertPostDischargeInfoTestData, new(), appConnectionString, _logger);

                    // ---------------------------------- REFRESH CONTACT STATUS TABLE ------------------------------

                    _logger?.LogInformation("Started refreshing Genesys Contact Info Reference table");

                    IEnumerable<string> refreshGenesysContactInfoResponse = await _dataLayer.ExecuteReader<string>(SQLConstants.RefreshGenesysContactInfo, new(), appConnectionString, _logger);

                    _logger?.LogInformation($"Ended refreshing Genesys Contact Info Reference table with result: {refreshGenesysContactInfoResponse}");

                    // ----------------------------------- GET MEMBERS BY LANG -----------------------------------------

                    // SQL parameters.
                    Dictionary<string, object> sqlParams = new()
                {
                    // TODO: swap the following before deploying code
                    { "@lang", _configuration["Language"] ?? lang },
                };

                    _logger?.LogInformation("Started fetching PD orders for all members");

                    IEnumerable<PostDischargeInfo_GenesysContactInfo> contactsToProcessInGenesys = await _dataLayer.ExecuteReader<PostDischargeInfo_GenesysContactInfo>(SQLConstants.GetPDAndGenesysInfo, sqlParams, appConnectionString, _logger);

                    _logger?.LogInformation($"Ended fetching PD orders with count: {contactsToProcessInGenesys?.Count()}");

                    // ------------------------------------- GET CONTACTS FROM GENESYS -------------------------------------

                    IEnumerable<GetContactsExportDataFromGenesysResponse> getContactsExportDataFromGenesysResponse = new List<GetContactsExportDataFromGenesysResponse>();
                    if (contactsToProcessInGenesys.Any())
                    {
                        _logger?.LogInformation($"Started fetching contacts via Genesys API for the contact list id: {Environment.GetEnvironmentVariable("AetnaEnglish")}");
                        getContactsExportDataFromGenesysResponse = await _genesysClientService?.GetContactsFromContactListExport(_logger);
                        _logger?.LogInformation($"Successfully fetched contacts for the contact list id: {Environment.GetEnvironmentVariable("AetnaEnglish")}");
                    }
                    IEnumerable<PostDischargeInfo_GenesysContactInfo> contactsToRemoveFromGenesys = contactsToProcessInGenesys.Where(c => c.ShouldRemoveFromContactList);
                    IEnumerable<long> contactsWithNoDialWrapUpCode = getContactsExportDataFromGenesysResponse
                    .Where(c => AgentWrapUpCodes.WrapUpCodesForDeletion.Contains(c.WrapUpCode)).Select(c => long.Parse(c.Id));

                    List<long> allContactsToRemoveFromGenesys = contactsToRemoveFromGenesys.Select(c => c.PostDischargeId).ToList();
                    allContactsToRemoveFromGenesys.AddRange(contactsWithNoDialWrapUpCode.Where(c2 => allContactsToRemoveFromGenesys.All(c1 => c1 != c2)));

                    if (allContactsToRemoveFromGenesys.Any())
                    {
                        _logger?.LogInformation($"Started removing contacts via Genesys API from the contact list with id: {Environment.GetEnvironmentVariable("AetnaEnglish")}");

                        //removeContactsFromGenesysResponse = await _genesysClientService?.DeleteContactsFromContactList(allContactsToRemoveFromGenesys, _logger);

                        _logger?.LogInformation($"Successfully removed contacts from contact list with id: {Environment.GetEnvironmentVariable("AetnaEnglish")}");

                        /*foreach (PostDischargeInfo_GenesysContactInfo contact in contactsToRemoveFromGenesys)
                        {
                            await UpdateGenesysContactStatus(_logger, appConnectionString, contact, Convert.ToInt64(contact?.PostDischargeId), "REMOVED", 2, _dataLayer);
                        }*/
                    }

                    // -------------------------------------- ADD CONTACTS TO GENESYS --------------------------------------

                    // if contact already in contact list with same discharge date and/or disposition code of not interested - DO NOT ADD
                    IEnumerable<AddContactsToGenesysResponse> addContactsToGenesysResponse = new List<AddContactsToGenesysResponse>();
                    List<PostDischargeInfo_GenesysContactInfo> contactsToAddToGenesys = contactsToProcessInGenesys.Where(c => c.ShouldAddToContactList && !c.IsDeletedFromContactList).ToList();
                    _ = contactsToAddToGenesys.RemoveAll(c2 => allContactsToRemoveFromGenesys.Exists(c => c == c2.PostDischargeId));
                    if (contactsToAddToGenesys.Any())
                    {
                        _logger?.LogInformation($"Started adding contacts via Genesys API to contact list with id: {Environment.GetEnvironmentVariable("AetnaEnglish")}");
                        addContactsToGenesysResponse = await _genesysClientService?.AddContactsToContactList(contactsToAddToGenesys, _logger);
                        _logger?.LogInformation($"Successfully added contacts to contact list with id: {Environment.GetEnvironmentVariable("AetnaEnglish")}");

                        foreach (PostDischargeInfo_GenesysContactInfo contact in contactsToAddToGenesys)
                        {
                            await UpdateGenesysContactStatus(_logger, appConnectionString, contact, Convert.ToInt64(contact?.PostDischargeId), "ADDED", 2, _dataLayer);
                        }
                    }

                    // -------------------------------------- UPDATE CONTACTS IN GENESYS --------------------------------------

                    IEnumerable<UpdateContactsInGenesysResponse> updateContactsInGenesysResponse = new List<UpdateContactsInGenesysResponse>();
                    // if contact already in contact list with same discharge date and/or disposition code of not interested -- update this value from code)
                    List<PostDischargeInfo_GenesysContactInfo> contactsToUpdateInGenesys = contactsToProcessInGenesys.Where(c => c.ShouldUpdateInContactList && !c.IsDeletedFromContactList).ToList();
                    // don't update contacts that have been removed from contact list
                    _ = contactsToUpdateInGenesys.RemoveAll(c2 => allContactsToRemoveFromGenesys.Exists(c => c == c2.PostDischargeId));
                    if (contactsToUpdateInGenesys.Any())
                    {
                        _logger?.LogInformation($"Started updating contacts via Genesys API in contact list with id: {Environment.GetEnvironmentVariable("AetnaEnglish")}");
                        updateContactsInGenesysResponse = await _genesysClientService?.UpdateContactsInContactList(contactsToUpdateInGenesys, _logger);
                        _logger?.LogInformation($"Successfully updated contacts in contact list with id: {Environment.GetEnvironmentVariable("AetnaEnglish")}");

                        foreach (PostDischargeInfo_GenesysContactInfo contact in contactsToUpdateInGenesys)
                        {
                            await UpdateGenesysContactStatus(_logger, appConnectionString, contact, Convert.ToInt64(contact?.PostDischargeId), "UPDATED", 2, _dataLayer);
                        }
                    }

                    _logger?.LogInformation("********* Member PD Orders => Genesys Contact List Execution Ended *********");
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
