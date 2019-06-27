using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentDASPaymentInfo
    {
        public string LearnerReferenceNumber { get; set; }

        public long LearnerUln { get; set; }

        // Aim sequence number, not supported.

        public string LearningAimReference { get; set; }

        //missing LearningStartDate

        public int LearningAimProgrammeType { get; set; }

        public int LearningAimStandardCode { get; set; }

        public int LearningAimFrameworkCode { get; set; }

        public int LearningAimPathwayCode { get; set; }

        // missing Price episode start date

        public string LearningAimFundingLineType { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public byte ContractType { get; set; }

        public byte TransactionType { get; set; }

        public byte FundingSource { get; set; }

        public byte DeliveryPeriod { get; set; }

        public byte CollectionPeriod { get; set; }

        public short AcademicYear { get; set; }

        public Decimal Amount { get; set; }
        public DateTime LearningStartDate { get; set; }
        public int UkPrn { get; set; }
    }
}