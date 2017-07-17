using System;
using Android.Views;
using Object = Java.Lang.Object;

namespace SalesApp.Droid.UI
{
    public class LambdaScrollChangedListener : Object, ViewTreeObserver.IOnScrollChangedListener
    {
        private Action _scrollChangedListener;

        public LambdaScrollChangedListener(Action scrollChangedListener)
        {
            _scrollChangedListener = scrollChangedListener;
        }

        public void OnScrollChanged()
        {
            _scrollChangedListener();
        }
    }
}