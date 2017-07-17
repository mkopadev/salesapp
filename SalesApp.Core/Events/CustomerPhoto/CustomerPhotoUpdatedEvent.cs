using System;

namespace SalesApp.Core.Events.CustomerPhoto
{
    public class CustomerPhotoUpdatedEvent : EventArgs
    {
        // public BL.Models.People.CustomerPhoto UpdatedPhoto { get; private set; }
        public int Position { get; private set; }

        public CustomerPhotoUpdatedEvent(int position)
        {
            this.Position = position;
        }
    }
}