using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment
{
    public class LearningDeliveryInfo
    {
        public int UKPRN { get; set; }

        public string LearnRefNumber { get; set; }

        public string LearnAimRef { get; set; }

        public int AimType { get; set; }

        public int AimSeqNumber { get; set; }

        public DateTime LearnStartDate { get; set; }

        public int? ProgType { get; set; }

        public int? StdCode { get; set; }

        public int? FworkCode { get; set; }

        public int? PwayCode { get; set; }
        
        public string SWSupAimId { get; set; }

        public ICollection<AppFinRecordInfo> AppFinRecords { get; set; }
    }
}