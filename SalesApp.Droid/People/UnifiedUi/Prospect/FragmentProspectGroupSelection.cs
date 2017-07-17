using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;

namespace SalesApp.Droid.People.UnifiedUi.Prospect
{
    public class FragmentProspectGroupSelection : FragmentGroupSelection
    {
        private SalesApp.Core.BL.Models.People.Prospect _prospect;

        protected override Lead Lead
        {
            get
            {
                return this._prospect;
            }
        }

        public override void SetData(string serializedString)
        {
            this._prospect = JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Prospect>(serializedString);
        }
    }
}