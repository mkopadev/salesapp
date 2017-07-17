using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.Tickets;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.Tickets
{
    [Activity(Label = "Raise Issue", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = false, ParentActivity = typeof(HomeView))]
    public class RaiseTicketActivity : ActivityBase2
    {
        private const string TicketFragmentTag = "TICKET_FRAGMENT_TAG";
        public const string AccountNumber = "AccountNumber";
        public const string Entity = "Entity";
        public const string PhoneNumber = "PhoneNumber";
        public const string SerialNumber = "SerialNumber";
        public const string StartScreenId = "StartScreenID";
        public const string TicketListSaved = "TicketListSaved";
        public const string ListStepAnswers = "StepAnswerList";
        public const string StepAnswerListSaved = "StepAnswerListSaved";
        public const string TicketFragment = "TicketFragment";

        private FragmentTicket _ticketFragment;
        private List<TicketQuestion> TicketQuestionList { get; set; }
        private string TicketQuestionListDefintion { get; set; }
        public List<StepAnswer> StepAnswersList { get; set; }
        private string StepAnswersListDefintion { get; set; }
        private string _entity;
        private string _accountNumber;
        private string _phoneNumber;
        private string _serialNumber;
        private int _startScreenId;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_createTicketWizard);

            if (savedInstanceState != null)
            {
                _entity = savedInstanceState.GetString(Entity);
                _accountNumber = savedInstanceState.GetString(AccountNumber);
                _phoneNumber = savedInstanceState.GetString(PhoneNumber);
                _serialNumber = savedInstanceState.GetString(SerialNumber);
                // TODO ensure the starting screen Id is a value that exists in the JSON filed located in the Asssets Folder
                _startScreenId = savedInstanceState.GetInt(StartScreenId, -1);
                TicketQuestionListDefintion = savedInstanceState.GetString(TicketListSaved);
                StepAnswersListDefintion = savedInstanceState.GetString(StepAnswerListSaved);
            }
            else
            {
                _entity = Intent.GetStringExtra(Entity);
                _accountNumber = Intent.GetStringExtra(AccountNumber);
                _phoneNumber = Intent.GetStringExtra(PhoneNumber);
                _serialNumber = Intent.GetStringExtra(SerialNumber);
                // TODO ensure the starting screen Id is a value that exists in the JSON filed located in the Asssets Folder
                _startScreenId = Intent.GetIntExtra(StartScreenId, -1);
            }

            UpdateScreen();

        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(AccountNumber, _accountNumber);
            outState.PutString(Entity, _entity);
            outState.PutString(PhoneNumber, _phoneNumber);
            outState.PutString(SerialNumber, _serialNumber);
            outState.PutInt(StartScreenId, _startScreenId);

            if (TicketQuestionList != null)
            {
                var savedTicketListDefinition = JsonConvert.SerializeObject(TicketQuestionList);
                Logger.Verbose("Saving state, Ticket Question Definiition");
                Logger.Verbose(savedTicketListDefinition);
                outState.PutString(TicketListSaved, savedTicketListDefinition);
            }

            if (StepAnswersList != null)
            {
                //var stepCount = StepAnswersList.Count;
                //if (stepCount > 1)
                //{
                //    StepAnswersList.RemoveAt(stepCount - 1);
                //}
                var savedStepAnswerListDefinition = JsonConvert.SerializeObject(StepAnswersList);
                Logger.Verbose("Saving state, Step Answer List Definiition");
                Logger.Verbose(savedStepAnswerListDefinition);
                outState.PutString(StepAnswerListSaved, savedStepAnswerListDefinition);
            }

            base.OnSaveInstanceState(outState);
        }

        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }

        public void ProcessTicket()
        {
            var intent = new Intent(this, typeof(TicketSubmissionActivity));
            intent.PutExtra(Entity, _entity);
            intent.PutExtra(AccountNumber, _accountNumber);
            intent.PutExtra(PhoneNumber, _phoneNumber);
            intent.PutExtra(SerialNumber, _serialNumber);
            var stepAnswerListString = JsonConvert.SerializeObject(StepAnswersList);
            intent.PutExtra(ListStepAnswers, stepAnswerListString);

            Logger.Verbose(stepAnswerListString);
            StartActivity(intent);
        }

        public void ShowLastStep()
        {
            Logger.Verbose("RaiseTicketActivity:ShowLastStep invoked");
            Logger.Verbose("RaiseTicketActivity:ShowLastStep Items in List before show and remove " + StepAnswersList.Count);

            //if there are still steps/questions
            if (StepAnswersList.Any())
            {

                //get the last step in the list
                var lastStep = StepAnswersList.Last();

                //display it
                ShowStep(lastStep.StepId);

                //then remove it from the list
                StepAnswersList.Remove(lastStep);
            }
            else
            {

                //if there are no steps remaining, call OnBackPressed
                base.OnBackPressed();
            }


        }

        public void ShowStep(int? stepId)
        {

            if (stepId == null)
            {
                //display alert dialog
                return;
            }

            //if the answer's next screen Id is not null, find it's definition and load the fragment
            try
            {
                var ticketStepDefinition = TicketQuestionList.Find(q => q.ScreenId == stepId);

                if (ticketStepDefinition != null)
                {
                    //_ticketFragment = new FragmentTicket(ticketStepDefinition);
                    _ticketFragment = new FragmentTicket();
                    _ticketFragment.SetArgument(TicketFragment, ticketStepDefinition);
                    ShowFragment(_ticketFragment);
                }
                else
                {
                    //display alertdialog stating step definition could not be found
                }
            }
            catch (Exception ex)
            {
                Logger.Verbose(ex.Message);
                return;
            }
        }

        private void ShowFragment(FragmentTicket newfragment)
        {
            try
            {                
                GetFragmentManager().BeginTransaction().Replace(Resource.Id.ticket_placeholder, newfragment, TicketFragmentTag).Show(newfragment).CommitAllowingStateLoss();
            }
            catch (Exception ex)
            {
                Logger.Verbose(ex.Message);
                return;
            }
        }

        public override void OnBackPressed()
        {
            Logger.Verbose("RaiseTicketActivity:onBackPressed invoked");

            ShowLastStep();
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

                if (TicketQuestionListDefintion == null)
                {
                    using (var sr = new StreamReader(Assets.Open("ticketscreens.json")))
                    {
                        //load the screen definitions from the json file located in the Assets folder
                        TicketQuestionListDefintion = sr.ReadToEnd();
                    }
                }

                if (TicketQuestionListDefintion != null)
                {
                    //deserialize the json structure
                    TicketQuestionList = JsonConvert.DeserializeObject<List<TicketQuestion>>(TicketQuestionListDefintion);

                    Logger.Verbose("Deserialized Screen List Count : " + TicketQuestionList.Count());

                    //if we have list of questions
                    if (TicketQuestionList.Any())
                    {
                        TicketQuestion startTicketQuestion = null;

                        if (StepAnswersListDefintion != null)
                        {
                            StepAnswersList = JsonConvert.DeserializeObject<List<StepAnswer>>(StepAnswersListDefintion);
                        }

                        //determine the starting screen based on whether we have an existing list of step answers
                        if (StepAnswersList == null)
                        {
                            //startTicketQuestion = TicketQuestionList.ElementAt(0);
							startTicketQuestion = TicketQuestionList.Find(q => q.ScreenId == _startScreenId);
                        }
                        else
                        {
                            //if there is persisted list, get the last one and use that as the starting point
                            if (StepAnswersList.Any())
                            {
                                var lastStepAnswer = StepAnswersList.ElementAt(StepAnswersList.Count() - 1);
                                Logger.Verbose("Last Step to be reloaded : " + lastStepAnswer.StepId);
                                startTicketQuestion = TicketQuestionList.Find(q => q.ScreenId == lastStepAnswer.StepId);
                            }
                            else
                            {
                                //startTicketQuestion = TicketQuestionList.ElementAt(0);
								startTicketQuestion = TicketQuestionList.Find(q => q.ScreenId == _startScreenId);
                            }
                        }

                        if (startTicketQuestion != null)
                        {
                            if (StepAnswersList == null)
                            {
                                StepAnswersList = new List<StepAnswer>();
                            }

                            _ticketFragment = new FragmentTicket();
                            _ticketFragment.SetArgument(TicketFragment, startTicketQuestion);
                            ShowFragment(_ticketFragment);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Verbose(ex.Message);
                return;
            }
        }

        public override void SetListeners()
        {
            throw new NotImplementedException();
        }

        public override bool Validate()
        {
            throw new NotImplementedException();
        }
    }
}