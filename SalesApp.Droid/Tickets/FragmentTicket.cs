using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using SalesApp.Core.BL.Models.Tickets;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Utils;

namespace SalesApp.Droid.Tickets
{
    public class FragmentTicket : FragmentBase3
    {
        //private Button _btnPrevious;
        private TextView _tvQuestionTitle;
        private LinearLayout _ticketLinearLayout;
        private TicketQuestion _currentTicketQuestion;
        private RelativeLayout _scrollTextRelativeLayout;
        private ScrollView _scrollViewAnswers;
        private Button _btnPrevious;

        public FragmentTicket()
        {

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.fragment_ticket, container, false);
            // build the screen
            Logger.Debug("Initializing UI");
            InitializeUI();
            Logger.Debug("Initializing Ticket Question");
            InitializeTicketQuestion();
            Logger.Debug("Updating UI");
            UpdateUI();

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Tickets");
            return view;
        }

        public TicketQuestion FragmentTicketQuestion
        {
            get { return _currentTicketQuestion; }
            set
            {
                _currentTicketQuestion = value;
            }
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
            _tvQuestionTitle = view.FindViewById<TextView>(Resource.Id.tViewQuestionTitle);
            _ticketLinearLayout = view.FindViewById<LinearLayout>(Resource.Id.ticketLinearLayout);
            _scrollViewAnswers = view.FindViewById<ScrollView>(Resource.Id.scrollViewAnswers);
            _scrollTextRelativeLayout = view.FindViewById<RelativeLayout>(Resource.Id.scrollTextRelativeLayout);
            _btnPrevious = view.FindViewById<Button>(Resource.Id.btnPrevious);
        }

        private void InitializeTicketQuestion()
        {
            FragmentTicketQuestion = GetArgument<TicketQuestion>(RaiseTicketActivity.TicketFragment);
            Logger.Verbose(FragmentTicketQuestion.ScreenTitle);
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
            try
            {
                if (FragmentTicketQuestion != null)
                {
                    _tvQuestionTitle.Text = FragmentTicketQuestion.ScreenTitle;

                    //loop through all the answers
                    foreach (var answer in FragmentTicketQuestion.Answers)
                    {

                        //create a button or edittext widget for each answer, depending on the HasTextBox boolean value
                        if (!answer.HasTextBox)
                        {
                            //the answer is button selection
                            Button answerButton = new Button(Activity);
                            answerButton.SetTextAppearance(Activity, Resource.Style.GreenButton);
                            answerButton.SetBackgroundResource(Resource.Drawable.button_green);
                            answerButton.Text = answer.Title;

                            //check whether it is a submit button or not
                            if (!answer.IsTextBoxSubmitButton)
                            {
                                //answerButton.Click += delegate { ProcessAnswer(answer); };
                                answerButton.Click += delegate { ProcessStepAnswer(answer); };
                            }
                            else
                            {
                                //submit button will submit the text input entered and validate if necessary
                                answerButton.Click += delegate
                                {
                                    //loop through all the edittext widgets on the screen, get the value of each and set it to the corresponding answer
                                    for (var x = 0; x < _ticketLinearLayout.ChildCount; x++)
                                    {
                                        var widget = _ticketLinearLayout.GetChildAt(x);

                                        if (!(widget is EditText)) continue;
                                        var textInputWidget = (EditText)widget;

                                        //get the Id of the editText widget which matches the Answer Id
                                        var answerId = textInputWidget.Id;

                                        //get the Tag of the editText which contains the boolean of whether value is required or not
                                        var isTextBoxValueRequired = (bool)textInputWidget.GetTag(Resource.Id.@fixed);

                                        if (isTextBoxValueRequired && string.IsNullOrEmpty(textInputWidget.Text))
                                        {
                                            AlertDialogBuilder.Instance
                                                .AddButton(Resource.String.ok, validateFail)
                                                .SetText(0, Resource.String.value_required)
                                                .Show(Activity);
                                            return;
                                        }

                                        //set the value of the answer to the value of the edittext
                                        FragmentTicketQuestion.Answers.Find(a => a.Id == answerId).TextBoxValue =
                                            textInputWidget.Text;

                                        ProcessStepAnswer(FragmentTicketQuestion.Answers.Find(a => a.Id == answerId));

                                        Logger.Verbose(textInputWidget.Text);
                                    }
                                };
                            }

                            LinearLayout.LayoutParams answerButtonParams = new LinearLayout.LayoutParams(
                                LinearLayout.LayoutParams.FillParent,
                                LinearLayout.LayoutParams.WrapContent
                                );

                            answerButtonParams.SetMargins(0, 0, 0, 12);
                            _ticketLinearLayout.AddView(answerButton, answerButtonParams);
                        }
                        else
                        {
                            //define the label to be placed above the text input and set it's title
                            TextView label = new TextView(Activity);
                            label.SetTextAppearance(Activity, Resource.Style.DefaultTextView);
                            label.Text = answer.Title;

                            LinearLayout.LayoutParams labelParams = new LinearLayout.LayoutParams(
                                LinearLayout.LayoutParams.WrapContent,
                                LinearLayout.LayoutParams.WrapContent
                                );

                            labelParams.SetMargins(5, 0, 0, 8);
                            labelParams.Gravity = GravityFlags.Left;

                            _ticketLinearLayout.AddView(label, labelParams);

                            //define the text input and set it's id to the Id of the answer so that it can be identified when the user hits "NEXT" button
                            EditText eText = new EditText(Activity);
                            eText.SetTextAppearance(Activity, Resource.Style.DefaultEditText);
                            eText.SetBackgroundResource(Resource.Drawable.edit_text_default);
                            eText.SetLines(8);
                            //eText.SetMaxLines(10);
                            //eText.VerticalScrollBarEnabled = IsVisible;
                            eText.Gravity = GravityFlags.Left | GravityFlags.Top;
                            eText.Id = answer.Id;
                            eText.SetTag(Resource.Id.@fixed, answer.IsTextBoxValueRequired);
                            eText.Text = answer.TextBoxValue;

                            LinearLayout.LayoutParams eTextParams = new LinearLayout.LayoutParams(
                                LinearLayout.LayoutParams.FillParent,
                                LinearLayout.LayoutParams.WrapContent
                                );

                            eTextParams.SetMargins(0, 0, 0, 20);
                            eTextParams.Gravity = GravityFlags.Left;

                            _ticketLinearLayout.AddView(eText, eTextParams);
                        }
                    }

                    //determine whether to show the text "Scroll Down to View More", for now 5 answers or more (HACK)
                    if (FragmentTicketQuestion.Answers.Count > 5)
                    {
                        _scrollTextRelativeLayout.Visibility = ViewStates.Visible;
                    }

                    _btnPrevious.Click += delegate
                    {
                        // when the previous button is clicked, show the last step/question
                        var parentActivity = (RaiseTicketActivity)this.Activity;
                        parentActivity.ShowLastStep();
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Verbose(ex.Message);
                return;
            }
        }

        protected override void SetEventHandlers()
        {
            throw new NotImplementedException();
        }

        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }

        public override bool Validate()
        {
            throw new NotImplementedException();
        }

        private void validateFail()
        {
            return;
        }

        private void ProcessStepAnswer(Answer currentAnswer)
        {
            var parentActivity = (RaiseTicketActivity)this.Activity;

            // get the current screen id, answer id and the value of the textedit input value
            var currentScreenId = FragmentTicketQuestion.ScreenId;
            var currentScreenQuestion = FragmentTicketQuestion.ScreenTitle;

            if (currentAnswer != null)
            {
                var currentAnswerId = currentAnswer.Id;
                var currentAnswerValue = "";
                var currentAnswerDataKey = "";

                if (currentAnswer.HasTextBox)
                {
                    currentAnswerValue = currentAnswer.TextBoxValue;
                    currentAnswerDataKey = currentAnswer.DataKey;
                }
                else
                {
                    currentAnswerValue = currentAnswer.Title;
                }
                               
                var existingTicketQuestion = parentActivity.StepAnswersList.Find(q => q.StepId == currentScreenId);

                // check whether the current step to be added doesnt exist already
                if (existingTicketQuestion != null)
                {
                    // if it does, remove the entry from the cache and add the new
                    parentActivity.StepAnswersList.Remove(existingTicketQuestion);
                }

                // save the current screen id, answer id and answer value (whether text input or button selection) AFTER question has been answered 
                parentActivity.StepAnswersList.Add(new StepAnswer { StepId = currentScreenId, AnswerId = currentAnswerId, AnswerValue = currentAnswerValue, DataKey = currentAnswerDataKey, QuestionValue = currentScreenQuestion});

                // move on to the next Question/Step and keep track of the question/steps navigated through
                if (!currentAnswer.IsLastAnswer)
                {
                    parentActivity.ShowStep(currentAnswer.NextScreenId);
                }
                else
                {
                    //send the wizard contents to the next static screen (summary)
                    parentActivity.ProcessTicket();
                }
            }
        }
    }
}