namespace SalesApp.Core.Services.Interfaces
{
    public interface IStorageService
    {
        string GetPathForFileAsync(string file);
    }
}
