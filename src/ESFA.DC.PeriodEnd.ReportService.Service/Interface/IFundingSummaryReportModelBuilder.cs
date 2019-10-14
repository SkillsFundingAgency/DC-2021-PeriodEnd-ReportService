using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Builders
{
    public interface IFundingSummaryReportModelBuilder
    {
        FundingSummaryReportModel BuildFundingSummaryReportModel(IReportServiceContext reportServiceContext, IPeriodisedValuesLookup periodisedValuesLookup);
    }
}