using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.PeriodEnd.ReportService.Model.Lars;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface ILarsProviderService
    {
        Task<Dictionary<string, LarsLearningDelivery>> GetLearningDeliveriesAsync(
            string[] validLearnerAimRefs,
            CancellationToken cancellationToken);

        Task<List<LearnerAndDeliveries>> GetFrameworkAimsAsync(
            string[] learnAimRefs,
            List<ILearner> learners,
            CancellationToken cancellationToken);

        Task<string> GetVersionAsync(CancellationToken cancellationToken);

        Task<string> GetStandardAsync(
            int learningDeliveryStandardCode,
            CancellationToken cancellationToken);

        Task<List<AppsMonthlyPaymentLarsLearningDeliveryInfo>> GetLarsLearningDeliveryInfoForAppsMonthlyPaymentReportAsync(string[] learnerAimRefs, CancellationToken cancellationToken);
    }
}
