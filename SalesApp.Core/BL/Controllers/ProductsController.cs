using SalesApp.Core.Services.Database;

namespace SalesApp.Core.BL.Controllers
{
    public class ProductsController : SQLiteDataService<Product>
    {
    }
}