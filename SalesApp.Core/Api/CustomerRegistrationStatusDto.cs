using System.Collections.Generic;

namespace SalesApp.Core.Api
{
    public class CustomerRegistrationStatusDto
    {
        public int RequestStatus { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerProduct { get; set; }
        public List<RegistrationStepDto> Steps { get; set; }
        public string Info { get; set; }
    }

    public class RegistrationStepDto
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; }
        public string StepStatus { get; set; }
    }
}
