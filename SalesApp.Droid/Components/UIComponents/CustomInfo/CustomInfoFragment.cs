using System;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid.Components.UIComponents.CustomInfo
{
  
    public class CustomInfoFragment : WizardOverlayFragment
    {
        public static readonly string InfoKey = "InfoKey";

        private readonly string actionBarTitleKey = "ActionBarTitleKey";
        private readonly string titleKey = "TitleKey";
        private readonly string contentKey = "ContentKey";
        private readonly string btnPositiveKey = "BtnPositiveKey";
        private readonly string btnNegativeKey = "BtnNegativeKey";
        private readonly string imageResourceKey = "ImageResourceKey";

        // Delegate to handle onclick within this Fragment.
        public delegate void BtnClick();

        private View _view;

        public event BtnClick PositiveAction;
        public event BtnClick NegativeAction;
        public event BtnClick OtherAction;

        private string _actionBarTitle, content;
        private string btnPositiveTitle, btnNegativeTitle;
        private string _title;
        private bool _swapped;

        private Button _positive;
        private Button _negative;
        private Button _one;
        private TextView _tvTitle;
        private TextView _tvText;
        private LinearLayout.LayoutParams _leftButton;
        private LinearLayout.LayoutParams _rightButton;
        private LinearLayout.LayoutParams _oneButton;
        private int _imageResource;
        
        /*public CustomInfoFragment(string title, string screenContent, string btnPositive, string btnNegative)
            :this(title,title,screenContent,btnPositive,btnNegative)
        {

        }

        public CustomInfoFragment(string actionBarTitle, string title, string screenContent, string btnPositive,
            string btnNegative)
            : this(title, title, screenContent, btnPositive, btnNegative, default(int))
        {
            
        }

        public CustomInfoFragment(string actionBarTitle, string title, string screenContent, string btnPositive, string btnNegative, int imageResource)
        {
            this._actionBarTitle = actionBarTitle;
            this._title = title;
            this.content = screenContent;
            this.btnPositiveTitle = btnPositive;
            this.btnNegativeTitle = btnNegative;
            this._imageResource = imageResource;
        }

        public CustomInfoFragment(string title, string screenContent, string btnPositive)
            : this(title, screenContent, btnPositive, string.Empty)
        {
            
        }
*/
        public CustomInfoFragment()
        {
        }

        public override bool Validate()
        {
            return true;
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
 
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {

        }

        protected override void SetEventHandlers()
        {

        }

        public override void SetViewPermissions()
        {
 
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (this.Arguments != null)
            {
                var info = GetArgument<Info>(InfoKey);
                _actionBarTitle = info.ActionBarTitle;
                _title = info.Title;
                content = info.Content;
                btnPositiveTitle = info.PositiveButtonCaption;
                btnNegativeTitle = info.NegativeButtonCaption;
                _imageResource = info.Image;
            }

            /*if (savedInstanceState != null)
            {
                _actionBarTitle = savedInstanceState.GetString(actionBarTitleKey);
                _title = savedInstanceState.GetString(titleKey);
                content = savedInstanceState.GetString(contentKey);
                btnPositiveTitle = savedInstanceState.GetString(btnPositiveKey);
                btnNegativeTitle = savedInstanceState.GetString(btnNegativeKey);
                _imageResource = savedInstanceState.GetInt(imageResourceKey);
            }*/
        }

        /*public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(actionBarTitleKey, _actionBarTitle);
            outState.PutString(titleKey, _title);
            outState.PutString(contentKey, content);
            outState.PutString(btnPositiveKey, btnPositiveTitle);
            outState.PutString(btnNegativeKey, btnNegativeTitle);
            outState.PutInt(imageResourceKey, _imageResource);
        }*/

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            _view = inflater.Inflate(Resource.Layout.layout_custom_info, container, false);

            Activity.RunOnUiThread
                (
                    () =>
                    {
                        _tvTitle = (TextView)_view.FindViewById(Resource.Id.tvmessageinfotitle);
                        _tvText = (TextView)_view.FindViewById(Resource.Id.tvmessageinfotext);
                        _positive = (Button)_view.FindViewById(Resource.Id.infoPositive);
                        _negative = (Button)_view.FindViewById(Resource.Id.infoNegative);
                        _one = (Button)_view.FindViewById(Resource.Id.infoOneButton);

                        _leftButton = new LinearLayout.LayoutParams(
                            LinearLayout.LayoutParams.MatchParent,
                            LinearLayout.LayoutParams.WrapContent
                            );

                        _rightButton = new LinearLayout.LayoutParams(
                            LinearLayout.LayoutParams.MatchParent,
                            LinearLayout.LayoutParams.WrapContent
                            );

                        _oneButton = new LinearLayout.LayoutParams(
                            LinearLayout.LayoutParams.MatchParent,
                            LinearLayout.LayoutParams.WrapContent
                            );

                        _leftButton.SetMargins(20, 0, 5, 20);
                        _rightButton.SetMargins(5, 0, 20, 20);
                        _oneButton.SetMargins(20, 0, 20, 20);

                        SetContent();
                        if (_imageResource != default(int))
                        {
                            ImageView imgView = _view.FindViewById<ImageView>(Resource.Id.img);
                            if (imgView != null)
                            {
                                imgView.SetBackgroundResource(_imageResource);
                            }
                        }
                    }
                );
            
            return _view;
        }

        public void SetContent()
        {
            SetContent(_actionBarTitle, _title,content,btnPositiveTitle,btnNegativeTitle);
        }

        public void SetContent(string title, string text)
        {
            SetContent(title, title, text, btnPositiveTitle, btnNegativeTitle);
        }

        public void SetContent(string actionBarTitle, string text, string btnPosText, string btnNegText)
        {
            SetContent(actionBarTitle,"",text,btnPosText,btnNegText);
        }
        public void SetContent(string actionBarTitle, string title, string text, string btnPosText, string btnNegText)
        {
            if (Activity == null || _view == null)
            {
                _actionBarTitle = actionBarTitle;
                _title = title;
                content = text;
                btnPositiveTitle = btnPosText;
                btnNegativeTitle = btnNegText;
                return;
            }
            Activity.RunOnUiThread
                (
                    () =>
                    {
                        _tvTitle.Text = title;
                        _tvText.Text = text;

                        if (Activity.ActionBar != null)
                        {
                            Activity.ActionBar.Title = actionBarTitle;
                        }
                        ConfigButtons(btnPosText, btnNegText);
                    }
                );
            
            
        }

        private void ConfigButtons(string btnPosText, string btnNegText)
        {
            // do not allow no buttons to be available
            if (string.IsNullOrEmpty(btnPosText) && string.IsNullOrEmpty(btnNegText))
            {
                btnPosText = GetText(Resource.String.ok);
            }

            // do not allow setting only the negative button for now
            if (string.IsNullOrEmpty(btnPosText) && !string.IsNullOrEmpty(btnNegText))
                throw new NullReferenceException(GetText(Resource.String.no_button_text));

            bool hasNegativeButton = !string.IsNullOrEmpty(btnNegText);

            
            if (hasNegativeButton)
            {

                _one.Visibility = ViewStates.Gone;
                _negative.Visibility = ViewStates.Visible;
                _positive.Visibility = ViewStates.Visible;

                _positive.Text = btnPosText;
                _positive.Visibility = ViewStates.Visible;
                _positive.Click += delegate
                {
                    ClickPositive();
                };
                
                _negative.Visibility = ViewStates.Visible;
                _negative.Text = btnNegText;
                _negative.Click += delegate
                {
                    ClickNegative();
                };


            }
            else
            {
                _negative.Visibility = ViewStates.Gone;
                _positive.Visibility = ViewStates.Gone;

                _one.Visibility = ViewStates.Visible;
                _one.Text = btnPosText;
                _one.Click += delegate
                {
                    ClickPositive();
                };
            }
        }

        private void ClickPositive()
        {
            if (PositiveAction != null)
            {
                PositiveAction();
            }
            else
            {
                Log.Warn("ClickPositive", "No event handler given for ClickPositive");
            }
        }

        private void ClickNegative()
        {
            if (!_swapped && NegativeAction != null)
            { 
                NegativeAction();
            }
            else if (PositiveAction != null)
            {
                PositiveAction();
            }
            else
            {
                Log.Warn("ClickNegative", "No event handler given for ClickNegative");
            }
        }

        public class Info
        {
            public string ActionBarTitle { get; set; }

            public string Title { get; set; }

            public string Content { get; set; }

            public string PositiveButtonCaption { get; set; }

            public string NegativeButtonCaption { get; set; }

            public int Image { get; set; }
        }
    }
}