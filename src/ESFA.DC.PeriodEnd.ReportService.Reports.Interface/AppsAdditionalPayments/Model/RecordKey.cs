using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model
{
    public struct RecordKey
    {
        public RecordKey(
            string learnerReferenceNumber,
            long uln,
            DateTime? learnStartDate,
            string reportingAimFundingLineType,
            string paymentType,
            string employerName,
            string employerId)
        {
            LearnerReferenceNumber = learnerReferenceNumber;
            Uln = uln;
            LearnStartDate = learnStartDate;
            ReportingAimFundingLineType = reportingAimFundingLineType;
            PaymentType = paymentType;
            EmployerName = employerName;
            EmployerId = employerId;
        }

        public string LearnerReferenceNumber { get; }

        public long Uln { get; }

        public DateTime? LearnStartDate { get; }

        public string ReportingAimFundingLineType { get; }

        public string PaymentType { get; }

        public string EmployerName { get; }

        public string EmployerId { get; }

        public override int GetHashCode()
            =>
            (
                LearnerReferenceNumber?.ToUpper(),
                Uln,
                LearnStartDate,
                ReportingAimFundingLineType,
                PaymentType,
                EmployerName,
                EmployerId
            ).GetHashCode();
    }
}
