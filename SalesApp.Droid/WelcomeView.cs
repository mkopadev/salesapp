using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using SalesApp.Droid.Views.ManageStock;

namespace SalesApp.Droid
{
	public class WelcomeView: BaseView
	{
		private TextView _textCustomerName;

        public event EventHandler RegisterProspectTouched;
        public event EventHandler RegisterCustomerTouched;
        public event EventHandler SwapComponentsTouched;

        public WelcomeView(ViewGroup root, Context context) : base(context)
        {
            ViewGroup btnNewProspect = root.FindViewById<ViewGroup>(Resource.Id.btnNewProspect);
            ViewGroup btnNewCustomer = root.FindViewById<ViewGroup>(Resource.Id.btnNewCustomer);
            ViewGroup btnSwapComponents = root.FindViewById<ViewGroup>(Resource.Id.btnSwapComponents);
            ViewGroup manageStockButton = root.FindViewById<ViewGroup>(Resource.Id.button_manage_stock);
            _textCustomerName = root.FindViewById<TextView>(Resource.Id.textCustomerName);
            TextView heading = root.FindViewById<TextView>(Resource.Id.txtHeading);

            heading.Text = context.Resources.GetString(Resource.String.hello);
            btnNewProspect.Click += (o, e) => OnRegisterProspectTouched(EventArgs.Empty);
            btnNewCustomer.Click += (o, e) => OnRegisterCustomerTouched(EventArgs.Empty);
            btnSwapComponents.Click += (o, e) => OnSwapComponentsTouched(EventArgs.Empty);
            manageStockButton.Click += this.ManageStockButtonOnClick;
        }

	    private void ManageStockButtonOnClick(object sender, EventArgs eventArgs)
	    {
            this.context.StartActivity(new Intent(this.context, typeof(ManageStockView)));
	    }

	    public void SetUser(string firstName, string lastName)
		{
			_textCustomerName.Text = string.Format("{0} {1}", firstName, lastName);
		}

		protected void OnRegisterProspectTouched(EventArgs e)
		{
			EventHandler handler = RegisterProspectTouched;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		protected void OnRegisterCustomerTouched(EventArgs e)
		{
			EventHandler handler = RegisterCustomerTouched;
			if (handler != null) {
				handler (this, e);
			}
		}

        protected void OnSwapComponentsTouched(EventArgs e)
        {
            EventHandler handler = SwapComponentsTouched;
            if (handler != null)
            {
                handler(this, e);
            }
        }
	}
}