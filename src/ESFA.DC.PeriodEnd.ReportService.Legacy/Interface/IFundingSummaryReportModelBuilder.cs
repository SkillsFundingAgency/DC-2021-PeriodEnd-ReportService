using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Reports.FundingSummaryReport.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Interface
{
    public interface IFundingSummaryReportModelBuilder
    {
        Task<FundingSummaryReportModel> BuildFundingSummaryReportModel(IReportServiceContext reportServiceContext, IPeriodisedValuesLookup periodisedValuesLookup, IDictionary<string, string> fcsContractAllocationFspCodeLookup, CancellationToken cancellationToken);
    }
}