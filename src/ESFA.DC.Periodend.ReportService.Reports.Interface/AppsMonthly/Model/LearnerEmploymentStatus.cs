using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model
{
    public class LearnerEmploymentStatus
    {
        public int? EmpId { get; set; }

        public int EmpStat { get; set; }

        public DateTime DateEmpStatApp { get; set; }
    }
}
