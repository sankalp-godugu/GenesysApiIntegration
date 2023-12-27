using GenesysContactsProcessJob.Model.DTO;
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
        /// <summary>
        /// Get list of contacts from Genesys asychronously.
        /// </summary>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id of the created Genesys.</returns>
        public Task<IEnumerable<GetContactsResponse>> GetContactsFromContactList(IEnumerable<PostDischargeInfoPlusGenesys> contactsToAdd, ILogger logger);

        /// <summary>
        /// Adds list of contacts in Genesys asychronously.
        /// </summary>
        /// <param name="contactsToAdd">Contacts To Add.<see cref="ContactsToAdd"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of added contacts in Genesys.</returns>
        public Task<IEnumerable<AddContactsResponse>> AddContactsToContactList(IEnumerable<PostDischargeInfoPlusGenesys> contactsToAdd, ILogger logger);

        /// <summary>
        /// Updates list of contacts in Genesys.
        /// </summary>
        /// <param name="contactsToUpdate">Contacts To Update.<see cref="ContactsToUpdate"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the list of tickets updated in Genesys.</returns>
        public Task<IEnumerable<UpdateContactsResponse>> UpdateContactsInContactList(IEnumerable<PostDischargeInfoPlusGenesys> contactsToUpdate, ILogger logger);

        /// <summary>
        /// Deletes list of contacts in Genesys asychronously.
        /// </summary>
        /// <param name="contactsToDelete">Contacts To Delete.<see cref="ContactsToDelete"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns a 200 response on successful deletion in Genesys.</returns>
        public Task<long> DeleteContactsFromContactList(IEnumerable<long> contactsToDelete, ILogger logger);
    }
}
