using Android.Views;

namespace SalesApp.Droid.Framework
{
    public static class ViewExtensions
    {
        public static void SetEnabledAll(this View v, bool enabled)
        {
            v.Enabled = enabled;
            v.Focusable = enabled;

            var @group = v as ViewGroup;
            if (@group != null)
            {
                ViewGroup vg = @group;
                for (int i = 0; i < vg.ChildCount; i++)
                {
                    SetEnabledAll(vg.GetChildAt(i), enabled);
                }
            }
        }

        public static View GetView(this ViewGroup viewGroup, int viewId)
        {
            View targetView = viewGroup.FindViewById(viewId);
            if (targetView != null)
            {
                return targetView;
            }
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                ViewGroup nextViewGroup = viewGroup.GetChildAt(i) as ViewGroup;
                if (nextViewGroup != null)
                {
                    return nextViewGroup.GetView(viewId);
                }
            }
            return null;
        }
    }
}