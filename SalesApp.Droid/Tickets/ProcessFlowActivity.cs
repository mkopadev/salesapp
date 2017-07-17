using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.Tickets;
using SalesApp.Core.Enums.Tickets;
using SalesApp.Core.Extensions;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.UI.Utils;

namespace SalesApp.Droid.Tickets
{
    [Activity(Label = "Report a problem", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = false, ParentActivity = typeof(HomeView))]
    public class ProcessFlowActivity : ActivityBase2, ProcessFlowFragment.IProcessFlowListener, ProcessFlowSummaryFragment.ITicketSubmissionListener
    {
        private const string TicketFragmentTag = "TICKET_FRAGMENT_TAG";
        private const string SummaryFragmentTag = "SummaryFragmentTag";
        private const string SubmitResultFragmentTag = "SubmitResultFragmentTag";
        public const string AccountNumber = "AccountNumber";
        public const string Entity = "Entity";
        public const string PhoneNumber = "PhoneNumber";
        public const string SerialNumber = "SerialNumber";
        public const string OutComeId = "OutComeId";
        public const string StepListSaved = "StepListSaved";
        public const string ListStepAnswers = "StepAnswerList";
        public const string StepAnswerListSaved = "StepAnswerListSaved";
        public const string ProcessFlowStep = "ProcessFlowStep";
        public const string ProcessFlowStepAnswer = "ProcessFlowStepAnswer";

        private string AccountNumberString { set; get; }
        private string EntityString { set; get; }
        private string PhoneNumberString { set; get; }
        private string SerialNumberString { set; get; }
        private string OutComeIdString { set; get; }
        public bool IsSubmitting { set; get; }

        private string ProcessFlowStringDefintion { get; set; }
        private string StepAnswersStringDefintion { get; set; }
        private List<Step> StepList { get; set; }
        private Dictionary<Guid, string> StepAnswersDictionary { get; set; }
        public string StepAnswerListStringDefinition { set; get; }
        public string TicketReferenceNumber { set; get; }

        private ProcessFlowSummaryFragment ProcessFlowSummaryFragmentInstance
        {
            get
            {
                var processFlowSummaryFragment = new ProcessFlowSummaryFragment();

                Bundle fragmentBundle = new Bundle();
                fragmentBundle.PutString(Entity, EntityString);
                fragmentBundle.PutString(AccountNumber, AccountNumberString);
                fragmentBundle.PutString(PhoneNumber, PhoneNumberString);
                fragmentBundle.PutString(SerialNumber, SerialNumberString);
                fragmentBundle.PutString(ListStepAnswers, StepAnswerListStringDefinition);
                fragmentBundle.PutString(OutComeId, OutComeIdString);

                processFlowSummaryFragment.Arguments = fragmentBundle;

                return processFlowSummaryFragment;
            }
        }

        private FragmentTicketSubmitResult PostSubmissionFragmentInstance
        {
            get
            {
                var fragmentTicketSubmitResult = new FragmentTicketSubmitResult();

                var bundle = new Bundle();
                bundle.PutString(FragmentTicketSubmitResult.MessageKey, TicketReferenceNumber);
                fragmentTicketSubmitResult.Arguments = bundle;

                return fragmentTicketSubmitResult;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_createTicketWizard);

            if (savedInstanceState != null)
            {
                EntityString = savedInstanceState.GetString(Entity);
                AccountNumberString = savedInstanceState.GetString(AccountNumber);
                PhoneNumberString = savedInstanceState.GetString(PhoneNumber);
                SerialNumberString = savedInstanceState.GetString(SerialNumber);
                ProcessFlowStringDefintion = savedInstanceState.GetString(StepListSaved);
                StepAnswersStringDefintion = savedInstanceState.GetString(StepAnswerListSaved);
                StepList = JsonConvert.DeserializeObject<List<Step>>(ProcessFlowStringDefintion);
                StepAnswersDictionary = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(StepAnswersStringDefintion);
            }
            else
            {
                EntityString = Intent.GetStringExtra(Entity);
                AccountNumberString = Intent.GetStringExtra(AccountNumber);
                PhoneNumberString = Intent.GetStringExtra(PhoneNumber);
                SerialNumberString = Intent.GetStringExtra(SerialNumber);
                ProcessFlowStringDefintion = Intent.GetStringExtra(ProcessFlowStep);
                StepList = new List<Step>();
                StepAnswersDictionary = new Dictionary<Guid, string>();
            }

            UpdateScreen();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(AccountNumber, AccountNumberString);
            outState.PutString(Entity, EntityString);
            outState.PutString(PhoneNumber, PhoneNumberString);
            outState.PutString(SerialNumber, SerialNumberString);

            if (StepList != null)
            {
                var savedTicketListDefinition = JsonConvert.SerializeObject(StepList);
                Logger.Verbose("Saving Step List into state");
                //Logger.Verbose(savedTicketListDefinition);
                outState.PutString(StepListSaved, savedTicketListDefinition);
            }

            if (StepAnswersDictionary != null)
            {
                var savedStepAnswerListDefinition = JsonConvert.SerializeObject(StepAnswersDictionary);
                Logger.Verbose("Saving Step Answer Dictionary into state");
                //Logger.Verbose(savedStepAnswerListDefinition);
                outState.PutString(StepAnswerListSaved, savedStepAnswerListDefinition);
            }

            base.OnSaveInstanceState(outState);
        }

        public override void OnBackPressed()
        {
            //disable the hardware back button depending on whether the final screen is visible or the Ticket is being published, otherwise show the last step

            if (IsSubmitting)
            {
                return;
            }

            var submitResultFragment = GetFragmentManager().FindFragmentByTag(SubmitResultFragmentTag);

            if (submitResultFragment == null)
            {
                ShowPreviousStep();
            }
            else
            {
                if (submitResultFragment.IsVisible)
                {
                    return;
                }
                else
                {
                    ShowPreviousStep();
                }
            }


        }

        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }

        public override void InitializeScreen()
        {
            throw new NotImplementedException();
        }

        public override void RetrieveScreenInput()
        {
            throw new NotImplementedException();
        }

        public override void UpdateScreen()
        {
            try
            {
                //deserialize the json string and display the root step of the process flow
                if (ProcessFlowStringDefintion != null)
                {
                    var processFlowStep = AsyncHelper.RunSync(
                        async () => await GetRootStep(ProcessFlowStringDefintion)
                        );

                    if (processFlowStep != null)
                    {
                        //show the step
                        ShowStep(processFlowStep);
                    }
                    else
                    {
                        //inform the user that there is no process flow defintion
                        AlertDialogBuilder.Instance
                            .AddButton(Resource.String.ok, okButtonHandler)
                            .SetText(0, Resource.String.missing_processflow_definition)
                            .Show(this);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void okButtonHandler()
        {
            return;
        }

        private async Task<Step> GetRootStep(string processFlowStringDefinition)
        {
            Step processFlowRootStep = null;

            try
            {
                processFlowRootStep = JsonConvert.DeserializeObject<ProcessFlow>(ProcessFlowStringDefintion).Step;
            }
            catch (JsonException j)
            {
                Logger.Error(j);
                return processFlowRootStep;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return processFlowRootStep;
            }

            return processFlowRootStep;
        }

        public override void SetListeners()
        {
            throw new NotImplementedException();
        }

        public override bool Validate()
        {
            throw new NotImplementedException();
        }

        public async void ShowStep(Step processFlowStep)
        {
            //either display the step or  remove it from the persisted step list 
            if (processFlowStep != null)
            {
                var stepToAddRemove = processFlowStep;

                if (StepList.Contains(stepToAddRemove))
                {
                    //text/numeric/date input answers to be removed from state
                    if (stepToAddRemove.Type != (int) StepInputTypeEnum.Options)
                    {
                        //remove the saved answer from persisted state
                        StepAnswersDictionary.Remove(stepToAddRemove.Id);
                    }

                    //if it already exists in the persisted step list, remove it
                    StepList.Remove(stepToAddRemove);

                    if (StepList.Any())
                    {
                        //and get the step before it
                        stepToAddRemove = StepList.Last();
                    }
                    else
                    {
                        //if after removing and there are no more steps, go back to the previous activity
                        base.OnBackPressed();
                    }
                }
                else
                {
                    //if it doesnt exist in the persisted step list, add it
                    StepList.Add(stepToAddRemove);
                }

                //instantiate a new Step Fragment
                var processFlowFragment = new ProcessFlowFragment();
                Bundle args = new Bundle();

                //serialize the object to a step definition string
                var processFlowString = JsonConvert.SerializeObject(stepToAddRemove);

                //pass the step string definition to the fragment in the bundle
                args.PutString(ProcessFlowStep, processFlowString);

                // if the step to be shown is a text/numeric/date input, check the dictionary for a saved answer entered previously and pass it to the fragment
                if (stepToAddRemove.Type != (int) StepInputTypeEnum.Options)
                {
                    string stepAnwer = null;
                    if(StepAnswersDictionary.TryGetValue(stepToAddRemove.Id, out stepAnwer))
                        Logger.Verbose(stepToAddRemove.HeaderText + " : Answer found in Dictionary to be shown in Fragment : " + stepAnwer);
                    
                    args.PutString(ProcessFlowStepAnswer, stepAnwer);
                }

                processFlowFragment.Arguments = args;

                //display the fragment
                ReplaceFragment(processFlowFragment, Resource.Id.ticket_placeholder, TicketFragmentTag);
            }
        }

        public void ShowPreviousStep()
        {
            //if there are still steps/questions
            if (StepList.Any())
            {
                //get the last step in the list
                var lastStep = StepList.Last();

                //display it
                ShowStep(lastStep);
            }
            else
            {
                //if there are no steps remaining, call OnBackPressed
                base.OnBackPressed();
            }
        }

        public void ShowProcessFlowSummary(Step lastProcessFlowStep)
        {
            List<StepAnswer> stepAnswersList = new List<StepAnswer>();

            if (lastProcessFlowStep != null)
                StepList.Add(lastProcessFlowStep);

            // loop throuch each of the steps and display the question and answers depending on the Step Type (Button, Text, Numeric, Date)
            foreach (var step in StepList)
            {
                var stepIndex = StepList.IndexOf(step);

                //skip the first step since it is the root step
                if (stepIndex > 0)
                {
                    //get the step interacted with before the current one
                    var stepBefore = StepList[stepIndex - 1];
                            
                    // if the step before the current one is of type button then the NavigationText attribute is the answer to the prior step
                    if (stepBefore.Type == (int) StepInputTypeEnum.Options)
                    {
                        //Logger.Verbose(stepBefore.HeaderText + " : " + step.NavigationText);
                        //Logger.Verbose("");
                        stepAnswersList.Add(new StepAnswer { AnswerValue = step.NavigationText, QuestionValue = stepBefore.HeaderText });

                    }
                    else
                    {
                        // else if the step before the current one is of type text/numeric/date then get the answer from the dictionary
                        string stepAnwer = "";
                        if (StepAnswersDictionary.TryGetValue(stepBefore.Id, out stepAnwer))
                        {
                            Logger.Verbose(stepBefore.HeaderText + " : " + stepAnwer);
                            //Logger.Verbose("");
                            stepAnswersList.Add(new StepAnswer { AnswerValue = stepAnwer, DataKey = stepBefore.DataKey, QuestionValue = stepBefore.HeaderText });

                        }
                    }
                }
            }

            //set the serialiazed string of step answers
            StepAnswerListStringDefinition = JsonConvert.SerializeObject(stepAnswersList);

            //set the OutcomeId of the last step (with an Endpoint)
            if (lastProcessFlowStep != null)
            {
                OutComeIdString = lastProcessFlowStep.Outcome.ToString();
            }

            //display the process flow summary
            ReplaceFragment(ProcessFlowSummaryFragmentInstance, Resource.Id.ticket_placeholder, SummaryFragmentTag);
        }

        public void PersistInputAnswerToState(Guid stepId, string stepAnswer)
        {
            //add or replace text/numeric/date values in the dictionary
            if (StepAnswersDictionary.ContainsKey(stepId))
            {
                StepAnswersDictionary[stepId] = stepAnswer;
            }
            else
            {
                StepAnswersDictionary.Add(stepId, stepAnswer);
            }
        }

        public void ShowPostSubmissionFragment(string ticketReferenceNumber)
        {
            TicketReferenceNumber = ticketReferenceNumber;

            ReplaceFragment(PostSubmissionFragmentInstance, Resource.Id.ticket_placeholder, SubmitResultFragmentTag);
        }
    }
}