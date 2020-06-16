using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model
{
    public class LearnerEmploymentStatus
    {
        public int? EmpId { get; set; }

        public int EmpStat { get; set; }

        public DateTime DateEmpStatApp { get; set; }
    }
}
