using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model
{
    public class Learner
    {
        public string LearnRefNumber { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public long UniqueLearnerNumber { get; set; }

        public List<LearnerEmploymentStatus> LearnerEmploymentStatuses { get; set; }
    }
}
