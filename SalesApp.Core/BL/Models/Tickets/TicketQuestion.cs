using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.Tickets
{
    public class TicketQuestion
    {
        public TicketQuestion()
        {
            Answers = new List<Answer>();
        }

        public int ScreenId { get; set; }
        public string ScreenTitle { get; set; }
        public int? ParentScreenId { get; set; }
        public List<Answer> Answers { get; set; }
    }
}
