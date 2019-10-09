using System.Collections.Generic;
using ESFA.DC.ILR1920.DataStore.EF.Valid;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport.PeriodisedValues;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class ALBLearner
    {
        public string LearnRefNumber { get; set; }

        public List<LearningDelivery> LearningDeliveries { get; set; }

        public List<LearnerPeriodisedValue> LearnerPeriodisedValues { get; set; }
    }
}