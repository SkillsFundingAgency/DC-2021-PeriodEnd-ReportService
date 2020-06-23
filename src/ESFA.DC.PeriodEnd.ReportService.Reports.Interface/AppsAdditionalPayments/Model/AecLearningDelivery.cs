using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model
{
    public class AecLearningDelivery
    {
        // Key Fields
        public string LearnRefNumber { get; set; }

        public string LearnAimRef { get; set; }

        public DateTime LearnStartDate { get; set; }

        public int? ProgType { get; set; }

        public int? StdCode { get; set; }

        public int? FworkCode { get; set; }

        public int? PwayCode { get; set; }

        // Data Fields

        public int AimSequenceNumber { get; set; }

        public int? LearnDelEmpIdFirstAdditionalPaymentThreshold { get; set; }

        public int? LearnDelEmpIdSecondAdditionalPaymentThreshold { get; set; }

        /*
SELECT TOP (1000) 
       ld.[UKPRN]
      ,ld.[LearnRefNumber]
      ,ld.[LearnAimRef]
      ,ld.[LearnStartDate]
      ,ld.[ProgType]
      ,ld.[StdCode]
      ,ld.[FworkCode]
      ,ld.[PwayCode]

      ,ld.[AimSeqNumber]

      ,AECld.LearnDelEmpIdFirstAdditionalPaymentThreshold
      ,AECld.LearnDelEmpIdSecondAdditionalPaymentThreshold
  FROM [Valid].[LearningDelivery] ld 
  left join Rulebase.AEC_LearningDelivery AECld on ld.UKPRN = AECld.UKPRN and ld.LearnRefNumber = AECld.LearnRefNumber and ld.AimSeqNumber = AECld.AimSeqNumber

  order by ld.UKPRN, ld.LearnRefNumber
         */
    }
}
