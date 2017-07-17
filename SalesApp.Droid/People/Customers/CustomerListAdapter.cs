using Android.App;
using SalesApp.Core.BL.Models.People;

namespace SalesApp.Droid.People.Customers
{
    class CustomerListAdapter : PersonSectionListAdapter<CustomerItem>
    {
        public CustomerListAdapter(Activity activity) : base(activity)
        {
        }
    }

    public class CustomerItem :  IPersonItem  
    {
        public CustomerItem()
        {
        }

        public CustomerItem(CustomerSearchResult customerSearchResult)
        {
            SearchResult = customerSearchResult;
        }

        public CustomerSearchResult SearchResult { get;  set; }

        public string GetFilterString()
        {
            return SearchResult.FullName + SearchResult.Phone;
        }
    }
}