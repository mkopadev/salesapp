using System;

namespace SalesApp.Core.BL.Models.Modules.Facts
{
    public class Fact
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ImageUrl
        {
            get
            {
                string fileName = this.Id.ToString("N") + ".jpg";

                return "Facts/" + fileName;
            }
        }
    }
}