using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Droid.Components;

namespace SalesApp.Droid.People.Prospects
{
    public class ProspectItemListAdapter : PersonItemListAdapter<ProspectItem>
    {
        public ProspectItemListAdapter(Activity context, List<ProspectItem> items) : base(context, items)
        {
            HasResults = true;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = GetItem(position).ToNetObject<ProspectItem>();
            View view = base.GetView(position, convertView, parent);
            if (HasResults)
            {
                viewHolder.PersonName.Text = item.SearchResult.FullName;
                viewHolder.PersonPhoneNo.Text = item.SearchResult.Phone;

                if (item.SyncStatus == RecordStatus.Synced)
                    viewHolder.SyncStatus.SetImageResource(Resource.Drawable.prospect_list_2);
                else if (item.SyncStatus == RecordStatus.Pending)
                    viewHolder.SyncStatus.SetImageResource(Resource.Drawable.prospect_list_3);
                else if (item.SyncStatus == RecordStatus.InError)
                    viewHolder.SyncStatus.SetImageResource(Resource.Drawable.prospect_list_1);

                viewHolder.ProspectStatus.SetImageResource(item.GetProspectPotentialColor());
                viewHolder.ProspectReminderTime.Text = GetProspectTimeText(item);
            }
            
            return view;
        }

        private string GetProspectTimeText(ProspectItem item)
        {
            string prospecttime = string.Empty;
            DateTime time = item.SearchResult.ReminderTime;
            const int daysTillToday = 0;
            const int daysTillTomorrow = 1;

            int days = (int)(time - DateTime.Today).Days;
            int hours = (int)(time - DateTime.Today).TotalHours;

            string day_string = days.ToString() + " " + Activity.GetString(Resource.String.days);


            if (days == 1)
                prospecttime = time.ToString("hh:mm tt");

            else if (days < -50000)
            {
                prospecttime = item.SearchResult.Created.ToString("dd-MM-yy");
            }
            else if (days < daysTillToday)
            {
                prospecttime = day_string;
            }
            else if (days == daysTillToday)
            {
                prospecttime = time.ToString("hh:mm tt");
            }
            else if (days > daysTillToday && days <= daysTillTomorrow)
            {
                prospecttime = day_string;
            }
            else if (days > daysTillTomorrow)
            {
                prospecttime = day_string;
            }

            return prospecttime;
        }

    }
}