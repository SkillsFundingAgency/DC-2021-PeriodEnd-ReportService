using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.AppsAdditionalPaymentsReport.Model
{
    public struct AppsAdditionalPaymentRecordKey
    {
        public AppsAdditionalPaymentRecordKey(string paymentLearnerReferenceNumber, long paymentUniqueLearnerNumber, DateTime? paymentLearningStartDate, string paymentLearningAimFundingLineType, string paymentTypeOfAdditionalPayment, string appsServiceEmployerName, string ilrEmployerIdentifier)
        {
            PaymentLearnerReferenceNumber = paymentLearnerReferenceNumber;
            PaymentUniqueLearnerNumber = paymentUniqueLearnerNumber;
            PaymentLearningStartDate = paymentLearningStartDate;
            PaymentLearningAimFundingLineType = paymentLearningAimFundingLineType;
            PaymentTypeOfAdditionalPayment = paymentTypeOfAdditionalPayment;
            AppsServiceEmployerName = appsServiceEmployerName;
            IlrEmployerIdentifier = ilrEmployerIdentifier;
        }

        public string PaymentLearnerReferenceNumber { get; }

        public long PaymentUniqueLearnerNumber { get; }

        public DateTime? PaymentLearningStartDate { get; }

        public string PaymentLearningAimFundingLineType { get; }

        public string PaymentTypeOfAdditionalPayment { get; }

        public string AppsServiceEmployerName { get; }

        public string IlrEmployerIdentifier { get; }

        public static IEqualityComparer<AppsAdditionalPaymentRecordKey> AppsAdditionalPaymentRecordKeyComparer { get; } = new AppsAdditionalPaymentRecordKeyEqualityComparer();

        public override int GetHashCode()
        {
            return (
                PaymentUniqueLearnerNumber,
                PaymentLearningStartDate,
                PaymentLearnerReferenceNumber.ToUpper(),
                PaymentLearningAimFundingLineType.ToUpper(),
                PaymentTypeOfAdditionalPayment.ToUpper(),
                AppsServiceEmployerName.ToUpper(),
                IlrEmployerIdentifier.ToUpper())
                .GetHashCode();
        }

        private sealed class AppsAdditionalPaymentRecordKeyEqualityComparer : IEqualityComparer<AppsAdditionalPaymentRecordKey>
        {
            public bool Equals(AppsAdditionalPaymentRecordKey x, AppsAdditionalPaymentRecordKey y)
            {
                return x.GetHashCode() == y.GetHashCode();
            }

            public int GetHashCode(AppsAdditionalPaymentRecordKey obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
