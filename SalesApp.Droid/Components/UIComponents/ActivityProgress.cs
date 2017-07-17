using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SalesApp.Droid.Components.UIComponents
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class ActivityProgress : ActivityBase
    {
        public const string Title = "Title";
        public const string Story = "Story";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_two_frame_layout);
            ShowProgressFragment
                (
                    Intent.GetIntExtra(Title,0)
                    ,Intent.GetIntExtra(Story,0)
                );
        }

        public override void SetViewPermissions()
        {
            
        }

        void ShowProgressFragment(int title,int story)
        {
            var progressFragment = new ProgressFragment();
            Bundle arguments = new Bundle();
            arguments.PutString(ProgressFragment.TitleKey, GetString(Resource.String.reset_pin_title));
            arguments.PutString(ProgressFragment.MessageKey, GetString(Resource.String.resetting_pin));
            progressFragment.Arguments = arguments;

            GetFragmentManager()
                .BeginTransaction()
                .Replace(Resource.Id.modalContent, progressFragment)
                .Commit();
        }
    }
}