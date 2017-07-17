using System.Threading.Tasks;
using Mkopa.Core.Enums.Api;

namespace Mkopa.Core.BL.Models.People
{

    public class AdditionalProductService : IAdditionalProductService
    {
        public Task RegisterAdditionalProduct(Customer customer)
        {

            throw new System.NotImplementedException();
        }
    }

    public interface IAdditionalProductService
    {
        Task RegisterAdditionalProduct(Customer customer);
    }
}