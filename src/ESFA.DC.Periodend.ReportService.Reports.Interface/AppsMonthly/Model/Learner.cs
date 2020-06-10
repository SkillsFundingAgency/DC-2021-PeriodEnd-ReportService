using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model
{
    public class Learner
    {
        public string LearnRefNumber { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public List<LearningDelivery> LearningDeliveries { get; set; }

        public List<ProviderMonitoring> ProviderSpecLearnMonitorings { get; set; }

        public List<LearnerEmploymentStatus> LearnerEmploymentStatuses { get; set; }
    }
}
