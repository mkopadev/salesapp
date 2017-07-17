namespace SalesApp.Droid.People.Search
{
    public class SearchInformation
    {
        public SearchInformation(string query, SearchStates searchState)
        {
            Query = query;
            SearchState = searchState;
        }

        public string Query { get; set; }

        public SearchStates SearchState { get; set; }
    }
}