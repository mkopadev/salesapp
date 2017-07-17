using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Core.Services.Interfaces;

namespace SalesApp.Core.Services.Person
{
    public class RegistrationStatusService
    {
        private IDataService<CustomerRegistrationStepsStatus> _customerRegistration = new CustomerRegistrationStepsStatusController();
        private static readonly string NotStarted = "NotStarted";
        private static readonly string Rejected = "Rejected";

        public IDataService<CustomerRegistrationStepsStatus> CustomerRegistrationStepsStatusController
        {
            set { _customerRegistration = value; }
        }

        public async Task<RegistrationStatusOverview> GetVisibleSteps(BL.Models.People.Customer customer,
            List<CustomerRegistrationStep> steps, string additionalInfo)
        {
            List<CustomerRegistrationStepsStatus> statusList = steps
                .Select(step => new CustomerRegistrationStepsStatus
                {
                    Customer = customer,
                    CustomerId = customer.Id,
                    StepName = step.StepName,
                    StepNumber = step.StepNumber,
                    StepStatus = step.StepStatus,
                    AdditionalInfo = additionalInfo
                }).ToList();

            return await GetOverViewAsync(statusList);
        }

        /// <summary>
        /// This method retrieves the steps for a product of a customer and returns a object indicating:
        /// 1. Previous, Current and Next step
        /// 2. Current Step No
        /// 3. Total No of Step
        /// </summary>
        /// <param name="customerId">Customer to get the regsitration step for</param>
        /// <returns>Overview of steps for the customer product registration</returns>
        public async Task<RegistrationStatusOverview> GetVisibleSteps(Guid customerId)
        {
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();

            // Create the resulting object
            List<CustomerRegistrationStepsStatus> statusList = await _customerRegistration.GetManyByCriteria(
                criteriaBuilder
                    .Add("CustomerId", customerId));
            return await this.GetOverViewAsync(statusList);
        }

        private async Task<RegistrationStatusOverview> GetOverViewAsync(List<CustomerRegistrationStepsStatus> statusList)
        {
            RegistrationStatusOverview overview = new RegistrationStatusOverview();

            if (statusList == null || statusList.Count == 0)
            {
                return null;
            }

            // order the list to make sure we have the right order in steps
            statusList = statusList.OrderBy(l => l.StepNumber).ToList();

            // set the total amount of steps
            overview.TotalSteps = statusList.Count;

            // scan list and find the right steps for the overview
            foreach (var status in statusList)
            {
                if (status.StepName == Rejected)
                {
                    // first NotStarted, store as current
                    overview.CurrentStep = status;
                    break;
                }

                // if Done
                if (overview.CurrentStep == null && status.StepStatus == NotStarted)
                {
                    // first NotStarted, store as current
                    overview.CurrentStep = status;
                }
                else if (overview.CurrentStep == null)
                {
                    // only do this when current not yet found (last item before current set is previous)
                    overview.PreviousStep = status;
                }
                else
                {
                    // set this one after current is set, then also break out of the list
                    overview.FutureStep = status;
                    break; // get out of the loop we are done
                }
            }

            return overview;
        }
    }
}
