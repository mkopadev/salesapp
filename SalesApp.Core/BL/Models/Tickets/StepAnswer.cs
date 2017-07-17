using System.Text;

namespace SalesApp.Core.BL.Models.Tickets
{
    public class StepAnswer
    {
        public StepAnswer()
        {

        }
        public int StepId { get; set; }
        public int AnswerId { get; set; }
        public string AnswerValue { get; set; }
        public string DataKey { get; set; }
        public string QuestionValue { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(QuestionValue.ToUpper());
            sb.AppendLine();
            sb.Append(AnswerValue.ToUpper());
            sb.AppendLine();

            return sb.ToString();

        }
    }
}