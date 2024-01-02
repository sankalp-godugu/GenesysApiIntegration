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
        /// Gets the contacts in Genesys asychronously.
        /// </summary>
        /// <param name="contactsToAdd">Contacts To Add.<see cref="ContactsToAdd"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns 1 for success.</returns>
        public IEnumerable<GetContactsResponse> GetContactsFromContactList(IEnumerable<PostDischargeInfo_GenesysContactInfo> contactsToGetFromGenesys, string lang, ILogger logger)
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
        public async Task<IEnumerable<AddContactsToGenesysResponse>> AddContactsToContactList(IEnumerable<PostDischargeInfo_GenesysContactInfo> contactsToAddToGenesys, string lang, ILogger logger)
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
        public async Task<IEnumerable<UpdateContactsInGenesysResponse>> UpdateContactsInContactList(IEnumerable<PostDischargeInfo_GenesysContactInfo> contactsToUpdateInGenesys, string lang, ILogger logger)
        {
            // Gets the request body for the Genesys API request.
            StringContent content = GetAddOrUpdateRequestBodyForGenesys(contactsToUpdateInGenesys, logger);
            return await UpdateContactsInContactListWithStringContent(content, lang, logger);
        }

        public async Task<UpdateContactsInGenesysResponse> UpdateContactInContactList(PostDischargeInfo_GenesysContactInfo contactToUpdateInGenesys, string lang, ILogger logger)
        {
            StringContent content = GetUpdateRequestBodyForGenesys(contactToUpdateInGenesys, logger);
            return await UpdateContactInContactListWithStringContent(contactToUpdateInGenesys.PostDischargeId, content, lang, logger);
        }

        /// <summary>
        /// Deletes the contacts in Genesys asynchronously.
        /// </summary>
        /// <param name="contactsToDeleteFromGenesys">Contacts To Delete.<see cref="ContactsToDelete"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns 1 for success.</returns>
        public async Task<long> DeleteContactsFromContactList(IEnumerable<long> contactsToDeleteFromGenesys, string lang, ILogger logger)
        {
            // Gets the request query arguments for the Genesys API request.
            string queryArgs = GetDeleteRequestQueryForGenesys(contactsToDeleteFromGenesys, logger);
            return await DeleteContactsFromContactListWithQueryArgs(queryArgs, lang, logger);
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

            // Set the Authorization header with the Bearer authentication credentials
            AccessTokenResponse tokenResponse = await AuthenticateAsync(httpClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            return httpClient;
        }

        private async Task<AccessTokenResponse> AuthenticateAsync(HttpClient client)
        {
            Uri tokenUrl = new(_configuration["Genesys:AppConfigurations:AccessTokenUrl"]);
            //Environment.GetEnvironmentVariable("AccessTokenUrl"));

            Dictionary<string, string> form = new()
            {
                {"grant_type", _configuration["Genesys:AppConfigurations:GrantType"]},
                //?? Environment.GetEnvironmentVariable("GrantType")},
                {"client_id", _configuration["Genesys:AppConfigurations:ClientId"]},
                //?? Environment.GetEnvironmentVariable("ClientId")},
                {"client_secret", _configuration["Genesys:AppConfigurations:ClientSecret"]}
                //?? Environment.GetEnvironmentVariable("ClientSecret")},
            };

            HttpResponseMessage result = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(form));
            _ = result.EnsureSuccessStatusCode();
            string response = await result.Content.ReadAsStringAsync();
            AccessTokenResponse token = JsonConvert.DeserializeObject<AccessTokenResponse>(response);
            return token;
        }

        /// <summary>
        /// Gets the API request body for Genesys.
        /// </summary>
        /// <param name="contactsToProcess">Contacts to Process.<see cref="ContactsToProcess"/></param>
        /// <returns>Returns the string content.</returns>
        private StringContent GetAddOrUpdateRequestBodyForGenesys(IEnumerable<PostDischargeInfo_GenesysContactInfo> contactsToProcess, ILogger logger)
        {
            try
            {
                Mapper mapper = MapperConfig.InitializeAutomapper(_configuration);
                IEnumerable<AddContactsRequest> ucr = mapper.Map<IEnumerable<AddContactsRequest>>(contactsToProcess);

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

        private StringContent GetUpdateRequestBodyForGenesys(PostDischargeInfo_GenesysContactInfo contactToProcess, ILogger logger)
        {
            try
            {
                Mapper mapper = MapperConfig.InitializeAutomapper(_configuration);
                AddContactsRequest ucr = mapper.Map<AddContactsRequest>(contactToProcess);

                // Serialize the dynamic object to JSON
                string jsonPayload = JsonConvert.SerializeObject(ucr, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                // Create StringContent from JSON payload
                StringContent content = new(jsonPayload, Encoding.UTF8, "application/json");
                return content;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed in processing the Update Single request body for Genesys API call with exception message: {ex.Message}");
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
        private string GetDeleteRequestQueryForGenesys(IEnumerable<long> contactsToDeleteFromGenesys, ILogger logger)
        {
            try
            {
                string contactIds = "";
                foreach (long contact in contactsToDeleteFromGenesys)
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
        ///  <param name="queryArgs">Content.<see cref="string"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns thelist of contacts from Genesys.</returns>
        private async Task<IEnumerable<GetContactsExportDataFromGenesysResponse>> GetContactsFromContactListExportWithQueryArgs(string queryArgs, string lang, ILogger logger)
        {
            // Gets the Genesys http client.
            using HttpClient httpClient = await GetGenesysHttpClient();

            string contactListId = lang == Languages.English ?
                _configuration["Genesys:AppConfigurations:AetnaEnglish"] : _configuration["Genesys:AppConfigurations:AetnaSpanish"];
            //Environment.GetEnvironmentVariable("AetnaEnglish") : Environment.GetEnvironmentVariable("AetnaSpanish");

            string baseUrl = _configuration["Genesys:AppConfigurations:BaseURL"];
            //Environment.GetEnvironmentVariable("BaseUrl");

            Uri requestUri = new($"{baseUrl}/{contactListId}/export?{queryArgs}");

            // Make the API request
            HttpResponseMessage response = await httpClient.GetAsync(_configuration["Genesys:ApiEndPoints:GetContacts"] ?? requestUri.OriginalString);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and deserialize the response content
                string responseContent = await response.Content.ReadAsStringAsync();

                GetContactsExportResponse getContactsExportResponse = JsonConvert.DeserializeObject<GetContactsExportResponse>(responseContent);

                requestUri = new(getContactsExportResponse.Uri);
                response = await httpClient.GetAsync(requestUri);
                Stream responseStream = await response.Content.ReadAsStreamAsync();

                using StreamReader reader = new(responseStream);
                using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
                _ = csv.Context.RegisterClassMap<GetContactsExportDataFromGenesysResponseMap>();
                return csv.GetRecords<GetContactsExportDataFromGenesysResponse>().ToList();
            }
            else
            {
                logger.LogError($"Error in Get Contacts API endpoint with response: {response}");
                return new List<GetContactsExportDataFromGenesysResponse>();
            }
        }

        /// <summary>
        /// Adds list of contacts with the passed body information.
        /// </summary>
        ///  <param name="content">Content.<see cref="StringContent"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns the list of contacts added to Genesys.</returns>
        private async Task<IEnumerable<AddContactsToGenesysResponse>> AddContactsToContactListWithStringContent(StringContent content, string lang, ILogger logger)
        {
            // Gets the Genesys http client.
            using HttpClient httpClient = await GetGenesysHttpClient();

            string contactListId = lang == Languages.English ?
                _configuration["Genesys:AppConfigurations:AetnaEnglish"] : _configuration["Genesys:AppConfigurations:AetnaSpanish"];
            //Environment.GetEnvironmentVariable("AetnaEnglish") : Environment.GetEnvironmentVariable("AetnaSpanish");

            string baseUrl = _configuration["Genesys:AppConfigurations:BaseURL"];
            //Environment.GetEnvironmentVariable("BaseUrl");

            Uri requestUri = new($"{baseUrl}/{contactListId}/contacts?priority=true");

            // Make the API request
            HttpResponseMessage response = await httpClient.PostAsync(_configuration["Genesys:ApiEndPoints:AddContacts"] ?? requestUri.OriginalString, content);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and deserialize the response content
                string responseContent = await response.Content.ReadAsStringAsync();

                // Return the deserialized response
                return JsonConvert.DeserializeObject<IEnumerable<AddContactsToGenesysResponse>>(responseContent);
            }
            else
            {
                logger.LogError($"Error in Add Contacts API endpoint with response: {response}");
                return new List<AddContactsToGenesysResponse>();
            }
        }

        /// <summary>
        /// Updates list of contacts with string content.
        /// </summary>
        ///  <param name="content">Content.<see cref="StringContent"/></param>
        /// <param name="logger">Logger.<see cref="Logger"/></param>
        /// <returns>Returns the updated list of contacts.</returns>
        private async Task<IEnumerable<UpdateContactsInGenesysResponse>> UpdateContactsInContactListWithStringContent(StringContent content, string lang, ILogger logger)
        {
            if (content != null)
            {
                // HttpClient
                using HttpClient httpClient = await GetGenesysHttpClient();

                string contactListId = lang == Languages.English ?
                _configuration["Genesys:AppConfigurations:AetnaEnglish"] : _configuration["Genesys:AppConfigurations:AetnaSpanish"];
                //Environment.GetEnvironmentVariable("AetnaEnglish") : Environment.GetEnvironmentVariable("AetnaSpanish");

                string baseUrl = _configuration["Genesys:AppConfigurations:BaseURL"];
                //Environment.GetEnvironmentVariable("BaseUrl");

                Uri requestUri = new($"{baseUrl}/{contactListId}/contacts?priority=true&clearSystemData=true");

                // Make the API request
                HttpResponseMessage response = await httpClient.PostAsync(_configuration["Genesys:ApiEndPoints:UpdateContacts"] ?? requestUri.OriginalString, content);


                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response content
                    string responseContent = await response?.Content?.ReadAsStringAsync();

                    // Return
                    return JsonConvert.DeserializeObject<IEnumerable<UpdateContactsInGenesysResponse>>(responseContent);
                }
                else
                {
                    logger.LogError($"Error in Update Contacts API endpoint with response: {response}");
                    return new List<UpdateContactsInGenesysResponse>();
                }
            }
            else { return new List<UpdateContactsInGenesysResponse>(); }
        }

        private async Task<UpdateContactsInGenesysResponse> UpdateContactInContactListWithStringContent(long id, StringContent content, string lang, ILogger logger)
        {
            // HttpClient
            using HttpClient httpClient = await GetGenesysHttpClient();

            string contactListId = lang == Languages.English ?
                _configuration["Genesys:AppConfigurations:AetnaEnglish"] : _configuration["Genesys:AppConfigurations:AetnaSpanish"];
            //Environment.GetEnvironmentVariable("AetnaEnglish") : Environment.GetEnvironmentVariable("AetnaSpanish");

            string baseUrl = _configuration["Genesys:Api:BaseUrl"];
            //Environment.GetEnvironmentVariable("BaseUrl");

            Uri requestUri = new($"{baseUrl}/{contactListId}/contacts/{id}");

            // Make the API request
            HttpResponseMessage response = await httpClient.PutAsync(_configuration["Genesys:ApiEndPoints:UpdateContact"] ?? requestUri.OriginalString, content);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and deserialize the response content
                string responseContent = await response?.Content?.ReadAsStringAsync();

                // Return
                return JsonConvert.DeserializeObject<UpdateContactsInGenesysResponse>(responseContent);
            }
            else
            {
                logger.LogError($"Error in Update Contacts API endpoint with response: {response}");
                return new UpdateContactsInGenesysResponse();
            }
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
                // HttpClient
                using HttpClient httpClient = await GetGenesysHttpClient();

                string contactListId = lang == Languages.English ? _configuration["Genesys:AppConfigurations:AetnaEnglish"] : _configuration["Genesys:AppConfigurations:AetnaSpanish"];
                //Environment.GetEnvironmentVariable("AetnaEnglish");

                string baseUrl = _configuration["Genesys:AppConfigurations:BaseURL"];
                //Environment.GetEnvironmentVariable("BaseUrl");

                Uri requestUri = new($"{baseUrl}/{contactListId}/contacts?contactIds={queryArgs}");

                // Make the API request
                HttpResponseMessage response = await httpClient.DeleteAsync(_configuration["Genesys:ApiEndPoints:DeleteContacts"] ?? requestUri.OriginalString);

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

        Task<IEnumerable<GetContactsResponse>> IGenesysClientService.GetContactsFromContactList(IEnumerable<PostDischargeInfo_GenesysContactInfo> contactsToGetFromGenesys, string lang, ILogger logger)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
