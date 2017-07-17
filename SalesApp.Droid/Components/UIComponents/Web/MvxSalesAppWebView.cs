using System;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Webkit;

namespace SalesApp.Droid.Components.UIComponents.Web
{
    public class MvxSalesAppWebView : WebView
    {
        private string _text;

        public MvxSalesAppWebView(Context context, IAttributeSet attrs): base(context, attrs)
        {
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                _text = value;

                SetBackgroundColor(Color.Transparent);
                LoadData(_text, "text/html", "utf-8");
                UpdatedHtmlContent();
            }
        }

        public event EventHandler HtmlContentChanged;

        private void UpdatedHtmlContent()
        {
            var handler = HtmlContentChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}