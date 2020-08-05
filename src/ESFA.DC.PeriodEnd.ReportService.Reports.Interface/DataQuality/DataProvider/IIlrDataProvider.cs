using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider
{
    public interface IIlrDataProvider
    {
        Task<ICollection<RuleStats>> ProvideTop20RuleViolationsAsync();

        Task<ICollection<ProviderSubmission>> ProvideProvidersWithoutValidLearners(CancellationToken cancellationToken);

        Task<ICollection<ProviderCount>> ProvideProvidersWithMostInvalidLearners(CancellationToken cancellationToken);
    }
}
