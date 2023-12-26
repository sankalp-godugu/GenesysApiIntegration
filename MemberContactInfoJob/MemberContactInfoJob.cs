using Dapper;
using MemberContactInfoJob.Model.Request;
using MemberContactInfoJob.Model.Response;
using MemberContactInfoJob.Utility;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MemberContactInfoJob
{
    public class MemberContactInfoJob
    {
        private readonly ILogger<MemberContactInfoJob> _logger;
        private readonly HttpClient _httpClient;

        public MemberContactInfoJob(ILogger<MemberContactInfoJob> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        // Visit https://aka.ms/sqlbindingsinput to learn how to use this input binding
        [FunctionName("ProcessEnglishContacts")]
        public async Task ProcessEnglishContacts([TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer) {

            //Test();

            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("Timer is running late!");
            }
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            AccessTokenResponse token = await AuthenticateAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            string connString = "Data Source=tcp:nhonlineordersql.database.windows.net;Initial Catalog=NHCRM_TEST2;User Id=NHOOAdmin;Password=nH3@r!ng321;Connect Timeout=0;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=true";

            //Connection
            using SqlConnection conn = new(connString);
            //string sql = "select * from provider.postdischargeinfo where language = 'ENG';";
            string sql = "select * from meals.GenesysIntegration gi join provider.postdischargeinfo pdi on gi.postdischargeid = pdi.postdischargeid where gi.language = 'ENG';";
            var contactsToProcess = await conn.QueryAsync<PostDischargeGenesysInfo>(sql);
            _logger.LogInformation($"{JsonConvert.SerializeObject(contactsToProcess)}");

            string contactListId = "0226dcdf-fa47-4cd2-a81c-5af821d899e2";

            var refreshResult = await conn.QueryAsync("EXEC meals.RefreshDayCountAndAttemptCountToday;");

            // PROCESS IN BULK
            var contactsToAdd = contactsToProcess.TakeWhile(c => c.ShouldAddToContactList == 1 && c.IsDeletedFromContactList == 0);
            var contactsToUpdate = contactsToProcess.TakeWhile(c => c.ShouldUpdateInContactList == 1 && c.IsDeletedFromContactList == 0);
            var contactsToRemove = contactsToProcess.TakeWhile(c => c.ShouldRemoveFromContactList == 1 && c.IsDeletedFromContactList == 0);

            // BULK GET CONTACTS
            //var getResult = await GetContactsFromContactList(token, contactListId);

            // BULK ADD CONTACTS
            // if member already in contact list with same discharge date and/or disposition code of not interested -- update this value from code)
            IEnumerable<AddContactsResponse> addResult;
            IEnumerable<UpdateContactsResponse> updateResult;
            DeleteContactsResponse removeResult;

            if (contactsToAdd.Any())
            {
                addResult = await AddContactsToContactList(contactsToAdd, contactListId);
            }

            // BULK UPDATE CONTACTS
            // if member already in contact list with same discharge date and/or disposition code of not interested -- update this value from code)
            if (contactsToUpdate.Any())
            {
                updateResult = await UpdateContactsInContactList(contactsToUpdate, contactListId);
            }

            // BULK REMOVE CONTACTS
            // if member already in contact list with same discharge date and/or disposition code of not interested -- update this value from code)
            if (contactsToRemove.Any())
            {
                removeResult = await DeleteContactsFromContactList(contactsToRemove, contactListId);
            }
        }

        private async void Test()
        {
            var url = "https://api.usw2.pure.cloud/api/v2/downloads/6217472fc7e5329f";
            var response = await _httpClient.GetByteArrayAsync(url);
            File.WriteAllBytes(@"C:\Temp\Downloadedfile.csv", response);
            string t = "";
        }

        public async Task<AccessTokenResponse> AuthenticateAsync()
        {
            Uri baseUrl = new("https://login.usw2.pure.cloud/oauth/token");
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

            HttpResponseMessage result = await _httpClient.PostAsync(baseUrl, new FormUrlEncodedContent(form));
            result.EnsureSuccessStatusCode();
            string response = await result.Content.ReadAsStringAsync();
            AccessTokenResponse token = JsonConvert.DeserializeObject<AccessTokenResponse>(response);
            //SetCacheToken(token);
            return token;
        }

        private async Task<GetContactsResponse> GetContactsFromContactList(AccessTokenResponse token, string contactListId)
        {
            Uri baseUrl = new($"https://api.usw2.pure.cloud/api/v2/outbound/contactlists/{contactListId}/export?download=true");

            HttpResponseMessage response = await _httpClient.GetAsync(baseUrl);
            string data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GetContactsResponse>(data);
        }
        private async Task<GetContactsResponse> GetContactsFromContactList_Export(AccessTokenResponse token, string contactListId)
        {
            Uri baseUrl = new($"https://api.usw2.pure.cloud/api/v2/outbound/contactlists/{contactListId}/contacts?priority=true");

            HttpResponseMessage response = await _httpClient.GetAsync(baseUrl);
            string data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GetContactsResponse>(data);
        }


        private async Task UpdateGenesysInfoTable()
        {

        }

        private async Task<IEnumerable<AddContactsResponse>> AddContactsToContactList(IEnumerable<PostDischargeGenesysInfo> contactsToAdd, string contactListId)
        {
            Uri baseUrl = new($"https://api.usw2.pure.cloud/api/v2/outbound/contactlists/{contactListId}/contacts?priority=true");

            var mapper = MapperConfig.InitializeAutomapper();
            IEnumerable<AddContactsRequest> acr = mapper.Map<List<AddContactsRequest>>(contactsToAdd);

            var body = JsonConvert.SerializeObject(acr, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            StringContent sc = new(body, Encoding.UTF8, "application/json");

            int count = 0;
            int maxTries = 3;
            while (true) {
                try {
                    HttpResponseMessage response = await _httpClient.PostAsync(baseUrl, sc);
                    response.EnsureSuccessStatusCode();
                    string data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IEnumerable<AddContactsResponse>>(data);
                }
                catch (HttpRequestException ex)
                {
                    if (++count >= maxTries) throw ex;
                }
            }
        }

        private async Task<IEnumerable<UpdateContactsResponse>> UpdateContactsInContactList(IEnumerable<PostDischargeGenesysInfo> contactsToAdd, string contactListId)
        {
            Uri baseUrl = new($"https://api.usw2.pure.cloud/api/v2/outbound/contactlists/{contactListId}/contacts?priority=true");

            var mapper = MapperConfig.InitializeAutomapper();
            IEnumerable<UpdateContactsRequest> ucr = mapper.Map<List<UpdateContactsRequest>>(contactsToAdd);

            var body = JsonConvert.SerializeObject(ucr, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            StringContent sc = new StringContent(body, Encoding.UTF8, "application/json");

            int count = 0;
            int maxTries = 3;
            while (true)
            {
                try
                {
                    HttpResponseMessage response = await _httpClient.PostAsync(baseUrl, sc);
                    response.EnsureSuccessStatusCode();
                    string data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IEnumerable<UpdateContactsResponse>>(data);
                }
                catch (HttpRequestException ex)
                {
                    if (++count >= maxTries) throw ex;
                }
            }
        }

        private async Task<DeleteContactsResponse> DeleteContactsFromContactList(IEnumerable<PostDischargeGenesysInfo> contactsToDelete, string contactListId)
        {
            string baseUrl = $"https://api.usw2.pure.cloud/api/v2/outbound/contactlists/{contactListId}/contacts?contactIds=";

            string contactIds = "";
            foreach (var contact in contactsToDelete)
            {
                contactIds += $"{contact.PostDischargeId},";
            }
            contactIds = contactIds.TrimEnd(',');
            Uri completeUrl = new(baseUrl + contactIds);

            int count = 0;
            int maxTries = 3;
            while (true)
            {
                try
                {
                    HttpResponseMessage response = await _httpClient.DeleteAsync(completeUrl);
                    response.EnsureSuccessStatusCode();
                    string data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<DeleteContactsResponse>(data);
                }
                catch (HttpRequestException ex)
                {
                    if (++count >= maxTries) throw ex;
                }
            }
        }

        private async Task<IEnumerable<AddContactsRequest>> Map(IEnumerable<GenesysIntegrationInfo> dqr)
        {
            //var config = new MapperConfiguration(cfg => cfg.CreateMap<IEnumerable<DatabaseQueryResult>, List<AddContactsRequest>>());
            //var mapper = new Mapper(config);
            var mapper = MapperConfig.InitializeAutomapper();
            return mapper.Map<List<AddContactsRequest>>(dqr);
        }

        private async Task<StringContent> GenerateBody(IEnumerable<GenesysIntegrationInfo> dqr)
        {
            IEnumerable<AddContactsRequest> acr = await Map(dqr);
            var body = JsonConvert.SerializeObject(acr, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            return new StringContent(body, Encoding.UTF8, "application/json");
        }

        public T DeserializeResponse<T>(string response)
        {
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions{
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
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
    }
}

/*[FunctionName("ProcessSpanishContacts")]
public async Task ProcessSpanishContacts([TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer)
{
if (myTimer.IsPastDue)
{
_logger.LogInformation("Timer is running late!");
}
_logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
AccessTokenResponse token = await AuthenticateAsync();

string connString = "Data Source=tcp:nhonlineordersql.database.windows.net;Initial Catalog=NHCRM_TEST2;User Id=NHOOAdmin;Password=nH3@r!ng321;Connect Timeout=0;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=true";

//Connection
using SqlConnection conn = new(connString);
string sql = "select * from meals.GenesysIntegration where language = 'SPA';";
var queryResult = await conn.QueryAsync<PostDischargeInfo>(sql);
_logger.LogInformation($"{JsonConvert.SerializeObject(queryResult)}");

var contactsToAdd = queryResult.TakeWhile(c => c.ShouldAddToContactList == 1);
var contactsToUpdate = queryResult.TakeWhile(c => c.ShouldUpdateInContactList == 1);
var contactsToRemove = queryResult.TakeWhile(c => c.ShouldRemoveFromContactList == 1);
string contactListId = "ad549fb7-e5bf-438d-880f-1c4e3d135dc9";

_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

// BULK ADD CONTACTS
// if member already in contact list with same discharge date and/or disposition code of not interested -- update this value from code)
var addResult = await AddContactsToContactList(contactsToAdd, token, contactListId);

// BULK UPDATE CONTACTS
// if member already in contact list with same discharge date and/or disposition code of not interested -- update this value from code)
var updateResult = await UpdateContactsInContactList(contactsToUpdate, token, contactListId);

// BULK REMOVE CONTACTS
// if member already in contact list with same discharge date and/or disposition code of not interested -- update this value from code)
var removeResult = await DeleteContactsFromContactList(contactsToRemove, token, contactListId);
}*/
