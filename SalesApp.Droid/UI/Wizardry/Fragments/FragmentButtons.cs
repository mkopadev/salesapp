using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.UI.Wizardry.Fragments
{
    public class FragmentButtons : FragmentBase3
    {
        private int _currentStep;

        /// <summary>
        /// A reference to the contextual next button
        /// </summary>
        private Button nextButton;

        /// <summary>
        /// A reference to the contextual previous button
        /// </summary>
        private Button previousButton;

        public event EventHandler NextClicked;

        public event EventHandler PrevClicked;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            this.view = inflater.Inflate(Resource.Layout.fragment_wizard_buttons, container, false);

            this.nextButton = this.view.FindViewById<Button>(Resource.Id.btnNext);
            this.previousButton = this.view.FindViewById<Button>(Resource.Id.btnPrev);

            this.SetEventHandlers();
            this.SelectStep(this._currentStep);
            return this.view;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.Logger.Debug("FragmentButtons.OnCreate");
        }

        void PreviousButtonClicked(object sender, EventArgs e)
        {
            if (PrevClicked != null)
            {
                PrevClicked(sender, e);
            }
        }

        public void SelectStep(int step)
        {
            _currentStep = step;

            if (view == null)
            {
                return;
            }

            if (this.previousButton == null)
            {
                return;
            }

            this.previousButton.Visibility = step == 0 ? ViewStates.Gone : ViewStates.Visible;
        }

        public override bool Validate()
        {
            throw new NotImplementedException();
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
            throw new NotImplementedException();
        }

        protected override void SetEventHandlers()
        {
            this.previousButton.Click += PreviousButtonClicked;
            this.nextButton.Click += NextButtonClicked;

        }

        private void NextButtonClicked(object sender, EventArgs eventArgs)
        {
            if (NextClicked != null)
            {
                ((Button)sender).Enabled = false;
                NextClicked(sender, eventArgs);
            }
        }

        /// <summary>
        /// Gets the contextual next button
        /// </summary>
        public Button ButtonNext
        {
            get
            {
                return this.nextButton;
            }
        }

        /// <summary>
        /// Gets the contextual previous button
        /// </summary>
        public Button ButtonPrevious
        {
            get
            {
                return this.previousButton;
            }
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
            throw new NotImplementedException();
        }

        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }

        public bool ButtonNextEnabled
        {
            get
            {
                if (view == null || ButtonNext == null)
                {
                    return false;
                }
                bool value = false;
                Activity.RunOnUiThread
                    (
                        () =>
                        {
                            value = ButtonNext.Enabled;
                        }
                    );
                return value;
            }
            set
            {
                if (view == null || ButtonNext == null)
                {
                    return;
                }
                Activity.RunOnUiThread
                    (
                        () =>
                        {
                            ButtonNext.Enabled = value;
                        }
                    );

            }
        }
    }
}