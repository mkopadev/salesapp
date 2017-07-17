using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using SalesApp.Core.Extensions;

namespace SalesApp.Droid.UI.Utils
{
    public class AlertDialogBuilder
    {
        private AlertDialog alertDialog;
        Dictionary<int,Action> _buttonsAndCallbacks = new Dictionary<int, Action>(); 


        private void Reset()
        {
            alertDialog = null;
            _buttonsAndCallbacks = new Dictionary<int, Action>();
            Indeterminate = true;
            Cancellable = false;
            Title = "";
            Message = "";
            IntTitle = 0;
            IntMessage = 0;
            Context = null;
        }

        private AlertDialogBuilder()
        {
            Reset();
        }

        private static List<AlertDialogBuilder> _builders = new List<AlertDialogBuilder>();

        private static void Kill(AlertDialogBuilder builder)
        {
            _builders.Remove(builder);
            builder = null;
        }

        public static AlertDialogBuilder Instance
        {
            get
            {
                _builders.Add(new AlertDialogBuilder());
                return _builders[_builders.Count - 1];
            }
        }

        public AlertDialogBuilder SetText(string title, string message)
        {
            Title = title;
            Message = message;

            return this;
        }

        private string Message { get; set; }

        private string Title { get; set; }

        public AlertDialogBuilder SetText(int title,int message)
        {
            IntTitle = title;
            IntMessage = message;
            return this;
        }

        

        public AlertDialogBuilder AddButton(int buttonText,Action callback)
        {
            if (_buttonsAndCallbacks.Count >= 3)
            {
                throw new Exception("Cannot add more than three buttons to the dialog");
            }
            if (_buttonsAndCallbacks.ContainsKey(buttonText))
            {
                _buttonsAndCallbacks[buttonText] = () =>
                {
                    callback();
                    Kill(this);
                };
            }
            else
            {
                _buttonsAndCallbacks.Add
                    (
                        buttonText, () =>
                        {
                            callback();
                            Kill(this);
            }
                    );
            }

            return this;
        }

        public void Show(Context context, bool indeterminate = true, bool cancellable = false)
        {
            if (_buttonsAndCallbacks.Count == 0)
            {
                throw new Exception("No buttons have been added to dialog");
            }

            SetContext(context);
            SetCancellable(cancellable);
            SetIndeterminate(indeterminate);
            SetDialogType((DialogType)_buttonsAndCallbacks.Count);
            Show(this);
        }

        private AlertDialogBuilder SetIndeterminate(bool indeterminate)
        {
            Indeterminate = indeterminate;
            return this;
        }

        private AlertDialogBuilder SetDialogType(DialogType Type)
        {
            DialogType = Type;
            return this;
        }

        private AlertDialogBuilder SetContext(Context ctx)
        {
            Context = ctx;
            return this;
        }

        private AlertDialogBuilder SetCancellable(bool cancel)
        {
            Cancellable = cancel;
            return this;
        }

        private Context Context { get; set; }

        private int IntTitle { get; set; }

        private int IntMessage { get; set; }

        private DialogType DialogType { get; set; }

        private bool Indeterminate { get; set; }

        private bool Cancellable { get; set; }

        private static Context _context;
        private static string _message;
        private static AlertDialog.Builder builder;

        private void Show(AlertDialogBuilder dm)
        {
            _context = dm.Context; 
            builder = new AlertDialog.Builder(_context);

            CreateDialog(builder, dm);
        }

        string GetButtonText(int index)
        {
            int val = _buttonsAndCallbacks.Count > index ? _buttonsAndCallbacks.Keys.ElementAt(index) : 0;
            if (val == 0)
            {
                return "";
            }
            return _context.GetString(val);
        }

        public void Close()
        {
            if (alertDialog != null)
            {
                alertDialog.Cancel();
            }
        }

        private void CreateDialog(AlertDialog.Builder builder1, AlertDialogBuilder dialogMessage)
        {
            alertDialog = builder.Create();

            int mes = dialogMessage.IntMessage == default(int) ? 0 : dialogMessage.IntMessage;
            string message = string.Empty;

            if (mes > 0)
            {
                message = _context.GetString(dialogMessage.IntMessage);
            }

            int tit = dialogMessage.IntTitle == default(int) ? 0 : dialogMessage.IntTitle;
            if (tit > 0 || !string.IsNullOrEmpty(Title))
            {
                string title = !Title.IsBlank() ? Title : _context.GetString(tit);
                if (!string.IsNullOrEmpty(title))
                {
                    alertDialog.SetTitle(title);
                }
            }
            if (!Message.IsBlank())
            {
                message = Message;
            }
            alertDialog.SetMessage(message);
            alertDialog.SetCancelable(dialogMessage.Cancellable);
            string button1Text = GetButtonText(0);
            string button2Text = GetButtonText(1);
            string button3Text = GetButtonText(2);

            alertDialog.SetButton(
                    button1Text,
                    (sender, args) => _buttonsAndCallbacks[_buttonsAndCallbacks.Keys.ElementAt(0)]());

            if (button2Text.IsBlank() == false)
            {
                alertDialog.SetButton2(
                    button2Text,
                    (sender, args) => _buttonsAndCallbacks[_buttonsAndCallbacks.Keys.ElementAt(1)]());
            }

            if (button3Text.IsBlank() == false)
            {
                alertDialog.SetButton3(
                    button2Text,
                    (sender, args) => _buttonsAndCallbacks[_buttonsAndCallbacks.Keys.ElementAt(2)]());
            }

            alertDialog.Show();
        }
    }
}