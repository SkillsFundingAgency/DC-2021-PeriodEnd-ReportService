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

        public byte ContractType { get; set; }

        public byte TransactionType { get; set; }

        public string ApprenticeshipLegalEntityName { get; set; }

        public decimal Amount { get; set; }
    }
}
