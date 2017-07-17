using System.Threading.Tasks;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Search;
using SalesApp.Droid.Components.UIComponents.List;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.Tickets;
using SalesApp.Droid.UI.Wizardry;
using SearchView = Android.Support.V7.Widget.SearchView;

namespace SalesApp.Droid.People
{
    /// <summary>
    /// An abstract class to show a list of models of type person
    /// </summary>
    /// <typeparam name="TListItemType">The list item type</typeparam>
    /// <typeparam name="TSearchService">The service to use for searching</typeparam>
    /// <typeparam name="TSearchResult">The result type that the service returns</typeparam>
    public abstract class PersonListActivity<TListItemType, TSearchService, TSearchResult> : ListActivityBase<TSearchService, TSearchResult>, AbsListView.IOnScrollListener where TListItemType : IPersonItem
        where TSearchService : ISearchService<TSearchResult>
        where TSearchResult : ISearchResult
    {
        /// <summary>
        /// Logger for this class 
        /// </summary>
        private static readonly ILog Logger = LogManager.Get(typeof(PersonListActivity<TListItemType, TSearchService, TSearchResult>));

        /// <summary>
        /// The list view
        /// </summary>
        protected ListView PersonListView;

        /// <summary>
        /// A warning text box
        /// </summary>
        private TextView txtWarningBox;

        /// <summary>
        /// An informational text box
        /// </summary>
        private TextView txtInformationBox;

        /// <summary>
        /// Note to user's to pull to refresh when the list is empty
        /// </summary>
        private Snackbar pullToRefresh;

        /// <summary>
        /// Raise issue button
        /// </summary>
        protected TextView RaiseIssue;

        /// <summary>
        /// Add prospect button
        /// </summary>
        protected TextView AddProspect;

        /// <summary>
        /// Create customer button
        /// </summary>
        protected TextView CreateCustomer;

        /// <summary>
        /// Gets the start point
        /// </summary>
       public abstract IntentStartPointTracker.IntentStartPoint IntentStartPoint { get; }

        /// <summary>
        /// Gets or sets the section adapter for the list view
        /// </summary>
        public PersonSectionListAdapter<TListItemType> PersonSectionAdapter { get; set; }

        public TextView TxtWarningBox
        {
            get { return this.txtWarningBox; }
        }

        public TextView TxtInformationBox
        {
            get { return this.txtInformationBox; }
        }

        public bool LastItemIsAtBottom
        {
            get
            {
                int lastVisibleIndex = GetIndex();

                View lastView = this.PersonListView.GetChildAt(lastVisibleIndex);

                if (lastView == null)
                {
                    return false;
                }

                ViewGroup.MarginLayoutParams listMargins = this.PersonListView.LayoutParameters as ViewGroup.MarginLayoutParams;

                if (listMargins == null)
                {
                    return lastView.Bottom == this.BottomToolbar.Bottom;
                }

                int verticalMargins = listMargins.BottomMargin + listMargins.TopMargin;

                bool viewIsAtBottom = lastView.Bottom + verticalMargins == this.BottomToolbar.Bottom;

                return this.IsLastItemVisible && viewIsAtBottom;
            }
        }

        public bool IsLastItemVisible
        {
            get
            {
                if (this.PersonListView == null || this.PersonListView.Adapter == null)
                {
                    return false;
                }

                bool value = this.PersonListView.LastVisiblePosition == this.PersonListView.Adapter.Count - 1;
                return value;
            }
        }

        protected LinearLayout BottomToolbar { get; private set; }

        private int GetIndex()
        {
            return this.PersonListView.LastVisiblePosition - this.PersonListView.FirstVisiblePosition;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.layout_list_person);
            
            this.InitializeScreen();
            this.SetListeners();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (this.SearchHelper.Searching)
            {
                this.UpdateScreen();
                return;
            }

            Task.Run(async () => { this.UpdateScreen(); });
        }

        public override void InitializeScreen()
        {
            this.PersonListView = this.FindViewById<ListView>(Resource.Id.person_list);
            this.PersonListView.SetOnScrollListener(this);
            this.PersonListView.SetSelector(Resource.Drawable.listselector);
            this.PersonListView.Adapter = PersonSectionAdapter;

            this.txtWarningBox = this.FindViewById<TextView>(Resource.Id.txtWarningBox);
            this.txtInformationBox = this.FindViewById<TextView>(Resource.Id.txtInformationBox);

            this.RaiseIssue = this.FindViewById<TextView>(Resource.Id.raise_issue);
            this.AddProspect = this.FindViewById<TextView>(Resource.Id.add_prospect);
            this.CreateCustomer = this.FindViewById<TextView>(Resource.Id.add_customer);

            this.BottomToolbar = this.FindViewById<LinearLayout>(Resource.Id.bottom_toolbar);
        }

        protected void SetNewAdapter<TListType>(PersonSectionListAdapter<TListItemType> newAdapter,bool isOnUiThread = false)
        {
            if (!isOnUiThread)
            {
                this.RunOnUiThread(
                        () =>
                        {
                            this.SetNewAdapter<TListType>(newAdapter,true);
                        });
                return;
            }

            this.PersonSectionAdapter = newAdapter;
            this.PersonListView.Adapter = newAdapter;
        }

        public string CurrentFilter { get; set; }

        public override void UpdateScreen()
        {
            Logger.Debug("Calling refresh list");
            this.RunOnUiThread(
                    () =>
                    {
                        this.RefreshList();
                    });
        }

        public override void SetListeners()
        {
            // set the item click listeners
            this.PersonListView.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                AdapterItemClick(sender as ListView, args);
            };

            this.RaiseIssue.Click += (sender, args) =>
            {
                new IntentStartPointTracker().StartIntentWithTracker(this, this.IntentStartPoint, typeof(TicketStartActivity));
            };

            this.AddProspect.Click += (sender, args) =>
            {
                WizardLauncher.Launch(this, WizardTypes.ProspectRegistration, this.IntentStartPoint);
            };

            this.CreateCustomer.Click += (sender, args) =>
            {
                WizardLauncher.Launch(this, WizardTypes.CustomerRegistration, this.IntentStartPoint);
            };
        }

        /// <summary>
        /// Show a snack bar
        /// </summary>
        /// <param name="messageResource">The message to display in the snack bar</param>
        protected void ShowSnackBar(int messageResource)
        {
            this.pullToRefresh = Snackbar.Make(
                                        this.PersonListView,
                                        messageResource,
                                        Snackbar.LengthIndefinite);
            this.pullToRefresh.Show();
        }

        /// <summary>
        /// Hides the snack bar
        /// </summary>
        protected void HideSnackBar()
        {
            if (this.pullToRefresh == null)
            {
                return;
            }

            this.pullToRefresh.Dismiss();
        }

        /// <summary>
        /// This method handles on click events on the item list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public abstract void AdapterItemClick(ListView sender, AdapterView.ItemClickEventArgs args);

        public override bool OnCreateOptionsMenu(IMenu menu)
        {   
            this.BuildMenu(menu);

            // SearchView.QueryTextChange += SearchView_QueryTextChange;
            this.SearchView.QueryTextSubmit += this.SearchViewOnQueryTextSubmit;
            return base.OnCreateOptionsMenu(menu);
        }

        private void SearchViewOnQueryTextSubmit(object sender, SearchView.QueryTextSubmitEventArgs queryTextSubmitEventArgs)
        {
            this.Search(queryTextSubmitEventArgs.Query);
        }

        protected void  EnableList(bool b)
        {
            RunOnUiThread
                    (
                        () =>
                        {
                            PersonListView.Enabled = b;
                        }
                    );
        }

        /// <summary>
        /// Callback method to be invoked when the list or grid has been scrolled. This will be called after the scroll has completed
        /// </summary>
        /// <param name="view">The view whose scroll state is being reported</param>
        /// <param name="firstVisibleItem">The index of the first visible cell (ignore if visibleItemCount == 0)</param>
        /// <param name="visibleItemCount">The number of visible cells</param>
        /// <param name="totalItemCount">The number of items in the list adaptor</param>
        public void OnScroll(AbsListView listView, int firstVisibleItem, int visibleItemCount, int totalItemCount)
        {
        }

        /// <summary>
        /// Callback method to be invoked while the list view or grid view is being scrolled.
        /// If the view is being scrolled, this method will be called before the next frame of the scroll is rendered.
        /// In particular, it will be called before any calls to getView(int, View, ViewGroup)
        /// </summary>
        /// <param name="view">The view whose scroll state is being reported</param>
        /// <param name="scrollState">The current scroll state. One of SCROLL_STATE_TOUCH_SCROLL or SCROLL_STATE_IDLE</param>
        public void OnScrollStateChanged(AbsListView view, ScrollState scrollState)
        {
            if (this.BottomToolbar == null)
            {
                return;
            }

            if (scrollState == ScrollState.TouchScroll || scrollState == ScrollState.Fling)
            {
                this.BottomToolbar.Animate().Cancel();
                this.BottomToolbar.Animate().TranslationYBy(150);
            }
            else
            {
                if (this.IsLastItemVisible && this.LastItemIsAtBottom)
                {
                    this.BottomToolbar.Animate().Cancel();
                    this.BottomToolbar.Animate().TranslationYBy(150);
                }
                else
                {
                    int initPosY = this.BottomToolbar.ScrollY;
                    this.BottomToolbar.Animate().Cancel();
                    this.BottomToolbar.Animate().TranslationY(initPosY);
                }
            }
        }

    }
}