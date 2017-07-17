using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Search;

namespace SalesApp.Droid.People.Search
{
    public class SearchHelper<TSearchService,TSearchResult>  
        where TSearchService : ISearchService<TSearchResult> 
        where TSearchResult : ISearchResult
    {
        public event EventHandler<SearchStateChangedEventArgs> SearchStateChanged;
        public event EventHandler SearchExited; 

        private SearchInformation _searchInfo;
        
        private readonly ILog _logger;

        public bool Searching { get; set; }
        

        public SearchHelper(Activity context)
        {
            Context = context;
            SetSearchState(SearchStates.NotSearching);
            _logger = Resolver.Instance.Get<ILog>();
        }


        public void SetSearchInfo(SearchInformation searchInfo)
        {
            this.SearchInfo = searchInfo;
            if (searchInfo == null)
            {
                return;
            }
            SetSearchState(searchInfo.SearchState);
        }
       

        private TextView TvInfo { get; set; }

        private Activity Context { get; set; }

        public void ExitSearch()
        {
            SearchInfo.Query = "";
           
            SetSearchState(SearchStates.NotSearching);
            if (SearchExited != null)
            {
                SearchExited(this, EventArgs.Empty);
            }
        }

        public async Task<List<TSearchResult>> Search(string query)
        {
            try
            {
                SearchInfo.Query = query;
                SetSearchState(SearchStates.Searching);
                TSearchService searchService = Activator.CreateInstance<TSearchService>();
                var results = await searchService.SearchAsync(query,true);
                SearchedOnline = searchService.SearchedOnline;
                if (SearchedOnline)
                {
                    if (results.Count > 0)
                    {
                        SetSearchState(SearchStates.SucceededSearchingOnlineAndFoundResults);
                    }
                    else
                    {
                        SetSearchState(SearchStates.SucceededSearchingOnlineButFoundNoResults);
                    }
                }
                else
                {
                    if (results.Count == 0)
                    {
                        SetSearchState(SearchStates.SearchLocalOnlyAndFoundZeroResults);
                    }
                    else
                    {
                        SetSearchState(SearchStates.ShowingLocalResults);
                    }
                }
                return results;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                SetSearchState(SearchStates.FailedFetchingOnlineResults);
            }
            return new List<TSearchResult>();

        }

        private void SetSearchState(SearchStates searchState, bool isOnUiThread = false)
        {
            if (Context == null)
            {
                return;
            }
            if (!isOnUiThread)
            {
                Context.RunOnUiThread
                    (
                        () => SetSearchState(searchState, true)
                    );
                return;
            }
            SearchInfo.SearchState = searchState;
            SearchStateChangedEventArgs searchStateChangedEventArgs = new SearchStateChangedEventArgs(SearchInfo);
            
            Searching = searchStateChangedEventArgs.Searching;
            
            if (SearchStateChanged != null)
            {
                SearchStateChanged(this,searchStateChangedEventArgs);
            }

        }

        public bool SearchedOnline { get; private set; }

        private ILog Logger
        {
            get { return _logger; }
        }

        public SearchInformation SearchInfo
        {
            get
            {
                if (_searchInfo == null)
                {
                    _searchInfo = new SearchInformation("",SearchStates.NotSearching);
                }
                return _searchInfo;
            }
            set { _searchInfo = value; }
        }
    }
}