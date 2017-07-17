using System;
using Android.Content;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;

namespace SalesApp.Droid.Components.UIComponents
{
    public class ClickControlledTabLayout : TabLayout
    {
        public bool CanClick { get; set; }

        public ClickControlledTabLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            this.CanClick = true;
        }

        public ClickControlledTabLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            this.CanClick = true;
        }

        public ClickControlledTabLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            this.CanClick = true;
        }

        public ClickControlledTabLayout(Context context) : base(context)
        {
            this.CanClick = true;
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (this.CanClick)
            {
                return false;
            }

            return true;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (this.CanClick)
            {
                return false;
            }

            return true;
        }
    }
}