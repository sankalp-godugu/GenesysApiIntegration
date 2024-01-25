using GenesysContactsProcessJob.Model.Request;
using GenesysContactsProcessJob.Model.Response;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenesysContactsProcessJob.GenesysLayer.Interfaces
{
    /// <summary>
    /// Interface for 
    /// </summary>
    public interface IGenesysClientService
    {
        public Task<GetContactListExportUriResponse> GetContactListFromCsv(string filePath);

        /// <summary>
        /// Initiate contact list export in Genesys asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Genesys.</returns>
        public Task<InitiateContactListExportResponse> InitiateContactListExport(string lang, ILogger logger);

        /// <summary>
        /// Get contact list export Uri from Genesys asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Genesys.</returns>
        public Task<GetContactListExportUriResponse> GetContactListExportUri(string lang, ILogger logger);

        /// <summary>
        /// Get contact list download Url from Genesys asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Genesys.</returns>
        public Task<GetContactListDownloadUrlResponse> GetContactListDownloadUrl(string uri, string lang, ILogger logger);

        /// <summary>
        /// Get list of contacts from Genesys asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Genesys.</returns>
        public Task<IEnumerable<GetContactListResponse>> GetContactList(string lang, ILogger logger);

        /// <summary>
        /// Adds list of contacts in Genesys asychronously.
        /// </summary>
        /// <param name="contactsToAddToGenesys">Contacts To Add.<see cref="ContactsToAdd"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of added contacts in Genesys.</returns>
        public Task<IEnumerable<PostContactListResponse>> AddContactsToContactList(IEnumerable<PostContactsRequest> contactsToAddToGenesys, string lang, ILogger logger);

        /// <summary>
        /// Updates list of contacts in Genesys.
        /// </summary>
        /// <param name="contactsToUpdateInGenesys">Contacts To Update.<see cref="ContactsToUpdate"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of updated contacts in Genesys.</returns>
        public Task<IEnumerable<PostContactListResponse>> UpdateContactsInContactList(IEnumerable<PostContactsRequest> contactsToUpdateInGenesys, string lang, ILogger logger);

        /// <summary>
        /// Updates list of contacts in Genesys.
        /// </summary>
        /// <param name="contactToUpdateInGenesys">Contacts To Update.<see cref="ContactToUpdate"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of updated contacts in Genesys.</returns>
        public Task<PostContactListResponse> UpdateContactInContactList(PostContactsRequest contactToUpdateInGenesys, string lang, ILogger logger);

        /// <summary>
        /// Deletes list of contacts in Genesys asychronously.
        /// </summary>
        /// <param name="contactsToRemoveFromGenesys">Contacts To Delete.<see cref="ContactsToDelete"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns a 200 response on successful deletion in Genesys.</returns>
        public Task<long> DeleteContactsFromContactList(IEnumerable<string> contactsToRemoveFromGenesys, string lang, ILogger logger);
    }
}
