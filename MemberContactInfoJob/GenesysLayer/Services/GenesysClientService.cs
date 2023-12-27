﻿using GenesysContactsProcessJob.GenesysLayer.Interfaces;
using GenesysContactsProcessJob.Model.DTO;
using GenesysContactsProcessJob.Model.Request;
using GenesysContactsProcessJob.Model.Response;
using GenesysContactsProcessJob.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GenesysContactsProcessJob.GenesysLayer.Services
{
    /// <summary>
    /// Genesys client service.
    /// </summary>
    public class GenesysClientService : IGenesysClientService
    {
        #region Private Fields
        private IHttpClientFactory _httpClientFactory;
        private IConfiguration _configuration;
        #endregion

        #region Constructor

        /// <summary>
        /// Genesys client service service initialization.
        /// </summary>
        /// <param name="httpClientFactory">Http client factory. <see cref="IHttpClientFactory"/></param>
        /// <param name="configuration">Configuration. <see cref="IConfiguration"/></param>
        public GenesysClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the contacts in Genesys asychronously.
        /// </summary>
        /// <param name="contactsToAdd">Contacts To Add.<see cref="ContactsToAdd"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns 1 for success.</returns>
        public async Task<IEnumerable<GetContactsResponse>> GetContactsFromContactList(IEnumerable<PostDischargeInfoPlusGenesys> contactsToGet, ILogger logger)
        {
            StringContent content = GetGetRequestBodyForGenesys(contactsToGet, logger);
            return await GetContactsToContactList(content, logger);
        }

        /// <summary>
        /// Adds the contacts in Genesys asychronously.
        /// </summary>
        /// <param name="contactsToAdd">Contacts To Add.<see cref="ContactsToAdd"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns 1 for success.</returns>
        public async Task<IEnumerable<AddContactsResponse>> AddContactsToContactList(IEnumerable<PostDischargeInfoPlusGenesys> contactsToAdd, ILogger logger)
        {
            // Gets the request body for the genesys client.
            StringContent content = GetAddOrUpdateRequestBodyForGenesys(contactsToAdd, logger);
            return await AddContactsToContactListWithStringContent(content, logger);
        }

        /// <summary>
        /// Updates the contacts in Genesys asynchronously.
        /// </summary>
        /// <param name="contactsToUpdate">Contacts To Update.<see cref="ContactsToUpdate"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns 1 for success.</returns>
        public async Task<IEnumerable<UpdateContactsResponse>> UpdateContactsInContactList(IEnumerable<PostDischargeInfoPlusGenesys> contactsToUpdate, ILogger logger)
        {
            // Gets the request body for the Genesys API request.
            StringContent content = GetAddOrUpdateRequestBodyForGenesys(contactsToUpdate, logger);
            return await UpdateContactsInContactListWithStringContent(content, logger);
        }

        /// <summary>
        /// Deletes the contacts in Genesys asynchronously.
        /// </summary>
        /// <param name="contactsToDelete">Contacts To Delete.<see cref="ContactsToDelete"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns 1 for success.</returns>
        public async Task<long> DeleteContactsFromContactList(IEnumerable<long> contactsToDelete, ILogger logger)
        {
            // Gets the request query arguments for the Genesys API request.
            // GET updated list of contacts (for wrap up codes) and associated list of contact IDs
            // TODO: ongoing issue where /bulk/ get endpoint returns lastResult info but only gives us system/machine wrap up codes, not machine wrap up codes
            string queryArgs = GetDeleteRequestQueryForGenesys(contactsToDelete, logger);
            return await DeleteContactsFromContactListWithQueryArgs(queryArgs, logger);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the Genesys http client.
        /// </summary>
        /// <returns>Returns the http client to make API requests.</returns>
        private async Task<HttpClient> GetGenesysHttpClient()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();

            // Set the Authorization header with the Basic authentication credentials
            AccessTokenResponse tokenResponse = await AuthenticateAsync(httpClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            return httpClient;
        }

        private async Task<AccessTokenResponse> AuthenticateAsync(HttpClient client)
        {
            Uri baseUrl = new(Environment.GetEnvironmentVariable("TokenUrl"));
            AccessTokenRequest atr = new();
            var form = new Dictionary<string, string>()
            {
                {"grant_type", atr.Grant_Type},
                {"client_id", atr.client_id},
                {"client_secret", atr.client_secret},
            };

            /*string cachedToken = RetrieveCachedToken();
            if (!string.IsNullOrWhiteSpace(cachedToken))
                return cachedToken;*/

            HttpResponseMessage result = await client.PostAsync(baseUrl, new FormUrlEncodedContent(form));
            result.EnsureSuccessStatusCode();
            string response = await result.Content.ReadAsStringAsync();
            AccessTokenResponse token = JsonConvert.DeserializeObject<AccessTokenResponse>(response);
            //SetCacheToken(token);
            return token;
        }

        /// <summary>
        /// Gets the API request body for Genesys.
        /// </summary>
        /// <param name="caseTicket">Case ticket.<see cref="CaseTickets"/></param>
        /// <returns>Returns the string content.</returns>
        private StringContent GetAddOrUpdateRequestBodyForGenesys(IEnumerable<PostDischargeInfoPlusGenesys> contactsToProcess, ILogger logger)
        {
            try
            {
                var mapper = MapperConfig.InitializeAutomapper();
                IEnumerable<AddContactsRequest> gcr = mapper.Map<IEnumerable<AddContactsRequest>>(contactsToProcess);

                // Serialize the dynamic object to JSON
                string jsonPayload = JsonConvert.SerializeObject(gcr, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                // Create StringContent from JSON payload
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                return content;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed in processing the request body for Genesys with exception message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the API request body for Genesys.
        /// </summary>
        /// <param name="caseTicket">Case ticket.<see cref="CaseTickets"/></param>
        /// <returns>Returns the string content.</returns>
        private StringContent GetGetRequestBodyForGenesys(IEnumerable<PostDischargeInfoPlusGenesys> contactsToGet, ILogger logger)
        {
            try
            {
                IEnumerable<long> gcr = contactsToGet.Select(c => c.PostDischargeId);

                // Serialize the dynamic object to JSON
                string jsonPayload = JsonConvert.SerializeObject(gcr, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                // Create StringContent from JSON payload
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                return content;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed in processing the request body for Genesys with exception message: {ex.Message}");
                return null;
            }
        }

        private string GetDeleteRequestQueryForGenesys(IEnumerable<long> contactsToDelete, ILogger logger)
        {
            try
            {
                string contactIds = "";
                foreach (var contact in contactsToDelete)
                {
                    contactIds += $"{contact},";
                }
                return contactIds.TrimEnd(',');
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed in processing the query string arguments for Genesys with exception message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets list of contacts with the passed body information.
        /// </summary>
        ///  <param name="content">Content.<see cref="StringContent"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns thelist of contacts from Genesys.</returns>
        private async Task<IEnumerable<GetContactsResponse>> GetContactsToContactList(StringContent content, ILogger logger)
        {
            // Gets the Genesys http client.
            using HttpClient httpClient = await GetGenesysHttpClient();
            var contactListId = Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId");
            var baseUrl = Environment.GetEnvironmentVariable("BaseUrl");
            Uri requestUri = new($"{baseUrl}/{contactListId}/contacts/bulk");

            // Make the API request
            //HttpResponseMessage response = await httpClient.PostAsync(_configuration["Genesys:ApiEndPoints:GetContacts"], content);
            HttpResponseMessage response = await httpClient.PostAsync(requestUri, content);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and deserialize the response content
                string responseContent = await response.Content.ReadAsStringAsync();

                // Return the deserialized response
                return JsonConvert.DeserializeObject<IEnumerable<GetContactsResponse>>(responseContent);
            }
            else
            {
                logger.LogError($"Failed to call the add contacts API endpoint with response: {response}");
                return new List<GetContactsResponse>();
            }
        }

        /// <summary>
        /// Adds list of contacts with the passed body information.
        /// </summary>
        ///  <param name="content">Content.<see cref="StringContent"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns the list of contacts added to Genesys.</returns>
        private async Task<IEnumerable<AddContactsResponse>> AddContactsToContactListWithStringContent(StringContent content, ILogger logger)
        {
            // Gets the Genesys http client.
            using HttpClient httpClient = await GetGenesysHttpClient();
            var contactListId = Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId");
            var baseUrl = Environment.GetEnvironmentVariable("BaseUrl");
            Uri requestUri = new($"{baseUrl}/{contactListId}/contacts?priority=true");

            // Make the API request
            //HttpResponseMessage response = await httpClient.PostAsync(_configuration["Genesys:ApiEndPoints:AddContacts"], content);
            HttpResponseMessage response = await httpClient.PostAsync(requestUri, content);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and deserialize the response content
                string responseContent = await response.Content.ReadAsStringAsync();

                // Return the deserialized response
                return JsonConvert.DeserializeObject<IEnumerable<AddContactsResponse>>(responseContent);
            }
            else
            {
                logger.LogError($"Failed to call the add contacts API endpoint with response: {response}");
                return new List<AddContactsResponse>();
            }
        }

        /// <summary>
        /// Updates list of contacts with string content.
        /// </summary>
        ///  <param name="content">Content.<see cref="StringContent"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns the updated ticket id.</returns>
        private async Task<IEnumerable<UpdateContactsResponse>> UpdateContactsInContactListWithStringContent(StringContent content, ILogger logger)
        {
            if (content != null)
            {
                // HttpClient
                using HttpClient httpClient = await GetGenesysHttpClient();
                var contactListId = Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId");
                var baseUrl = Environment.GetEnvironmentVariable("BaseUrl");
                Uri requestUri = new($"{baseUrl}/{contactListId}/contacts?priority=true&clearSystemData=true");

                // Make the API request
                //HttpResponseMessage response = await httpClient.PostAsync(_configuration["Genesys:ApiEndPoints:UpdateContacts"], content);
                HttpResponseMessage response = await httpClient.PostAsync(requestUri, content);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response content
                    string responseContent = await response?.Content?.ReadAsStringAsync();

                    // Return
                    return JsonConvert.DeserializeObject<IEnumerable<UpdateContactsResponse>>(responseContent);
                }
                else
                {
                    logger.LogError($"Failed to call the update contacts API endpoint with response: {response}");
                    return new List<UpdateContactsResponse>();
                }
            }
            else { return new List<UpdateContactsResponse>(); }
        }

        /// <summary>
        /// Deletes list of contacts with string content.
        /// </summary>
        ///  <param name="content">Content.<see cref="StringContent"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns the updated ticket id.</returns>
        private async Task<long> DeleteContactsFromContactListWithQueryArgs(string queryArgs, ILogger logger)
        {
            if (!string.IsNullOrWhiteSpace(queryArgs))
            {
                // HttpClient
                using HttpClient httpClient = await GetGenesysHttpClient();
                var contactListId = Environment.GetEnvironmentVariable("AetnaEnglishCampaignClId");

                var baseUrl = Environment.GetEnvironmentVariable("BaseUrl");
                Uri requestUri = new($"{baseUrl}/{contactListId}/contacts?contactIds={queryArgs}");

                // Make the API request
                //HttpResponseMessage response = await httpClient.DeleteAsync(_configuration["Genesys:ApiEndPoints:DeleteContacts"]);
                HttpResponseMessage response = await httpClient.DeleteAsync(requestUri);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response content
                    string responseContent = await response?.Content?.ReadAsStringAsync();

                    // Return
                    return 1;
                }
                else
                {
                    logger.LogError($"Failed to call the delete contacts API endpoint with response: {response}");
                    return 0;
                }
            }
            else { return 0; }
        }

        private async Task Test()
        {
            using (HttpClient httpClient = await GetGenesysHttpClient())
            {
                var url = "https://api.usw2.pure.cloud/api/v2/downloads/6217472fc7e5329f";
                var response = await httpClient.GetByteArrayAsync(url);
                File.WriteAllBytes(@"C:\Temp\Downloadedfile.csv", response);
                string t = "";
            }
        }

        #endregion
    }
}
