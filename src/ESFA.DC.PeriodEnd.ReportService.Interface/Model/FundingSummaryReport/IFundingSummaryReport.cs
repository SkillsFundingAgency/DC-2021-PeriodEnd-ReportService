﻿using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport
{
    public interface IFundingSummaryReport
    {
        List<IFundingCategory> FundingCategories { get; }

        IDictionary<string, string> HeaderData { get; }

        IDictionary<string, string> FooterData { get; }
    }
}
