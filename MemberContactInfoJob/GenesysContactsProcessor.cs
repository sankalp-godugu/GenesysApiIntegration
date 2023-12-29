using GenesysContactsProcessJob.DataLayer.Interfaces;
using GenesysContactsProcessJob.GenesysLayer.Interfaces;
using GenesysContactsProcessJob.TriggerUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Threading.Tasks;

namespace GenesysContactsProcessJob
{
    /// <summary>
    /// Azure timer function for processing all contacts.
    /// </summary>
    public class GenesysContactsProcessor
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
        public GenesysContactsProcessor(IDataLayer dataLayer, IConfiguration configuration, IGenesysClientService genesysClientService)
        {
            _dataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _genesysClientService = genesysClientService ?? throw new ArgumentNullException(nameof(genesysClientService));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Genesys English member contacts processor.
        /// </summary>
        /// <param name="req">Request.<see cref="req"/></param>
        /// <param name="_logger">Logger.<see cref="ILogger"/></param>
        [FunctionName("AetnaEnglishContactsProcessor")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        public async Task ProcessEnglishContactsAsync([TimerTrigger("0 12 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger logger)
        {
            _ = await GenesysApiUtilities.ProcessGenesysContacts(logger, _configuration, _dataLayer, _genesysClientService, "ENG");
        }

        /// <summary>
        /// Genesys Spanish member contacts processor.
        /// </summary>
        /// <param name="req">Request.<see cref="req"/></param>
        /// <param name="_logger">Logger.<see cref="ILogger"/></param>
        [FunctionName("AetnaSpanishContactsProcessor")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        public void ProcessSpanishContacts([TimerTrigger("0 12 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger logger)
        {
            //_ = await GenesysApiUtilities.ProcessGenesysContacts(logger, _configuration, _dataLayer, _genesysClientService, "SPA");
        }

        #endregion
    }
}
