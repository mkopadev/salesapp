namespace SalesApp.Core.BL.Models.Tickets
{
    public class ProcessFlow
    {
        public ProcessFlow()
        {
            //Steps = new List<Step>();
            Step = new Step();
        }

        public Step Step { get; set; }
    }
}