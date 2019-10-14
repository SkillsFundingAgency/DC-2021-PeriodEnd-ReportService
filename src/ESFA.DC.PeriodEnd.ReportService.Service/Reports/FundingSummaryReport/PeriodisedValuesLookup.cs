using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class PeriodisedValuesLookup : Dictionary<FundingDataSource, Dictionary<string, Dictionary<string, decimal?[][]>>>, IPeriodisedValuesLookup
    {
        public IReadOnlyCollection<decimal?[]> GetPeriodisedValues(FundingDataSource fundModel, IEnumerable<string> fundLines, IEnumerable<string> attributes)
        {
            var periodisedValuesList = new List<decimal?[]>();

            if (fundLines == null || attributes == null)
            {
                return periodisedValuesList;
            }

            if (TryGetValue(fundModel, out var fundLineDictionary))
            {
                foreach (var fundLine in fundLines)
                {
                    if (fundLineDictionary.TryGetValue(fundLine, out var attributesDictionary))
                    {
                        foreach (var attribute in attributes)
                        {
                            if (attributesDictionary.TryGetValue(attribute, out var attributePeriodisedValues))
                            {
                                if (attributePeriodisedValues != null)
                                {
                                    periodisedValuesList.AddRange(attributePeriodisedValues);
                                }
                            }
                        }
                    }
                }
            }

            return periodisedValuesList;
        }
    }
}