using System.Threading.Tasks;
using SalesApp.Core.BL.Models.People;

namespace SalesApp.Core.BL.Managers
{
    public interface ISalesAppManager
    {
        //Task<User> Login(string pin, bool isFirstTime);

        /// <summary>
        /// Checks the status of a person (prospect or customer). 
        /// </summary>
        /// <param name="phone">The phone.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">phone</exception>
        Task<Person> CheckPerson(string phone);
        Task<Status> RegisterProspect(Prospect prospect);
        Task<Status> RegisterCustomer(Customer customer);
        //List<Product> ListProducts();
        //List<Message> ListMessages();
    }
}