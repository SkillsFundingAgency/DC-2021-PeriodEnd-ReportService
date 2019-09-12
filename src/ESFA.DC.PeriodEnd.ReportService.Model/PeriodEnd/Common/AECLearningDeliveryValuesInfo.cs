using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common
{
    public class AECLearningDeliveryValuesInfo
    {
        public bool LearnDelMathEng { get; set; }

        public string LearnDelInitialFundLineType { get; set; }

        public string LearnAimRef { get; set; }

        public DateTime AppAdjLearnStartDate { get; set; }

        public int AgeAtProgStart { get; set; }
    }
}
