using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Webkit;
using SalesApp.Droid.Services.GAnalytics;
using Fragment = Android.Support.V4.App.Fragment;

namespace SalesApp.Droid.Views.Modules.Calculator
{
    public class CalculatorModuleFragment : Fragment, IPreviousNavigator
    {
        private IFragmentLoadStateListener _fragmentLoadStateListener;
        private WebView _webView;
        public string IndexUrl
        {
            get
            {
                return "file:///android_asset/WebApps/Calculator/index.html#cost";
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _webView = (WebView)inflater.Inflate(Resource.Layout.fragment_calculator_module, container, false);
            var settings = _webView.Settings;
            _webView.SetWebViewClient(new CalculatorWebViewClient());
            _webView.SetScrollContainer(false);
            settings.AllowUniversalAccessFromFileURLs = true;
            _webView.AddJavascriptInterface(new CalculatorInteface(Activity), "CSharp");
            settings.AllowFileAccessFromFileURLs = true;
            settings.JavaScriptEnabled = true;
            settings.DomStorageEnabled =true;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                WebView.SetWebContentsDebuggingEnabled(true);
                _webView.SetLayerType(LayerType.Hardware, null);
            }
            else
            {
                _webView.SetLayerType(LayerType.Software, null);
            }

            _webView.LoadUrl(this.IndexUrl);

            GaTracking();
            return _webView;
        }

        private void GaTracking()
        {
            string category = Activity.GetString(Resource.String.module_calculator);
            string label = Activity.GetString(Resource.String.calculator_label);
            string action = Activity.GetString(Resource.String.calculator_action);

            GoogleAnalyticService.Instance.TrackEvent(category, label, action);
        }

        public override void OnResume()
        {
            base.OnResume();
            this._fragmentLoadStateListener.IndicatorStateChanged(false);
            this._fragmentLoadStateListener.CanRegisterChanged(true);
            this._fragmentLoadStateListener.RequestOrintation(ScreenOrientation.Nosensor);
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            this._fragmentLoadStateListener = activity as IFragmentLoadStateListener;
        }

        public bool Previous()
        {
            _webView.GoBack();
            return false;
        }
    }
}