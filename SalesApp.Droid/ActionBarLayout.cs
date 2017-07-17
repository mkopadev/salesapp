using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace SalesApp.Droid
{
    public class ActionBarLayout : RelativeLayout
    {
        private LayoutInflater inflatorservice;

        public ActionBarLayout(Context context) : base(context)
        {
            this.Initialize(context);
        }

        public ActionBarLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            this.Initialize(context);
        }

        public ActionBarLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            this.Initialize(context);
        }

        private void Initialize(Context context)
        {
            this.inflatorservice = (LayoutInflater) context.GetSystemService(Context.LayoutInflaterService);
            this.inflatorservice.Inflate(Resource.Layout.layout_action_bar, this);
        }
    }
}