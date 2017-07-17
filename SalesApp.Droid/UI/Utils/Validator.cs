using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

namespace SalesApp.Droid.UI.Utils
{
    public class Validator
    {
        private readonly Activity _activity;


        public Validator(Activity activity)
        {
            _activity = activity;
        }

        private readonly Dictionary<int, Func<string, int>[]> _validationMap = new Dictionary<int, Func<string, int>[]>();
        private readonly Dictionary<int, int> _errorDisplayMaps = new Dictionary<int, int>();

        public Validator Add(int editTextId, int errorTextViewId, Func<string, int>[] validationRules)
        {
            _validationMap.Add(editTextId, validationRules);
            _errorDisplayMaps.Add(editTextId, errorTextViewId);
            return this;
        }

        
        public bool ValidateAll(View containerView, bool isOnUiThread = false)
        {
            bool validationResult = false;
            /*if (!isOnUiThread)
            {
                _activity.RunOnUiThread(() => { validationResult = ValidateAll(containerView, true); });
                return validationResult;
            }*/

            bool allValid = true;
            foreach (var map in _validationMap)
            {
                EditText editText = containerView.FindViewById<EditText>(map.Key);
                TextView tvError = containerView.FindViewById<TextView>(_errorDisplayMaps[map.Key]);
                if (tvError != null)
                {
                    _activity.RunOnUiThread(() => tvError.Text = string.Empty);
                }

                if (editText != null && editText.Visibility == ViewStates.Visible)
                {
                    ViewGroup viewParent = editText.Parent as ViewGroup;
                    if (viewParent != null && viewParent.Visibility == ViewStates.Visible)
                    {
                        foreach (var validationRule in map.Value)
                        {
                            int result = validationRule(editText.Text);
                            if (result != 0)
                            {
                                if (tvError != null)
                                {
                                    allValid = false;
                                    _activity.RunOnUiThread(() => tvError.Text = _activity.GetString(result));
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return allValid;
        }
    }
}