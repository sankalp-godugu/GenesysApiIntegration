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
        /// <returns>Returns the ticket id of the created zendesk.</returns>
        public Task<IEnumerable<GetContactsResponse>> GetContactsFromContactList(ILogger logger);

        /// <summary>
        /// Creates the CMT ticket in zendesk asychronously.
        /// </summary>
        /// <param name="contactsToAdd">Contacts To Add.<see cref="ContactsToAdd"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id of the created zendesk.</returns>
        public Task<IEnumerable<AddContactsResponse>> AddContactsToContactList(IEnumerable<PostDischargeInfoPlusGenesys> contactsToAdd, ILogger logger);

        /// <summary>
        /// Update the CMT ticket in zendesk.
        /// </summary>
        /// <param name="contactsToUpdate">Contacts To Update.<see cref="ContactsToUpdate"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id from the zendesk.</returns>
        public Task<IEnumerable<UpdateContactsResponse>> UpdateContactsInContactList(IEnumerable<PostDischargeInfoPlusGenesys> contactsToUpdate, ILogger logger);

        /// <summary>
        /// Creates the admin ticket in zendesk asychronously.
        /// </summary>
        /// <param name="contactsToDelete">Contacts To Delete.<see cref="ContactsToDelete"/></param>
        /// <param name="logger">Logger.<see cref="ILogger"/></param>
        /// <returns>Returns the ticket id of the created zendesk.</returns>
        public Task<long> DeleteContactsFromContactList(IEnumerable<PostDischargeInfoPlusGenesys> contactsToDelete, ILogger logger);
    }
}
