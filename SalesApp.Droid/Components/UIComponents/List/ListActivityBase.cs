using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using SalesApp.Core.BL.Controllers.Synchronization;
using SalesApp.Core.Events.DownSync;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Search;
using SalesApp.Droid.People.Search;
using SalesApp.Droid.UI;
using Exception = System.Exception;
using SearchView = Android.Support.V7.Widget.SearchView;

namespace SalesApp.Droid.Components.UIComponents.List
{
    public abstract class ListActivityBase<TSearchService, TSearchResult> : ActivityWithNavigation where TSearchService : ISearchService<TSearchResult> where TSearchResult : ISearchResult
    {
        private static readonly ILog Logger = LogManager.Get(typeof (ListActivityBase<TSearchService, TSearchResult>));

        private const string BundledSearchResults = "BundledSearchResults";
        private const string BundledSearchInfo = "BundledSearchInfo";

        private SyncingController _syncingController;

        protected object SearchResults { get; set; }

        public SearchView _searchView;
        
        // layout with refresh icon and actions
        private SwipeRefreshLayout _swipeRefreshLayout;

        protected SwipeRefreshLayout SwipeRefreshLayout
        {
            get { return _swipeRefreshLayout; }
        }

        public SearchView SearchView
        {
            get { return _searchView; }
        }

        public SyncingController SyncController
        {
            get
            {
                _syncingController = _syncingController ?? new SyncingController();
                return _syncingController;
            }
        }

        /// <summary>
        /// Method called when sync has just started
        /// </summary>
        /// <param name="sender">The sender of the updates</param>
        /// <param name="e">Event with data about the sync start</param>
        public void OnSyncStarted(object sender, SyncStartedEventArgs e)
        {
            // Toast.MakeText(this, "Sync started", ToastLength.Long).Show();
        }

        /// <summary>
        /// Method to be called whenever there is an update about the sync progress
        /// </summary>
        /// <param name="sender">The sender of the updates</param>
        /// <param name="e">Event with data about the update</param>
        public void OnUpdateSyncStatus(object sender, SyncStatusEventArgs e)
        {
            // Toast.MakeText(this, "Percent Done = " + e.PercentProcessed, ToastLength.Long).Show();
        }

        /// <summary>
        /// Method to be called when the sync ended successfully
        /// </summary>
        /// <param name="sender">The sender of the updates</param>
        /// <param name="e">Event with data about the sync completion</param>
        public void OnSyncCompleted(object sender, SyncCompleteEventArgs e)
        {
            // Toast.MakeText(this, "Sync finished successfully", ToastLength.Long).Show();
        }

        /// <summary>
        /// Method to be called when sync encountered an error
        /// </summary>
        /// <param name="sender"> The sender of the event</param>
        /// <param name="e">Event with the specific error</param>
        public void OnSyncError(object sender, SyncErrorEventArgs e)
        {
            /* string error = e.Error.Message;
            Toast.MakeText(this, "Sync finished with error: " + error, ToastLength.Long).Show(); */
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.SubscribeToSyncEvents();

            try
            {
                // remove shadow from actionbar, consider moving to base
                this.ActionBar.Elevation = 0;

                // load the refresh item
                this._swipeRefreshLayout = this.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);

                this._swipeRefreshLayout.SetColorSchemeResources(
                    Resource.Color.blue,
                              Resource.Color.white,
                              Resource.Color.gray1,
                              Resource.Color.green);

                this._swipeRefreshLayout.Refresh += async delegate
                {
                    await this.SwipeRefreshHandler();
                };

                this.FindViewById<TextView>(Resource.Id.txtInformationBox).Click += TvInformationClicked;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            this.UnSubscribeToSyncEvents();
        }
            
        /// <summary>
        /// A common place to unsubscribe to down sync events
        /// </summary>
        private void UnSubscribeToSyncEvents()
        {
            this.SyncController.SyncStatusHandler -= this.OnUpdateSyncStatus;
            this.SyncController.SyncStartedHandler -= this.OnSyncStarted;
            this.SyncController.SyncCompleteHandler -= this.OnSyncCompleted;
            this.SyncController.SyncErroredHandler -= this.OnSyncError;
        }

        /// <summary>
        /// A common place to subscribe to down sync events
        /// </summary>
        private void SubscribeToSyncEvents()
        {
            this.SyncController.SyncStatusHandler += this.OnUpdateSyncStatus;
            this.SyncController.SyncStartedHandler += this.OnSyncStarted;
            this.SyncController.SyncCompleteHandler += this.OnSyncCompleted;
            this.SyncController.SyncErroredHandler += this.OnSyncError;
        }

        public void TvInformationClicked(object sender, EventArgs e)
        {
            SearchStates[] allowedStates =
            {
                SearchStates.FailedFetchingOnlineResults,
                SearchStates.SearchLocalOnlyAndFoundZeroResults,
                SearchStates.ShowingLocalResults
            };

            if (!allowedStates.Any(state => state == SearchHelper.SearchInfo.SearchState))
            {
                return;
            }

            Search(SearchHelper.SearchInfo.Query);
        }

        protected async Task SwipeRefreshHandler()
        {
            Logger.Verbose("Refresh list called.");
            _swipeRefreshLayout.Refreshing = true;
            await SynchronizeList();
            RefreshList();
            _swipeRefreshLayout.Refreshing = false;
        }
       
        /// <summary>
        /// This method is called when the user "pulls to synchronize", it refreshes the list depending on activity implementation. 
        /// </summary>
        public abstract Task SynchronizeList();

        /// <summary>
        /// This method refreshes the list with local data
        /// </summary>
        /// <returns></returns>
        public abstract Task RefreshList();

        private SearchHelper<TSearchService, TSearchResult> _searchHelper;

        protected SearchHelper<TSearchService, TSearchResult> SearchHelper
        {
            get
            {
                if (_searchHelper == null)
                {
                    _searchHelper = new SearchHelper<TSearchService, TSearchResult>(this);
                    _searchHelper.SearchStateChanged += _searchHelper_SearchStateChanged;
                }
                return _searchHelper;
            }
        }

        public void Search(string query)
        {
            Task.Run
                (
                    async () =>
                    {
                        SearchResults = await SearchHelper.Search(query);
                        await RefreshList();
                    }
                );
        }

        
        private ViewTreeObserverCompat _searchTreeObserver;

        protected virtual void BuildMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.list_menu, menu);

            var actionSearch = menu.FindItem(Resource.Id.action_search);
            var searchItem = MenuItemCompat.GetActionView(actionSearch);
            _searchView = searchItem.JavaCast<SearchView>();

            _searchTreeObserver = ViewTreeObserverCompat.From(_searchView.ViewTreeObserver);
            _searchTreeObserver.GlobalLayoutCompat += _searchTreeObserver_GlobalLayoutCompat;
            _searchView.SetIconifiedByDefault(true);
            

            var id = Class.ForName("android.support.v7.appcompat.R$id");
            int searchPlateId = (int) id.GetField("search_plate").Get(id);
            int searchSrcTextId = (int) id.GetField("search_src_text").Get(id);
            
            LinearLayout searchPlate = _searchView.FindViewById<LinearLayout>(searchPlateId);
            SearchView.SearchAutoComplete textArea =
                _searchView.FindViewById<SearchView.SearchAutoComplete>(searchSrcTextId);
            if (searchPlate != null)
            {
                searchPlate.SetBackgroundColor(Resources.GetColor(Resource.Color.white));                
            }
            if (textArea != null)
            {
                textArea.SetTextColor(Resources.GetColor(Resource.Color.black));
            }

            if (bundledSearchInformation != null)
            {
                SearchHelper.SetSearchInfo(bundledSearchInformation);
                bundledSearchInformation = null;
            }

            if (!string.IsNullOrEmpty(SearchHelper.SearchInfo.Query) && SearchView != null)
            {
                actionSearch.ExpandActionView();
                SearchView.SetQuery(SearchHelper.SearchInfo.Query, false);
            }
        }

        private readonly object _searchUiLock = new object();

        private void _searchTreeObserver_GlobalLayoutCompat(object sender, EventArgs e)
        {
            lock (_searchUiLock)
            {
                if (SearchHelper.Searching && SearchView.Iconified)
                {
                    SearchHelper.ExitSearch();
                } 
            }
        }

        private void SetInfoState(ViewStates viewState)
        {
            const string black = "000";
            SetInfoState(default(int), black, black, viewState, false);
        }

        private void SetInfoState(int textId, string hexBackColor, string hexForeColor, ViewStates viewState, bool clickable, bool isOnUiThread = false)
        {
            if (!isOnUiThread)
            {
                RunOnUiThread
                    (
                        () =>
                        {
                            SetInfoState(textId, hexBackColor, hexForeColor, viewState, clickable, true);
                        }
                    );
                return;
            }

            hexBackColor = "#" + hexBackColor;
            hexForeColor = "#" + hexForeColor;
            TextView txtInformationBox = FindViewById<TextView>(Resource.Id.txtInformationBox);
            txtInformationBox.Visibility = viewState == ViewStates.Visible ? ViewStates.Visible : ViewStates.Gone;
            if (viewState != ViewStates.Visible)
            {
                return;
            }
            txtInformationBox.SetBackgroundColor(Color.ParseColor(hexBackColor));
            txtInformationBox.SetTextColor(Color.ParseColor(hexForeColor));
            txtInformationBox.Text = GetString(textId);
            txtInformationBox.Clickable = clickable;
        }
        
        private void _searchHelper_SearchStateChanged(object sender, SearchStateChangedEventArgs e)
        {
            switch (e.SearchInfo.SearchState)
            {
                case SearchStates.NotSearching:
                    SearchResults = null;
                    SetInfoState(ViewStates.Invisible);

                    break;
                case SearchStates.ShowingLocalResults:
                    SetInfoState(Resource.String.customer_list_search_local, "c9eea8", "4b8233", ViewStates.Visible,
                        true);
                    break;
                case SearchStates.Searching:
                    if (SwipeRefreshLayout != null)
                    {
                        SwipeRefreshLayout.Refreshing = true;
                    }
                    break;
                case SearchStates.SucceededSearchingOnlineAndFoundResults:
                    SetInfoState(ViewStates.Invisible);
                    break;
                case SearchStates.FailedFetchingOnlineResults:
                    SetInfoState(Resource.String.cust_search_error, "ffb301", "fff6f5", ViewStates.Visible, true);
                    break;
                case SearchStates.SearchLocalOnlyAndFoundZeroResults:
                    SetInfoState(Resource.String.cust_search_device_only, "c9eea8", "4b8233", ViewStates.Visible,
                        true);
                    break;
                case SearchStates.SucceededSearchingOnlineButFoundNoResults:
                    SetInfoState(Resource.String.customer_list_no_results_online, "c9eea8", "4b8233", ViewStates.Visible,
                        true);
                    break;
            }
            if (e.SearchInfo.SearchState != SearchStates.Searching && SwipeRefreshLayout != null)
            {
                SwipeRefreshLayout.Refreshing = false;
            }
        }

        public override void OnBackPressed()
        {
            if (SearchHelper.Searching)
            {
                SearchHelper.ExitSearch();
                return;
            }

            base.OnBackPressed();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            if (SearchHelper.Searching)
            {
                if (SearchResults != null)
                {
                    outState.PutString(BundledSearchResults, JsonConvert.SerializeObject(SearchResults));
                    outState.PutString(BundledSearchInfo, JsonConvert.SerializeObject(SearchHelper.SearchInfo));
                }
            }
            base.OnSaveInstanceState(outState);
        }


        private SearchInformation bundledSearchInformation = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SearchHelper.SearchExited += SearchHelper_SearchExited;
            if (savedInstanceState != null)
            {
                var bundledResults = savedInstanceState.GetString(BundledSearchResults);
                if (bundledResults != null)
                {
                    SearchResults = JsonConvert.DeserializeObject<List<TSearchResult>>(bundledResults);
                    string bundledSearchInfo = savedInstanceState.GetString(BundledSearchInfo);
                    bundledSearchInformation = JsonConvert.DeserializeObject<SearchInformation>(bundledSearchInfo);
                    SearchHelper.Searching = true;
                }
            }
        }

        private void SearchHelper_SearchExited(object sender, EventArgs e)
        {
            Task.Run
                (
                    async () =>
                    {
                        await RefreshList();
                    }
                );
        }
    }
}