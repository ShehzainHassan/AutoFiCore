namespace AutoFiCore.Models
{
    public class SaveQuestionnaireResponse
    {
        public Questionnaire Questionnaire { get; set; } = new Questionnaire();
        public LoanCalculation Loan { get; set; } = new LoanCalculation();
    }  
}
