using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.UI.Wizardry.Fragments;
using SalesApp.Droid.UI.Wizardry.Helpers;
using Exception = System.Exception;
using Fragment = Android.Support.V4.App.Fragment;

namespace SalesApp.Droid.UI.Wizardry
{
    [Activity(Label = "WizardActivity", NoHistory = false, LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(HomeView),
        Theme = "@style/AppTheme.SmallToolbar", WindowSoftInputMode = SoftInput.AdjustPan)]
    public class WizardActivity : ActivityBase2, IWizardActivity
    {
        public const string KeyBundledItems = "BundledItems";
        public const string BundledWizardType = "BundledWizardType";
        public const string WizardCurrentStepKey = "WizardCurrentStepKey";
        public const string WizardCurrentFragmentKey = "WizardCurrentFragmentKey";
        public const string MainFragmentKey = "MainFragmentKey";
        public const string ButtonsFragmentKey = "ButtonsFragmentKey";
        public const string WizardStepHistoryKey = "WizardStepHistoryKey";

        private const string FragmentOverlayTag = "OverlayFragment";
        private const string WizardDataBundled = "WizardDataBundled";
        private const string DontCheckKey = "DontCheckKey";

        private readonly Dictionary<bool, int> _overLays = new Dictionary<bool, int>
        {
            { true, Resource.Id.frameBigOverlay },
            { false, Resource.Id.frameSmallOverlay }
        };

        private string _serializedData = string.Empty;
        private int _currentStep;

        private ILog _logger;
        private ProgressFragment _progressFragment;
        private Dictionary<WizardTypes, Type> _wizardTypeHelpers;
        private WizardHelperBase _wizardHelper;
        private FragmentButtons _fragmentButtons;
        private ProgressDialog _progressDialog;
        private Type _currentFragmentType;
        private bool _isRestore;

        private TextView _tvTitle;

        public WizardTypes WizardType { get; private set; }

        public IntentStartPointTracker.IntentStartPoint StartPoint { get; private set; }

        private Dictionary<int, Type> _stepsHistory;

        private bool _isHidden;

        public bool ButtonNextEnabled
        {
            get
            {
                if (_fragmentButtons == null)
                {
                    return false;
                }

                return _fragmentButtons.ButtonNextEnabled;
            }

            set
            {
                if (_fragmentButtons == null)
                {
                    return;
                }

                _fragmentButtons.ButtonNextEnabled = value;
            }
        }

        public Dictionary<string, object> BundledItems { get; set; }

        public Button ButtonNext
        {
            get
            {
                if (_fragmentButtons == null)
                {
                    return null;
                }

                return _fragmentButtons.ButtonNext;
            }
        }

        public Button ButtonPrevious
        {
            get
            {
                if (_fragmentButtons == null)
                {
                    return null;
                }

                return _fragmentButtons.ButtonPrevious;
            }
        }

        private new ILog Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = Resolver.Instance.Get<ILog>();
                    _logger.Initialize(this.GetType().FullName);
                }

                return _logger;
            }
        }

        private WizardHelperBase WizardHelper
        {
            get
            {
                RegisterHelpers();
                _wizardHelper = Activator.CreateInstance(_wizardTypeHelpers[WizardType]) as WizardHelperBase;
                if (_wizardHelper == null)
                {
                    throw new Exception("Could not create an instance of wizard helper type. Probably because you haven't registered a helper.");
                }
                return _wizardHelper;
            }
        }

        public override void SetViewPermissions()
        {
        }

        public override void InitializeScreen()
        {
        }

        private void ResolveFragmentToLoad(Bundle bundle)
        {
            if (bundle != null)
            {
                _currentFragmentType = JsonConvert.DeserializeObject<Type>(bundle.GetString(WizardCurrentFragmentKey));
            }
            else
            {
                _currentFragmentType = WizardHelper.GetFirstFragment();
            }

            if (_currentFragmentType == null)
            {
                throw new Exception("Problem loading current fragment type, unable to load fragment.");
            }
        }

        public override void RetrieveScreenInput()
        {
        }

        public override void UpdateScreen()
        {
        }

        public override void SetListeners()
        {
        }

        public override bool Validate()
        {
            return true;
        }

        public void ShowOverlay(WizardOverlayFragment fragment, bool big)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    HideOverlay(false);
                    HideContentFragment();

                    // hack: show the small overlay again if needed
                    if (!_isHidden)
                    {
                        SupportFragmentManager.BeginTransaction()
                        .Add(_overLays[big], fragment, FragmentOverlayTag)
                        .CommitAllowingStateLoss();
                    }

                    HideKeyboard(true);
                    if (!big)
                    {
                        BringFrameToFront(Resource.Id.frameSmallOverlay);
                    }
                }
                catch (IllegalStateException e)
                {
                    Logger.Warning(e);
                }
            });
        }

        /// <summary>
        /// No comments here :)
        /// </summary>
        /// <param name="showKeyboard"></param>
        public void HideOverlay(bool showKeyboard)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    Fragment fragmentOverlay = SupportFragmentManager.FindFragmentByTag(FragmentOverlayTag);
                    if (fragmentOverlay == null)
                    {
                        return;
                    }

                    if (!_isHidden)
                    {
                        SupportFragmentManager.BeginTransaction()
                        .Remove(fragmentOverlay)
                        .CommitAllowingStateLoss();
                    }

                    if (showKeyboard)
                    {
                        ShowKeyboard(null);
                    }
                    ShowContentFragment();
                    BringFrameToFront(Resource.Id.frameButtons);
                }
                catch (IllegalStateException e)
                {
                    Logger.Warning(e);
                }
            });
        }

        public void HideWait()
        {
            RunOnUiThread(() =>
            {
                if (_progressDialog != null)
                {
                    _progressDialog.Dismiss();
                    _progressDialog = null;
                }
            });
        }


        protected override void OnPause()
        {
            base.OnPause();
            HideWait();
        }

        /// <summary>
        /// This is a safe place to commit fragment transactions
        /// We also set IsHidden to false so signal that we the UI is reaady.
        /// Please note that RunCachedAction() may include RragmentTransaction.Commit() so dont move it to any other licecycle method unles you know what you are doing
        /// </summary>
        protected override void OnPostResume()
        {
            base.OnPostResume();
            _isHidden = false;
            RunCachedAction();
            ConfigureButtons();
        }

        public void ShowWaitInfo(int titleId, int storyId)
        {
            RunOnUiThread(() =>
            {
                if (_progressDialog != null)
                {
                    _progressDialog.SetTitle(GetString(titleId));
                    _progressDialog.SetMessage(GetString(storyId));
                }
                else
                {
                    _progressDialog = ProgressDialog.Show(
                        this,
                        GetString(titleId),
                        GetString(storyId),
                        true);
                }
            });
        }

        public void ShowWaitInfo(string title, string story)
        {
            _progressFragment = new ProgressFragment();
            Bundle arguments = new Bundle();
            arguments.PutString(ProgressFragment.TitleKey, title);
            arguments.PutString(ProgressFragment.MessageKey, story);
            _progressFragment.Arguments = arguments;

            ShowOverlay(_progressFragment, true);
        }

        public void GoNext()
        {
            this.FragmentButtons_NextClicked(this, EventArgs.Empty);
        }

        [Obsolete("Replace with Extension Method")]
        public void ToggleContentEnabledState(ViewGroup viewGroup, bool enabled)
        {
            if (viewGroup == null)
            {
                return;
            }

            this.RunOnUiThread(() =>
            {
                for (int i = 0; i < viewGroup.ChildCount; i++)
                {
                    this.ToggleContentEnabledState(viewGroup.GetChildAt(i) as ViewGroup, enabled);
                    viewGroup.GetChildAt(i).Enabled = enabled;
                }
            });
        }

        /// <summary>
        /// This method is called if the wizard activity is launched while it is on top os the stack.
        /// This coupled with LaunchMode = SingleTop, ensures that we dont stack wizards on top of each other
        /// </summary>
        /// <param name="intent">The new intent to use for re-launching the wizard</param>
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            this.HideOverlay(true);
            this.Intent = intent;

            this.ResolveWizardType(intent.Extras);
            this.ResolveBundledItems(intent.Extras);

            this.SetScreenTitle(this.WizardHelper.ScreenTitle);

            this._fragmentButtons = new FragmentButtons();
            this.SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.frameButtons, this._fragmentButtons, this._fragmentButtons.FragmentTag)
                .Commit();

            _currentFragmentType = WizardHelper.GetFirstFragment();

            this.LoadFragment();

            this._tvTitle = this.FindViewById<TextView>(Resource.Id.tvTitle);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this._isHidden = false;
            Bundle bundledInfo = bundle ?? this.Intent.Extras;
            this.ResolveWizardType(bundledInfo);
            this.ResolveBundledItems(bundledInfo);
            this.SetContentView(Resource.Layout.activity_wizard);

            this.AddToolbar(this.WizardHelper.ScreenTitle);

            if (bundle == null)
            {
                this._fragmentButtons = new FragmentButtons();
                this.SupportFragmentManager.BeginTransaction()
                    .Add(Resource.Id.frameButtons, this._fragmentButtons, this._fragmentButtons.FragmentTag)
                    .Commit();

                this.ResolveFragmentToLoad(null);
                this.LoadFragment();
            }
            else
            {
                // get the history back
                this._stepsHistory = JsonConvert.DeserializeObject<Dictionary<int, Type>>(bundle.GetString(WizardStepHistoryKey));

                // rebuild all the fragments
                this.CurrentFragment = this.SupportFragmentManager.GetFragment(bundle, MainFragmentKey) as WizardStepFragment;
                this._fragmentButtons = this.SupportFragmentManager.GetFragment(bundle, ButtonsFragmentKey) as FragmentButtons;

                if (this.CurrentFragment != null)
                {
                    this.CurrentFragment.SetData(this._serializedData);
                }

                IsProspectConversion = bundle.GetBoolean(DontCheckKey);
            }

            this._tvTitle = this.FindViewById<TextView>(Resource.Id.tvTitle);
        }

        /// <summary>
        /// Set the wizard title. This method is repeated in branch feature/ProspectProduct (Merge wisely)
        /// </summary>
        public void SetWizardTitle()
        {
            if (this._tvTitle == null)
            {
                this._tvTitle = this.FindViewById<TextView>(Resource.Id.tvTitle);
            }

            if (this.CurrentFragment.StepTitle == 0)
            {
                this._tvTitle.Visibility = ViewStates.Gone;
                return;
            }

            this._tvTitle.Visibility = ViewStates.Visible;
            this._tvTitle.Text = this.GetString(this.CurrentFragment.StepTitle);
            this._tvTitle.Gravity = this.CurrentFragment.TitleGravity;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            this._isHidden = true;

            if (this.BundledItems != null)
            {
                outState.PutString(KeyBundledItems, JsonConvert.SerializeObject(this.BundledItems));
            }

            outState.PutInt(BundledWizardType, (int)this.WizardType);
            if (this.CurrentFragment != null)
            {
                outState.PutString(WizardDataBundled, this.CurrentFragment.GetData());
            }

            // store the current step as well
            outState.PutInt(WizardCurrentStepKey, this._currentStep);
            outState.PutString(WizardCurrentFragmentKey, JsonConvert.SerializeObject(this._currentFragmentType));

            outState.PutString(WizardStepHistoryKey, JsonConvert.SerializeObject(this._stepsHistory));

            // save the current fragments
            this.SupportFragmentManager.PutFragment(outState, MainFragmentKey, this.CurrentFragment);
            this.SupportFragmentManager.PutFragment(outState, ButtonsFragmentKey, this._fragmentButtons);

            outState.PutBoolean(DontCheckKey, IsProspectConversion);
        }

        private void ResolveBundledItems(Bundle bundle)
        {
            if (bundle != null)
            {
                string bundledItems = bundle.GetString(KeyBundledItems);
                if (!bundledItems.IsBlank())
                {
                    BundledItems = JsonConvert.DeserializeObject<Dictionary<string, object>>(bundledItems);
                }

                string startPoint = bundle.GetString(IntentStartPointTracker.ActivityStartPoint);
                if (!startPoint.IsBlank())
                {
                    StartPoint = startPoint.ToEnumValue<IntentStartPointTracker.IntentStartPoint>();
                }

                _serializedData = bundle.GetString(WizardDataBundled);

                // load the current step
                _currentStep = bundle.GetInt(WizardCurrentStepKey, 0);
            }
            else
            {
                if (Intent.Extras != null)
                {
                    ResolveBundledItems(Intent.Extras);
                }
            }
        }

        private void RegisterHelpers()
        {
            _wizardTypeHelpers = new Dictionary<WizardTypes, Type>();
            _wizardTypeHelpers.Add(WizardTypes.CustomerRegistration, typeof(CustomerRegistrationWizardHelper));
            _wizardTypeHelpers.Add(WizardTypes.ProspectRegistration, typeof(ProspectRegistrationWizardHelper));
        }

        /// <summary>
        /// Resolve the wizard type from bundle into an enum.
        /// </summary>
        /// <param name="extras">Bundle containing the type</param>
        private void ResolveWizardType(Bundle extras)
        {
            try
            {
                if (extras == null)
                {
                    throw new Exception("Could not find a bundle with the wizard type");
                }

                int wizType = extras.GetInt(BundledWizardType);
                if (wizType == default(int))
                {
                    throw new Exception("Wizard type was not bundled");
                }

                WizardType = (WizardTypes)wizType;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        /// <summary>
        /// This method will initiate going back or forward depending on boolean given.
        /// </summary>
        /// <param name="goNext">True if go next, otherwise pass false</param>
        public void Go(bool goNext)
        {
            _serializedData = CurrentFragment.GetData();

            bool goPrevious = !goNext;
            if (goNext)
            {
                if (!CurrentFragment.IsLastStep)
                {
                    _currentStep++;
                }

                _currentFragmentType = CurrentFragment.GetNextFragment();
            }
            else
            {
                _currentStep--;
                if (!StepsHistory.ContainsKey(_currentStep))
                {
                    return;
                }

                _currentFragmentType = StepsHistory[_currentStep];
            }

            // if the final step, finish the wizard, otherwise load fragment
            if (!CurrentFragment.IsLastStep || goPrevious)
            {
                LoadFragment();
            }
            else
            {
                CurrentFragment.FinishWizard();
                WizardFinished = true;
            }
        }

        /// <summary>
        /// This method does the actual loading of the fragment depending on current step.
        /// </summary>
        private void LoadFragment()
        {
            if (_currentFragmentType == null)
            {
                throw new InvalidOperationException("Cannot load fragment if there is CurrentFragmentType == null.");
            }

            // load the fragment needed
            CurrentFragment = Activator.CreateInstance(_currentFragmentType) as WizardStepFragment;

            if (CurrentFragment == null)
            {
                throw new Exception("Fragments used for steps in the wizard must inherit WizardStepFragment. Yours does not.");
            }


            if (!StepsHistory.ContainsKey(_currentStep))
            {
                StepsHistory.Add(_currentStep, CurrentFragment.GetType());
            }
            else
            {
                StepsHistory[_currentStep] = CurrentFragment.GetType();
            }

            if (!_serializedData.IsBlank())
            {
                RunOnUiThread(() => { CurrentFragment.SetData(_serializedData); });
            }

            // load the actual fragment needed
            if (!_isHidden)
            {
                SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.frameContent, CurrentFragment, CurrentFragment.FragmentTag)
                .Commit();
            }


            this.SetButtonTitles();

            _fragmentButtons.SelectStep(_currentStep);
            HideKeyboard(true);
        }

        public bool WizardFinished { get; set; }

        private Dictionary<int, Type> StepsHistory
        {
            get
            {
                _stepsHistory = _stepsHistory ?? new Dictionary<int, Type>();
                return _stepsHistory;
            }
        }

        private void SetButtonTitles()
        {
            RunOnUiThread(() =>
            {
                if (CurrentFragment == null)
                {
                    return;
                }
                if (ButtonNext != null)
                {
                    ButtonNext.Text = GetString(CurrentFragment.NextButtonText);
                }

                if (ButtonPrevious != null)
                {
                    ButtonPrevious.Text = GetString(CurrentFragment.PreviousButtonText);
                }
            });
        }

        private void HideContentFragment()
        {
            RunOnUiThread(() =>
            {
                ToggleContentEnabledState(FindViewById<FrameLayout>(Resource.Id.frameContent), false);
            });
        }

        private void ShowContentFragment()
        {
            RunOnUiThread(() =>
            {
                ToggleContentEnabledState(FindViewById<FrameLayout>(Resource.Id.frameContent), true);
            });
        }

        private void ConfigureButtons()
        {
            // first un-register the event handler
            _fragmentButtons.NextClicked -= FragmentButtons_NextClicked;
            _fragmentButtons.PrevClicked -= FragmentButtons_PrevClicked;

            // then register the event handlers
            _fragmentButtons.NextClicked += FragmentButtons_NextClicked;
            _fragmentButtons.PrevClicked += FragmentButtons_PrevClicked;

            this.SetButtonTitles();
            _fragmentButtons.SelectStep(_currentStep);
        }

        private void FragmentButtons_PrevClicked(object sender, EventArgs e)
        {
            if (CurrentFragment.OnPreviousClicked == default(Action))
            {
                this.DefaultPreviousAction();
                return;
            }

            CurrentFragment.OnPreviousClicked();
        }

        private void DefaultPreviousAction()
        {
            if (CurrentFragment == null)
            {
                return;
            }

            _serializedData = CurrentFragment.GetData();
            this.Go(false);
        }

        private void FragmentButtons_NextClicked(object sender, EventArgs e)
        {
            if (CurrentFragment.OnNextClicked == default(Action))
            {
                this.DefaultNextAction();
                return;
            }

            CurrentFragment.OnNextClicked();
        }

        public WizardStepFragment CurrentFragment { get; set; }

        public bool IsProspectConversion { get; set; }

        private void DefaultNextAction()
        {
            if (CurrentFragment == null)
            {
                return;
            }

            bool isValid = false;
            bool canGoNext = false;

            ShowWaitInfo(Resource.String.wizard_validating_title, Resource.String.wizard_validating_story);

            Task.Run(async () =>
            {
                try
                {
                    if (!_isHidden)
                    {
                        isValid = CurrentFragment.Validate();
                        canGoNext = await CurrentFragment.BeforeGoNextAsync();
                    }
                    else
                    {
                        WizardCache.CachedAction = () =>
                        {
                            Toast.MakeText(this, GetString(Resource.String.action_not_completed),
                                ToastLength.Long).Show();
                        };
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                    WizardCache.CachedAction = () =>
                    {
                        Toast.MakeText(this, GetString(Resource.String.action_not_completed),
                            ToastLength.Long).Show();
                    };
                }
                finally
                {
                    HideWait();
                    _fragmentButtons.ButtonNextEnabled = false;
                }

                if (isValid && canGoNext)
                {
                    WizardCache.CachedAction = () =>
                    {
                        _serializedData = CurrentFragment.GetData();
                        RunOnUiThread(() => Go(true));
                    };

                    if (!_isHidden)
                    {
                        RunCachedAction();
                    }
                }
            });
        }

        private void RunCachedAction()
        {
            if (WizardCache.CachedAction != null)
            {
                WizardCache.CachedAction();
                WizardCache.CachedAction = null;
            }
        }

        private void BringFrameToFront(int id)
        {
            FrameLayout frame = FindViewById<FrameLayout>(id);
            if (frame == null)
            {
                return;
            }

            frame.BringToFront();
        }
    }
}