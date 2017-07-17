using System;
using System.Collections.Generic;
using SalesApp.Core.Enums.People;
using SQLite.Net.Attributes;

namespace SalesApp.Core.BL.Models.People
{
    /// <summary>
    /// Class that represents a client who is a customer
    /// </summary>
    public class Customer : Lead
    {
        private List<CustomerPhoto> _photos;
        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        public Customer()
        {
            this.PersonType = PersonTypeEnum.Customer;
            this.DontSync = true; // syncs before saving to device
        }

        /// <summary>
        /// Gets or sets the user id
        /// </summary>
        [Ignore]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the customer's registration ID;
        /// </summary>
        public Guid RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the photo being taken
        /// </summary>
        public PhotoType TypeOfPhotoBeingTaken { get; set; }

        public string AccountStatus { get; set; }

        [Ignore]
        public bool IsAdditionalProduct { get; set; }

        [Ignore]
        public bool IsRejected { get; set; }

        [Ignore]
        public List<CustomerPhoto> Photos {
            get
            {
                if (this._photos == null)
                {
                    this._photos = new List<CustomerPhoto>();
                }

                return this._photos;
            }

            set { this._photos = value; }
        }
    }
}