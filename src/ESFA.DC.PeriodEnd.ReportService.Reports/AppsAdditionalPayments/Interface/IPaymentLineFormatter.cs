using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface
{
    public interface IPaymentLineFormatter
    {
        string GetAdditionalPaymentType(byte transactionType);
        string GetApprenticeshipLegalEntityName(Payment payment);
        string GetEmployerId(AecLearningDelivery learningDelivery, Payment payment);
        string GetUpdatedFindingLineType(string paymentLearningAimFundingLineType);
    }
}