using System;
using Android.Content;
using SalesApp.Core.Extensions;

namespace SalesApp.Droid.People.Prospects
{
    public class IntentStartPointTracker
    {
        public enum IntentStartPoint
        {
            Undefined = 0,
            WelcomeScreen = 1,
            ProspectsList = 2,
            CustomerList = 3,
            ProspectConversion = 4,
            Modules = 5,
            TicketList = 6
        }

        public const string ActivityStartPoint = "ActivityStartPoint";


        public void StartIntentWithTracker(Context context, IntentStartPoint startingPoint, Type intentToStart,Intent intent = null)
        {
            if (intent == null)
            {
                intent = GetIntentWithTracking(context, intentToStart, startingPoint);
            }
            else
            {
                intent.PutExtra(ActivityStartPoint, startingPoint.ToString());
            }
            
            context.StartActivity(intent);
        }

        public IntentStartPoint GetStartPoint(Intent intent)
        {
            if (intent != null)
            {
                string activityStartPointStr = intent.GetStringExtra(ActivityStartPoint);
                if (activityStartPointStr != null)
                    return activityStartPointStr.ToEnumValue<IntentStartPoint>();
            }

            return IntentStartPoint.Undefined;
        }

        public Intent GetIntentWithTracking(Context context, Type typeToStart, IntentStartPoint startingPoint)
        {
            Intent intent = new Intent(context, typeToStart);
            intent.PutExtra(ActivityStartPoint, startingPoint.ToString());
            return intent;
        }


        
    }
}