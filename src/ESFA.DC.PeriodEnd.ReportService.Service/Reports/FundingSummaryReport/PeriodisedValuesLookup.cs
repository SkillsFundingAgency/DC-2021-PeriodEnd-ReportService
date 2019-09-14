using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport
{
    public class PeriodisedValuesLookup : Dictionary<FundingDataSources, Dictionary<string, Dictionary<string, decimal?[][]>>>, IPeriodisedValuesLookup
    {
        public IEnumerable<decimal?[]> GetPeriodisedValues(FundingDataSources fundModel, IEnumerable<string> fundLines, IEnumerable<string> attributes)
        {
            var periodisedValuesList = new List<decimal?[]>();

            if (fundLines == null || attributes == null)
            {
                return null;
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