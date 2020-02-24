using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentDasEarningEventModel
    {
        public long? Id { get; set; }

        public Guid? EventId { get; set; }

        public int? Ukprn { get; set; }

        public byte? ContractType { get; set; }

        public byte? CollectionPeriod { get; set; }

        public short? AcademicYear { get; set; }

        public string LearnerReferenceNumber { get; set; }

        public long? LearnerUln { get; set; }

        public string LearningAimReference { get; set; }

        public int? LearningAimProgrammeType { get; set; }

        public int? LearningAimStandardCode { get; set; }

        public int? LearningAimFrameworkCode { get; set; }

        public int? LearningAimPathwayCode { get; set; }

        public string LearningAimFundingLineType { get; set; }

        public DateTime? LearningStartDate { get; set; }

        public string AgreementId { get; set; }

        public DateTime? IlrSubmissionDateTime { get; set; }

        public long? JobId { get; set; }

        public DateTimeOffset? EventTime { get; set; }

        public DateTimeOffset? CreationDate { get; set; }

        public byte? LearningAimSequenceNumber { get; set; }

        public decimal? SfaContributionPercentage { get; set; }

        public string IlrFileName { get; set; }

        public string EventType { get; set; }
    }
}