using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model
{
    public class LearnerEmploymentStatus
    {
        public string LearnRefNumber { get; set; }

        public int? EmpId { get; set; }

        public int? EmpStat { get; set; }

        public DateTime? DateEmpStatApp { get; set; }
    }
}
