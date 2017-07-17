using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using SalesApp.Core.BL.Models.TicketList;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.ViewModels.TicketList;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.Tickets;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid.Views.TicketList
{
    /// <summary>
    /// A base fragment for displaying a list of tickets
    /// </summary>
    public abstract class TicketFragmentBase : MvxFragmentBase, AbsListView.IOnScrollListener
    {
        public const string HasSnackBarBundleKey = "HasSnackBarBundleKey";
        private bool hasSnackbar;

        /// <summary>
        /// The model for this view
        /// </summary>
        protected TicketListViewModel vm;

        /// <summary>
        /// The activity to which this fragment is attached
        /// </summary>
        private TicketListView parentActivity;

        /// <summary>
        /// Whether or not the last item of the list view is visible
        /// </summary>
        private bool lastVisible;

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
        /// Gets the ticket list
        /// </summary>
        public MvxListView TicketList { get; private set; }

        protected LinearLayout BottomToolbar { get; private set; }

        protected string SnackBarMessage 
        {
            get
            {
                string message = this.GetString(Resource.String.ticket_list_alert_no_tickets);

                if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
                {
                    message = this.GetString(Resource.String.ticket_list_alert_no_internet);
                }

                return message;
            }
        }

        public bool IsLastItemVisible
        {
            get
            {
                if (this.TicketList == null || this.TicketList.Adapter == null)
                {
                    return false;
                }

                bool value = this.TicketList.LastVisiblePosition == this.TicketList.Adapter.Count - 1;
                return value;
            }
        }

        private int GetIndex()
        {
            return this.TicketList.LastVisiblePosition - this.TicketList.FirstVisiblePosition;
        }

        public bool LastItemIsAtBottom
        {
            get
            {
                int lastVisibleIndex = GetIndex();

                View lastView = this.TicketList.GetChildAt(lastVisibleIndex);

                if (lastView == null)
                {
                    return false;
                }

                ViewGroup.MarginLayoutParams listMargins = this.TicketList.LayoutParameters as ViewGroup.MarginLayoutParams;

                if (listMargins == null)
                {
                    return lastView.Bottom == this.BottomToolbar.Bottom;
                }

                int verticalMargins = listMargins.BottomMargin + listMargins.TopMargin;

                bool viewIsAtBottom = lastView.Bottom + verticalMargins == this.BottomToolbar.Bottom;

                return this.IsLastItemVisible && viewIsAtBottom;
            }
        }

        public bool CanPullToSync
        {
            get
            {
                if (this.TicketList == null)
                {
                    return false;
                }

                if (this.TicketList.ChildCount == 0)
                {
                  return true;
                }

                bool firstItemVisible = this.TicketList.FirstVisiblePosition == 0;
                // check if the top of the first item is visible
                bool topOfFirstItemVisible = this.TicketList.GetChildAt(0).Top == 0;
                // enabling or disabling the refresh layout
                return firstItemVisible && topOfFirstItemVisible;
            }
        }

        public void ShowSnackBar()
        {
            vm.ShowSnackBar = true;
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
            if (parentActivity == null || listView == null)
            {
                return;
            }

            if (totalItemCount == 0)
            {
                this.parentActivity.SwipeRefreshLayout.Enabled = true;
                return;
            }

            // Enable / Disable pull to refresh accordingly
            bool enable = false;
            if (listView.ChildCount > 0)
            {
                // check if the first item of the list is visible
                bool firstItemVisible = listView.FirstVisiblePosition == 0;
                // check if the top of the first item is visible
                bool topOfFirstItemVisible = listView.GetChildAt(0).Top == 0;
                // enabling or disabling the refresh layout
                enable = firstItemVisible && topOfFirstItemVisible;

                this.lastVisible = listView.LastVisiblePosition == totalItemCount - 1;
            }

            int currentPage = this.parentActivity.CurrentPage;
            var adapter = listView.Adapter as MvxAdapter;

            if (adapter == null)
            {
                this.parentActivity.SwipeRefreshLayout.Enabled = enable;
                return;
            }

            var item = adapter.GetRawItem(totalItemCount - 1) as AbstractTicketBase;

            if (item == null)
            {
                this.parentActivity.SwipeRefreshLayout.Enabled = enable;
                return;
            }

            if (currentPage == TicketListView.CustomerTicketsPage && item.GetType() == typeof(CustomerTicket))
            {
                this.parentActivity.SwipeRefreshLayout.Enabled = enable;
            }

            if (currentPage == TicketListView.DsrTicketsPage && item.GetType() == typeof(DsrTicket))
            {
                this.parentActivity.SwipeRefreshLayout.Enabled = enable;
            }
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
                if (this.lastVisible && this.LastItemIsAtBottom)
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

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            this.parentActivity = activity as TicketListView;
        }

        /// <summary>
        /// Create and return this fragment's view
        /// </summary>
        /// <param name="inflater">The inflator</param>
        /// <param name="container">The container view</param>
        /// <param name="inState">The saved state if any</param>
        /// <returns>The inflated view</returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedState)
        {
            base.OnCreateView(inflater, container, savedState);
            this.FragmentView = this.BindingInflate(Resource.Layout.fragment_ticket_list, null);

            this.BottomToolbar = this.FragmentView.FindViewById<LinearLayout>(Resource.Id.bottom_toolbar);
            this.TicketList = this.FragmentView.FindViewById<MvxListView>(Resource.Id.ticket_list);
            this.TicketList.SetOnScrollListener(this);

            this.RaiseIssue = this.FragmentView.FindViewById<TextView>(Resource.Id.raise_issue);
            this.AddProspect = this.FragmentView.FindViewById<TextView>(Resource.Id.add_prospect);
            this.CreateCustomer = this.FragmentView.FindViewById<TextView>(Resource.Id.add_customer);

            this.RaiseIssue.Click += (sender, args) =>
            {
                new IntentStartPointTracker().StartIntentWithTracker(this.Activity, IntentStartPointTracker.IntentStartPoint.WelcomeScreen, typeof(TicketStartActivity));
            };

            this.AddProspect.Click += (sender, args) =>
            {
                WizardLauncher.Launch(this.Activity, WizardTypes.ProspectRegistration, IntentStartPointTracker.IntentStartPoint.WelcomeScreen);
            };

            this.CreateCustomer.Click += (sender, args) =>
            {
                WizardLauncher.Launch(this.Activity, WizardTypes.CustomerRegistration, IntentStartPointTracker.IntentStartPoint.WelcomeScreen);
            };

            this.vm = new TicketListViewModel();
            this.ViewModel = vm;

            if (this.Arguments != null)
            {
                this.hasSnackbar = this.Arguments.GetBoolean(HasSnackBarBundleKey);
            }

            if (this.hasSnackbar)
            {
                Snackbar snackbar = Snackbar.Make(this.TicketList, this.SnackBarMessage, Snackbar.LengthIndefinite);
                BindableSnackBar bindableSnackBar = new BindableSnackBar(snackbar);

                var set = this.CreateBindingSet<TicketFragmentBase, TicketListViewModel>();
                set.Bind(bindableSnackBar).For(obj => obj.Visible).To(x => x.ShowSnackBar);
                set.Apply();
            }

            return this.FragmentView;
        }
    }
}