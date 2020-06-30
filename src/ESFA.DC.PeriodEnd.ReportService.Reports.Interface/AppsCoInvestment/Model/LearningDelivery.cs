using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model
{
    public class LearningDelivery
    {
        public string LearnRefNumber { get; set; }

        public string LearnAimRef { get; set; }

        public DateTime LearnStartDate { get; set; }

        public int AimType { get; set; }

        public int AimSeqNumber { get; set; }
        
        public int FundModel { get; set; }

        public int? ProgType { get; set; }

        public int? StdCode { get; set; }

        public int? FworkCode { get; set; }

        public int? PwayCode { get; set; }

        public string SWSupAimId { get; set; }

        public IReadOnlyCollection<AppFinRecord> AppFinRecords { get; set; }

        public IReadOnlyCollection<LearningDeliveryFam> LearningDeliveryFams { get; set; }

        public List<AECApprenticeshipPriceEpisodePeriodisedValues> AECApprenticeshipPriceEpisodePeriodisedValues { get; set; }

        public AECLearningDelivery AECLearningDelivery { get; set; }
    }
}
