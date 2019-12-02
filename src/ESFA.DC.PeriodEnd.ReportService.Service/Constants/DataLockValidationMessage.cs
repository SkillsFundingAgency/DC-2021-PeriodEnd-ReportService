namespace ESFA.DC.PeriodEnd.ReportService.Service.Constants
{
    public sealed class DataLockValidationMessage
    {
        public DataLockValidationMessage(string ruleId, char severity, string errorMessage)
        {
            RuleId = ruleId;
            Severity = severity;
            ErrorMessage = errorMessage;
        }

        public string RuleId { get; }

        public char Severity { get; }

        public string ErrorMessage { get; }
    }
}
