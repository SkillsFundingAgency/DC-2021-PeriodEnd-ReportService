using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentDasPaymentModel
    {
        public int? Ukprn { get; set; }

        public short? AcademicYear { get; set; }

        public string LearnerReferenceNumber { get; set; }

        public long? LearnerUln { get; set; }

        public string LearningAimReference { get; set; }

        public DateTime? LearningStartDate { get; set; }

        public Guid? EarningEventId { get; set; }

        public int? LearningAimProgrammeType { get; set; }

        public int? LearningAimStandardCode { get; set; }

        public int? LearningAimFrameworkCode { get; set; }

        public int? LearningAimPathwayCode { get; set; }

        public string LearningAimFundingLineType { get; set; }

        public string ReportingAimFundingLineType { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public byte? ContractType { get; set; }

        public byte? TransactionType { get; set; }

        public byte? FundingSource { get; set; }

        public byte? DeliveryPeriod { get; set; }

        public byte? CollectionPeriod { get; set; }

        public decimal? Amount { get; set; }
    }
}