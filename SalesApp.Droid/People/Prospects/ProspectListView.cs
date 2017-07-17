using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using SalesApp.Core.Api.DownSync;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Notification;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Notifications;
using SalesApp.Core.Services.Person.Prospect;
using SalesApp.Droid.Components;
using SalesApp.Droid.Enums;
using Enumerable = System.Linq.Enumerable;

namespace SalesApp.Droid.People.Prospects
{
    [Activity(NoHistory = false, ParentActivity = typeof(HomeView), Theme = "@style/AppTheme.DefaultToolbar")]
    class ProspectListView : PersonListActivity<ProspectItem, ProspectSearchService, ProspectSearchResult>
    {
        private static readonly ILog Logger = LogManager.Get(typeof(ProspectListView));

        private readonly IConnectivityService _connectivityService = Resolver.Instance.Get<IConnectivityService>();

        public override IntentStartPointTracker.IntentStartPoint IntentStartPoint
        {
            get { return IntentStartPointTracker.IntentStartPoint.ProspectsList; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            PersonSectionAdapter = new ProspectListAdapter(this);
            base.OnCreate(bundle);

            this.AddToolbar(Resource.String.prospects_list_title, true);

            Task.Run
                (
                    async () => await InvalidateOverdueNotifications()
                );
        }

        async Task InvalidateOverdueNotifications()
        {
            await new NotificationsCoreService().SetAllOverdueNotificationsViewed(NotificationTypes.ProspectReminder);
        }

        

        public override void SetViewPermissions()
        {
            
        }

        public override async Task SynchronizeList()
        {
            Logger.Verbose("Synchronize data.");
            if (!this._connectivityService.HasConnection())
            {
                this.ShowAlertNoInternet();
            }
            else
            {
                try
                {
                    EnableList(false);
                    // do the up sync
                    await this.SyncController.PushAllAsync(typeof(Prospect).Name);

                    // now do a down sync
                    await this.SyncController.SyncDownAsync<DownSyncServerResponse<Prospect>, Prospect>();
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    this.ShowAlertSynchronizeFailed();
                }
                finally
                {
                    EnableList(true);
                }
            }

            this.HideSnackbar();
        }

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

            Logger.Verbose("Refresh local list.");
            PersonSectionAdapter.ClearSections();
            
            // prepare the lists for each section
            List<ProspectItem> todayProspects = new List<ProspectItem>();
            List<ProspectItem> overdueProspects = new List<ProspectItem>();
            List<ProspectItem> tomorrowProspects = new List<ProspectItem>();
            List<ProspectItem> laterProspects = new List<ProspectItem>();
            List<ProspectItem> noReminderProspects = new List<ProspectItem>();

            // Get all the prospects  
            Logger.Verbose("Retrieve all prospects.");
            List<ProspectSearchResult> prospects = SearchResults as List<ProspectSearchResult>;
            SetNewAdapter<ProspectItem>(new ProspectListAdapter(this));

            // remove converted prospects
            prospects = Enumerable.ToList(Enumerable.Where(prospects, prospect => !prospect.Converted));
            // store whether the original list has prospects
            bool noProspects = prospects.Count == 0;

            // Apply search filter
            if (CurrentFilter.IsBlank() == false)
            {
                prospects = prospects.Where
                    (
                        prosp => prosp.FullName.ToLower().Contains(CurrentFilter.ToLower())
                                 || prosp.Phone.Contains(CurrentFilter)
                    ).ToList();
            }

            // Clear all sections
            PersonSectionAdapter.ClearSections();

            // process each prospect into its given section
            if (prospects.Count > 0)
            {
                foreach (var p in prospects)
                {
                    int days = GetDifferenceInDaysX(DateTime.Today, p.ReminderTime);
                    ProspectItem prospectItem = new ProspectItem(p);

                    if (p.SyncRecord != null)
                    {
                        prospectItem.SyncStatus = p.SyncRecord.Status;
                    }
                    else
                    {
                        prospectItem.SyncStatus = RecordStatus.Synced;
                    }

                    if (p.ReminderTime == DateTime.MinValue)
                        noReminderProspects.Add(prospectItem);
                    else if (days < 0)
                        overdueProspects.Add(prospectItem);
                    else if (days == 0)
                        todayProspects.Add(prospectItem);
                    else if (days == 1)
                        tomorrowProspects.Add(prospectItem);
                    else if (days > 1)
                        laterProspects.Add(prospectItem);
                }

                if (overdueProspects.Count > 0)
                {
                    ProspectItemListAdapter overdueAdapter = new ProspectItemListAdapter(this,
                        overdueProspects);
                    PersonSectionAdapter.AddSection(
                        GetString(Resource.String.prospects_list_title_overdue), overdueAdapter,
                        Resource.Color.red);
                }

                if (todayProspects.Count > 0)
                {
                    ProspectItemListAdapter todayAdapter = new ProspectItemListAdapter(this,
                        todayProspects);
                    PersonSectionAdapter.AddSection(
                        GetString(Resource.String.prospects_list_title_today), todayAdapter,
                        Resource.Color.green);
                }

                if (tomorrowProspects.Count > 0)
                {
                    ProspectItemListAdapter tomorrowAdapter = new ProspectItemListAdapter(this,
                        tomorrowProspects);
                    PersonSectionAdapter.AddSection(
                        GetString(Resource.String.prospects_list_title_tomorrow), tomorrowAdapter,
                        Resource.Color.orange);
                }

                if (laterProspects.Count > 0)
                {
                    ProspectItemListAdapter laterAdapter = new ProspectItemListAdapter(this,
                        laterProspects);
                    PersonSectionAdapter.AddSection(
                        GetString(Resource.String.prospects_list_title_later), laterAdapter,
                        Resource.Color.yellow);
                }

                if (noReminderProspects.Count > 0)
                {
                    ProspectItemListAdapter noReminderAdapter = new ProspectItemListAdapter(this,
                        noReminderProspects);
                    PersonSectionAdapter.AddSection(
                        GetString(Resource.String.prospects_list_title_none), noReminderAdapter,
                        Resource.Color.gray1);
                }
            }

            if (noProspects)
            {               
                if (SearchHelper.Searching)
                {
                    this.HideSnackBar();
                }
                else
                {
                    if (this.ConnectedToNetwork)
                    {
                        this.ShowSnackBar(Resource.String.prospects_refresh_prompt);
                    }
                    else
                    {
                        this.ShowSnackBar(Resource.String.prospects_refresh_prompt_no_internet);
                    }
                }
               
            }
            else
            {
                this.HideSnackBar();
            }

            PersonSectionAdapter.NotifyDataSetChanged();
        }

        public override async Task RefreshList()
        {
            if (SearchHelper.Searching == false)
            {
                SearchResults = await new ProspectSearchService().GetAllLocalAsync();
            }

            UpdateDataSet();
        }

        private int GetDifferenceInDaysX(DateTime startDate, DateTime endDate)
        {
            return (int)(endDate.Date - startDate.Date).TotalDays;
        }

        public override void AdapterItemClick(ListView sender, AdapterView.ItemClickEventArgs args)
        {
            ProspectListAdapter adapter = null;
            if (sender != null)
            {
                adapter = sender.Adapter as ProspectListAdapter;
            }

            if (adapter == null)
            {
                Logger.Error("No Adapter found of type [ProspectListAdapter] something's wrong with setup.");
                return;
            }
            if (adapter.GetItem(args.Position) != null)
            {
                try
                {
                    var prospect = adapter.GetItem(args.Position).ToNetObject<ProspectItem>();

                    if (GetString(Resource.String.no_search_results).Equals(prospect.SearchResult.FirstName))
                    {
                        return;
                    }

                    Intent intent = new ProspectDetailsHelper().GetPropsectDetailIntent(this, prospect);
                    intent.PutExtra(ProspectDetailActivity.ProspectDetailsOrigin, ProspectDetailsOrigin.ProspectListItemClick.ToString());

                    this.StartActivityForResult(intent, 1);
                    
                }
                catch (InvalidCastException ice)
                {
                    Logger.Error("Header clicked:" + ice.Message);
                }
            }
        }
        
        public override void RetrieveScreenInput()
        {
            throw new NotImplementedException();
        }

        public override bool Validate()
        {
            return true;
        }
    }

    /*public class SearchViewExpandListener : Object, MenuItemCompat.IOnActionExpandListener
    {
        private readonly IFilterable _adapter;

        public SearchViewExpandListener(IFilterable adapter)
        {
            _adapter = adapter;
        }

        public bool OnMenuItemActionCollapse(IMenuItem item)
        {
            _adapter.Filter.InvokeFilter("");
            return true;
        }

        public bool OnMenuItemActionExpand(IMenuItem item)
        {
            return true;
        }
    }*/
}