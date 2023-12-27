using GenesysContactsProcessJob.DataLayer.Interfaces;
using GenesysContactsProcessJob.GenesysLayer.Interfaces;
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
    /// Zendesk utilities.
    /// </summary>
    public static class GenesysApiUtilities
    {
        /// <summary>
        /// Processes Zendesk tickets, performing operations such as logging, configuration retrieval,
        /// data layer interaction, and Zendesk API service calls.
        /// </summary>
        /// <param name="_logger">An instance of the <see cref="ILogger"/> interface for logging.</param>
        /// <param name="_configuration">An instance of the <see cref="IConfiguration"/> interface for accessing configuration settings.</param>
        /// <param name="_dataLayer">An instance of the <see cref="IDataLayer"/> interface or class for interacting with the data layer.</param>
        /// <param name="_genesysClientService">An instance of the <see cref="IGenesysClientService"/> interface or class for Zendesk API service calls.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the Zendesk ticket processing.</returns>
        public static async Task<IActionResult> ProcessGenesysContacts(ILogger _logger, IConfiguration _configuration, IDataLayer _dataLayer, IGenesysClientService _genesysClientService)
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

                    _logger?.LogInformation("********* Member PD Order => Genesys Execution Started **********");

                    // APP connection string.
                    //string APPConnectionString = _configuration["DataBase:APPConnectionString"];
                    string APPConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:Test2Conn");

                    // SQL parameters.
                    var sqlParams = new Dictionary<string, object>
                    {
                        //{"@lang", _configuration["Language"]}
                        {"@lang", "ENG" }
                    };

                    //Test();

                    _logger?.LogInformation("Started fetching the PD orders for all members");

                    var contactsToProcess = await _dataLayer.ExecuteReader<PostDischargeInfoPlusGenesys>(SQLConstants.GetAllPDOrders, sqlParams, APPConnectionString, _logger);
                    //var contactsToProcess = await _dataLayer.Query<PostDischargeGenesysInfo>(SQLConstants.GetAllPDOrders, sqlParams, APPConnectionString, _logger);

                    _logger?.LogInformation($"Ended fetching the PD orders with count: {contactsToProcess?.Count}");

                    _logger?.LogInformation("Started refreshing Genesys info table");

                    var refreshResult = await _dataLayer.ExecuteReader<string>(SQLConstants.RefreshGenesysTable, new(), APPConnectionString, _logger);

                    _logger?.LogInformation($"Ended refreshing Genesys info table with result: {refreshResult}");

                    // PROCESS IN BULK
                    var contactsToAdd = contactsToProcess.TakeWhile(c => c.ShouldAddToContactList && !c.IsDeletedFromContactList);
                    var contactsToUpdate = contactsToProcess.TakeWhile(c => c.ShouldUpdateInContactList && !c.IsDeletedFromContactList);
                    var contactsToRemove = contactsToProcess.TakeWhile(c => c.ShouldRemoveFromContactList && !c.IsDeletedFromContactList);

                    // BULK GET CONTACTS
                    //var getResult = await GetContactsFromContactList(token, contactListId);

                    IEnumerable<AddContactsResponse> addResult;
                    IEnumerable<UpdateContactsResponse> updateResult;
                    long deleteResult;

                    // BULK ADD CONTACTS
                    // if member already in contact list with same discharge date and/or disposition code of not interested -- update this value from code)
                    if (contactsToAdd.Any())
                    {
                        _logger?.LogInformation($"Started adding contacts via Genesys API for the contact list id: {Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId")}");
                        addResult = await _genesysClientService?.AddContactsToContactList(contactsToAdd, _logger);
                        _logger?.LogInformation($"Successfully added contacts for the contact list id: {Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId")}");
                    }

                    // BULK UPDATE CONTACTS
                    // if member already in contact list with same discharge date and/or disposition code of not interested -- update this value from code)
                    if (contactsToUpdate.Any())
                    {
                        _logger?.LogInformation($"Started updating contacts via Genesys API for the contact list id: {Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId")}");
                        updateResult = await _genesysClientService?.UpdateContactsInContactList(contactsToUpdate, _logger);
                        _logger?.LogInformation($"Successfully updating contacts for the contact list id: {Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId")}");
                    }

                    // BULK REMOVE CONTACTS
                    // if member already in contact list with same discharge date and/or disposition code of not interested -- update this value from code)
                    if (contactsToRemove.Any())
                    {
                        _logger?.LogInformation($"Started deleting contacts via Genesys API for the contact list id: {Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId")}");
                        //await UpdatesCMTZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, caseManagementTicket, 0, 2, _dataLayer);
                        deleteResult = await _genesysClientService?.DeleteContactsFromContactList(contactsToUpdate, _logger);
                        _logger?.LogInformation($"Successfully deleted contacts for the contact list id: {Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId")}");
                        //await UpdatesCMTZendeskTicketReferenceAndIsProcessedStatus(_logger, brConnectionString, caseManagementTicket, 0, 2, _dataLayer);
                    }

                    _logger?.LogInformation("********* Member PD Orders => Genesys Execution Ended *********");
                });

                return new OkObjectResult("Task of PD Orders to Genesys has been allocated to azure function and see logs for more information about its progress...");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed with an exception with message: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
