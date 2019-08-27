using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentDasPayments2Payment
    {
        public short AcademicYear { get; set; }

        public string Ukprn { get; set; }       // value used to confirm the database results are the Ukprn we requested

        public string LearnerReferenceNumber { get; set; }

        public string LearnerUln { get; set; }

        public string LearningAimReference { get; set; }

        public DateTime? LearningStartDate { get; set; }

        public Guid EarningEventId { get; set; }

        public string LearningAimProgrammeType { get; set; }

        public string LearningAimStandardCode { get; set; }

        public string LearningAimFrameworkCode { get; set; }

        public string LearningAimPathwayCode { get; set; }

        public string LearningAimFundingLineType { get; set; }

        public string ReportingAimFundingLineType { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public string ContractType { get; set; }

        public byte TransactionType { get; set; }

        public byte FundingSource { get; set; }

        public string DeliveryPeriod { get; set; }

        public byte CollectionPeriod { get; set; }

        public Decimal Amount { get; set; }
    }
}