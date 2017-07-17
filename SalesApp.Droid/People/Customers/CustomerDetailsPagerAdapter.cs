using Android.OS;
using Android.Support.V4.App;
using Java.Lang;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;
using SalesApp.Droid.Views.TicketList.Customer;

namespace SalesApp.Droid.People.Customers
{
    /// <summary>
    /// Pager adapter for the customer details pager
    /// </summary>
    public class CustomerDetailsPagerAdapter : FragmentPagerAdapter
    {
        /// <summary>
        /// Represents the position of the customer registration info fragment
        /// </summary>
        public const int CustomerRegistrationInfoFragment = 0;

        /// <summary>
        /// Represents the position of the customer tickets fragment
        /// </summary>
        public const int CustomerTicketsFragmentPage = 1;

        /// <summary>
        /// Represents the position of the customer tickets fragment
        /// </summary>
        public const int CustomerPhotosFragmentPage = 2;

        /// <summary>
        /// The customer details to display in the 
        /// </summary>
        private CustomerSearchResult customerDetails;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerDetailsPagerAdapter"/> class
        /// </summary>
        /// <param name="fm">The fragment manager</param>
        /// <param name = "customerDetails" > The customer details</param>
        public CustomerDetailsPagerAdapter(FragmentManager fm, CustomerSearchResult customerDetails) : base(fm)
        {
            this.customerDetails = customerDetails;
        }

        /// <summary>
        /// Gets the number of pages (tabs)
        /// </summary>
        public override int Count
        {
            get
            {
                return 3;
            }
        }

        /// <summary>
        /// Returns a fragment given a position
        /// </summary>
        /// <param name="position">The position</param>
        /// <returns>Returns the fragment</returns>
        public override Fragment GetItem(int position)
        {
            string json = JsonConvert.SerializeObject(this.customerDetails);
            switch (position)
            {
                case CustomerTicketsFragmentPage:
                    CustomerTicketsFragment customerTicketsFragment = new CustomerTicketsFragment();
                    customerTicketsFragment.SetArgument(CustomerTicketsFragment.CustomerBundleKey, this.customerDetails);
                    return customerTicketsFragment;
                case CustomerRegistrationInfoFragment:
                    CustomerDetailFragment customerDetailFragment = new CustomerDetailFragment();
                    Bundle registrationInfoBundle = new Bundle();
                    registrationInfoBundle.PutString(CustomerDetailFragment.SearchResult, json);
                    customerDetailFragment.Arguments = registrationInfoBundle; 
                    return customerDetailFragment;
                case CustomerPhotosFragmentPage:
                    FragmentCustomerPhotos customerPhotosFragment =  new FragmentCustomerPhotos();
                    Bundle photosBundle = new Bundle();
                    photosBundle.PutString(FragmentCustomerPhotos.CustomerNationalIdBundleKey, this.customerDetails.NationalId);
                    photosBundle.PutString(FragmentCustomerPhotos.CustomerPhoneBundleKey, this.customerDetails.Phone);
                    customerPhotosFragment.Arguments = photosBundle;
                    return customerPhotosFragment;
            }

            throw new IllegalArgumentException(string.Format("I, {0} dont know what fragment to return for page {1}", this.GetType().FullName, position));
        }

        /// <summary>
        /// Returns the title to show for the given tab
        /// </summary>
        /// <param name="position">The tabs position</param>
        /// <returns>The title</returns>
        public override ICharSequence GetPageTitleFormatted(int position)
        {
            switch (position)
            {
                case CustomerTicketsFragmentPage:
                    return new String("Tickets");
                case CustomerRegistrationInfoFragment:
                    return new String("Registration");
                case CustomerPhotosFragmentPage:
                    return new String("Photos");
            }

            return base.GetPageTitleFormatted(position);
        }
    }
}