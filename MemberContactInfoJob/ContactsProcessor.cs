using GenesysContactsProcessJob.DataLayer.Interfaces;
using GenesysContactsProcessJob.GenesysLayer.Interfaces;
using GenesysContactsProcessJob.Model.Request;
using GenesysContactsProcessJob.Model.Response;
using GenesysContactsProcessJob.TriggerUtilities;
using GenesysContactsProcessJob.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GenesysContactsProcessJob
{
    /// <summary>
    /// Azure timer function for processing all contacts.
    /// </summary>
    public class ContactsProcessor
    {
        #region Private ReadOnly Fields

        private readonly IDataLayer _dataLayer;
        private readonly IConfiguration _configuration;
        private readonly IGenesysClientService _genesysClientService;

        #endregion

        #region Constructor

        /// <summary>
        /// Http function that will be invoked via endpoint.
        /// </summary>
        /// <param name="dataLayer">Datalayer.<see cref="IDataLayer"/></param>
        /// <param name="configuration">Configuration.<see cref="IConfiguration"/></param>
        /// <param name="genesysClientService">Genesys client service.<see cref="IGenesysClientService"/></param>
        public ContactsProcessor(IDataLayer dataLayer, IConfiguration configuration, IGenesysClientService genesysClientService)
        {
            _dataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _genesysClientService = genesysClientService ?? throw new ArgumentNullException(nameof(genesysClientService));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Case management tickets processor.
        /// </summary>
        /// <param name="req">Request.<see cref="req"/></param>
        /// <param name="_logger">Logger.<see cref="ILogger"/></param>
        [FunctionName("AetnaEnglishContactsProcessor")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        public async Task ProcessEnglishContacts([TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger _logger)
        {
            await GenesysApiUtilities.ProcessGenesysContacts(_logger, _configuration, _dataLayer, _genesysClientService);
        }

        /// <summary>
        /// Case management tickets processor.
        /// </summary>
        /// <param name="req">Request.<see cref="req"/></param>
        /// <param name="_logger">Logger.<see cref="ILogger"/></param>
        /*[FunctionName("AetnaSpanishContactsProcessor")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        public async Task<IActionResult> ProcessSpanishContacts([TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger _logger)
        {
            return GenesysApiUtilities.ProcessGenesysContacts(_logger, _configuration, _dataLayer, _genesysClientService);
        }*/


        private async Task<IEnumerable<AddContactsRequest>> Map(IEnumerable<GenesysIntegrationInfo> dqr)
        {
            //var config = new MapperConfiguration(cfg => cfg.CreateMap<IEnumerable<DatabaseQueryResult>, List<AddContactsRequest>>());
            //var mapper = new Mapper(config);
            var mapper = MapperConfig.InitializeAutomapper();
            return mapper.Map<List<AddContactsRequest>>(dqr);
        }

        private T DeserializeResponse<T>(string response)
        {
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions
            {
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

        #endregion
    }
}