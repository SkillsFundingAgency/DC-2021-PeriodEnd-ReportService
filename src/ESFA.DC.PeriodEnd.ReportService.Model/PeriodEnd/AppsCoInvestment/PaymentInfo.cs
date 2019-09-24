using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment
{
    public class PaymentInfo
    {
        public string LearnerReferenceNumber { get; set; }

        public string LearningAimReference { get; set; }

        public long LearnerUln { get; set; }

        public int LearningAimProgrammeType { get; set; }

        public int LearningAimStandardCode { get; set; }

        public int LearningAimFrameworkCode { get; set; }

        public int LearningAimPathwayCode { get; set; }

        public byte ContractType { get; set; }

        public byte DeliveryPeriod { get; set; }

        public byte CollectionPeriod { get; set; }

        public short AcademicYear { get; set; }

        public byte TransactionType { get; set; }

        public byte FundingSource { get; set; }

        public DateTime? LearningStartDate { get; set; }

        public string LegalEntityName { get; set; }

        public long UkPrn { get; set; }

        public decimal Amount { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public decimal SfaContributionPercentage { get; set; }

        public string EmployerName { get; set; }
    }
}