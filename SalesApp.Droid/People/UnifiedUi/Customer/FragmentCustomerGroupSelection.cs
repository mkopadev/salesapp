using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;

namespace SalesApp.Droid.People.UnifiedUi.Customer
{
    public class FragmentCustomerGroupSelection : FragmentGroupSelection
    {
        private SalesApp.Core.BL.Models.People.Customer _customer;

        protected override Lead Lead
        {
            get
            {
                return this._customer;
            }
        }

        public override void SetData(string serializedString)
        {
            this._customer = JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Customer>(serializedString);
        }
    }
}