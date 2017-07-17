using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.BL.Controllers;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.Adapters;
using Debug = System.Diagnostics.Debug;
using Message = SalesApp.Core.BL.Message;

namespace SalesApp.Droid
{
    [Activity(Label = "Inbox")]
    [Obsolete("This is a deprecated feature.")]
    public class BroadcastMessagesListActivity : Activity
    {
        List<NotificationListItem> notificationListItems = new List<NotificationListItem>();
        MessageListAdapter messageListAdapter;
        SwipeRefreshLayout refresher;

        MessagesController _messagesController;

        private MessagesController messagesController
        {
            get
            {
                if (_messagesController == null)
                {
                    _messagesController = new MessagesController();
                }
                return _messagesController;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.layout_messages);

            refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.messagesRefresher);

            refresher.Refresh += async delegate
            {
                RefreshMessages();
            };

            //load and display cached data
            LoadData();

            //wire up on list item clicked event handler
            FindViewById<ListView>(Resource.Id.broadcastMessageList).ItemClick += listView_ItemClick;
        }

        async void listView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var selectedItem = notificationListItems[e.Position];

            if (selectedItem.ItemType == ItemType.Header)
                return;

            //mark message as read if not already
            if (!selectedItem.Message.IsRead)
            {
                //mark item as read on the db
                selectedItem.Message.IsRead = true;
                await messagesController.SaveAsync(selectedItem.Message);

                //mark item as read on the list
                var record = notificationListItems
                    .FirstOrDefault(x => x.ItemType == ItemType.Detail
                        && x.Message.MessageId == selectedItem.Message.MessageId);

                if (record != null)
                    record.Message.IsRead = true;
            }

            var serializedMessage = JsonConvert.SerializeObject(selectedItem.Message);

            //open "Message details" page and display the selected message
            Intent intent = new Intent(this, typeof(BroadcastMessageDetails));
            intent.PutExtra("selected_message", serializedMessage);

            StartActivity(intent);
        }

        private async void LoadData()
        {
            notificationListItems.Insert(0, new NotificationListItem("UNREAD"));
            notificationListItems.Insert(1, new NotificationListItem("READ"));

            try
            {
                var items = await messagesController.GetAllAsync();

                if (items.Any())
                {
                    notificationListItems.AddRange(items.Select(x => new NotificationListItem(x)));
                }

                var progressDialog = ProgressDialog.Show(this, GetString(Resource.String.please_wait), GetString(Resource.String.updating_inbox), true);

                new Thread(new ThreadStart(delegate
                {
                    //refresh new messages from server
                    RefreshFromOnlineFeed(progressDialog);

                })).Start();
            }
            catch (Exception ex)
            {
                //TODO log this exception
                Debug.WriteLine(ex.Message);
            }
        }

        private void RefreshMessages()
        {
            try
            {
                var progressDialog = ProgressDialog.Show(this, GetString(Resource.String.please_wait), GetString(Resource.String.updating_inbox), true);

                new Thread(new ThreadStart(delegate
                {
                    //refresh new messages from server
                    RefreshFromOnlineFeed(progressDialog);
                })).Start();
            }
            catch (Exception ex)
            {
                RunOnUiThread(new Action(() =>
                {
                    Toast.MakeText(this, GetString(Resource.String.unable_to_refresh_) + " " + ex.Message, ToastLength.Long).Show();
                }));
            }
        }

        private async void RefreshFromOnlineFeed(ProgressDialog loadingDiaglog)
        {
            //check for network connection
            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var activeConnection = connectivityManager.ActiveNetworkInfo;

            if ((activeConnection != null) && activeConnection.IsConnected)
            {
                try
                {
                    var salesAppManager = new SalesAppManager();
                    DateTime? lastFetchDate = Settings.Instance.LastMessageFetchDate;

                    //if phone has never fetched data, set it to fetch existing records ever since app was launched in 2014/15
                    if (lastFetchDate == null)
                        lastFetchDate = DateTime.Parse("2014-1-1");

                    var data = await salesAppManager.ListMessages(lastFetchDate.Value);


                    await resolveNewRecords(data);

                    Settings.Instance.LastMessageFetchDate = DateTime.Now.Date;

                    RunOnUiThread(() =>
                    {
                        refreshMessagesListView();
                        loadingDiaglog.Hide();
                        refresher.Refreshing = false;
                    });
                }
                catch (Exception ex)
                {
                    //General Exception thrown while refreshing from server, display error message and display cached data 
                    //TODO log this exception
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, ex.Message, ToastLength.Long);

                        refreshMessagesListView();

                        loadingDiaglog.Hide();

                        refresher.Refreshing = false;
                    });
                    //assessProspect.DisplayUnsuccessfulRetryAlert(this.Activity.Resources.GetString(Resource.String.prospect_registration));
                }
            }
            else
            {
                //no internet connection, displaly cached records
                RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, GetString(Resource.String.connection_not_available), ToastLength.Long).Show();
                        refreshMessagesListView();
                        loadingDiaglog.Hide();
                        refresher.Refreshing = false;
                    }
                );

                //assessProspect.DisplayNetworkRequiredAlert();
            }
        }

        private async Task resolveNewRecords(List<Message> newData)
        {
            if (newData != null && !newData.Any())
                return;

            foreach (var updatedServerRecord in newData)
            {
                var existingRecord = notificationListItems
                            .Where(x => x.ItemType == ItemType.Detail && x.Message.MessageId == updatedServerRecord.MessageId)
                            .Select(x => x.Message)
                            .FirstOrDefault();

                if (existingRecord != null)
                {
                    if (existingRecord.ExpiryDate != updatedServerRecord.ExpiryDate)
                    {
                        existingRecord.ExpiryDate = updatedServerRecord.ExpiryDate;

                        try
                        {
                            await SaveRecordToDb(existingRecord);
                        }
                        catch (Exception ex)
                        {
                            Toast.MakeText(this, ex.Message, ToastLength.Long);
                        }
                    }
                }
                else
                {
                    try
                    {
                        //this.messagesRepository.SaveItem(updatedServerRecord);
                        await SaveRecordToDb(updatedServerRecord);
                        notificationListItems.Add(new NotificationListItem(updatedServerRecord));
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, ex.Message, ToastLength.Long);
                    }
                }
            }

            refreshMessagesListView();
        }

        private void refreshMessagesListView()
        {
            RunOnUiThread(() =>
            {
                bindDataToList();
            });
        }

        private async Task SaveRecordToDb(Message message)
        {
            await messagesController.SaveAsync(message);
        }

        private void bindDataToList()
        {
            //sort data
            notificationListItems.Sort((x, y) => x.Compare(y, x));

            //set up List Adapters

            ListView listView = FindViewById<ListView>(Resource.Id.broadcastMessageList);

            if (listView.Adapter == null)
            {
                messageListAdapter = new MessageListAdapter(this, notificationListItems);
                SharedFields.BroadcastMessageListAdapter = messageListAdapter;
                messageListAdapter.SortMessages();
                listView.Adapter = messageListAdapter;
            }

            //invoke refresh event on list view
            messageListAdapter.NotifyDataSetChanged();
        }
    }
}