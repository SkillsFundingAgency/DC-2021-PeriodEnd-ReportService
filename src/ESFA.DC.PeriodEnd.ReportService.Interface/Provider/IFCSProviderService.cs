using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataExtractReport;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IFCSProviderService
    {
        Task<List<DataExtractFcsInfo>> GetFCSForDataExtractReport(IEnumerable<string> OrganisationIds, CancellationToken cancellationToken);

        Task<AppsMonthlyPaymentFcsInfo> GetFcsInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken);

        Task<IDictionary<string, string>> GetContractAllocationNumberFSPCodeLookupAsync(int ukprn, CancellationToken cancellationToken);
    }
}