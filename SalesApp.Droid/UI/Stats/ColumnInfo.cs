using Android.Views;

namespace SalesApp.Droid.UI.Stats
{
    public class ColumnInfo
    {
        public int Weight { get; set; }

        public GravityFlags Gravity { get; set; }

        public ColumnInfo(int weight, GravityFlags gravity = GravityFlags.Center )
        {
            Gravity = gravity;
            this.Weight = weight;
        }
    }
}