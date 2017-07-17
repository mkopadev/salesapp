using Android.App;
using Android.Content.PM;
using Android.OS;
using Newtonsoft.Json;
using SalesApp.Core.Extensions;
using SalesApp.Droid.Components.UIComponents.CustomInfo;

namespace SalesApp.Droid.Components.UIComponents
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class ActivityMessage : ActivityBase2
    {
        public const string PromptTitle = "title";
        public const string Message = "message";
        public const string PositiveButton = "positiveButton";
        public const string NegativeButton = "negativeButton";
        public const string ActionBarTitle = "ActionBarTitle";
        public const string DynamicValues = "DynamicValues";


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.activity_two_frame_layout);
            ShowMessageFragment
                (
                    Intent.GetIntExtra(ActionBarTitle, 0)
                    , Intent.GetIntExtra(PromptTitle, 0)
                    , Intent.GetIntExtra(Message, 0)
                    , Intent.GetIntExtra(PositiveButton, 0)
                    , Intent.GetIntExtra(NegativeButton, 0)
                    , Intent.GetStringExtra(DynamicValues)
                );

        }

        public override void SetViewPermissions()
        {
            
        }


        private void ShowMessageFragment(int actionBarTitle, int title, int message, int positiveButton, int negativeButton,string dynamicValues)
        {
            string btnNegative = "";
            if (negativeButton > 0)
            {
                btnNegative = GetText(negativeButton).ToTitleCase();
            }
            string msg = GetText(message);
            if (dynamicValues.IsBlank() == false)
            {
                object[] vals = JsonConvert.DeserializeObject<object[]>(dynamicValues);
                msg = msg.GetFormated(vals);
            }

            CustomInfoFragment.Info info = new CustomInfoFragment.Info()
            {
                ActionBarTitle = GetText(actionBarTitle),
                Title = GetText(title),
                Content = msg,
                PositiveButtonCaption = GetText(positiveButton).ToTitleCase(),
                NegativeButtonCaption = btnNegative
            };

            CustomInfoFragment customInfoFragment = new CustomInfoFragment();
            customInfoFragment.SetArgument(CustomInfoFragment.InfoKey, info);

            GetFragmentManager()
                .BeginTransaction()
                .Replace
                (
                    Resource.Id.modalContent
                    , customInfoFragment
                )
                .Commit();
            customInfoFragment.PositiveAction += customInfoFragment_PositiveAction;
            customInfoFragment.NegativeAction += customInfoFragment_NegativeAction;

        }

        void customInfoFragment_NegativeAction()
        {
            SetResult(Result.Canceled);
            Finish();
        }

        void customInfoFragment_PositiveAction()
        {
            SetResult(Result.Ok);
            Finish();
        }

        public override void InitializeScreen()
        {
        }

        public override void RetrieveScreenInput()
        {
        }

        public override void UpdateScreen()
        {
        }

        public override void SetListeners()
        {
        }

        public override bool Validate()
        {
            return true;
        }
    }
}