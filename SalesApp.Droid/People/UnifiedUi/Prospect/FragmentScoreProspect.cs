using System;
using System.Collections.Generic;
using System.Reflection;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Droid.UI.Utils.ViewsHelper;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid.People.UnifiedUi.Prospect
{
    public class FragmentScoreProspect : WizardStepFragment
    {
        private readonly Dictionary<int, string> _scoringAttributes = new Dictionary<int, string>
        {
            { Resource.Id.btnNeed, "Need" },
            { Resource.Id.btnMoney, "Money" },
            { Resource.Id.btnAuthority, "Authority" }
        };

        private ViewsHelper<SalesApp.Core.BL.Models.People.Prospect> _viewsHelper;

        private SalesApp.Core.BL.Models.People.Prospect _personRegistrationInformation;

        public override void SetData(string serializedString)
        {
            _personRegistrationInformation =
                JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Prospect>(
                    serializedString);
        }

        public override string GetData()
        {
            return JsonConvert.SerializeObject(_personRegistrationInformation);
        }

        public override Type GetNextFragment()
        {
            return typeof(FragmentProspectConfirmationScreen);
        }

        /// <summary>
        /// Creates this fragment
        /// </summary>
        /// <param name="savedState">The saved state</param>
        public override void OnCreate(Bundle savedState)
        {
            base.OnCreate(savedState);
            RetainInstance = true;
            if (savedState != null)
            {
                string savedJson = savedState.GetString(BundledRegistrationInfo);
                this.SetData(savedJson);
            }
        }

        /// <summary>
        /// Save our current state into a bundle
        /// </summary>
        /// <param name="outState">The bundle</param>
        public override void OnSaveInstanceState(Bundle outState)
        {
            string serializedString = JsonConvert.SerializeObject(this._personRegistrationInformation);
            outState.PutString(BundledRegistrationInfo, serializedString);

            base.OnSaveInstanceState(outState);
        }

        /// <summary>
        /// What happens before we go next
        /// </summary>
        /// <returns>The validation before going to the next fragment</returns>
        public override bool BeforeGoNext()
        {
            return true;
        }

        public override int StepTitle
        {
            get { return Resource.String.unified_score_your_prospect; }
        }

        public override bool Validate()
        {
            return this._personRegistrationInformation.Score > 0;
        }

        public override GravityFlags TitleGravity
        {
            get { return GravityFlags.Center; }
        }

        private ViewsHelper<SalesApp.Core.BL.Models.People.Prospect> ViewsHelper
        {
            get
            {
                if (_viewsHelper == null)
                {
                    _viewsHelper = new ViewsHelper<SalesApp.Core.BL.Models.People.Prospect>(Activity, this.FragmentView);
                }

                return _viewsHelper;
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedState)
        {
            base.OnCreateView(inflater, container, savedState);
            this.FragmentView = inflater.Inflate(Resource.Layout.fragment_unified_prospect_scoring, container, false);

            ViewsHelper.BindEvent(BindableEvents.OnClick, Resource.Id.btnMoney, this.ScoreChanged)
                .BindEvent(BindableEvents.OnClick, Resource.Id.btnNeed, this.ScoreChanged)
                .BindEvent(BindableEvents.OnClick, Resource.Id.btnAuthority, this.ScoreChanged);

            if (savedState != null)
            {
                string savedJson = savedState.GetString(BundledRegistrationInfo);
                this.SetData(savedJson);

                // Restore the state of the rings
                this.UpdateRings(this._personRegistrationInformation.Score);

                // Also update the next button enabled state
                int score = this._personRegistrationInformation.Score;
                this.WizardActivity.ButtonNextEnabled = score > 0;

                // Also restore the state of the buttons
                this.UpdateButtons(this._personRegistrationInformation);
            }

            return this.FragmentView;
        }

        /// <summary>
        /// Called when the fragment is ready for diaplay
        /// </summary>
        public override void OnResume()
        {
            base.OnResume();
            this.WizardActivity.ButtonNext.Visibility = ViewStates.Visible;

            // Restore the state of the rings
            this.UpdateRings(this._personRegistrationInformation.Score);

            // Also update the next button enabled state
            int score = this._personRegistrationInformation.Score;
            this.WizardActivity.ButtonNextEnabled = score > 0;

            // Also restore the state of the buttons
            this.UpdateButtons(this._personRegistrationInformation);
        }

        private void UpdateScoreAttribute(View vwButton)
        {
            if (!_scoringAttributes.ContainsKey(vwButton.Id))
            {
                throw new Exception("Button clicked is not valid for scoring prospect. Button text is '" + (vwButton as Button).Text + "'");
            }

            string propertyName = _scoringAttributes[vwButton.Id];
            bool boolValue = GetScoringPropertyValue(propertyName);
            _personRegistrationInformation.GetType()
                .GetProperty(propertyName)
                .SetValue(_personRegistrationInformation, !boolValue);

        }

        private bool GetScoringPropertyValue(string propertyName)
        {
            PropertyInfo attributeProperty = _personRegistrationInformation.GetType()
                .GetProperty(propertyName);
            if (attributeProperty == null)
            {
                throw new Exception
                    (
                        String.Format("Cannot find property '{0}' in object Prospect. Could it have been renamed?", propertyName)
                    );
            }

            if (attributeProperty.PropertyType != typeof(bool))
            {
                throw new Exception(string.Format("Property called '{0}' is of the wrong type, boolean expected.",
                    propertyName));
            }

            object attribValue = attributeProperty
                .GetValue(_personRegistrationInformation);
            return (bool)attribValue;
        }

        /// <summary>
        /// Update the prospect score
        /// </summary>
        /// <param name="viewButton">The score viewButton that was clicked</param>
        private void ScoreChanged(View viewButton)
        {
            this.UpdateScoreAttribute(viewButton);

            if (this.GetScoringPropertyValue(this._scoringAttributes[viewButton.Id]))
            {
                viewButton.SetBackgroundResource(Resource.Drawable.button_green);
                (viewButton as Button).SetTextColor(Color.White);
            }
            else
            {
                viewButton.SetBackgroundResource(Resource.Drawable.button_grey_gradient);
                (viewButton as Button).SetTextColor(Color.Black);
            }

            int score = this._personRegistrationInformation.Score;
            this.UpdateRings(score);
            this.WizardActivity.ButtonNextEnabled = score > 0;
        }

        /// <summary>
        /// Update the buttons state
        /// </summary>
        /// <param name="prospect">The prospect with the needed scores</param>
        private void UpdateButtons(SalesApp.Core.BL.Models.People.Prospect prospect)
        {
            if (prospect.Need)
            {
                Button btn = this.FragmentView.FindViewById<Button>(Resource.Id.btnNeed);
                btn.SetBackgroundResource(Resource.Drawable.button_green);
                btn.SetTextColor(Color.White);
            }

            if (prospect.Authority)
            {
                Button btn = this.FragmentView.FindViewById<Button>(Resource.Id.btnAuthority);
                btn.SetBackgroundResource(Resource.Drawable.button_green);
                btn.SetTextColor(Color.White);
            }

            if (prospect.Money)
            {
                Button btn = this.FragmentView.FindViewById<Button>(Resource.Id.btnMoney);
                btn.SetBackgroundResource(Resource.Drawable.button_green);
                btn.SetTextColor(Color.White);
            }
        }

        /// <summary>
        /// Update the rings
        /// </summary>
        /// <param name="score">The prospect score</param>
        private void UpdateRings(int score)
        {
            this.Activity.RunOnUiThread(
                    () =>
                    {
                        TextView tvCircles = this.FragmentView.FindViewById<TextView>(Resource.Id.tvCircles);
                        TextView tvPleasePress = this.FragmentView.FindViewById<TextView>(Resource.Id.tvPleasePress);
                        tvPleasePress.Text = this.GetString(Resource.String.unified_score_generated);
                        switch (score)
                        {
                            case 0:
                                tvCircles.SetBackgroundResource(Resource.Drawable.prospect_assessment_circles);
                                tvCircles.Text = string.Empty;
                                tvPleasePress.Text = this.GetString(Resource.String.unified_please_press_buttons_to_score);
                                break;
                            case 1:
                                tvCircles.SetBackgroundResource(Resource.Drawable.prospect_assessment_circles_cold);
                                tvCircles.Text = this.GetString(Resource.String.unified_cold);
                                break;
                            case 2:
                                tvCircles.SetBackgroundResource(Resource.Drawable.prospect_assessment_circles_warm);
                                tvCircles.Text = this.GetString(Resource.String.unified_warm);
                                break;
                            case 3:
                                tvCircles.SetBackgroundResource(Resource.Drawable.prospect_assessment_circles_hot);
                                tvCircles.Text = this.GetString(Resource.String.unified_hot);
                                break;
                        }
                    });
        }
    }
}