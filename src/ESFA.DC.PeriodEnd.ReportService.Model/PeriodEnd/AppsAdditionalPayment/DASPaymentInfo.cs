using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment
{
    public class DASPaymentInfo
    {
        public long Ukprn { get; set; }

        public string LearnerReferenceNumber { get; set; }

        public string LearningAimReference { get; set; }

        public long LearnerUln { get; set; }

        public string LearningAimFundingLineType { get; set; }

        public byte TransactionType { get; set; }

        public int LearningAimProgrammeType { get; set; }

        public int LearningAimStandardCode { get; set; }

        public int LearningAimFrameworkCode { get; set; }

        public int LearningAimPathwayCode { get; set; }

        public byte ContractType { get; set; }

        //public byte DeliveryPeriod { get; set; }

        public byte CollectionPeriod { get; set; }

        public short AcademicYear { get; set; }

        public decimal Amount { get; set; }

        public byte FundingSource { get; set; }

        public DateTime? LearningStartDate { get; set; }

        public long? ApprenticeshipId { get; set; }
    }
}