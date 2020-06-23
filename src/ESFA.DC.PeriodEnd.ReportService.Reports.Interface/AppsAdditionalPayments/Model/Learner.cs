using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model
{
    public class Learner
    {
        public string LearnRefNumber { get; set; }

        public string FamilyName { get; set; }

        public string GivenName { get; set; }

        public string ProvSpecLearnMonA { get; set; }

        public string ProvSpecLearnMonB { get; set; }

        /*
SELECT TOP (1000) l.[UKPRN]
      ,l.[LearnRefNumber]
      ,[ULN]
      ,[FamilyName]
      ,[GivenNames]
      , pslmA.ProvSpecLearnMon as  ProvSpecLearnMonA
      , pslmB.ProvSpecLearnMon as ProvSpecLearnMonB 
  FROM [Valid].[Learner] l
  left outer join Valid.ProviderSpecLearnerMonitoring pslmA on l.UKPRN = pslmA.UKPRN and l.LearnRefNumber = pslmA.LearnRefNumber and pslmA.ProvSpecLearnMonOccur = 'A' and pslmA.ProvSpecLearnMon is not null
  left outer join Valid.ProviderSpecLearnerMonitoring pslmB on l.UKPRN = pslmB.UKPRN and l.LearnRefNumber = pslmB.LearnRefNumber and pslmB.ProvSpecLearnMonOccur = 'B' and pslmB.ProvSpecLearnMon is not null
  where l.UKPRN = '10000001'
  order by ukprn, l.LearnRefNumber
         */
    }
}
