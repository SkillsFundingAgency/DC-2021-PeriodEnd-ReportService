using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class EasFundingLine
    {
        public string FundLine { get; set; }
        public IReadOnlyCollection<EasSubmissionValue> EasSubmissionValues { get; set; }
    }
}