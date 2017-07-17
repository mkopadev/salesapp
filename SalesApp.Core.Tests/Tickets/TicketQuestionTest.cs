using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using NUnit.Framework;
using SalesApp.Core.BL.Models.Tickets;

namespace SalesApp.Core.Tests.Tickets
{
    [TestFixture]
    public class TicketQuestionTest : TestsBase
    {
        readonly List<TicketQuestion> _screens;

        public TicketQuestionTest()
        {
            _screens = new List<TicketQuestion>();

            var screen1 = AddQuestion(1, "WHAT IS THE PROBLEM ABOUT ?", null);
            screen1.Answers.Add(AddButtonAnswer(1, "COMMISSION & BONUSES", false, 2));
            screen1.Answers.Add(AddButtonAnswer(2, "STOCK", false, 3));
            screen1.Answers.Add(AddButtonAnswer(3, "HR", false, 4));
            screen1.Answers.Add(AddButtonAnswer(4, "OTHER", false, 5));

            var screen2 = AddQuestion(2, "WHICH COMMISSION OR BONUS ?", 1);
            screen2.Answers.Add(AddButtonAnswer(1, "DAILY COMISSION", false, 6));
            screen2.Answers.Add(AddButtonAnswer(2, "MONTHLY COMISSION", false, 7));
            screen2.Answers.Add(AddButtonAnswer(3, "ALLOWANCES", false, 8));
            screen2.Answers.Add(AddButtonAnswer(4, "BONUSES", false, 9));

            var screen3 = AddQuestion(3, "WHAT IS THE ISSUE ?", 1);
            screen3.Answers.Add(AddButtonAnswer(1, "DEMO ISSUE", false, 10));
            screen3.Answers.Add(AddButtonAnswer(2, "STOCK DISCREPANCY", false, 11));
            screen3.Answers.Add(AddButtonAnswer(3, "OTHER", false, 12));

            var screen4 = AddQuestion(4, "What is the HR Issue ?", 1);
            screen4.Answers.Add(AddTextInputAnswer(1, "Describe the problem", "", true, true, null));
            screen4.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));

            var screen5 = AddQuestion(5, "What is the Issue ?", 1);
            screen5.Answers.Add(AddTextInputAnswer(1, "Describe the problem", "", true, true, null));
            screen5.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));

            var screen6 = AddQuestion(6, "What is the issue with commissions ?", 2);
            screen6.Answers.Add(AddTextInputAnswer(1, "Describe the problem", "", true, true, null));
            screen6.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));

            var screen7 = AddQuestion(7, "What is the issue with commissions ?", 2);
            screen7.Answers.Add(AddTextInputAnswer(1, "Describe the problem", "", true, true, null));
            screen7.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));       

            var screen8 = AddQuestion(8, "What is the Issue with Allowances ?", 2);
            screen8.Answers.Add(AddTextInputAnswer(1, "Describe the problem", "", true, true, null));
            screen8.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));    

            var screen9 = AddQuestion(9, "What is the Issue with Bonuses ?", 2);
            screen9.Answers.Add(AddTextInputAnswer(1, "Describe the problem", "", true, true, null));
            screen9.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));     

            var screen10 = AddQuestion(10, "What is the Issue with the Demo ?", 3);
            screen10.Answers.Add(AddTextInputAnswer(1, "Describe the problem", "", true, true, null));
            screen10.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));

            var screen11 = AddQuestion(11, "What is the Issue with the Stock ?", 3);
            screen11.Answers.Add(AddTextInputAnswer(1, "Describe the problem", "", true, true, null));
            screen11.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));

            var screen12 = AddQuestion(12, "What is the Issue with the Stock ?", 3);
            screen12.Answers.Add(AddTextInputAnswer(1, "Describe the problem", "", true, true, null));
            screen12.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));                                              

            var screen20 = AddQuestion(20, "What is the customer's product", 3);
            screen20.Answers.Add(AddButtonAnswer(1, "Product 3", false, 23));
            screen20.Answers.Add(AddButtonAnswer(2, "Product 4", false, 23));
            screen20.Answers.Add(AddButtonAnswer(3, "Product 5", false, 21));
            screen20.Answers.Add(AddButtonAnswer(4, "Product 6", false, 22));

            //21
            var screen21 = AddQuestion(21, "What is the Issue ?", 20);
            screen21.Answers.Add(AddTextInputAnswer(1, "Describe the problem", "", true, true, null));
            screen21.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));

            //22
            var screen22 = AddQuestion(22, "What is the Issue ?", 20);
            screen22.Answers.Add(AddTextInputAnswer(1, "Describe the problem", "", true, true, null));
            screen22.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));            

            //23
            var screen23 = AddQuestion(23, "Is the word 'Credit’ there ?", 20);
            screen23.Answers.Add(AddButtonAnswer(1, "YES", false, 24));
            screen23.Answers.Add(AddButtonAnswer(2, "NO", true, null));       

            //24
            var screen24 = AddQuestion(24, "Is the line around the battery solid or dashed ?", 23);
            screen24.Answers.Add(AddButtonAnswer(1, "SOLID", false, 25));
            screen24.Answers.Add(AddButtonAnswer(2, "DASHED", true, null));
            screen24.Answers.Add(AddButtonAnswer(3, "NONE", true, null));

            //25
            var screen25 = AddQuestion(25, "Is an error code showing ?", 24);
            screen25.Answers.Add(AddButtonAnswer(1, "1", true, null));
            screen25.Answers.Add(AddButtonAnswer(2, "2", true, null));
            screen25.Answers.Add(AddButtonAnswer(3, "3", true, null));
            screen25.Answers.Add(AddButtonAnswer(4, "4", true, null));
            screen25.Answers.Add(AddButtonAnswer(5, "5", true, null));
            screen25.Answers.Add(AddButtonAnswer(6, "6", true, null));
            screen25.Answers.Add(AddButtonAnswer(7, "7", true, null));
            screen25.Answers.Add(AddButtonAnswer(8, "No Error Code", false, 26));                                                   

            //26
            var screen26 = AddQuestion(26, "Is there physical damage ?", 25);
            screen26.Answers.Add(AddButtonAnswer(1, "YES", true, null));
            screen26.Answers.Add(AddButtonAnswer(2, "NO", false, 27));       

            //27
            var screen27 = AddQuestion(27, "Do you have the correct credits ?", 26);
            screen27.Answers.Add(AddButtonAnswer(1, "YES", false, 28));
            screen27.Answers.Add(AddButtonAnswer(2, "NO", false, 28));

            //28
            var screen28 = AddQuestion(28, "How many credits are showing ?", 27);
            screen28.Answers.Add(AddTextInputAnswer(1, "Enter the number of credits", "Credit", true, false, 29));
            screen28.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", false, 29));             

            //29
            var screen29 = AddQuestion(29, "How many network bars are there ?", 28);
            screen29.Answers.Add(AddButtonAnswer(1, "0", false, 30));
            screen29.Answers.Add(AddButtonAnswer(2, "1", false, 30));
            screen29.Answers.Add(AddButtonAnswer(3, "2", false, 30));
            screen29.Answers.Add(AddButtonAnswer(4, "3", false, 30));
            screen29.Answers.Add(AddButtonAnswer(5, "4", false, 30));
            screen29.Answers.Add(AddButtonAnswer(6, "5", false, 30));           

            //30
            var screen30 = AddQuestion(30, "Is there a G or X ?", 29);
            screen30.Answers.Add(AddButtonAnswer(1, "G", false, 31));
            screen30.Answers.Add(AddButtonAnswer(2, "X", false, 31));
            screen30.Answers.Add(AddButtonAnswer(3, "NOTHING", false, 31));                

            //31
            var screen31 = AddQuestion(31, "How many bars are on the battery ?", 30);
            screen31.Answers.Add(AddButtonAnswer(1, "0", false, 32));
            screen31.Answers.Add(AddButtonAnswer(2, "1", false, 32));
            screen31.Answers.Add(AddButtonAnswer(3, "2", false, 32));
            screen31.Answers.Add(AddButtonAnswer(4, "3", false, 32));
            screen31.Answers.Add(AddButtonAnswer(5, "4", false, 32));

            //32
            var screen32 = AddQuestion(32, "Is the battery charging ?", 31);
            screen32.Answers.Add(AddButtonAnswer(1, "YES", false, 33));
            screen32.Answers.Add(AddButtonAnswer(2, "NO", false, 33));

            //33
            var screen33 = AddQuestion(33, "What is the country code ?", 32);
            screen33.Answers.Add(AddButtonAnswer(1, "KE", true, null));
            screen33.Answers.Add(AddButtonAnswer(2, "UG", true, null));
            screen33.Answers.Add(AddButtonAnswer(3, "TZ", true, null));
            screen33.Answers.Add(AddButtonAnswer(4, "GH", true, null));
            screen33.Answers.Add(AddButtonAnswer(5, "OTHER", false, 34));         

            //34
            var screen34 = AddQuestion(34, "What is the country code ?", 33);
            screen34.Answers.Add(AddTextInputAnswer(1, "Enter the other country code", "", true, true, null));
            screen34.Answers.Add(AddSubmitButtonAnswer(2, "NEXT", true, null));

            _screens.Add(screen1);
            _screens.Add(screen2);
            _screens.Add(screen3);
            _screens.Add(screen4);
            _screens.Add(screen5);
            _screens.Add(screen6);
            _screens.Add(screen7);
            _screens.Add(screen8);
            _screens.Add(screen9);
            _screens.Add(screen10);
            _screens.Add(screen11);
            _screens.Add(screen12);                                            
            _screens.Add(screen20);
            _screens.Add(screen21);
            _screens.Add(screen22);
            _screens.Add(screen23);
            _screens.Add(screen24);
            _screens.Add(screen25);
            _screens.Add(screen26);
            _screens.Add(screen27);
            _screens.Add(screen28);
            _screens.Add(screen29);
            _screens.Add(screen30);
            _screens.Add(screen31);
            _screens.Add(screen32);
            _screens.Add(screen33);
            _screens.Add(screen34);          
        }

        private static TicketQuestion AddQuestion(int screenId, string title, int? parentscreenId)
        {
            var newQuestion = new TicketQuestion
            {
                ScreenId = screenId,
                ScreenTitle = title,
                ParentScreenId = parentscreenId
            };

            return newQuestion;
        }

        private Answer AddAnswer(int answerId, string title, bool hasTextBox, bool isTextBoxValueRequired, bool isLastAnswer, bool isTextBoxSubmitButton, int? nextScreenId)
        {
            var newAnswer = new Answer
            {
                Id = answerId,
                Title = title,
                HasTextBox = hasTextBox,
                IsTextBoxValueRequired = isTextBoxValueRequired,
                IsLastAnswer = isLastAnswer,
                IsTextBoxSubmitButton = isTextBoxSubmitButton,
                NextScreenId = nextScreenId
            };

            return newAnswer;
        }

        private Answer AddButtonAnswer(int answerId, string title, bool isLastAnswer, int? nextScreenId)
        {
            var newAnswer = new Answer
            {
                Id = answerId,
                Title = title,
                IsLastAnswer = isLastAnswer,
                NextScreenId = nextScreenId
            };

            return newAnswer;
        }

        private Answer AddSubmitButtonAnswer(int answerId, string title, bool isLastAnswer, int? nextScreenId)
        {
            var newAnswer = new Answer
            {
                Id = answerId,
                Title = title,
                IsLastAnswer = isLastAnswer,
                IsTextBoxSubmitButton = true,
                NextScreenId = nextScreenId
            };

            return newAnswer;
        }

        private Answer AddTextInputAnswer(int answerId, string title, string dataKey, bool isTextBoxValueRequired, bool isLastAnswer, int? nextScreenId)
        {
            var newAnswer = new Answer
            {
                Id = answerId,
                Title = title,
                HasTextBox = true,
                DataKey = dataKey,
                IsTextBoxValueRequired = isTextBoxValueRequired,
                IsLastAnswer = isLastAnswer,
                NextScreenId = nextScreenId
            };

            return newAnswer;
        }

        [Test]
        public void TestTicketQuestions()
        {
            string json = JsonConvert.SerializeObject(_screens);
            Debug.WriteLine(json);
        }
    }
}
