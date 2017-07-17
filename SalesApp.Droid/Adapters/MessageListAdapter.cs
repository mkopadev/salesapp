using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Widget;
using MK.Solar.BL;
using Android.App;
using Android;
using Android.Views;

namespace MK.Solar.Adapters
{
    public class MessageListAdapter : BaseAdapter<Message>
    {
        protected Activity context = null;
        protected IList<Message> Messages = new List<Message>();

        public MessageListAdapter(Activity context, IList<Message> messages)
            : base()
        {
            this.context = context;
            this.Messages = messages;
        }

        public override Message this[int position]
        {
            get { return Messages[position]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return Messages.Count; }
        }

        public override Android.Views.View GetView(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {
            // Get our object for position
            var item = Messages[position];
            View view;

            //Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
            // gives us some performance gains by not always inflating a new view
            if (convertView == null)
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.MessageListItem, null);
            }
            else
            {
                view = convertView;
            }

            var nameLabel = view.FindViewById<TextView>(Resource.Id.lblName);
            nameLabel.Text = item.From;
            var notesLabel = view.FindViewById<TextView>(Resource.Id.lblDescription);
            notesLabel.Text = item.Body;

            // TODO: set the check.
            var checkMark = view.FindViewById<ImageView>(Resource.Id.checkMark);
            checkMark.Visibility = item.IsRead ? ViewStates.Visible : ViewStates.Gone;

            //Finally return the view
            return view;
        }
    }
}