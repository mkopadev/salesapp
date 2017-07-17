using System;
using Android.App;
using Android.OS;
using Android.Widget;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace SalesApp.Droid.Components.UIComponents.DateAndTime
{
    class FragmentDatePicker : DialogFragment , DatePickerDialog.IOnDateSetListener
    {
        public event EventHandler<EventArgs> DateSelected;
        public const string TagDateFragment = "TagDateFragment";
        private DateTime _date;


        public FragmentDatePicker(DateTime date)
        {
            _date = date;
        }

        public override Dialog OnCreateDialog(Bundle savedInstance)
        {
            return new DatePickerDialog
                (
                    Activity
                    ,this
                    , _date.Year
                    , _date.Month - 1
                    , _date.Day
                );
        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            if (DateSelected != null)
            {
                DateSelected.Invoke
                    (
                        new DateTime(year,monthOfYear + 1,dayOfMonth)
                        ,EventArgs.Empty
                    );
            }
        }
    }
}