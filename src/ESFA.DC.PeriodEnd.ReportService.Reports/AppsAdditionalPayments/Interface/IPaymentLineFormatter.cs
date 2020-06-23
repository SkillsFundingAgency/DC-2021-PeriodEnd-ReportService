using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface
{
    public interface IPaymentLineFormatter
    {
        void FormatFundingLines(IEnumerable<Payment> payments);
        string GetAdditionalPaymentType(byte transactionType);
        string GetApprenticeshipLegalEntityName(Payment payment);
        string GetEmployerId(LearningDelivery learningDelivery, Payment payment);
    }
}