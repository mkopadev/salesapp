namespace SalesApp.Core.BL.Models.Tickets
{
    public class Ticket
    {

        public Ticket()
        {

        }

        public string entity { get; set; }
        //public int startScreenId { get; set; }
        public EntityIdentifier entityIdentifier { get; set; }
        public string date { get; set; }
        public Wizard wizard { get; set; }

    }
}
