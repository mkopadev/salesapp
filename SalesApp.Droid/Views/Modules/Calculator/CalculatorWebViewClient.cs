using Android.Graphics;
using Android.Webkit;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Droid.Views.Modules.Calculator
{
    public class CalculatorWebViewClient : WebViewClient
    {
        private readonly ILog logger = Resolver.Instance.Get<ILog>();
        public override bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            view.LoadUrl(url);
            return true;
        }
        public override void OnPageStarted(WebView view, string url, Bitmap favicon)
        {
            base.OnPageStarted(view, url, favicon);
            logger.Debug("An error occured when loading calculator : " + " Url " + url);
        }

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            logger.Debug("An error occured when loading calculator : " +  " Url " + url);

        }
        public override void OnReceivedError(WebView view, ClientError errorCode, string description, string failingUrl)
        {
            base.OnReceivedError(view, errorCode, description, failingUrl);
            logger.Debug("An error occured when loading calculator : " + description +" Error code "+errorCode +" Url "+failingUrl);
        }
    }
}