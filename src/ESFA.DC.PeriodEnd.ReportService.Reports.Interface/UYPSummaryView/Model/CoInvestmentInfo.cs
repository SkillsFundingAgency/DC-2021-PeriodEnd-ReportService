using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model
{
    public class CoInvestmentInfo
    {
        public string LearnRefNumber { get; set; }

        public string LearnAimRef { get; set; }

        public DateTime AFinDate { get; set; }

        public string AFinType { get; set; }

        public int AFinCode { get; set; }

        public int AFinAmount { get; set; }
    }
}
