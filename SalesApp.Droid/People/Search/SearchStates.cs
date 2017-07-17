namespace SalesApp.Droid.People.Search
{
    public enum SearchStates
    {
        NotSearching = 1
        ,ShowingLocalResults = 2
        ,Searching = 3
        ,SucceededSearchingOnlineAndFoundResults = 4
        ,FailedFetchingOnlineResults = 5
        ,SearchLocalOnlyAndFoundZeroResults = 6
        ,SucceededSearchingOnlineButFoundNoResults = 7
        
   }
}