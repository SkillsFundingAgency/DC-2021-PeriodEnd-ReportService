using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Interface
{
    public interface IFundingSummaryReportModelBuilder
    {
        FundingSummaryReportModel BuildFundingSummaryReportModel(IReportServiceContext reportServiceContext, IPeriodisedValuesLookup periodisedValuesLookup, IDictionary<string, string> fcsContractAllocationFspCodeLookup);
    }
}