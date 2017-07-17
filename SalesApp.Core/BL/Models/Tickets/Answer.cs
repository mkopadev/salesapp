namespace SalesApp.Core.BL.Models.Tickets
{
    public class Answer
    {
        public Answer()
        {

        }

        public int Id { get; set; }
        public string Title { get; set; }
        public bool HasTextBox { get; set; }
        public string DataKey { get; set; }
        public string TextBoxValue { get; set; }
        public int? NextScreenId { get; set; }
        public bool IsLastAnswer { get; set; }
        public bool IsTextBoxValueRequired { get; set; }
        public bool IsTextBoxSubmitButton { get; set; }
    }
}
