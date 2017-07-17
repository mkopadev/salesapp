using System;
using System.Diagnostics;
using Newtonsoft.Json;
using NUnit.Framework;
using SalesApp.Core.BL.Models.Tickets;
using SalesApp.Core.Enums.Tickets;

namespace SalesApp.Core.Tests.Tickets
{
    [TestFixture]
    public class CustomerProcessFlowTest : TestsBase
    {
        private readonly ProcessFlow _customerProcessFlow;

        public CustomerProcessFlowTest()
        {
            var startStep = AddStep("What is the customer'sproduct ?", "", StepInputTypeEnum.Options);

            var product3 = AddStep("Is the word \'Credit\' there ?", "Product 3", StepInputTypeEnum.Options);

            var product3YesStep = AddStep("Is the line around the battery solid or dashed ?", "Yes", StepInputTypeEnum.Options);
            var solidLine = AddStep("Is an error code showing ?", "Solid", StepInputTypeEnum.Options);
            solidLine.SubSteps.Add(AddStep("Endpoint Reached", "1", StepInputTypeEnum.Options, true));
            solidLine.SubSteps.Add(AddStep("Endpoint Reached", "2", StepInputTypeEnum.Options, true));
            solidLine.SubSteps.Add(AddStep("Endpoint Reached", "3", StepInputTypeEnum.Options, true));
            solidLine.SubSteps.Add(AddStep("Endpoint Reached", "4", StepInputTypeEnum.Options, true));
            solidLine.SubSteps.Add(AddStep("Endpoint Reached", "5", StepInputTypeEnum.Options, true));
            solidLine.SubSteps.Add(AddStep("Endpoint Reached", "6", StepInputTypeEnum.Options, true));
            solidLine.SubSteps.Add(AddStep("Endpoint Reached", "7", StepInputTypeEnum.Options, true));
            var noErrCode = AddStep("Is there physical damage ?", "No Error Code", StepInputTypeEnum.Options);
            noErrCode.SubSteps.Add(AddStep("Endpoint Reached", "Yes", StepInputTypeEnum.Options, true));

            var creditsSubStep = AddStep("Do you have the correct credits ?", "No", StepInputTypeEnum.Options);

            var creditsYesSubStep = AddNumericInputStep("How many credits are showing ?", "Yes", "Enter the number of credits");
            var networkBarsStep = AddNumericInputStep("How many network bars are there ?", "NEXT", "Enter the number of network bars");
            var gXStep = AddStep("Is there a G or X ?", "NEXT", StepInputTypeEnum.Options);

            var gVisibleSubStep = AddNumericInputStep("How many bars are on the battery ?", "G", "Enter the number of battery bars");
            var xVisibleSubStep = AddNumericInputStep("How many bars are on the battery ?", "X", "Enter the number of battery bars");
            var nothingVisibleSubStep = AddNumericInputStep("How many bars are on the battery ?", "NOTHING", "Enter the number of battery bars");

            var battChargingYes = AddStep("What is the country code ?", "Yes", StepInputTypeEnum.Options);
                battChargingYes.SubSteps.Add(AddStep("Endpoint Reached", "KE", StepInputTypeEnum.Options, true));
                battChargingYes.SubSteps.Add(AddStep("Endpoint Reached", "UG", StepInputTypeEnum.Options, true));
                battChargingYes.SubSteps.Add(AddStep("Endpoint Reached", "TZ", StepInputTypeEnum.Options, true));
                battChargingYes.SubSteps.Add(AddStep("Endpoint Reached", "GH", StepInputTypeEnum.Options, true));
                var countryCodeOther = AddTextInputStep("What is the country code ?", "OTHER", "Enter the other country code");
                countryCodeOther.SubSteps.Add(AddStep("Endpoint Reached", "COMPLETE", StepInputTypeEnum.Options, true));
            battChargingYes.SubSteps.Add(countryCodeOther);

            var battChargingNo = AddStep("What is the country code ?", "No", StepInputTypeEnum.Options);
                battChargingNo.SubSteps.Add(AddStep("Endpoint Reached", "KE", StepInputTypeEnum.Options, true));
                battChargingNo.SubSteps.Add(AddStep("Endpoint Reached", "UG", StepInputTypeEnum.Options, true));
                battChargingNo.SubSteps.Add(AddStep("Endpoint Reached", "TZ", StepInputTypeEnum.Options, true));
                battChargingNo.SubSteps.Add(AddStep("Endpoint Reached", "GH", StepInputTypeEnum.Options, true));
                var countryCodeOther2 = AddTextInputStep("What is the country code ?", "OTHER", "Enter the other country code");
                countryCodeOther2.SubSteps.Add(AddStep("Endpoint Reached", "COMPLETE", StepInputTypeEnum.Options, true));
            battChargingNo.SubSteps.Add(countryCodeOther2);

            var isBatChargingStep = AddStep("Is the battery charging ?", "NEXT", StepInputTypeEnum.Options);
            isBatChargingStep.SubSteps.Add(battChargingYes);
            isBatChargingStep.SubSteps.Add(battChargingNo);

            gVisibleSubStep.SubSteps.Add(isBatChargingStep);
            xVisibleSubStep.SubSteps.Add(isBatChargingStep);
            nothingVisibleSubStep.SubSteps.Add(isBatChargingStep);

            gXStep.SubSteps.Add(gVisibleSubStep);
            gXStep.SubSteps.Add(xVisibleSubStep);
            gXStep.SubSteps.Add(nothingVisibleSubStep);

            networkBarsStep.SubSteps.Add(gXStep);

            creditsYesSubStep.SubSteps.Add(networkBarsStep);

            var creditsNoSubStep = AddNumericInputStep("How many credits are showing ?", "No", "Enter the number of credits");
            creditsNoSubStep.SubSteps.Add(networkBarsStep);

            creditsSubStep.SubSteps.Add(creditsYesSubStep);
            creditsSubStep.SubSteps.Add(creditsNoSubStep);

            noErrCode.SubSteps.Add(creditsSubStep);

            solidLine.SubSteps.Add(noErrCode);

            product3YesStep.SubSteps.Add(solidLine);
            product3YesStep.SubSteps.Add(AddStep("Endpoint Reached", "Dashed", StepInputTypeEnum.Options, true));
            product3YesStep.SubSteps.Add(AddStep("Endpoint Reached", "None", StepInputTypeEnum.Options, true));

            var product3Nostep = AddStep("Endpoint Reached", "No", StepInputTypeEnum.Options, true);
            product3.SubSteps.Add(product3YesStep);
            product3.SubSteps.Add(product3Nostep);

            var product4 = AddStep("Is the word \'Credit\' there ?", "Product 4", StepInputTypeEnum.Options);
            product4.SubSteps.Add(product3YesStep);
            product4.SubSteps.Add(product3Nostep);

            var product5 = AddTextInputStep("WHAT IS THE ISSUE ?", "DProduct 5", "Describe the problem");
            product5.SubSteps.Add(AddStep("Endpoint Reached", "Complete", StepInputTypeEnum.Options, true));

            var product6 = AddTextInputStep("WHAT IS THE ISSUE ?", "Product 6", "Describe the problem");
            product6.SubSteps.Add(AddStep("Endpoint Reached", "Complete", StepInputTypeEnum.Options, true));

            startStep.SubSteps.Add(product3);
            startStep.SubSteps.Add(product4);
            startStep.SubSteps.Add(product5);
            startStep.SubSteps.Add(product6);

            _customerProcessFlow = new ProcessFlow { Step = startStep };
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

        private Step AddTextInputStep(string headerText, string navigationText, string numericInputLabelTitle, bool isEndPoint = false)
        {
            var step = new Step
            {
                Id = Guid.NewGuid(),
                HeaderText = headerText,
                NavigationText = navigationText,
                DiagnosticText = numericInputLabelTitle,
                Type = (int)StepInputTypeEnum.TextInput,
                IsEndPoint = isEndPoint
            };

            return step;
        }

        private Step AddNumericInputStep(string headerText, string navigationText, string textInputLabelTitle, bool isEndPoint = false)
        {
            var step = new Step
            {
                Id = Guid.NewGuid(),
                HeaderText = headerText,
                NavigationText = navigationText,
                DiagnosticText = textInputLabelTitle,
                Type = (int)StepInputTypeEnum.NumericInput,
                IsEndPoint = isEndPoint
            };

            return step;
        }

        [Test]
        public void TestCustomerProcessFlow()
        {
            string json = JsonConvert.SerializeObject(_customerProcessFlow);
            Debug.WriteLine(json);
        }
    }
}
