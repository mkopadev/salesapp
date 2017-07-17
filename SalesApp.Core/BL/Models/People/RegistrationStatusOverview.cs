namespace SalesApp.Core.BL.Models.People
{
    public class RegistrationStatusOverview
    {
        public CustomerRegistrationStepsStatus PreviousStep { get; set; }
        public CustomerRegistrationStepsStatus CurrentStep { get; set; }
        public CustomerRegistrationStepsStatus FutureStep { get; set; }

        public bool HasPreviousStep
        {
            get { return PreviousStep != null; }
        }

        public bool HasCurrentStep
        {
            get { return CurrentStep != null; }
        }

        public bool HasFutureStep
        {
            get { return FutureStep != null; }
        }

        public int CurrentStepNo
        {
            get { return CurrentStep != null ? CurrentStep.StepNumber : 0; }
        }

        public int TotalSteps { get; set; }
    }
}
