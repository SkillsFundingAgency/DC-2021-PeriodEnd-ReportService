using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface
{
    public interface IAppsAdditionalPaymentsModelBuilder
    {
        IEnumerable<AppsAdditionalPaymentRecord> Build(
            ICollection<Payment> payments,
            ICollection<Learner> learners,
            ICollection<AecLearningDelivery> learningDeliveries,
            ICollection<ApprenticeshipPriceEpisodePeriodisedValues> periodisedValues);
    }
}