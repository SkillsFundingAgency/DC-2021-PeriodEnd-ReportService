using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport
{
    public interface IPeriodisedValuesLookup
    {
        IReadOnlyCollection<decimal?[]> GetPeriodisedValues(FundingDataSource fundingDataSource, IEnumerable<string> fundLines, IEnumerable<string> attributes);

        IReadOnlyCollection<decimal?[]> GetPeriodisedValues(FundingDataSource dataSource, IEnumerable<string> fundLines, IEnumerable<byte> fundingSources, IEnumerable<int> transactionTypes);
    }
}