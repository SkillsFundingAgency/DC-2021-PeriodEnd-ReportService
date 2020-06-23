using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model
{
    public class Payment
    {
        // Key fields
        public string LearnerReferenceNumber { get; set; }

        public long LearnerUln { get; set; }

        public string LearningAimReference { get; set; }

        public DateTime? LearningStartDate { get; set; }

        public int LearningAimProgrammeType { get; set; }

        public int LearningAimStandardCode { get; set; }

        public int LearningAimFrameworkCode { get; set; }

        public int LearningAimPathwayCode { get; set; }

        public string LearningAimFundingLineType { get; set; } 

        //Data fields
        public byte CollectionPeriod { get; set; }

        public decimal Amount { get; set; }

        public byte ContractType { get; set; }

        public byte TransactionType { get; set; }

        public byte FundingSource { get; set; }

        public string ApprenticeshipLegalEntityName { get; set; }
        /*
SELECT TOP (1000) 
p.Ukprn
,LearnerReferenceNumber
,learnerUln
,LearningAimReference
,LearningStartDate
,LearningAimProgrammeType
,LearningAimStandardCode
,LearningAimFrameworkCode
,LearningAimPathwayCode
,LearningAimFundingLineType
,ContractType
,SUM(Amount)
--,AcademicYear
,CollectionPeriod 
,TransactionType
,FundingSource
,a.LegalEntityName

  FROM [Payments2].[Payment] p
  left join [Payments2].[Apprenticeship] a on ApprenticeshipId = a.id
  where AcademicYear = '1920'
  and TransactionType in (4, 5, 6, 7, 16)
  group by 
  p.Ukprn
  , CollectionPeriod 
,learnerUln
,TransactionType
,LearnerReferenceNumber
,LearningAimReference
,LearningStartDate
,LearningAimProgrammeType
,LearningAimStandardCode
,LearningAimFrameworkCode
,LearningAimPathwayCode
,LearningAimFundingLineType
,ContractType
,FundingSource
,a.LegalEntityName
  order by ukprn, LearnerReferenceNumber


         */
    }
}
