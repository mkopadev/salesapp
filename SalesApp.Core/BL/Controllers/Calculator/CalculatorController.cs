using SalesApp.Core.Services.Database;

namespace SalesApp.Core.BL.Controllers.Calculator
{
    public class CalculatorController : SQLiteDataService<Models.Calculator.Calculator>
    {
    }
}