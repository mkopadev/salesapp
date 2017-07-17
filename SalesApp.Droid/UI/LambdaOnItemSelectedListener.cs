using System;
using Android.Views;
using Android.Widget;
using Object = Java.Lang.Object;

namespace SalesApp.Droid.UI
{
    public class LambdaOnItemSelectedListener : Object, AdapterView.IOnItemSelectedListener
    {
        private Action<AdapterView, View, int, long> itemSelectedListener;

        public LambdaOnItemSelectedListener(Action<AdapterView, View, int, long> itemSelectedListener)
        {
            this.itemSelectedListener = itemSelectedListener;
        }

        public void OnItemSelected(AdapterView parent, View view, int position, long id)
        {
            itemSelectedListener(parent, view, position, id);
        }

        public void OnNothingSelected(AdapterView parent)
        {
        }
    }
}