using System;
using System.Collections.Generic;
using Android.App;

namespace SalesApp.Droid.UI.Utils
{
    /// <summary>
    /// Alert dialog builder class that uses fragaments.
    /// How to instatiate a one button dialog
    /// <code>
    ///   AlertDialogBuilder2.Instance
    ///  .SetText("Are you sure you want to cancel customer registration?")
    ///  .SetButton("Continue", Continue)
    ///  .show(FragmentManager);
    /// </code>
    /// How to instatiate a two button dialog
    /// <code>
    ///   AlertDialogBuilder2.Instance
    ///  .SetText("Are you sure you want to cancel customer registration?")
    ///  .SetButton("Continue", Continue)
    ///  .show(FragmentManager);
    /// </code>
    /// </summary>
    public class AlertDialogBuilder2
    {
        private static string Message { get; set; }

        private static string Title { get; set; }

        private static int Button { get; set; }

        public static AlertDialogBuilder2 Instance = new AlertDialogBuilder2();

        public Dictionary<string, Action> Buttons = new Dictionary<string, Action>();

        public AlertDialogBuilder2()
        {

        }

        public AlertDialogBuilder2 SetText(string message, string title = null)
        {
            Message = message;
            Title = title;
            return this;
        }

        public AlertDialogBuilder2 SetButton(string buttonName, Action callBack)
        {
            if (!Buttons.ContainsKey(buttonName))
            {
                Buttons.Add(buttonName, callBack);
            }

            return this;
        }

        private AlertDialogFragment alert = null;

        public void show(FragmentManager fm)
        {
            alert = AlertDialogFragment.NewInstance(Message, Title, Buttons);
            alert.Show(fm.BeginTransaction(), "Dialog");
        }

        public void Dismiss()
        {
            if (alert != null)
            {
                alert.Dismiss();
            }
        }
    }
}