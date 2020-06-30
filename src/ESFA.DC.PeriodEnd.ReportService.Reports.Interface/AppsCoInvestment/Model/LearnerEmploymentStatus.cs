using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model
{
    public class LearnerEmploymentStatus
    {
        public string LearnRefNumber { get; set; }

        public DateTime DateEmpStatApp { get; set; }

        public int? EmpId { get; set; }
    }
}
