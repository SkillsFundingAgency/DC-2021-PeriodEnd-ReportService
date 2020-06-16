using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary
{
    public class PeriodisedValuesLookup : IPeriodisedValuesLookup
    {
        private readonly Dictionary<FundingDataSource, Dictionary<string, Dictionary<string, decimal?[][]>>> _fundLineAttributeDictionary = new Dictionary<FundingDataSource, Dictionary<string, Dictionary<string, decimal?[][]>>>();

        private readonly
            Dictionary<FundingDataSource, Dictionary<string, Dictionary<int, Dictionary<int, decimal?[][]>>>> _fundlineFundingSourceTransactionTypeDictionary = new Dictionary<FundingDataSource, Dictionary<string, Dictionary<int, Dictionary<int, decimal?[][]>>>>();

        public void Add(FundingDataSource fundingDataSource, Dictionary<string, Dictionary<string, decimal?[][]>> dictionary) => _fundLineAttributeDictionary.Add(fundingDataSource, dictionary);

        public void Add(FundingDataSource fundingDataSource, Dictionary<string, Dictionary<int, Dictionary<int, decimal?[][]>>> dictionary) => _fundlineFundingSourceTransactionTypeDictionary.Add(fundingDataSource, dictionary);

        public IReadOnlyCollection<decimal?[]> GetPeriodisedValues(FundingDataSource fundModel, IEnumerable<string> fundLines, IEnumerable<string> attributes)
        {
            var periodisedValuesList = new List<decimal?[]>();

            if (fundLines == null || attributes == null)
            {
                return periodisedValuesList;
            }

            if (_fundLineAttributeDictionary.TryGetValue(fundModel, out var fundLineDictionary))
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

        public IReadOnlyCollection<decimal?[]> GetPeriodisedValues(FundingDataSource dataSource, IEnumerable<string> fundLines, IEnumerable<byte> fundingSources, IEnumerable<int> transactionTypes)
        {
            var periodisedValuesList = new List<decimal?[]>();

            if (fundLines == null || fundingSources == null || transactionTypes == null)
            {
                return periodisedValuesList;
            }

            if (_fundlineFundingSourceTransactionTypeDictionary.TryGetValue(dataSource, out var fundLineDictionary))
            {
                foreach (var fundLine in fundLines)
                {
                    if (fundLineDictionary.TryGetValue(fundLine, out var fundingSourcesDictionary))
                    {
                        foreach (var fundingSource in fundingSources)
                        {
                            if (fundingSourcesDictionary.TryGetValue(fundingSource, out var transactionTypesDictionary))
                            {
                                foreach (var transactionType in transactionTypes)
                                {
                                    if (transactionTypesDictionary.TryGetValue(transactionType, out var periodisedValues))
                                    {
                                        if (periodisedValues != null)
                                        {
                                            periodisedValuesList.AddRange(periodisedValues);
                                        }
                                    }
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