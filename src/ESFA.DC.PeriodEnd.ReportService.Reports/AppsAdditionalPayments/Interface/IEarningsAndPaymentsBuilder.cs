using System;
using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface
{
    public interface IEarningsAndPaymentsBuilder
    {
        EarningsAndPayments Build(
            IEnumerable<PaymentAndLearningDelivery> paymentAndLearningDeliveries,
            IEnumerable<ApprenticeshipPriceEpisodePeriodisedValues> periodisedValuesForLearner);

        string[] GetAttributesForTransactionType(byte transactionType);

        decimal GetEarningsForPeriod(
            List<ApprenticeshipPriceEpisodePeriodisedValues> periodisedValuesForPayment,
            string[] attributeTypes,
            Func<ApprenticeshipPriceEpisodePeriodisedValues, decimal?> periodSelector);
    }
}