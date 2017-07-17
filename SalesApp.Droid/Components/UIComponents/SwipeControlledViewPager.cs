using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;

namespace SalesApp.Droid.Components.UIComponents
{
    public class SwipeControlledViewPager : ViewPager
    {
        public bool AllowSwipe { get; set; }

        public SwipeControlledViewPager(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            AllowSwipe = true;
        }

        public SwipeControlledViewPager(Context context) : base(context)
        {
            AllowSwipe = true;
        }

        public SwipeControlledViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            AllowSwipe = true;
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (AllowSwipe)
            {
                return base.OnInterceptTouchEvent(ev);
            }

            return false;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (AllowSwipe)
            {
                return base.OnTouchEvent(e);
            }

            return false;
        }
    }
}