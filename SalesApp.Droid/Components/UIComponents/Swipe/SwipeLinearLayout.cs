using System;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace SalesApp.Droid.Components.UIComponents.Swipe
{
    public class SwipeLinearLayout : LinearLayout
    {
        private float lastX;
        private float lastY;
        private event EventHandler<SwipeEventArgs> swipe;
        public event EventHandler<SwipeEventArgs> Swipe
        {
            add
            {
                if (swipe == null || !swipe.GetInvocationList().Contains(value))
                {
                    swipe += value;
                }
            }
            remove
            {
                swipe -= value;
            }
        }

        protected SwipeLinearLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public SwipeLinearLayout(Context context) : base(context)
        {
        }

        public SwipeLinearLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public SwipeLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public SwipeLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            ViewConfiguration vc = ViewConfiguration.Get(Context);
            float scaledTouchSlop = vc.ScaledTouchSlop;

            if (ev.Action == MotionEventActions.Down)
            {
                lastX = ev.RawX;
                lastY = ev.RawY;

                // Bubble the event down to any child views
                return false;
            }

            if (ev.Action == MotionEventActions.Move)
            {
                float deltaX = ev.RawX - lastX;
                float deltaY = ev.RawY - lastY;

                if (Math.Abs(deltaX) < scaledTouchSlop)
                {
                    // does not meet the threshhold for a horizontal scroll
                    return false;
                }

                if (this.swipe == null)
                {
                    // Bubble the event down to any child views as no one is subscribe to our touch events
                    return false;
                }

                // A swipe occurred, do something. Don't bubble the event to child views
                this.swipe(this, new SwipeEventArgs(deltaX, deltaY));
                return true;
            }

            // Bubble the event down to any child views
            return false;
        }
    }
}