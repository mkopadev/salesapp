using System;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;
using SalesApp.Core.BL.Models.Tickets;
using SalesApp.Core.Enums.Tickets;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Utils;
using Exception = System.Exception;

namespace SalesApp.Droid.Tickets
{
    public class ProcessFlowFragment : FragmentBase3
    {

        private IProcessFlowListener _processFlowListener;
        private TextView _tvQuestionTitle;
        private LinearLayout _ticketLinearLayout;
        private RelativeLayout _scrollTextRelativeLayout;
        private Button _btnPrevious;
        private Step ProcessFlowStep { get; set; }
        private string ProcessFlowStepAnswer { get; set; }

        public interface IProcessFlowListener
        {
            // events to be sent to the host activity
            void ShowStep(Step processFlowStep);
            void ShowPreviousStep();
            void ShowProcessFlowSummary(Step lastProcessFlowStep);
            void PersistInputAnswerToState(Guid stepId, string stepAnswer);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments != null)
            {
                //Logger.Verbose("Fragment recreating Step Object from " + processFlowString);
                ProcessFlowStep = GetArgument<Step>(ProcessFlowActivity.ProcessFlowStep);

                if (ProcessFlowStep.Type != (int)StepInputTypeEnum.Options)
                {
                    ProcessFlowStepAnswer = Arguments.GetString(ProcessFlowActivity.ProcessFlowStepAnswer);
                }

            }
            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Process Flow");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.fragment_ticket, container, false);
            
            // build the screen
            Logger.Debug("Initializing UI");
            InitializeUI();
            Logger.Debug("Updating UI");
            UpdateUI();
            return view;
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);

            if (activity is IProcessFlowListener)
            {
                _processFlowListener = (IProcessFlowListener) activity;
            }
            else
            {
                throw new ClassCastException(activity.ToString() + " must implement ProcessFlowFragment.IProcessFlowListener");
            }
        }

        public override bool Validate()
        {
            throw new NotImplementedException();
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
            if (!isOnUiThread)
            {
                Activity.RunOnUiThread
                    (
                        () =>
                        {
                            InitializeUI(true);
                        }
                    );
                return;
            }

            _tvQuestionTitle = view.FindViewById<TextView>(Resource.Id.tViewQuestionTitle);
            _ticketLinearLayout = view.FindViewById<LinearLayout>(Resource.Id.ticketLinearLayout);
            _scrollTextRelativeLayout = view.FindViewById<RelativeLayout>(Resource.Id.scrollTextRelativeLayout);
            _btnPrevious = view.FindViewById<Button>(Resource.Id.btnPrevious);
            _btnPrevious.Click += delegate
            {
                // when the previous button is clicked, show the last step/question
                _processFlowListener.ShowPreviousStep();
            };
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
            try
            {

                if (!calledFromUiThread)
                {
                    Activity.RunOnUiThread(() => UpdateUI(true));
                    return;
                }

                if (ProcessFlowStep != null)
                {
                    //set the title
                    _tvQuestionTitle.Text = ProcessFlowStep.HeaderText;

                    //depending on the processflow step's type property, render either buttons, edittexts or date picker respectively
                    switch (ProcessFlowStep.Type)
                    {
                        case (int)StepInputTypeEnum.Options:

                            //each substep is rendered as a button
                            foreach (var substep in ProcessFlowStep.SubSteps)
                            {
                                var processFlowSubStep = substep;

                                var answerButton = new Button(Activity);
                                answerButton.SetTextAppearance(Activity, Resource.Style.GreenButton);
                                answerButton.SetBackgroundResource(Resource.Drawable.button_green);
                                answerButton.Text = processFlowSubStep.NavigationText;
                                answerButton.Click += delegate
                                {
                                    // If this is the last step, inform parent activity to show summary in next activity
                                    if (processFlowSubStep.IsEndPoint)
                                    {
                                        _processFlowListener.ShowProcessFlowSummary(processFlowSubStep);
                                    }
                                    else
                                    {
                                        // If not, display the step
                                        _processFlowListener.ShowStep(processFlowSubStep);
                                    }
                                };

                                LinearLayout.LayoutParams answerButtonParams = new LinearLayout.LayoutParams(
                                    LinearLayout.LayoutParams.FillParent,
                                    LinearLayout.LayoutParams.WrapContent
                                    );

                                answerButtonParams.SetMargins(0, 0, 0, 12);
                                _ticketLinearLayout.AddView(answerButton, answerButtonParams);
                            }

                            //determine whether to show the text "Scroll Down to View More", for now 5 answers or more (HACK)
                            if (ProcessFlowStep.SubSteps.Count > 5)
                            {
                                _scrollTextRelativeLayout.Visibility = ViewStates.Visible;
                            }

                            break;
                        case (int)StepInputTypeEnum.TextInput:

                            AddInputView(ProcessFlowStep.DiagnosticText, ProcessFlowStep.Type, ProcessFlowStepAnswer);

                            break;
                        case (int)StepInputTypeEnum.NumericInput:

                            AddInputView(ProcessFlowStep.DiagnosticText, ProcessFlowStep.Type, ProcessFlowStepAnswer);

                            break;
                        case (int)StepInputTypeEnum.DateInput:

                            AddInputView(ProcessFlowStep.DiagnosticText, ProcessFlowStep.Type, ProcessFlowStepAnswer);

                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Verbose(e.Message);
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

        private void AddInputView(string  labelTitle, int stepType, string persistedStepAnswer)
        {
            //define the label to be placed above the text input and set it's title
            TextView label = new TextView(Activity);
            label.SetTextAppearance(Activity, Resource.Style.DefaultTextView);
            label.Text = labelTitle;

            LinearLayout.LayoutParams labelParams = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.WrapContent,
                LinearLayout.LayoutParams.WrapContent
                );

            labelParams.SetMargins(5, 0, 0, 8);
            labelParams.Gravity = GravityFlags.Left;

            _ticketLinearLayout.AddView(label, labelParams);

            //define the text input 
            EditText eText = new EditText(Activity);
            eText.SetTextAppearance(Activity, Resource.Style.DefaultEditText);
            eText.SetBackgroundResource(Resource.Drawable.edit_text_default);

            //and set the inputtype depending on whether a text, numeric or date input is required
            switch (ProcessFlowStep.Type)
            {
                case (int)StepInputTypeEnum.TextInput:

                    eText.SetLines(8);
                    eText.Gravity = GravityFlags.Left | GravityFlags.Top;

                    break;
                case (int)StepInputTypeEnum.NumericInput:

                    eText.InputType = InputTypes.ClassNumber;

                    break;
                case (int)StepInputTypeEnum.DateInput:

                    eText.InputType = InputTypes.ClassDatetime;

                    break;
            }

            if (persistedStepAnswer != null)
            {
                eText.Text = persistedStepAnswer;
            }

            LinearLayout.LayoutParams eTextParams = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.FillParent,
                LinearLayout.LayoutParams.WrapContent
                );

            eTextParams.SetMargins(0, 0, 0, 20);
            eTextParams.Gravity = GravityFlags.Left;

            _ticketLinearLayout.AddView(eText, eTextParams);

            //show the button(s), if any, under the text input (typically a NEXT button should be present)
            if (ProcessFlowStep.SubSteps.Any())
            {
                //each substep is rendered as a button
                foreach (var substep in ProcessFlowStep.SubSteps)
                {
                    var processFlowSubStep = substep;

                    var answerButton = new Button(Activity);
                    answerButton.SetTextAppearance(Activity, Resource.Style.GreenButton);
                    answerButton.SetBackgroundResource(Resource.Drawable.button_green);
                    answerButton.Text = substep.NavigationText;
                    answerButton.Click += delegate
                    {
                        // If this is the last step, inform parent activity to show summary in next activity
                        if (processFlowSubStep.IsEndPoint)
                        {
                            string stepAnswer = null;

                            //loop through all the edittext widgets on the screen, get the value of each and set it to the corresponding answer
                            for (var x = 0; x < _ticketLinearLayout.ChildCount; x++)
                            {
                                var widget = _ticketLinearLayout.GetChildAt(x);

                                if (!(widget is EditText)) continue;
                                var textInputWidget = (EditText)widget;

                                if (string.IsNullOrEmpty(textInputWidget.Text))
                                {
                                    AlertDialogBuilder.Instance
                                        .AddButton(Resource.String.ok, validateFail)
                                        .SetText(0, Resource.String.value_required)
                                        .Show(Activity);
                                    return;
                                }

                                stepAnswer = textInputWidget.Text;

                            }

                            //for now we assume there is only one textinput per step
                            _processFlowListener.PersistInputAnswerToState(ProcessFlowStep.Id, stepAnswer);

                            _processFlowListener.ShowProcessFlowSummary(processFlowSubStep);
                        }
                        else
                        {
                            string stepAnswer = null;

                            //loop through all the edittext widgets on the screen, get the value of each and set it to the corresponding answer
                            for (var x = 0; x < _ticketLinearLayout.ChildCount; x++)
                            {
                                var widget = _ticketLinearLayout.GetChildAt(x);

                                if (!(widget is EditText)) continue;
                                var textInputWidget = (EditText)widget;

                                if (string.IsNullOrEmpty(textInputWidget.Text))
                                {
                                    AlertDialogBuilder.Instance
                                        .AddButton(Resource.String.ok, validateFail)
                                        .SetText(0, Resource.String.value_required)
                                        .Show(Activity);
                                    return;
                                }

                                stepAnswer = textInputWidget.Text;

                            }

                            //for now we assume there is only one textinput per step
                            _processFlowListener.PersistInputAnswerToState(ProcessFlowStep.Id, stepAnswer);

                            // If not, display the step
                            _processFlowListener.ShowStep(processFlowSubStep);
                        }
                    };

                    LinearLayout.LayoutParams answerButtonParams = new LinearLayout.LayoutParams(
                        LinearLayout.LayoutParams.FillParent,
                        LinearLayout.LayoutParams.WrapContent
                        );

                    answerButtonParams.SetMargins(0, 0, 0, 12);
                    _ticketLinearLayout.AddView(answerButton, answerButtonParams);
                }
            }            
        }

        private void validateFail()
        {
            return;
        }
    }
}