using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using SalesApp.Droid.UI.Stats;

namespace SalesApp.Droid.UI.SwipableViews
{
    public class SwipableScrollView : ScrollView
    {
        protected SwipableScrollView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public SwipableScrollView(Context context) : base(context)
        {
        }

        public SwipableScrollView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public SwipableScrollView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public SwipableScrollView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }
        
        protected override void OnScrollChanged(int x, int y, int oldx, int oldy)
        {
            ISwipeRefreshable swipeRefreshable = (ISwipeRefreshable) Context;
            if (swipeRefreshable == null)
            {
                return;
            }

            swipeRefreshable.ScrollYChanged(new ScrollInformation { Sender = this, Y = y });
        }
    }
}