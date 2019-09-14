using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Model
{
    public interface IPeriodisedValuesLookup
    {
        IEnumerable<decimal?[]> GetPeriodisedValues(FundingDataSources fundModel, IEnumerable<string> fundLines, IEnumerable<string> attributes);
    }
}