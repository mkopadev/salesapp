using System;

namespace SalesApp.Droid.People.Search
{
    public class SearchStateChangedEventArgs : EventArgs
    {
        public bool Searching { get; private set; }
        public SearchStateChangedEventArgs(SearchInformation searchInfo)
        {
            SearchInfo = searchInfo;
            Searching = searchInfo.SearchState != SearchStates.NotSearching;
        }

        public SearchInformation SearchInfo { get; set; }
    }
}