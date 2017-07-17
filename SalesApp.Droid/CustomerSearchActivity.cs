using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Api;
using SalesApp.Droid.Api;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid
{
    [Activity(Label = "Registration Status")]
    public class CustomerSearchActivity : ActivityBase
    {
        ISalesAppApi salesAppApi;
        Button searchButtom;
        EditText phoneNumberView;

        public CustomerSearchActivity()
        {
            salesAppApi = new SalesAppApi();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            SetContentView(Resource.Layout.layout_customerSearch);
            searchButtom = FindViewById<Button>(Resource.Id.btnSearchProspect);
            phoneNumberView = FindViewById<EditText>(Resource.Id.customerSearchPhoneNumber);

            searchButtom.Click += searchButtom_Click;
        }

        public override void SetViewPermissions()
        {
            
        }

        async void searchButtom_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(phoneNumberView.Text) && !string.IsNullOrWhiteSpace(phoneNumberView.Text))
            {
                string phoneNumber = phoneNumberView.Text;
                var progressDialog = ProgressDialog.Show(this, GetString(Resource.String.please_wait), GetString(Resource.String.loading_registration_status), true);

                new Thread(new ThreadStart(async delegate
                {
                    //refresh new messages from server
                    await GetStatus(phoneNumber,progressDialog);
                })).Start();
            }
        }

        private async Task GetStatus(string phoneNumber, ProgressDialog progressDialog)
        {
            try
            {
                //TODO check for number format
                var record = await salesAppApi.GetRegistrationSteps(phoneNumber);
                RunOnUiThread(() => progressDialog.Hide());

                if (record == null)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, GetString(Resource.String.customer_record_not_found_), ToastLength.Long).Show();
                    });
                }
                else
                {
                    Intent intent = new Intent(this, typeof(CustomerRegistrationStepsActivity));
                    var jsonData = JsonConvert.SerializeObject(record);
                    intent.PutExtra("customerRecord", jsonData);

                    StartActivity(intent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}