using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.Org;
using ESFA.DC.ReferenceData.Organisations.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IOrgProviderService
    {
        Task<IEnumerable<OrgModel>> GetOrgDetailsForUKPRNsAsync(List<long> uKPRNs, CancellationToken cancellationToken);

        Task<IDictionary<long, string>> GetOrgDetailsDictionaryForUKPRNSAsync(List<long> ukprns, CancellationToken cancellationToken);
    }
}
