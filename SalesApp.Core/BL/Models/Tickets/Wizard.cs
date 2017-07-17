using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.Tickets
{
    public class Wizard
    {

        public Wizard()
        {
            StepAnswersList = new List<StepAnswer>();
        }

        public string Id { get; set; }
        public string OutcomeId { get; set; }
        public List<StepAnswer> StepAnswersList { get; set; }
    }
}
