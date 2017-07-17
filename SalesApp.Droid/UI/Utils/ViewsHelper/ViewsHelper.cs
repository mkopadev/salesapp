using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.Views;
using Android.Widget;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.UI.Utils.ViewsHelper
{
    public class ViewsHelper<TObject> where TObject : new()
    {




        private readonly Activity _activity;
        private TObject _obj;
        private readonly View _containerView;
        private readonly Dictionary<int, string> _bindings;
        private readonly Dictionary<View, List<KeyValuePair<int, Predicate<string>>>> _viewEnablingPredicates;

        public ViewsHelper(Activity activity, View containerView)
        {
            if (activity == null)
            {
                throw new Exception("An instance of Activity that also inherits ActivityBase must be passed to views helper. You did not do this.");
            }
            _activity = activity;
            _obj = new TObject();
            _containerView = containerView;
            _bindings = new Dictionary<int, string>();
            _viewEnablingPredicates = new Dictionary<View, List<KeyValuePair<int, Predicate<string>>>>();
        }

        public ViewsHelper<TObject> BindEditText(int viewId, string propertyName)
        {
            if (_obj == null)
            {
                throw new Exception(
                    "No data holding object has been passed into the BindingBuilder, cannot perform BindReader");
            }
            if (_bindings.ContainsKey(viewId))
            {
                _bindings.Remove(viewId);
            }
            _bindings.Add(viewId, propertyName);
            return this;
        }

        public void WriteBoundViews(TObject obj)
        {
            _activity.RunOnUiThread(
                    () =>
                    {
                        if (_bindings == null || _bindings.Count == 0)
                        {
                            return;
                        }
                        _obj = (TObject)obj;
                        foreach (var binding in _bindings)
                        {
                            if (_obj == null)
                            {
                                Write(binding.Key, string.Empty);
                            }
                            else
                            {
                                PropertyInfo propBound = _obj.GetType().GetProperty(binding.Value);
                                if (propBound == default(PropertyInfo))
                                {
                                    throw new Exception(string.Format(
                                        "Cannot find property '{0}' in object '{1}'",
                                        binding.Key, _obj.GetType().FullName));
                                }
                                if (!propBound.CanRead)
                                {
                                    throw new Exception(String.Format("Property '{0}' of object '{1}' is write-only",
                                        propBound.Name, _obj.GetType().FullName));
                                }
                                object value = propBound.GetValue(_obj);
                                Write(binding.Key, value != null ? value.ToString() : "");
                            }
                        }
                    }
                );
        }

        public TObject Read(bool isOnUiThread = false)
        {
            TObject result = default(TObject);
            /*if (!isOnUiThread)
            {
                _activity.RunOnUiThread
                    (
                        () =>
                        {
                            result = Read(true);
                        }
                    );
                return result;
            }*/

            foreach (var binding in _bindings)
            {
                PropertyInfo propInfo = _obj.GetType().GetProperty(binding.Value);
                if (propInfo != null && propInfo.CanWrite)
                {
                    propInfo.SetValue(_obj, Read(binding));
                }
            }
            return _obj;
        }

        private string Read(KeyValuePair<int, string> binding, bool isOnUiThread = false)
        {
            string value = "";
            /*if (!isOnUiThread)
            {
                _activity.RunOnUiThread
                    (
                        () =>
                        {
                            value = Read(binding, true);
                        }
                    );
                return value;
            }
            */
            TextView tv = _containerView.FindViewById<TextView>(binding.Key);
            if (tv != null)
            {
                return tv.Text;
            }

            return "";
        }

        public ViewsHelper<TObject> Write(int viewId, string text)
        {
            _activity.RunOnUiThread
                (
                    () =>
                    {
                        View view = _containerView.FindViewById(viewId);
                        if (view == null)
                        {
                            throw new Exception("Could not find view with specified id. Cannot write.");
                        }
                        PropertyInfo propText = view.GetType().GetProperty("Text");
                        if (propText == default(PropertyInfo))
                        {
                            throw new Exception(
                                "Specified view does not have a 'Text' property. Could not write value to it");
                        }
                        propText.SetValue(view, text);
                        SetViewEnabledState();
                    }
                );
            return this;
        }

        public void SetViewEnabledState()
        {
            foreach (var targetView in _viewEnablingPredicates.Keys)
            {
                SetViewEnabledState(targetView);
            }
        }

        public void SetViewEnabledState(View targetView)
        {
            if (!_viewEnablingPredicates.ContainsKey(targetView))
            {
                return;
            }
            SetViewEnabledState(targetView, _viewEnablingPredicates[targetView]);
        }

        public ViewsHelper<TObject> Write(int spinnerId, List<string> items, int selectedPosition = 0,
            Action<View, string> itemSelectedCallback = default(Action<View, string>))
        {
            _activity.RunOnUiThread(() =>
            {
                Spinner spinner = _containerView.FindViewById<Spinner>(spinnerId);
                if (spinner == null)
                {
                    throw new Exception("Could not find spinner with specified id. Cannot write.");
                }

                // clear the spinner

                items = items.Where(item => item != null).ToList();
                if (items == null)
                {
                    items = new List<string>();
                }

                if (items.Count == 0 && !string.IsNullOrEmpty(spinner.Prompt))
                {
                    items.Add(spinner.Prompt);
                }

                spinner.Adapter = new DefaultSpinnerAdapter().GetAdapter(items.ToArray(), _activity);

                spinner.SetSelection(selectedPosition);
                if (itemSelectedCallback != default(Action<View, string>))
                {
                    spinner.ItemSelected +=
                        new EventHandler<AdapterView.ItemSelectedEventArgs>(
                            (object sender, AdapterView.ItemSelectedEventArgs e) =>
                            {
                                Spinner spin = (Spinner)sender;
                                if (e.View == null)
                                {
                                    return;
                                }
                                PropertyInfo propInfo = e.View.GetType().GetProperty("Text");
                                if (propInfo == null)
                                {
                                    return;
                                }
                                object text = propInfo.GetValue(e.View);
                                if (text == null)
                                {
                                    return;
                                }
                                itemSelectedCallback(e.View, text.ToString());
                            });
                }
            });
            return this;
        }

        public ViewsHelper<TObject> CompileBindings()
        {


            foreach (var binding in _bindings)
            {
                PropertyInfo propertyInfo = _obj.GetType().GetProperty(binding.Value);
                if (propertyInfo == null)
                {
                    throw new Exception(
                        string.Format(
                                "Error building bindings, could not find property called '{0}' in object of type '{1}'. Please check property spelling.",
                                binding.Value,
                                _obj.GetType()));
                }
            }
            //UnbundleBindings();
            return this;
        }

        public ViewsHelper<TObject> BindEvent(BindableEvents bindableEvent, int viewId, Action<View> callback)
        {
            _activity.RunOnUiThread(
                    () =>
                    {
                        View view = _containerView.FindViewById(viewId);
                        if (view == null)
                        {
                            throw new Exception(
                                string.Format(
                                    "Could not bind to event '{0}' as view with specified id could not be found",
                                    bindableEvent.ToString()));
                        }
                        if (bindableEvent == BindableEvents.OnLostFocus)
                        {
                            view.FocusChange += (sender, args) =>
                            {
                                if (args.HasFocus)
                                {
                                    return;
                                }
                                callback(view);
                            };
                        }
                        else if (bindableEvent == BindableEvents.OnClick)
                        {
                            view.Click += (sender, args) => callback(view);
                        }
                        else if (bindableEvent == BindableEvents.SpinnerItemSelected)
                        {
                            Spinner spinner = view as Spinner;
                            if (spinner == null)
                            {
                                throw new Exception("Could not bind event as specified view is not a spinner");
                            }
                            spinner.ItemSelected -= (sender, args) => callback(spinner);
                            spinner.ItemSelected += (sender, args) => callback(spinner);
                        }
                    });

            return this;
        }

        /// <summary>
        /// This method allows for setting a list predicates to allow for a view to be enabled. The Predicate need to return 0 to validate.
        /// The predicate list contains an Int as Key, the resource for content to be checked. Value is the Predicate to run.
        /// </summary>
        /// <param name="targetView">View to be enabled</param>
        /// <param name="checks">Predicates to run on the content of the view</param>
        /// <returns>Reference to self (Builder)</returns>
        public ViewsHelper<TObject> ViewEnabledPredicates(
            View targetView,
            List<KeyValuePair<int, Predicate<string>>> checks)
        {
            _activity.RunOnUiThread(() =>
            {
                if (_viewEnablingPredicates.ContainsKey(targetView))
                {
                    _viewEnablingPredicates.Remove(targetView);
                }
                foreach (var check in checks)
                {
                    View view = _containerView.FindViewById(check.Key);
                    if (view != null)
                    {
                        EditText editText = view as EditText;
                        if (editText != null)
                        {
                            editText.TextChanged += (sender, args) =>
                            {
                                SetViewEnabledState(targetView, checks);
                            };
                        }
                    }
                }
                _viewEnablingPredicates.Add(targetView, checks);
                SetViewEnabledState(targetView, checks);
            });
            return this;
        }



        /* public bool EntryNotNull(string viewText)
         {
             if (viewText == null)
             {
                 return false;
             }

             return !viewText.IsBlank();
         }*/

        private void SetViewEnabledState(View targetView, List<KeyValuePair<int, Predicate<string>>> checks)
        {
            _activity.RunOnUiThread(
                () =>
                {
                    targetView.Enabled = true;
                    foreach (var check in checks)
                    {
                        View view = _containerView.FindViewById(check.Key);

                        if (view != null && view.Visibility == ViewStates.Visible)
                        {
                            ViewGroup viewParent = view.Parent as ViewGroup;
                            if (viewParent != null && viewParent.Visibility == ViewStates.Visible)
                            {
                                PropertyInfo propertyInfo = view.GetType().GetProperty("Text");
                                if (propertyInfo != null && propertyInfo.CanRead)
                                {
                                    var value = propertyInfo.GetValue(view);
                                    string stringValue = value == null ? string.Empty : value.ToString();
                                    targetView.Enabled = targetView.Enabled && check.Value(stringValue);
                                    if (!targetView.Enabled)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                });
        }
    }
}