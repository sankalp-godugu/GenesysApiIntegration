using GenesysContactsProcessJob.DataLayer.Interfaces;
using GenesysContactsProcessJob.GenesysLayer.Interfaces;
using GenesysContactsProcessJob.TriggerUtilities;
using GenesysContactsProcessJob.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        /// TODO: update timer trigger expression after manual testing is complete
        [FunctionName("AetnaEnglishContactsProcessor")]
        public async Task ProcessEnglishContactsAsync([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest request, ILogger logger)
        {
            _ = await GenesysApiUtilities.ProcessGenesysContacts(logger, _configuration, _dataLayer, _genesysClientService, Campaigns.AetnaEnglish);
        }

        /// <summary>
        /// Genesys Spanish member contacts processor.
        /// </summary>
        /// <param name="req">Request.<see cref="req"/></param>
        /// <param name="_logger">Logger.<see cref="ILogger"/></param>
        /// TODO: update timer trigger expression after manual testing is complete
        [FunctionName("AetnaSpanishContactsProcessor")]
        public async Task ProcessSpanishContactsAsync([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest request, ILogger logger)
        {
            _ = await GenesysApiUtilities.ProcessGenesysContacts(logger, _configuration, _dataLayer, _genesysClientService, Campaigns.AetnaSpanish);
        }

        /// <summary>
        /// Genesys Spanish member contacts processor.
        /// </summary>
        /// <param name="req">Request.<see cref="req"/></param>
        /// <param name="_logger">Logger.<see cref="ILogger"/></param>
        /// TODO: update timer trigger expression after manual testing is complete
        [FunctionName("NationsMarketEnglishContactsProcessor")]
        public async Task ProcessNationsMarketEnglishContactsAsync([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest request, ILogger logger)
        {
            _ = await GenesysApiUtilities.ProcessGenesysContacts(logger, _configuration, _dataLayer, _genesysClientService, Campaigns.NationsMarketEnglish);
        }

        /// <summary>
        /// Genesys Spanish member contacts processor.
        /// </summary>
        /// <param name="req">Request.<see cref="req"/></param>
        /// <param name="_logger">Logger.<see cref="ILogger"/></param>
        /// TODO: update timer trigger expression after manual testing is complete
        [FunctionName("NationsMarketSpanishContactsProcessor")]
        public async Task ProcessNationsMarketSpanishContactsAsync([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest request, ILogger logger)
        {
            _ = await GenesysApiUtilities.ProcessGenesysContacts(logger, _configuration, _dataLayer, _genesysClientService, Campaigns.NationsMarketSpanish);
        }

        #endregion
    }
}
