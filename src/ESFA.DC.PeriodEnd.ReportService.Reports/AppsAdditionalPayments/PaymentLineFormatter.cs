using System;
using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments
{
    public class PaymentLineFormatter : IPaymentLineFormatter
    {
        private static readonly string NotAvailable = "Not Available";

        public static readonly Dictionary<byte, string> AdditionalPaymentTypes = new Dictionary<byte, string>(5)
        {
            [4] = "Employer",
            [5] = "Provider",
            [6] = "Employer",
            [7] = "Provider",
            [16] = "Apprentice"
        };

        public void FormatFundingLines(IEnumerable<Payment> payments)
        {
            foreach (var payment in payments)
            {
                if (FundLineConstants.NonLevyApprenticeship1618.Equals(payment.LearningAimFundingLineType))
                {
                    payment.LearningAimFundingLineType = FundLineConstants.NonLevyApprenticeship1618NonProcured;
                }
                else if (FundLineConstants.NonLevyApprenticeship19Plus.Equals(payment.LearningAimFundingLineType))
                {
                    payment.LearningAimFundingLineType = FundLineConstants.NonLevyApprenticeship19PlusNonProcured;

                }
            }
        }

        public string GetAdditionalPaymentType(byte transactionType)
        {
            if (AdditionalPaymentTypes.TryGetValue(transactionType, out string paymentType))
            {
                return paymentType;
            }

            throw new ApplicationException($"Unexpected TransactionType [{transactionType}]");
        }

        public string GetApprenticeshipLegalEntityName(Payment payment)
        {
            if (payment.ContractType == 1 && 
                (payment.TransactionType == DASPayments.TransactionType.First_16To18_Employer_Incentive || 
                 payment.TransactionType == DASPayments.TransactionType.Second_16To18_Employer_Incentive))
            {
                return payment.ApprenticeshipLegalEntityName;
            }

            return null;
        }

        public string GetEmployerId(LearningDelivery learningDelivery, Payment payment)
        {
            if (payment.TransactionType == DASPayments.TransactionType.First_16To18_Employer_Incentive || 
                payment.TransactionType == DASPayments.TransactionType.Second_16To18_Employer_Incentive)
            {
                var learnerEmployerId = payment.TransactionType == DASPayments.TransactionType.First_16To18_Employer_Incentive
                    ? learningDelivery?.LearnDelEmpIdFirstAdditionalPaymentThreshold
                    : learningDelivery?.LearnDelEmpIdSecondAdditionalPaymentThreshold;

                return learnerEmployerId?.ToString() ?? NotAvailable;
            }

            return null;
        }
    }
}