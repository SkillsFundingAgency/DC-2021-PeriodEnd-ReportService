using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary
{
    public interface IFundingSummaryModelBuilder
    {
        FundingSummaryReportModel Build(IReportServiceContext reportServiceContext, IFundingSummaryDataModel fundingSummaryDataModel);
    }
}
