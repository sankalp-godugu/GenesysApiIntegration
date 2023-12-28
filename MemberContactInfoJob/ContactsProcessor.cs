using AutoMapper;
using GenesysContactsProcessJob.DataLayer.Interfaces;
using GenesysContactsProcessJob.GenesysLayer.Interfaces;
using GenesysContactsProcessJob.Model.DTO;
using GenesysContactsProcessJob.Model.Request;
using GenesysContactsProcessJob.Model.Response;
using GenesysContactsProcessJob.TriggerUtilities;
using GenesysContactsProcessJob.Utilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
        /// Genesys English member contacts processor.
        /// </summary>
        /// <param name="req">Request.<see cref="req"/></param>
        /// <param name="_logger">Logger.<see cref="ILogger"/></param>
        [FunctionName("AetnaEnglishContactsProcessor")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        public async Task ProcessEnglishContactsAsync([TimerTrigger("0 12 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger logger)
        {
            _ = await GenesysApiUtilities.ProcessGenesysContacts(logger, _configuration, _dataLayer, _genesysClientService);
        }
        public static byte[] ReadFully(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;


            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;


                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();


                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }


                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
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
            //_ = await GenesysApiUtilities.ProcessGenesysContacts(logger, _configuration, _dataLayer, _genesysClientService);
        }

        private IEnumerable<AddContactsRequest> Map(IEnumerable<GenesysMemberContactInfo> dqr)
        {
            //var config = new MapperConfiguration(cfg => cfg.CreateMap<IEnumerable<DatabaseQueryResult>, List<AddContactsRequest>>());
            //var mapper = new Mapper(config);
            Mapper mapper = MapperConfig.InitializeAutomapper();
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
