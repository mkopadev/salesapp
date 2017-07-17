using System;
using Android.App;
using Android.OS;
using Android.Text.Format;
using Android.Widget;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace SalesApp.Droid.Components.UIComponents.DateAndTime
{
    class FragmentTimePicker : DialogFragment , TimePickerDialog.IOnTimeSetListener
    {
        private DateTime _time;

        public event EventHandler<TimePickerDialog.TimeSetEventArgs> TimeSelected;
        public const string TagFragmentTimePicker = "TagFragmentTimePicker";

        public FragmentTimePicker(DateTime time)
        {
            _time = time;
        }
        
        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            return new TimePickerDialog
                (
                    Activity
                    , TimeSelected
                    , _time.Hour
                    , _time.Minute
                    , DateFormat.Is24HourFormat(Activity)
                );
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            
        }
    }
}