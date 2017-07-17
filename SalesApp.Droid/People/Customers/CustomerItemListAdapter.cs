using System.Collections.Generic;
using Android.App;
using Android.Views;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Droid.Components;

namespace SalesApp.Droid.People.Customers
{
    public class CustomerItemListAdapter : PersonItemListAdapter<CustomerItem>
    {
        public CustomerItemListAdapter(Activity context, List<CustomerItem> items) : base(context, items)
        {
            HasResults = true;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            CustomerItem item = GetItem(position).ToNetObject<CustomerItem>();
            View view = base.GetView(position, convertView, parent);
            
            if (HasResults)
            {
                viewHolder.PersonName.Text = item.SearchResult.FullName;
                viewHolder.PersonPhoneNo.Text = item.SearchResult.Phone;

                DataChannel syncChannel = item.SearchResult.Channel;
                switch (syncChannel)
                {
                    case DataChannel.Fallback:
                        switch (item.SearchResult.SyncStatus)
                        {
                            case RecordStatus.FallbackSent:
                                viewHolder.SyncStatus.SetImageResource(Resource.Drawable.sms_sync_success);
                                break;
                            case RecordStatus.Pending:
                                viewHolder.SyncStatus.SetImageResource(Resource.Drawable.sms_sync_fail);
                                break;
                            case RecordStatus.Synced:
                                viewHolder.SyncStatus.SetImageResource(Resource.Drawable.prospect_list_2);
                                break;
                        }
                        break;
                    default:
                        viewHolder.SyncStatus.SetImageResource(Resource.Drawable.prospect_list_2);
                        break;
                }


                viewHolder.ProspectStatus.Visibility = ViewStates.Gone;
                viewHolder.ProspectReminderTime.Visibility = ViewStates.Gone;
            }

            return view;
        }
    }
}