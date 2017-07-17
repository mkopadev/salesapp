using Android.Content.PM;

namespace SalesApp.Droid.Views.Modules
{
    public interface IFragmentLoadStateListener
    {
        void TitleChanged(string newTitle);

        void IndicatorStateChanged(bool newState);

        void CanRegisterChanged(bool canRegister);

        void RequestOrintation(ScreenOrientation newOrientation);
    }
}