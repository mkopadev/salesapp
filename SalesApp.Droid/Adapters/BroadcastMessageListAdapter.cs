using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using SalesApp.Core.BL;

namespace SalesApp.Droid.Adapters
{
    public class MessageListAdapter : BaseAdapter<NotificationListItem>
    {
        Activity context;
        List<NotificationListItem> items;

        public MessageListAdapter(Activity context, List<NotificationListItem> notificationListItems)
        {
            if (notificationListItems == null)
                notificationListItems = new List<NotificationListItem>();

            this.context = context;
            items = notificationListItems;
        }

        public override NotificationListItem this[int position]
        {   
            get
            {
                return items[position];
            }
        }

        public override int Count
        {
            get { return items.Count(); }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                View view = convertView;
                var item = items[position];

                if (item.ItemType == ItemType.Detail)
                {
                    //render the message row view i.e. view represeting each message item

                    //verify that the convert view is not null and matches the message row type
                    if (view == null || view.FindViewById<TextView>(Resource.Id.messageContent) == null)
                        view = context.LayoutInflater.Inflate(Resource.Layout.layout_message_row, null);

                    view.FindViewById<TextView>(Resource.Id.messageSubject).Text = item.Message.Subject;
                    view.FindViewById<TextView>(Resource.Id.messageContent).Text = item.Message.Body;
                    view.FindViewById<TextView>(Resource.Id.messageTime).Text = item.Message.MessageDate.ToString("d MMM, HH:mm");

                    if (!item.Message.IsRead)
                    {
                        //view.FindViewById<View>(Resource.Id.indicatorUnread).Visibility = ViewStates.Invisible;
                        var messageContentView = view.FindViewById<TextView>(Resource.Id.messageContent);
                        view.FindViewById<TextView>(Resource.Id.messageSubject).SetTypeface(null, TypefaceStyle.Bold);
                        view.FindViewById<TextView>(Resource.Id.messageTime).SetTypeface(null, TypefaceStyle.Bold);
                        view.FindViewById<TextView>(Resource.Id.messageSubject).SetTextColor(Color.Black);
                        view.FindViewById<TextView>(Resource.Id.messageSubject).SetTextColor(Color.Black);
                    }
                    else
                    {
                        //view.FindViewById<View>(Resource.Id.indicatorUnread).Visibility = ViewStates.Visible;
                        view.FindViewById<TextView>(Resource.Id.messageSubject).SetTypeface(null, TypefaceStyle.Normal);
                        view.FindViewById<TextView>(Resource.Id.messageSubject).SetTextColor(Color.ParseColor("#999999")); //TODO replace with string resource @color/gray1
                        view.FindViewById<TextView>(Resource.Id.messageTime).SetTypeface(null, TypefaceStyle.Normal);
                        
                    }

                    return view;
                }
                else
                {
                    //render list group header view

                    //verify that the convert view is not null and matches the header row type
                    if (view == null || view.FindViewById<TextView>(Resource.Id.headerText) == null)
                        view = context.LayoutInflater.Inflate(Resource.Layout.layout_broadcastmessagelist_header, null);

                    view.FindViewById<TextView>(Resource.Id.headerText).Text = item.SectionHeader;

                    if (item.SectionHeader == "UNREAD")
                    {
                        view.FindViewById<ImageView>(Resource.Id.broadcastMessageSectionIcon).SetImageResource(Resource.Drawable.unreadMessage);
                        view.FindViewById<TextView>(Resource.Id.headerText).SetTextColor(Color.Black);
                    }
                    else
                    {
                        view.FindViewById<ImageView>(Resource.Id.broadcastMessageSectionIcon).SetImageResource(Resource.Drawable.archivedMessage);
                    }
                    
                    return view;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return convertView;
            }
        }

        public void SortMessages()
        {
            items.Sort((x, y) => x.Compare(y,x));
        }
    }

    public class NotificationListItem :IComparer<NotificationListItem>
    {
        public NotificationListItem(string header)
        {
            ItemType = ItemType.Header;
            SectionHeader = header;
        }

        public NotificationListItem(Message message)
        {
            ItemType = ItemType.Detail;
            Message = message;
            SectionHeader = null;
        }

        public Message Message { get; set; }
        public ItemType ItemType { get; set; }
        public string SectionHeader { get; set; }

        /// <summary>
        /// Compares two notification list items to evalauate for sorting based on the date of creation
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>0 if they are equal, 1 if x is greater than y, -1 if x is less than y</returns>
        public int Compare(NotificationListItem x, NotificationListItem y)
        {
            const string ARCHIVED = "READ";
            const string UNREAD = "UNREAD";
            /*
             * ORDER OF SORTING
             * Unread Header Sction
             * Unread Messages (sorted by date descending)
             * Archived Header Section
             * Archived Messages (sorted by date descending)
             */

            /*
             * UNREAD   ARCHIVED
             * ARCHIVED UNREAD  
             * NB mutual exclusion betweewn the two
             */
            if (x.ItemType == ItemType.Header && y.ItemType == ItemType.Header)
            {
                if (x.SectionHeader == UNREAD)
                    return 1; // do y path                
            }

            /*
             * UNREAD   read 
             * UNREAD   unread
             * ARCHIVED read
             * ARCHIVED unread
             */
            if (x.ItemType == ItemType.Header && y.ItemType == ItemType.Detail)
            {
                if (x.SectionHeader == UNREAD)
                    return 1;

                if (x.SectionHeader == ARCHIVED)
                {
                    if (y.Message.IsRead)
                        return 1;

                    return -1;
                }
            }

            if (x.ItemType == ItemType.Detail && y.ItemType == ItemType.Detail)
            {
                //unread messages go up the queue
                if (!x.Message.IsRead && y.Message.IsRead)
                    return 1;

                if (!y.Message.IsRead && x.Message.IsRead)
                    return -1;

                //if both messages are read/unread, evalauate by creation date
                return x.Message.Created.CompareTo(y.Message.Created);
            }

            return -1 * Compare(y, x);
        }
    }

    public enum ItemType
    {
        Header,
        Detail
    }
}