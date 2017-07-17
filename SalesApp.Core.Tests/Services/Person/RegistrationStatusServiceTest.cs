using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mkopa.Core.Api.Person;
using Mkopa.Core.BL.Controllers.People;
using Mkopa.Core.BL.Models.People;
using Mkopa.Core.Services.Interfaces;
using Mkopa.Core.Services.Person;
using NSubstitute;
using NUnit.Framework;

namespace MKopa.CoreTests.Services.Person
{
    [TestFixture]
    public class RegistrationStatusServiceTest : BaseTest
    {
        [Test]
        public void GetVisibleStepsTest()
        {

            // Lists are unordered to check against that.
            var service = new RegistrationStatusService();
            var mockDataService = Substitute.For<IDataService<CustomerRegistrationStepsStatus>>();
            service.CustomerRegistrationStepsStatusController = mockDataService;

            List<CustomerRegistrationStepsStatus> stepList = new List<CustomerRegistrationStepsStatus>
            {
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 2",
                    StepNumber = 2,
                    StepStatus = "NotStarted"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 1",
                    StepNumber = 1,
                    StepStatus = "Done"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 3",
                    StepNumber = 3,
                    StepStatus = "NotStarted"
                }
            };
            
            var dataServiceResponse = Task.FromResult(stepList);

            mockDataService.GetManyByCriteria(null).ReturnsForAnyArgs(dataServiceResponse);

            var result = service.GetVisibleSteps(Guid.Empty);

            Assert.That(result.PreviousStep.StepNumber, Is.EqualTo(1));
            Assert.That(result.CurrentStep.StepNumber, Is.EqualTo(2));
            Assert.That(result.FutureStep.StepNumber, Is.EqualTo(3));
            Assert.That(result.HasPreviousStep, Is.True);
            Assert.That(result.HasCurrentStep, Is.True);
            Assert.That(result.HasFutureStep, Is.True);
            Assert.That(result.CurrentStepNo, Is.EqualTo(2));
            Assert.That(result.TotalSteps, Is.EqualTo(3));
            

            stepList = new List<CustomerRegistrationStepsStatus>
            {
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 1",
                    StepNumber = 1,
                    StepStatus = "Done"
                },
                
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 4",
                    StepNumber = 4,
                    StepStatus = "NotStarted"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 3",
                    StepNumber = 3,
                    StepStatus = "NotStarted"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 5",
                    StepNumber = 5,
                    StepStatus = "NotStarted"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 2",
                    StepNumber = 2,
                    StepStatus = "Done"
                }
            };

            // set new mock
            mockDataService.GetManyByCriteria(null).ReturnsForAnyArgs(Task.FromResult(stepList));

            result = service.GetVisibleSteps(Guid.Empty);

            Assert.That(result.PreviousStep.StepNumber, Is.EqualTo(2));
            Assert.That(result.CurrentStep.StepNumber, Is.EqualTo(3));
            Assert.That(result.FutureStep.StepNumber, Is.EqualTo(4));
            Assert.That(result.HasPreviousStep, Is.True);
            Assert.That(result.HasCurrentStep, Is.True);
            Assert.That(result.HasFutureStep, Is.True);
            Assert.That(result.CurrentStepNo, Is.EqualTo(3));
            Assert.That(result.TotalSteps, Is.EqualTo(5));

            // no previous step available
            stepList = new List<CustomerRegistrationStepsStatus>
            {
                
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 2",
                    StepNumber = 2,
                    StepStatus = "NotStarted"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 3",
                    StepNumber = 3,
                    StepStatus = "NotStarted"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 1",
                    StepNumber = 1,
                    StepStatus = "NotStarted"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 4",
                    StepNumber = 4,
                    StepStatus = "NotStarted"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 5",
                    StepNumber = 5,
                    StepStatus = "NotStarted"
                }
            };

            // set new mock
            mockDataService.GetManyByCriteria(null).ReturnsForAnyArgs(Task.FromResult(stepList));

            result = service.GetVisibleSteps(Guid.Empty);

            Assert.That(result.PreviousStep, Is.Null);
            Assert.That(result.CurrentStep.StepNumber, Is.EqualTo(1));
            Assert.That(result.FutureStep.StepNumber, Is.EqualTo(2));
            Assert.That(result.HasPreviousStep, Is.False);
            Assert.That(result.HasCurrentStep, Is.True);
            Assert.That(result.HasFutureStep, Is.True);
            Assert.That(result.CurrentStepNo, Is.EqualTo(1));
            Assert.That(result.TotalSteps, Is.EqualTo(5));

            // no previous step available
            stepList = new List<CustomerRegistrationStepsStatus>
            {
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 1",
                    StepNumber = 1,
                    StepStatus = "Done"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 2",
                    StepNumber = 2,
                    StepStatus = "Done"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 3",
                    StepNumber = 3,
                    StepStatus = "Done"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 4",
                    StepNumber = 4,
                    StepStatus = "Done"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 5",
                    StepNumber = 5,
                    StepStatus = "NotStarted"
                }
            };

            // set new mock
            mockDataService.GetManyByCriteria(null).ReturnsForAnyArgs(Task.FromResult(stepList));

            result = service.GetVisibleSteps(Guid.Empty);

            Assert.That(result.PreviousStep.StepNumber, Is.EqualTo(4));
            Assert.That(result.CurrentStep.StepNumber, Is.EqualTo(5));
            Assert.That(result.FutureStep, Is.Null);

            // all done
            stepList = new List<CustomerRegistrationStepsStatus>
            {
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 1",
                    StepNumber = 1,
                    StepStatus = "Done"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 2",
                    StepNumber = 2,
                    StepStatus = "Done"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 3",
                    StepNumber = 3,
                    StepStatus = "Done"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 4",
                    StepNumber = 4,
                    StepStatus = "Done"
                },
                new CustomerRegistrationStepsStatus()
                {
                    StepName = "Step 5",
                    StepNumber = 5,
                    StepStatus = "Done"
                }
            };

            // set new mock
            mockDataService.GetManyByCriteria(null).ReturnsForAnyArgs(Task.FromResult(stepList));

            result = service.GetVisibleSteps(Guid.Empty);

            Assert.That(result.PreviousStep.StepNumber, Is.EqualTo(5));
            Assert.That(result.CurrentStep, Is.Null);
            Assert.That(result.FutureStep, Is.Null);
            Assert.That(result.HasPreviousStep, Is.True);
            Assert.That(result.HasCurrentStep, Is.False);
            Assert.That(result.HasFutureStep, Is.False);
            Assert.That(result.CurrentStepNo, Is.EqualTo(0));
            Assert.That(result.TotalSteps, Is.EqualTo(5));

        }
    }
}
