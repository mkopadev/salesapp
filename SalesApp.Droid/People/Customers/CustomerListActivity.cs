using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using MK.Solar.People.Prospects;
using Mkopa.Core.Api.DownSync;
using Mkopa.Core.Api.Person;
using Mkopa.Core.BL.Controllers.People;
using Mkopa.Core.BL.Models.People;
using Mkopa.Core.Extensions;
using Mkopa.Core.Services.Connectivity;
using Mkopa.Core.Services.DependancyInjection;
using Mkopa.Core.Services.Person.Customer;
using Mkopa.Core.Services.Settings;
using Newtonsoft.Json;

namespace MK.Solar.People.Customers
{
    [Activity(NoHistory = false, ParentActivity = typeof(WelcomeActivity))]
    class CustomerListActivity : PersonListActivity<CustomerItem, CustomerSearchService, CustomerSearchResult>
    {
        public const string CustomerStatusBundled = "CustomerStatusBundled";
        public const string BundledStatuses = "BundledStatuses";
        private const string BundledCustomerId = "BundledCustomerId";

        private CustomersController _customersController;

        public CustomersController CustomersController
        {
            get
            {
                _customersController = _customersController ?? new CustomersController();
                return _customersController;
            }
        }

        private CustomerStatusApi _custsStatusApi;

        private CustomerRegistrationStepsStatusController _custRegistrationStepsStatusController;
        
        private CustomerStatusApi CustsStatusApi
        {
            get
            {
                _custsStatusApi = _custsStatusApi ??
                                  new CustomerStatusApi();
                return _custsStatusApi;
            }
        }

        private CustomerRegistrationStepsStatusController CustRegistrationStepsStatusController
        {
            get
            {
                _custRegistrationStepsStatusController = _custRegistrationStepsStatusController ??
                                                         new CustomerRegistrationStepsStatusController();
                return _custRegistrationStepsStatusController;
            }
        }

        private Settings _settings;

        public override IntentStartPointTracker.IntentStartPoint IntentStartPoint
        {
            get { return IntentStartPointTracker.IntentStartPoint.CustomerList; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            PersonSectionAdapter = new CustomerListAdapter(this);
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                string statusesString = savedInstanceState.GetString(BundledStatuses);
                if (!statusesString.IsBlank())
                {
                    _statuses = JsonConvert.DeserializeObject<Dictionary<Guid, CustomerStatus>>(statusesString);
                }

                string custId = savedInstanceState.GetString(BundledCustomerId);
                if (!custId.IsBlank())
                {
                    _currentCustomerId = new Guid(custId);
                }
            }
            SetScreenTitle(Resource.String.customer_list_title);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (SearchHelper.Searching)
                    {
                        SearchHelper.ExitSearch();
                    }
                    else
                    {
                        base.OnOptionsItemSelected(item);
                    }
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        /*public override void OnBackPressed()
        {
            if (SearchHelper.Searching)
            {
                SearchHelper.ExitSearch();
                return;
            }
            base.OnBackPressed();
        }*/

        protected override void OnResume()
        {
            base.OnResume();
            /*if (!SearchHelper.Searching)
            {
                SwipeRefreshLayout.Post(async () =>
                {
                    await RefreshDataAndScreen();
                });
            }*/
        }

        public override void SetViewPermissions()
        {
            
        }

        private Dictionary<Guid,CustomerStatus> _statuses = new Dictionary<Guid, CustomerStatus>();
        private Guid _currentCustomerId;

        public override void AdapterItemClick(ListView sender, AdapterView.ItemClickEventArgs args)
        {
            PersonSectionListAdapter<CustomerItem> adapter = sender.Adapter as PersonSectionListAdapter<CustomerItem>;
            if (adapter != null)
            {
                try
                {
                     var item = adapter.GetItem(args.Position).ToNetObject<CustomerItem>();
                    _currentCustomerId = item.SearchResult.Id;
                    if (_statuses.ContainsKey(_currentCustomerId))
                    {
                        item.SearchResult.Status = _statuses[_currentCustomerId];
                    }

                     Intent intent = new Intent(this, typeof(CustomerDetailActivity));
                     intent.PutExtra(
                             CustomerDetailActivity.BundledCustomer,
                             JsonConvert.SerializeObject(item.SearchResult));
                    StartActivityForResult(intent, 0);
                }
                catch (Exception e)
                {
                    Logger.Error("Clicked item can not be rendered as a CustomerItem");
                    return;
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (_currentCustomerId == default(Guid))
                {
                    return;
                }
                if (_statuses == null || _statuses.ContainsKey(_currentCustomerId))
                {
                    return;
                }
                if (data == null)
                {
                    return;
                }
                string results = data.GetStringExtra(CustomerStatusBundled);
                if (results == null)
                {
                    return;
                }
                if (!results.IsBlank())
                {
                    _statuses.Add(_currentCustomerId, JsonConvert.DeserializeObject<CustomerStatus>(results));
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        public override async Task SynchronizeList()
        {
            try
            {
                if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
                {
                    ShowAlertNoInternet();
                    return;
                }

                EnableList(false);

                Logger.Debug("Uploading list...");
                await UploadCustomerList();

                // Get customer from the server
                await GetCustomerFromServer();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                ShowAlertSynchronizeFailed();
            }
            finally
            {
                EnableList(true);
            }

            this.HideSnackbar();
        }

        private async Task UploadCustomerList()
        {
            try
            {
                Logger.Debug("Getting list of customers for syncing");
                List<Customer> customers = await CustomersController.GetAllAsync();
                if (customers == null)
                {
                    return;
                }
                foreach (var cust in customers)
                {
                    Logger.Debug("Getting sync status of product for customer with id " + cust.Id);

                    CustomerStatus status = await CustsStatusApi.GetAsync(cust.Phone);
                    Logger.Debug("Getting the status was not as problem");
                    if (status != null)
                    {
                        Logger.Debug("Saving sync status");
                        /*if (status.AccountStatus.Equals("Rejected"))
                        {
                            cust.AccountStatus = "Rejected";
                            CustomersController.SaveAsync(cust, true);
                        }*/
                        await CustRegistrationStepsStatusController.SaveAsync(status, cust);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                ShowAlertSynchronizeFailed();
            }
       }

        private async Task GetCustomerFromServer()
            {
            await this.SyncController.SyncDownAsync<DownSyncServerResponse<Customer>, Customer>();
        }

        
        private List<CustomerItem> customerItems = null;
       
        private void UpdateDataSet(bool isOnUiThread = false)
        {
            if (!isOnUiThread)
            {
                RunOnUiThread
                    (
                        () =>
                        {
                            UpdateDataSet(true);
                        }
                    );
                return;
            }
            PersonSectionAdapter.ClearSections();
            SetNewAdapter<CustomerItem>(new CustomerListAdapter(this));

            List<CustomerSearchResult> customerSearch = SearchResults as List<CustomerSearchResult>;;

           
            if (customerSearch == null)
            {
                customerSearch = new List<CustomerSearchResult>();
            }
            customerItems = new List<CustomerItem>();

            
            // check if there are customers before search filter
            bool noCustomers = customerSearch.Count == 0;

            customerSearch = customerSearch
                .OrderByDescending(cust => cust.DateCreated).ToList();

            Logger.Debug("Refreshing list before doing the task run");


            foreach (var customer in customerSearch)
            {
                
                Logger.Debug("Adding new item to adapter");


                CustomerItem customerItem = new CustomerItem(customer);

                customerItems.Add(customerItem);
            }

            Logger.Debug("Adding stuff to sections: " + customerItems.Count);

            AddToSection(customerItems.Where(item => !item.SearchResult.ProductIsActive).ToList(),
                Resource.String.customer_list_title_in_progress,
                Resource.Color.red);

            AddToSection(customerItems.Where(item => item.SearchResult.ProductIsActive).ToList(), 
                Resource.String.customer_list_title_active,
                Resource.Color.green);

            AddToSection(customerItems.Where(item => item.SearchResult.IsRejected).ToList(), 
                Resource.String.customer_rejected,
                Resource.Color.red);
            
            Logger.Debug("Finished loading stuff into adapter");
            PersonSectionAdapter.NotifyDataSetChanged();

            // show textview if no customers
            if (noCustomers)
            {
               
                if (SearchHelper.Searching)
                {
                    this.HideSnackBar();
                }
                else
                {
                // ShowNoSearchResults(GetString(Resource.String.no_customers));
                if (this.ConnectedToNetwork)
                {
                    this.ShowSnackBar(Resource.String.customers_refresh_prompt);
                }
                else
                {
                    this.ShowSnackBar(Resource.String.customers_refresh_prompt_no_internet);
                }
            }
            }
            else
            {
                this.HideSnackBar();
            }
            }


        public override async Task RefreshList()
        {
            if (SearchHelper.Searching == false)
            {
                SearchResults = await new CustomerSearchService().GetAllLocalAsync();
            }
            UpdateDataSet();
        }

        private void AddToSection(List<CustomerItem> customerItems,int statusId,int color)
        {
            if (customerItems == null || customerItems.Count == 0)
            {
                return;
            }
            string statusString = GetString(statusId);

            
            CustomerItemListAdapter itemListAdapter = new CustomerItemListAdapter
                (
                    this
                    , customerItems
                );
            Logger.Debug("Adding item to adapter in section ~".GetFormated(statusString));
            PersonSectionAdapter.AddSection(statusString,itemListAdapter,color);
        }

        public override void RetrieveScreenInput()
        {
        }

        public override bool Validate()
        {
            // do nothing
            return true;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(BundledStatuses,JsonConvert.SerializeObject(_statuses));
            outState.PutString(BundledCustomerId,_currentCustomerId.ToString());
            base.OnSaveInstanceState(outState);
        }
    }
}