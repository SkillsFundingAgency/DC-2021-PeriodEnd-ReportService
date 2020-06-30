using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model
{
    public class Learner
    {
        public string LearnRefNumber { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public ICollection<LearningDelivery> LearningDeliveries { get; set; }

        public ICollection<LearnerEmploymentStatus> LearnerEmploymentStatuses { get; set; }
    }
}
