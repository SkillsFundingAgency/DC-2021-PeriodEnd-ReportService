using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment
{
    public class LearnerEmploymentStatusInfo
    {
        public string LearnRefNumber { get; set; }

        public DateTime DateEmpStatApp { get; set; }

        public int? EmpId { get; set; }
    }
}