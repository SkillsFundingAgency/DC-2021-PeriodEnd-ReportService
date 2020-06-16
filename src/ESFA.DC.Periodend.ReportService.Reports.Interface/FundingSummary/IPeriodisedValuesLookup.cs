using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary
{
    public interface IPeriodisedValuesLookup
    {
        IReadOnlyCollection<decimal?[]> GetPeriodisedValues(FundingDataSource fundingDataSource, IEnumerable<string> fundLines, IEnumerable<string> attributes);

        IReadOnlyCollection<decimal?[]> GetPeriodisedValues(FundingDataSource dataSource, IEnumerable<string> fundLines, IEnumerable<byte> fundingSources, IEnumerable<int> transactionTypes);
    }
}