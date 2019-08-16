using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.DataExtractReport;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IFCSProviderService
    {
        Task<List<DataExtractFcsInfo>> GetFCSForDataExtractReport(IEnumerable<string> OrganisationIds, CancellationToken cancellationToken);
    }
}