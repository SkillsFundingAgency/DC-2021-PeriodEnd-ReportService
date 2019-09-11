using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport
{
    public interface IFundingSummaryReport
    {
        List<IFundingCategory> FundingCategories { get; }
    }
}
