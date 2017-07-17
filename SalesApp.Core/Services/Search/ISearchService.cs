using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesApp.Core.Services.Search
{
    public interface ISearchService<TResult> where TResult : ISearchResult
    {
        bool SearchedOnline { get; }

        Task<List<TResult>> GetAllLocalAsync();

        Task<List<TResult>> SearchAsync(string queryString, bool alsoSearchOnline);
    }
}