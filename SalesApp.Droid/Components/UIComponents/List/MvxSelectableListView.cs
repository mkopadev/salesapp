using System.Collections;
using System.Linq;
using Android.Content;
using Android.Util;
using MvvmCross.Binding.Droid.Views;

namespace SalesApp.Droid.Components.UIComponents.List
{
    public class MvxSelectableListView : MvxListView
    {
        public MvxSelectableListView(Context context, IAttributeSet attrs) : base(context, attrs){}

        public MvxSelectableListView(Context context, IAttributeSet attrs, IMvxAdapter adapter) : base(context, attrs, adapter){}

        public IList SelectedItems
        {
            set
            {
                var objects = this.Adapter.ItemsSource.Cast<object>().ToList();
                foreach (var item in objects)
                {
                    int position = objects.IndexOf(item);
                    bool isChecked = value.Contains(item);
                    this.SetItemChecked(position, isChecked);
                }
            }
        }
    }
}