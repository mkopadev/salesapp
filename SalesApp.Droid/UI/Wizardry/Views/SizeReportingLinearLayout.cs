using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace SalesApp.Droid.UI.Wizardry.Views
{
    public class SizeReportingLinearLayout : LinearLayout
    {

        public event EventHandler<SizeChangedEventArgs> SizeChanged; 

        protected SizeReportingLinearLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public SizeReportingLinearLayout(Context context) : base(context)
        {
        }

        public SizeReportingLinearLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public SizeReportingLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public SizeReportingLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            if (SizeChanged != null)
            {
                SizeChanged(this,new SizeChangedEventArgs(w,h,oldw,oldh));
            }
        }
    }
}