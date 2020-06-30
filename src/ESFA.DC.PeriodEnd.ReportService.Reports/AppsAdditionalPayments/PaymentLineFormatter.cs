using System;
using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments
{
    public class PaymentLineFormatter : IPaymentLineFormatter
    {
        public static readonly Dictionary<byte, string> AdditionalPaymentTypes = new Dictionary<byte, string>(5)
        {
            [4] = PaymentTypeConstants.EmployerPaymentType,
            [5] = PaymentTypeConstants.ProviderPaymentType,
            [6] = PaymentTypeConstants.EmployerPaymentType,
            [7] = PaymentTypeConstants.ProviderPaymentType,
            [16] = PaymentTypeConstants.ApprenticePaymentType
        };

        public void FormatFundingLines(IEnumerable<Payment> payments)
        {
            foreach (var payment in payments)
            {
                if (FundLineConstants.NonLevyApprenticeship1618.Equals(payment.LearningAimFundingLineType, StringComparison.OrdinalIgnoreCase))
                {
                    payment.LearningAimFundingLineType = FundLineConstants.NonLevyApprenticeship1618NonProcured;
                }
                else if (FundLineConstants.NonLevyApprenticeship19Plus.Equals(payment.LearningAimFundingLineType, StringComparison.OrdinalIgnoreCase))
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

        public string GetEmployerId(AecLearningDelivery learningDelivery, Payment payment)
        {
            if (learningDelivery == null)
            {
                return GenericConstants.NotAvailable;
            }

            if (payment.TransactionType == DASPayments.TransactionType.First_16To18_Employer_Incentive || 
                payment.TransactionType == DASPayments.TransactionType.Second_16To18_Employer_Incentive)
            {
                var learnerEmployerId = payment.TransactionType == DASPayments.TransactionType.First_16To18_Employer_Incentive
                    ? learningDelivery?.LearnDelEmpIdFirstAdditionalPaymentThreshold
                    : learningDelivery?.LearnDelEmpIdSecondAdditionalPaymentThreshold;

                return learnerEmployerId?.ToString() ?? GenericConstants.NotAvailable;
            }

            return null;
        }
    }
}