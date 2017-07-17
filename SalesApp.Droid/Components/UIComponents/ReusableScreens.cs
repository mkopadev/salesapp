using Android.App;
using Android.Content;
using Newtonsoft.Json;

namespace SalesApp.Droid.Components.UIComponents
{
    public class ReusableScreens
    {
        private readonly Activity _activity;
        public const int ResultProgressScreen = 1000;
        public const int ResultMessageScreen = 1001;

        public ReusableScreens(Activity activity)
        {
            _activity = activity;
        }

        public void ShowProgress(int title, int story)
        {
            Intent intent = new Intent(_activity, typeof(ActivityProgress));
            intent.PutExtra(ActivityProgress.Title, title);
            intent.PutExtra(ActivityProgress.Story, story);
            _activity.StartActivityForResult(intent, ResultProgressScreen);
        }

        public void HideProgress()
        {

            _activity.FinishActivity(ResultProgressScreen);
        }


        public void ShowQuestion(int actionBarTitle, int title, int message, int positiveButton, int negativeButton)
        {
            Intent intent = new Intent(_activity,typeof(ActivityMessage));
            intent.PutExtra(ActivityMessage.ActionBarTitle, actionBarTitle);
            intent.PutExtra(ActivityMessage.Message, message);
            intent.PutExtra(ActivityMessage.PromptTitle, title);
            intent.PutExtra(ActivityMessage.PositiveButton, positiveButton);
            intent.PutExtra(ActivityMessage.NegativeButton, negativeButton);

            _activity.StartActivityForResult(intent,ResultMessageScreen);
        }

        public void ShowInfo(int actionBarTitle, int title, int message, int positiveButton, params object[] dynamicValues)
        {
            Intent intent = new Intent(_activity, typeof(ActivityMessage));
            intent.PutExtra(ActivityMessage.ActionBarTitle, actionBarTitle);
            intent.PutExtra(ActivityMessage.Message, message);
            intent.PutExtra(ActivityMessage.PromptTitle, title);
            intent.PutExtra(ActivityMessage.PositiveButton, positiveButton);
            if (dynamicValues != null && dynamicValues.Length > 0)
            {
                intent.PutExtra(ActivityMessage.DynamicValues, JsonConvert.SerializeObject(dynamicValues));
            }

            _activity.StartActivityForResult(intent, ResultMessageScreen);
        }

        public void ShowInfo(int actionBarTitle, int title, string message, int positiveButton, params object[] dynamicValues)
        {
            Intent intent = new Intent(_activity, typeof(ActivityMessage));
            intent.PutExtra(ActivityMessage.ActionBarTitle, actionBarTitle);
            intent.PutExtra(ActivityMessage.Message, message);
            intent.PutExtra(ActivityMessage.PromptTitle, title);
            intent.PutExtra(ActivityMessage.PositiveButton, positiveButton);
            if (dynamicValues != null && dynamicValues.Length > 0)
            {
                intent.PutExtra(ActivityMessage.DynamicValues, JsonConvert.SerializeObject(dynamicValues));
            }

            _activity.StartActivityForResult(intent, ResultMessageScreen);
        }
    }
}