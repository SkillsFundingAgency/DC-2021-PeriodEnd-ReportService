﻿using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Provider
{
    public interface IPeriodisedValuesLookupProviderService
    {
        IPeriodisedValuesLookup Provide(IEnumerable<FundingDataSources> fundingDataSources, IReportServiceDependentData reportServiceDependentData);
    }
}