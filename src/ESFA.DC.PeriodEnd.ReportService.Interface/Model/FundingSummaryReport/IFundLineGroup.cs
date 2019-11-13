using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport
{
    public interface IFundLineGroup : IFundingSummaryReportRow
    {
        IList<IFundLine> FundLines { get; }
    }
}