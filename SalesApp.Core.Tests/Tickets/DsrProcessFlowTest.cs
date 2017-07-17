using System;
using System.Diagnostics;
using Newtonsoft.Json;
using NUnit.Framework;
using SalesApp.Core.BL.Models.Tickets;
using SalesApp.Core.Enums.Tickets;

namespace SalesApp.Core.Tests.Tickets
{
    [TestFixture]
    public class DsrProcessFlowTest : TestsBase
    {
        private readonly ProcessFlow _dsrProcessFlow;

        public DsrProcessFlowTest()
        {
            var startStep = AddStep("WHAT IS THE PROBLEM ABOUT ?", "", StepInputTypeEnum.Options);

            var comissionSubstep = AddStep("WHICH COMMISSION OR BONUS ?", "COMMISSION & BONUSES", StepInputTypeEnum.Options);

            var dailycomission = AddTextInputStep("What is the issue with commissions ?", "DAILY COMISSION", "Describe the problem");
            dailycomission.SubSteps.Add(AddStep("Endpoint Reached", "COMPLETE", StepInputTypeEnum.Options, true));

            comissionSubstep.SubSteps.Add(dailycomission);

            var monthlycomission = AddTextInputStep("What is the issue with commissions ?", "MONTHLY COMISSION", "Describe the problem");
            monthlycomission.SubSteps.Add(AddStep("Endpoint Reached", "COMPLETE", StepInputTypeEnum.Options, true));

            comissionSubstep.SubSteps.Add(monthlycomission);

            var alllowances = AddTextInputStep("What is the issue with allowances ?", "ALLOWANCES", "Describe the problem");
            alllowances.SubSteps.Add(AddStep("Endpoint Reached", "COMPLETE", StepInputTypeEnum.Options, true));

            comissionSubstep.SubSteps.Add(alllowances);

            var bonuses = AddTextInputStep("What is the issue with bonuses ?", "BONUSES", "Describe the problem");
            bonuses.SubSteps.Add(AddStep("Endpoint Reached", "COMPLETE", StepInputTypeEnum.Options, true));

            comissionSubstep.SubSteps.Add(bonuses);

            var stockSubstep = AddStep("WHAT IS THE ISSUE ?", "STOCK", StepInputTypeEnum.Options);

            var demoIssue = AddTextInputStep("What is the Issue with the Demo ?", "DEMO ISSUE", "Describe the problem");
            demoIssue.SubSteps.Add(AddStep("Endpoint Reached", "COMPLETE", StepInputTypeEnum.Options, true));

            var discrepancyIssue = AddTextInputStep("What is the Issue with the Stock ?", "STOCK DISCREPANCY", "Describe the problem");
            discrepancyIssue.SubSteps.Add(AddStep("Endpoint Reached", "COMPLETE", StepInputTypeEnum.Options, true));

            var otherIssue = AddTextInputStep("What is the Issue with the Stock ?", "OTHER", "Describe the problem");
            otherIssue.SubSteps.Add(AddStep("Endpoint Reached", "COMPLETE", StepInputTypeEnum.Options, true));

            stockSubstep.SubSteps.Add(demoIssue);
            stockSubstep.SubSteps.Add(discrepancyIssue);
            stockSubstep.SubSteps.Add(otherIssue);

            var hrSubstep = AddTextInputStep("WHAT IS THE HR ISSUE ?", "HR", "Describe the problem");
            hrSubstep.SubSteps.Add(AddStep("Endpoint Reached", "COMPLETE", StepInputTypeEnum.Options, true));

            var otherSubStep = AddTextInputStep("WHAT IS THE ISSUE ?", "OTHER", "Describe the problem");
            otherSubStep.SubSteps.Add(AddStep("Endpoint Reached", "COMPLETE", StepInputTypeEnum.Options, true));

            startStep.SubSteps.Add(comissionSubstep);
            startStep.SubSteps.Add(stockSubstep);
            startStep.SubSteps.Add(hrSubstep);
            startStep.SubSteps.Add(otherSubStep);

            _dsrProcessFlow = new ProcessFlow {Step = startStep};
        }

        private Step AddStep(string headerText, string navigationText, StepInputTypeEnum stepType, bool isEndPoint = false)
        {
            var step = new Step
            {
                Id = Guid.NewGuid(),
                HeaderText = headerText,
                NavigationText = navigationText,
                Type = (int)stepType,
                IsEndPoint = isEndPoint
            };

            return step;
        }

        private Step AddTextInputStep(string headerText, string navigationText, string textInputLabelTitle, bool isEndPoint = false)
        {
            var step = new Step
            {
                Id = Guid.NewGuid(),
                HeaderText = headerText,
                NavigationText = navigationText,
                DiagnosticText = textInputLabelTitle,
                Type = (int) StepInputTypeEnum.TextInput,
                IsEndPoint = isEndPoint
            };

            return step;
        }

        [Test]
        public void TestDsrTicketProcessFlow()
        {
            string json = JsonConvert.SerializeObject(_dsrProcessFlow);
            Debug.WriteLine(json);
        }
    }
}
