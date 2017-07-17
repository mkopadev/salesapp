using System;
using Newtonsoft.Json.Converters;

namespace SalesApp.Core.BL.Models.Tickets
{
    public class StepConverter : CustomCreationConverter<Step>
    {
        public override Step Create(Type objectType)
        {
            return new Step();
        }
    }
}
