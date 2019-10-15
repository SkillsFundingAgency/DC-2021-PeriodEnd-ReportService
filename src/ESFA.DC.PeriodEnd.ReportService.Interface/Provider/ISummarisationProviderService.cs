using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface ISummarisationProviderService
    {
        Task<IEnumerable<DataExtractModel>> GetSummarisedActualsForDataExtractReport(
            string collectionType,
            IReadOnlyCollection<string> collectionReturnCodes,
            CancellationToken cancellationToken);
    }
}
