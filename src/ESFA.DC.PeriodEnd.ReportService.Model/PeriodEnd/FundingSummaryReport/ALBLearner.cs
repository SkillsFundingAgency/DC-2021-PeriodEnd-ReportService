using System.Collections.Generic;
using ESFA.DC.ILR1920.DataStore.EF.Valid;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class ALBLearner
    {
        public ALBLearner() { }

        public string LearnRefNumber { get; set; }
        public List<LearningDelivery> LearningDeliveries { get; set; }
        public List<LearnerPeriodisedValues> LearnerPeriodisedValues { get; set; }
    }
}