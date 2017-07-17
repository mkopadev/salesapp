using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using SalesApp.Core.Events.CustomerPhoto;

namespace SalesApp.Droid.Views.Person.Customer
{
    public class CustomerPhotoListAdapter : MvxAdapter, View.IOnClickListener
    {
        public event EventHandler<CustomerPhotoUpdatedEvent> PhotoUpdated;

        public CustomerPhotoListAdapter(Context context, IMvxAndroidBindingContext bindingContext) : base(context, bindingContext)
        {
        }

        protected override View GetView(int position, View convertView, ViewGroup parent, int templateId)
        {
            View v = base.GetView(position, convertView, parent, templateId);

            if (v == null)
            {
                return null;
            }

            Button photoResendButton = v.FindViewById<Button>(Resource.Id.resend_photo);
            photoResendButton.Tag = position;
            photoResendButton.SetOnClickListener(this);
            return v;
        }

        public void OnClick(View view)
        {
            var position = (int)view.Tag;

            if (this.PhotoUpdated == null)
            {
                return;
            }

            this.PhotoUpdated.Invoke(this, new CustomerPhotoUpdatedEvent(position));
        }
    }
}