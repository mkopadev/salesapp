using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Api;

namespace SalesApp.Droid
{
    [Activity(Label = "Registration Status")]
    public class CustomerRegistrationStepsActivity : Activity
    {        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            SetContentView(Resource.Layout.layout_customerRegistrationSteps);

            var stepsContainer = this.FindViewById<LinearLayout>(Resource.Id.stepsLayoutContainer);
            //doneButtom=this.FindViewById<Button>
            var data = Intent.GetStringExtra("customerRecord");

            CustomerRegistrationStatusDto registrationStatus = JsonConvert.DeserializeObject<CustomerRegistrationStatusDto>(data);
            
            FindViewById<TextView>(Resource.Id.fullName).Text = registrationStatus.CustomerName;
            FindViewById<TextView>(Resource.Id.productType).Text = registrationStatus.CustomerProduct;
            FindViewById<TextView>(Resource.Id.phoneNumber).Text = registrationStatus.CustomerPhone;
            
            foreach (var x in registrationStatus.Steps)
            {
                var view = this.LayoutInflater.Inflate(Resource.Layout.layout_customerRegistrationStepItem, null);
                var statusIcon = view.FindViewById<ImageView>(Resource.Id.statusIcon);
                var registrationStatusLabel = view.FindViewById<TextView>(Resource.Id.registrationLabel);

                registrationStatusLabel.Text = x.StepName;

                if (x.StepStatus.Equals("done", StringComparison.CurrentCultureIgnoreCase))
                {
                    statusIcon.SetImageResource(Resource.Drawable.done);
                }
                else
                {
                    statusIcon.SetImageResource(Resource.Drawable.not_done);
                }

                //add step to view
                stepsContainer.AddView(view);
                
            }

            FindViewById<TextView>(Resource.Id.statusInformation).Text = registrationStatus.Info;
        }
    }
}