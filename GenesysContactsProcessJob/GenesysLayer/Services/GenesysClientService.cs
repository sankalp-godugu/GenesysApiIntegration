using AutoMapper;
using CsvHelper;
using GenesysContactsProcessJob.GenesysLayer.Interfaces;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

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
        /// Initiate the export for Genesys contact list async
        /// </summary>
        public async Task<InitiateContactListExportResponse> InitiateContactListExport(string lang, ILogger logger)
        {
            return await InitiateContactListExportAsync(lang, logger);
        }

        /// <summary>
        /// Gets the contacts in Genesys asychronously.
        /// </summary>
        /// <param name="contactsToAdd">Contacts To Add.<see cref="ContactsToAdd"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns 1 for success.</returns>
        public IEnumerable<GetContactsResponse> GetContactsFromContactList(IEnumerable<GetContactsRequest> contactsToGetFromGenesys, string lang, ILogger logger)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the contacts in Genesys via export file asychronously.
        /// </summary>
        /// <param name="contactsToAdd">Contacts To Add.<see cref="ContactsToAdd"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns 1 for success.</returns>
        public async Task<IEnumerable<GetContactsExportDataFromGenesysResponse>> GetContactsFromContactListExport(string lang, ILogger logger)
        {
            return await GetContactsFromContactListExportWithQueryArgs("download=false", lang, logger);
        }

        /// <summary>
        /// Adds the contacts in Genesys asychronously.
        /// </summary>
        /// <param name="contactsToAddToGenesys">Contacts To Add.<see cref="ContactsToAdd"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns 1 for success.</returns>
        public async Task<IEnumerable<PostContactsToGenesysResponse>> AddContactsToContactList(IEnumerable<PostContactsRequest> contactsToAddToGenesys, string lang, ILogger logger)
        {
            // Gets the request body for the genesys client.
            StringContent content = GetAddOrUpdateRequestBodyForGenesys(contactsToAddToGenesys, logger);
            return await AddContactsToContactListWithStringContent(content, lang, logger);
        }

        /// <summary>
        /// Updates the contacts in Genesys asynchronously.
        /// </summary>
        /// <param name="contactsToUpdateInGenesys">Contacts To Update.<see cref="ContactsToUpdate"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns 1 for success.</returns>
        public async Task<IEnumerable<PostContactsToGenesysResponse>> UpdateContactsInContactList(IEnumerable<PostContactsRequest> contactsToUpdateInGenesys, string lang, ILogger logger)
        {
            // Gets the request body for the Genesys API request.
            StringContent content = GetAddOrUpdateRequestBodyForGenesys(contactsToUpdateInGenesys, logger);
            return await UpdateContactsInContactListWithStringContent(content, lang, logger);
        }

        public async Task<PostContactsToGenesysResponse> UpdateContactInContactList(PostContactsRequest contactToUpdateInGenesys, string lang, ILogger logger)
        {
            List<PostContactsRequest> contactsToUpdateInGenesys = new() { contactToUpdateInGenesys };
            StringContent content = GetAddOrUpdateRequestBodyForGenesys(contactsToUpdateInGenesys, logger);
            return await UpdateContactInContactListWithStringContent(contactToUpdateInGenesys.Id, content, lang, logger);
        }

        /// <summary>
        /// Deletes the contacts in Genesys asynchronously.
        /// </summary>
        /// <param name="contactsToDeleteFromGenesys">Contacts To Delete.<see cref="ContactsToDelete"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns 1 for success.</returns>
        public async Task<long> DeleteContactsFromContactList(IEnumerable<string> contactsToDeleteFromGenesys, string lang, ILogger logger)
        {
            // Gets the request query arguments for the Genesys API request.
            List<string> contactsToRemove = contactsToDeleteFromGenesys.ToList();
            while (contactsToRemove.Count > 0)
            {
                int max = contactsToRemove.Count < 100 ? contactsToRemove.Count : 100;
                string queryArgs = GetDeleteRequestQueryForGenesys(contactsToRemove.GetRange(0, max), logger);
                contactsToRemove.RemoveRange(0, max);
                _ = await DeleteContactsFromContactListWithQueryArgs(queryArgs, lang, logger);
            }
            return 1;
        }

        public Task<IEnumerable<GetContactsExportDataFromGenesysResponse>> GetListFromCsv(string filePath)
        {
            IEnumerable<GetContactsExportDataFromGenesysResponse> contactsToScrub = new List<GetContactsExportDataFromGenesysResponse>();
            if (File.Exists(filePath))
            {
                using StreamReader reader = new(File.OpenRead(filePath));
                using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
                _ = csv.Context.RegisterClassMap<GetContactsExportDataFromGenesysResponseMap>();
                contactsToScrub = csv.GetRecords<GetContactsExportDataFromGenesysResponse>().ToList();
            }
            return Task.FromResult(contactsToScrub);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the API request body for Genesys.
        /// </summary>
        /// <param name="contactsToProcess">Contacts to Process.<see cref="ContactsToProcess"/></param>
        /// <returns>Returns the string content.</returns>
        private StringContent GetAddOrUpdateRequestBodyForGenesys(IEnumerable<PostContactsRequest> contactsToProcess, ILogger logger)
        {
            try
            {
                Mapper mapper = MapperConfig.InitializeAutomapper(_configuration);
                IEnumerable<PostContactsRequest> ucr = mapper.Map<IEnumerable<PostContactsRequest>>(contactsToProcess);

                // Serialize the dynamic object to JSON
                string jsonPayload = JsonConvert.SerializeObject(ucr, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                // Create StringContent from JSON payload
                StringContent content = new(jsonPayload, Encoding.UTF8, "application/json");
                return content;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed in processing the Add/Update request body for Genesys API call with exception message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the API request body for Genesys.
        /// </summary>
        /// <param name="contactsToGetFromGenesys">Contacts to Process.<see cref="ContactsToGet"/></param>
        /// <returns>Returns the string content.</returns>
        private StringContent GetGetRequestBodyForGenesys(IEnumerable<PostDischargeInfo_GenesysContactInfo> contactsToGetFromGenesys, ILogger logger)
        {
            try
            {
                IEnumerable<long> gcr = contactsToGetFromGenesys.Select(c => c.PostDischargeId);

                // Serialize the dynamic object to JSON
                string jsonPayload = JsonConvert.SerializeObject(gcr, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                // Create StringContent from JSON payload
                StringContent content = new(jsonPayload, Encoding.UTF8, "application/json");
                return content;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed in processing the GET request body for Genesys API call with exception message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the API request body for Genesys.
        /// </summary>
        /// <param name="contactsToDeleteFromGenesys">Contacts to Process.<see cref="ContactsToDelete"/></param>
        /// <returns>Returns the string content.</returns>
        private string GetDeleteRequestQueryForGenesys(IEnumerable<string> contactsToDeleteFromGenesys, ILogger logger)
        {
            try
            {
                string contactIds = "";
                foreach (string contact in contactsToDeleteFromGenesys)
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
        /// Gets the Genesys http client.
        /// </summary>
        /// <returns>Returns the http client to make API requests.</returns>
        private HttpClient GetGenesysHttpClient()
        {
            _ = new HttpClientHandler() { AllowAutoRedirect = false };
            return _httpClientFactory.CreateClient();
        }

        private async Task<string> GetAuthToken()
        {
            using HttpClient httpClient = GetGenesysHttpClient();
            string tokenUrl = _configuration[ConfigConstants.TokenUrlKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.TokenUrlKey);
            Uri tokenUri = new(tokenUrl);
            Dictionary<string, string> form = new()
            {
                {"grant_type", _configuration[ConfigConstants.GrantTypeKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.GrantTypeKey)},
                {"client_id", _configuration[ConfigConstants.ClientIdKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.ClientIdKey)},
                {"client_secret", _configuration[ConfigConstants.ClientSecretKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.ClientSecretKey)}
            };

            HttpResponseMessage result = await httpClient.PostAsync(tokenUri, new FormUrlEncodedContent(form));
            _ = result.EnsureSuccessStatusCode();
            string response = await result.Content.ReadAsStringAsync();
            AccessTokenResponse tokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(response);
            return tokenResponse.AccessToken;
        }

        /// <summary>
        /// Initiates contact list export
        /// </summary>
        ///  <param name="content">Content.<see cref="StringContent"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns 1 if success.</returns>
        private async Task<InitiateContactListExportResponse> InitiateContactListExportAsync(string lang, ILogger logger)
        {
            string contactListIdKey = lang == Languages.English ? ConfigConstants.ContactListIdAetnaEnglishKey : ConfigConstants.ContactListIdAetnaSpanishKey;
            string contactListId = _configuration[contactListIdKey] ?? Environment.GetEnvironmentVariable(contactListIdKey);
            string baseUrl = _configuration[ConfigConstants.BaseUrlKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.BaseUrlKey);

            // HttpClient
            using HttpClient httpClient = GetGenesysHttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthToken());
            httpClient.BaseAddress = new(baseUrl);

            // Make the API request
            Uri requestUri = new($"outbound/contactlists/{contactListId}/export", UriKind.Relative);
            HttpResponseMessage response = await httpClient.PostAsync(requestUri, null);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and deserialize the response content
                string responseContent = await response?.Content?.ReadAsStringAsync();

                // Return
                return JsonConvert.DeserializeObject<InitiateContactListExportResponse>(responseContent);
            }
            else
            {
                logger.LogError($"Error in Initiate Contact List Export API endpoint with response: {response}");
                return new InitiateContactListExportResponse();
            }
        }

        /// <summary>
        /// Gets list of contacts with the passed body information.
        /// </summary>
        ///  <param name="queryArgs">Content.<see cref="string"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns thelist of contacts from Genesys.</returns>
        private async Task<IEnumerable<GetContactsExportDataFromGenesysResponse>> GetContactsFromContactListExportWithQueryArgs(string queryArgs, string lang, ILogger logger)
        {
            string contactListIdKey = lang == Languages.English ? ConfigConstants.ContactListIdAetnaEnglishKey : ConfigConstants.ContactListIdAetnaSpanishKey;
            string contactListId = _configuration[contactListIdKey] ?? Environment.GetEnvironmentVariable(contactListIdKey);
            string baseUrl = _configuration[ConfigConstants.BaseUrlKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.BaseUrlKey);

            // HttpClient
            using HttpClient httpClient = GetGenesysHttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthToken());
            httpClient.BaseAddress = new(baseUrl);

            // Make the API request
            Uri requestUri = new($"outbound/contactlists/{contactListId}/export?{queryArgs}", UriKind.Relative);
            HttpResponseMessage response = await httpClient.GetAsync(requestUri);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and deserialize the response content
                string responseContent = await response.Content.ReadAsStringAsync();
                GetContactsExportResponse getContactsExportResponse = JsonConvert.DeserializeObject<GetContactsExportResponse>(responseContent);

                response = await httpClient.GetAsync($"{getContactsExportResponse.Uri}?issueRedirect=false&redirectToAuth=false");
                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response content
                    responseContent = await response.Content.ReadAsStringAsync();

                    GetContactsDownloadResponse getContactsDownloadResponse = JsonConvert.DeserializeObject<GetContactsDownloadResponse>(responseContent);

                    // Download to file
                    WebClient webClient = new();
                    webClient.DownloadFile(getContactsDownloadResponse.Url, @"c:\Temp\DownloadAPI_ResponseHeaders_Location.txt");
                }
                else
                {
                    logger.LogError($"Error in Get Contacts Download URI with response: {response}");
                }

                response = await httpClient.GetAsync(getContactsExportResponse.Uri);
                if (response.IsSuccessStatusCode)
                {
                    Stream responseStream = await response.Content.ReadAsStreamAsync();
                    using StreamReader reader = new(responseStream);
                    using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
                    _ = csv.Context.RegisterClassMap<GetContactsExportDataFromGenesysResponseMap>();
                    return csv.GetRecords<GetContactsExportDataFromGenesysResponse>().ToList();
                }
                else
                {
                    logger.LogError($"Error in Get Contacts Data API endpoint with response: {response}");
                }
            }
            else
            {
                logger.LogError($"Error in Get Contacts Export API endpoint with response: {response}");
            }
            return new List<GetContactsExportDataFromGenesysResponse>();
        }

        /// <summary>
        /// Deletes list of contacts with string content.
        /// </summary>
        ///  <param name="content">Content.<see cref="StringContent"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns 1 if success.</returns>
        private async Task<long> DeleteContactsFromContactListWithQueryArgs(string queryArgs, string lang, ILogger logger)
        {
            if (!string.IsNullOrWhiteSpace(queryArgs))
            {
                string contactListIdKey = lang == Languages.English ? ConfigConstants.ContactListIdAetnaEnglishKey : ConfigConstants.ContactListIdAetnaSpanishKey;
                string contactListId = _configuration[contactListIdKey] ?? Environment.GetEnvironmentVariable(contactListIdKey);
                string baseUrl = _configuration[ConfigConstants.BaseUrlKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.BaseUrlKey);

                // HttpClient
                using HttpClient httpClient = GetGenesysHttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthToken());
                httpClient.BaseAddress = new(baseUrl);

                // Make the API request
                Uri requestUri = new($"outbound/contactlists/{contactListId}/contacts?contactIds={queryArgs}", UriKind.Relative);
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
                    logger.LogError($"Error in Remove Contacts API endpoint with response: {response}");
                    return 0;
                }
            }
            else { return 0; }
        }

        /// <summary>
        /// Adds list of contacts with the passed body information.
        /// </summary>
        ///  <param name="content">Content.<see cref="StringContent"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns the list of contacts added to Genesys.</returns>
        private async Task<IEnumerable<PostContactsToGenesysResponse>> AddContactsToContactListWithStringContent(StringContent content, string lang, ILogger logger)
        {
            // Gets the Genesys http client.
            string contactListIdKey = lang == Languages.English ? ConfigConstants.ContactListIdAetnaEnglishKey : ConfigConstants.ContactListIdAetnaSpanishKey;
            string contactListId = _configuration[contactListIdKey] ?? Environment.GetEnvironmentVariable(contactListIdKey);
            string baseUrl = _configuration[ConfigConstants.BaseUrlKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.BaseUrlKey);

            // HttpClient
            using HttpClient httpClient = GetGenesysHttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthToken());
            httpClient.BaseAddress = new(baseUrl);

            // Make the API request
            Uri requestUri = new($"outbound/contactlists/{contactListId}/contacts?priority=true");
            HttpResponseMessage response = await httpClient.PostAsync(requestUri, content);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and deserialize the response content
                string responseContent = await response.Content.ReadAsStringAsync();

                // Return the deserialized response
                return JsonConvert.DeserializeObject<IEnumerable<PostContactsToGenesysResponse>>(responseContent);
            }
            else
            {
                logger.LogError($"Error in Add Contacts API endpoint with response: {response}");
                return new List<PostContactsToGenesysResponse>();
            }
        }

        /// <summary>
        /// Updates list of contacts with string content.
        /// </summary>
        ///  <param name="content">Content.<see cref="StringContent"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns the updated list of contacts.</returns>
        private async Task<IEnumerable<PostContactsToGenesysResponse>> UpdateContactsInContactListWithStringContent(StringContent content, string lang, ILogger logger)
        {
            if (content != null)
            {
                // Gets the Genesys http client.
                string contactListIdKey = lang == Languages.English ? ConfigConstants.ContactListIdAetnaEnglishKey : ConfigConstants.ContactListIdAetnaSpanishKey;
                string contactListId = _configuration[contactListIdKey] ?? Environment.GetEnvironmentVariable(contactListIdKey);
                string baseUrl = _configuration[ConfigConstants.BaseUrlKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.BaseUrlKey);

                // HttpClient
                using HttpClient httpClient = GetGenesysHttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthToken());
                httpClient.BaseAddress = new(baseUrl);

                // Make the API request
                Uri requestUri = new($"outbound/contactlists/{contactListId}/contacts?priority=true&clearSystemData=true");
                HttpResponseMessage response = await httpClient.PostAsync(requestUri, content);


                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response content
                    string responseContent = await response?.Content?.ReadAsStringAsync();

                    // Return
                    return JsonConvert.DeserializeObject<IEnumerable<PostContactsToGenesysResponse>>(responseContent);
                }
                else
                {
                    logger.LogError($"Error in Update Contacts API endpoint with response: {response}");
                    return new List<PostContactsToGenesysResponse>();
                }
            }
            else { return new List<PostContactsToGenesysResponse>(); }
        }

        private async Task<PostContactsToGenesysResponse> UpdateContactInContactListWithStringContent(long id, StringContent content, string lang, ILogger logger)
        {
            // Gets the Genesys http client.
            string contactListIdKey = lang == Languages.English ? ConfigConstants.ContactListIdAetnaEnglishKey : ConfigConstants.ContactListIdAetnaSpanishKey;
            string contactListId = _configuration[contactListIdKey] ?? Environment.GetEnvironmentVariable(contactListIdKey);
            string baseUrl = _configuration[ConfigConstants.BaseUrlKey] ?? Environment.GetEnvironmentVariable(ConfigConstants.BaseUrlKey);

            // HttpClient
            using HttpClient httpClient = GetGenesysHttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthToken());
            httpClient.BaseAddress = new(baseUrl);

            // Make the API request
            Uri requestUri = new($"outbound/contactlists/{contactListId}/contacts/{id}");
            HttpResponseMessage response = await httpClient.PutAsync(requestUri, content);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and deserialize the response content
                string responseContent = await response?.Content?.ReadAsStringAsync();

                // Return
                return JsonConvert.DeserializeObject<PostContactsToGenesysResponse>(responseContent);
            }
            else
            {
                logger.LogError($"Error in Update Contacts API endpoint with response: {response}");
                return new PostContactsToGenesysResponse();
            }
        }

        Task<IEnumerable<GetContactsResponse>> IGenesysClientService.GetContactsFromContactList(IEnumerable<GetContactsRequest> contactsToGetFromGenesys, string lang, ILogger logger)
        {
            throw new NotImplementedException();
        }

        private void SetCacheToken(AccessTokenResponse accessTokenResponse)
        {
            //In a real-world application we should store the token in a cache service and set an TTL.
            Environment.SetEnvironmentVariable("token", accessTokenResponse.AccessToken);
        }

        private static string RetrieveCachedToken()
        {
            //In a real-world application, we should retrieve the token from a cache service.
            return Environment.GetEnvironmentVariable("token");
        }

        #endregion
    }
}
