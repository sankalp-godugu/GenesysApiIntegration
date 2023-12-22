using AutoMapper;
using Dapper;
using MemberContactInfoJob.Model.Request;
using MemberContactInfoJob.Model.Response;
using MemberContactInfoJob.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MemberContactInfoJob
{
    public static class MemberContactInfoJob
    {
        // Visit https://aka.ms/sqlbindingsinput to learn how to use this input binding
        [FunctionName("MemberContactInfoJob")]
        public static async Task Run([TimerTrigger("5,15,25,35,45,55 * * * * *")] TimerInfo myTimer, ILogger logger) {

            if (myTimer.IsPastDue)
            {
                logger.LogInformation("Timer is running late!");
            }
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            AccessTokenResponse token = await AuthenticateAsync();

            // GET MEMBER DISCHARGE INFO
            //Query to fetch data
            
            string connString = "Data Source=tcp:nhonlineordersql.database.windows.net;Initial Catalog=NHCRM_TEST2;User Id=NHOOAdmin;Password=nH3@r!ng321;Connect Timeout=0;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=true";

            //Connection
            using (SqlConnection conn = new(connString))
            {
                string sql = "select * from orders.PdiForGenesys;";
                var queryResult = await conn.QueryAsync<PostDischargeInfo>(sql);

                // CALL GENESYS API TO SEND CONTACT LIST
                var result = await AddContactsToContactList(queryResult, token);

                // CALL GENESYS API TO UPDATE CONTACT
                var result2 = await UpdateContact(queryResult, token);
            }

            //return new OkObjectResult(result);
        }

        public static async Task<AccessTokenResponse> AuthenticateAsync()
        {
            using HttpClient client = new HttpClient();
            Uri baseUrl = new Uri("https://login.usw2.pure.cloud/oauth/token");
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
           /* Stream response = await result.Content.ReadAsStreamAsync();
            AccessTokenResponse token = JsonSerializer.Deserialize<AccessTokenResponse>(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));*/
            string response = await result.Content.ReadAsStringAsync();
            AccessTokenResponse token = JsonConvert.DeserializeObject<AccessTokenResponse>(response);
            SetCacheToken(token);
            return token;
        }

        private static StringContent GenerateBody()
        {
            AccessTokenRequest atr = new();
            var body = JsonConvert.SerializeObject(atr);
            /*JsonSerializer.Serialize(atr,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });*/
            return new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
        }

        private static void SetCacheToken(AccessTokenResponse accessTokenResponse)
        {
            //In a real-world application we should store the token in a cache service and set an TTL.
            Environment.SetEnvironmentVariable("token", accessTokenResponse.AccessToken);
        }

        private static string RetrieveCachedToken()
        {
            //In a real-world application, we should retrieve the token from a cache service.
            return Environment.GetEnvironmentVariable("token");
        }

        private static async Task<AddContactsResponse> GetContactsFromContactList(IEnumerable<PostDischargeInfo> queryResult, AccessTokenResponse token)
        {
            using HttpClient client = new HttpClient();
            Uri baseUrl = new("https://api.usw2.pure.cloud/api/v2/outbound/contactlists/8518a928-6c33-491e-b43a-bf98ec790f7b/contacts?priority=true");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var mapper = MapperConfig.InitializeAutomapper();
            IEnumerable<AddContactsRequest> acr = mapper.Map<List<AddContactsRequest>>(queryResult);

            var body = JsonConvert.SerializeObject(acr, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            StringContent hc = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.GetAsync(baseUrl);
            string data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AddContactsResponse>(data);
        }

        private static async Task<AddContactsResponse> AddContactsToContactList(IEnumerable<PostDischargeInfo> queryResult, AccessTokenResponse token)
        {
            using HttpClient client = new HttpClient();
            Uri baseUrl = new("https://api.usw2.pure.cloud/api/v2/outbound/contactlists/8518a928-6c33-491e-b43a-bf98ec790f7b/contacts?priority=true");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var mapper = MapperConfig.InitializeAutomapper();
            IEnumerable<AddContactsRequest> acr = mapper.Map<List<AddContactsRequest>>(queryResult);

            var body = JsonConvert.SerializeObject(acr, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            StringContent hc = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(baseUrl, hc);
            string data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AddContactsResponse>(data);
        }

        private static async Task<UpdateContactResponse> UpdateContact(IEnumerable<PostDischargeInfo> queryResult, AccessTokenResponse token)
        {
            using HttpClient client = new HttpClient();
            Uri baseUrl = new($"https://api.usw2.pure.cloud/api/v2/outbound/contactlists/8518a928-6c33-491e-b43a-bf98ec790f7b/contacts/1");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var mapper = MapperConfig.InitializeAutomapper();
            IEnumerable<UpdateContactRequest> ucr = mapper.Map<List<UpdateContactRequest>>(queryResult);

            var body = JsonConvert.SerializeObject(ucr, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            StringContent sc = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync(baseUrl, sc);
            string data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UpdateContactResponse>(data);
        }

        private static async Task<AddContactsResponse> DeleteContactsFromContactList(IEnumerable<PostDischargeInfo> queryResult, AccessTokenResponse token)
        {
            using HttpClient client = new HttpClient();
            Uri baseUrl = new Uri("https://api.usw2.pure.cloud/api/v2/outbound/contactlists/8518a928-6c33-491e-b43a-bf98ec790f7b/contacts?contactIds=1");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var mapper = MapperConfig.InitializeAutomapper();
            IEnumerable<AddContactsRequest> acr = mapper.Map<List<AddContactsRequest>>(queryResult);

            var body = JsonConvert.SerializeObject(acr, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            StringContent hc = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.DeleteAsync(baseUrl);
            string data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AddContactsResponse>(data);
        }

        private static async Task<IEnumerable<AddContactsRequest>> Map(IEnumerable<PostDischargeInfo> dqr)
        {
            //var config = new MapperConfiguration(cfg => cfg.CreateMap<IEnumerable<DatabaseQueryResult>, List<AddContactsRequest>>());
            //var mapper = new Mapper(config);
            var mapper = MapperConfig.InitializeAutomapper();
            return mapper.Map<List<AddContactsRequest>>(dqr);
        }

        private static async Task<StringContent> GenerateBody(IEnumerable<PostDischargeInfo> dqr)
        {
            IEnumerable<AddContactsRequest> acr = await Map(dqr);
            var body = JsonConvert.SerializeObject(acr, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            return new StringContent(body, Encoding.UTF8, "application/json");
        }

        public static T DeserializeResponse<T>(string response)
        {
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions{
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}
