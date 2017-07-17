using System.Threading.Tasks;

namespace SalesApp.Core.ViewModels.Dialog
{
    public interface IDialogService
    {
        Task<bool?> ShowAsync(string message, string dontCancel, string cancelButtonContent);
    }
}