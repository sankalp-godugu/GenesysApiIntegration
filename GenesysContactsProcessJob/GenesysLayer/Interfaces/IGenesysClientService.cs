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
        public Task<IEnumerable<GetContactsExportDataFromGenesysResponse>> GetListFromCsv(string filePath);

        /// <summary>
        /// Initiate contact list export in Genesys asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Genesys.</returns>
        public Task<InitiateContactListExportResponse> InitiateContactListExport(string lang, ILogger logger);

        /// <summary>
        /// Get list of contacts from Genesys asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Genesys.</returns>
        public Task<IEnumerable<GetContactsResponse>> GetContactsFromContactList(IEnumerable<GetContactsRequest> contactsToGetFromGenesys, string lang, ILogger logger);

        /// <summary>
        /// Get list of contacts from export from Genesys asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of contacts from Genesys.</returns>
        public Task<IEnumerable<GetContactsExportDataFromGenesysResponse>> GetContactsFromContactListExport(string lang, ILogger logger);

        /// <summary>
        /// Adds list of contacts in Genesys asychronously.
        /// </summary>
        /// <param name="contactsToAddToGenesys">Contacts To Add.<see cref="ContactsToAdd"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of added contacts in Genesys.</returns>
        public Task<IEnumerable<PostContactsToGenesysResponse>> AddContactsToContactList(IEnumerable<PostContactsRequest> contactsToAddToGenesys, string lang, ILogger logger);

        /// <summary>
        /// Updates list of contacts in Genesys.
        /// </summary>
        /// <param name="contactsToUpdateInGenesys">Contacts To Update.<see cref="ContactsToUpdate"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of updated contacts in Genesys.</returns>
        public Task<IEnumerable<PostContactsToGenesysResponse>> UpdateContactsInContactList(IEnumerable<PostContactsRequest> contactsToUpdateInGenesys, string lang, ILogger logger);

        /// <summary>
        /// Updates list of contacts in Genesys.
        /// </summary>
        /// <param name="contactToUpdateInGenesys">Contacts To Update.<see cref="ContactToUpdate"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of updated contacts in Genesys.</returns>
        public Task<PostContactsToGenesysResponse> UpdateContactInContactList(PostContactsRequest contactToUpdateInGenesys, string lang, ILogger logger);

        /// <summary>
        /// Deletes list of contacts in Genesys asychronously.
        /// </summary>
        /// <param name="contactsToRemoveFromGenesys">Contacts To Delete.<see cref="ContactsToDelete"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns a 200 response on successful deletion in Genesys.</returns>
        public Task<long> DeleteContactsFromContactList(IEnumerable<string> contactsToRemoveFromGenesys, string lang, ILogger logger);
    }
}
