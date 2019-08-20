using System;
using System.Security.Cryptography.X509Certificates;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentDasEarningEventInfo
    {
        public long Id { get; set; }

        public Guid EventId { get; set; }

        public long Ukprn { get; set; }

        public int ContractType { get; set; }

        public int CollectionPeriod { get; set; }

        public int AcademicYear { get; set; }

        public string LearnerReferenceNumber { get; set; }

        public long LearnerUln { get; set; }

        public string LearningAimReference { get; set; }

        public int LearningAimProgrammeType { get; set; }

        public int LearningAimStandardCode { get; set; }

        public int LearningAimFrameworkCode { get; set; }

        public int LearningAimPathwayCode { get; set; }

        public string LearningAimFundingLineType { get; set; }

        public DateTime? LearningStartDate { get; set; }

        public string AgreementId { get; set; }

        public DateTime IlrSubmissionDateTime { get; set; }

        public long JobId { get; set; }

        public DateTimeOffset EventTime { get; set; }

        public DateTimeOffset CreationDate { get; set; }

        public long? LearningAimSequenceNumber { get; set; }
    }
}