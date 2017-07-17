using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SalesApp.Core.BL.Models.Tickets
{
    public class Step
    {
        public Step()
        {
            SubSteps = new List<Step>();
        }

        //private StepInputTypeEnum stepType;
        public Guid Id { get; set; }
        [JsonIgnore]
        public string Code { get; set; }
        public int Type { get; set; }
        public string NavigationText { get; set; }
        public string HeaderText { get; set; }
        public string DiagnosticText { get; set; }
        [JsonIgnore]
        public string BreadCrumbText { get; set; }
        [JsonIgnore]
        public bool IsActive { get; set; }
        public bool IsEndPoint { get; set; }
        [JsonIgnore]
        public Guid Outcome { get; set; }
        public string DataKey { get; set; }
        public List<Step> SubSteps { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(HeaderText.ToUpper());
            sb.AppendLine();
            sb.Append(NavigationText.ToUpper());
            sb.AppendLine();
            //sb.Append("OUTCOME ID");
            //sb.AppendLine();
            //sb.Append(Outcome.ToString());

            if (SubSteps.Any())
            {
                sb.Append("CHILD STEP COUNT");
                sb.AppendLine();
                sb.Append(SubSteps.Count.ToString());
            }

            return sb.ToString();

        }
    }
}
