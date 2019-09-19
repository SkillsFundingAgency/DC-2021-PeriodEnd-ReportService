using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport
{
    public interface IPeriodisedValuesLookup
    {
        IReadOnlyCollection<decimal?[]> GetPeriodisedValues(FundingDataSource fundModel, IEnumerable<string> fundLines, IEnumerable<string> attributes);
    }
}