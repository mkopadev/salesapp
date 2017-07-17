using System.Threading.Tasks;

namespace SalesApp.Droid.Views.Modules.Calculator
{
    public class CalculatorFetchProducts
    {
        public string Product { set; get; }

        public static CalculatorFetchProducts Instance = new CalculatorFetchProducts();
        private CalculatorFetchProducts()
        {
        }

        public async Task FetchProducts(string _params)
        {
            string products = string.Empty;

            if (string.IsNullOrWhiteSpace(products))
            {

                Product = string.Empty;

            }
            else
            {
                Product = products;
            }
        }

    }
}