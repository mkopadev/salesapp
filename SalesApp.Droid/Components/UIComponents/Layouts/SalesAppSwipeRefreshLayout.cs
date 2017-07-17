using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Util;

namespace SalesApp.Droid.Components.UIComponents.Layouts
{
    /// <summary>
    /// Override <see cref="SwipeRefreshLayout" allow initial refresh by tracking when OnMeasure has been called. />
    /// </summary>
    public class SalesAppSwipeRefreshLayout : SwipeRefreshLayout
    {
        private bool _measured;
        private bool _preMeasureRefreshing;

        protected SalesAppSwipeRefreshLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public SalesAppSwipeRefreshLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public SalesAppSwipeRefreshLayout(Context context) : base(context)
        {
        }

        public override bool Refreshing
        {
            get { return base.Refreshing; }
            set
            {
                if (_measured)
                {
                    base.Refreshing = value;
                }
                else
                {
                    this._preMeasureRefreshing = value;
                }
            }
        }

        public override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            if (!_measured)
            {
                this._measured = true;
                this.Refreshing = _preMeasureRefreshing;
            }
        }
    }
}