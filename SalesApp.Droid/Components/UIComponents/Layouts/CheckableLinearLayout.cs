using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace SalesApp.Droid.Components.UIComponents.Layouts
{
    public class CheckableLinearLayout : LinearLayout, ICheckable
    {
        private int[] _checkedStateSet = { Android.Resource.Attribute.StateChecked };
        private bool _checked;

        public CheckableLinearLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CheckableLinearLayout(Context context) : base(context)
        {
        }

        public CheckableLinearLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public CheckableLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public CheckableLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        public  void Toggle()
        {
            Checked = !_checked;
        }
        

        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                if (value != _checked)
                {
                    _checked = value;
                    RefreshDrawableState();
                }
            }
        }

        protected override int[] OnCreateDrawableState(int extraSpace)
        {
            int[] drawableState = base.OnCreateDrawableState(extraSpace + 1);
            if (Checked)
            {
                MergeDrawableStates(drawableState, _checkedStateSet);
            }

            return drawableState;
        }
    }
}