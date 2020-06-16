using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model
{
    public class FundLineGroup : IFundingSummaryReportRow
    {
        private readonly FundingDataSource _fundModel;
        private readonly IEnumerable<string> _fundLines;
        private readonly IPeriodisedValuesLookup _periodisedValues;

        public FundLineGroup()
        {
        }

        public FundLineGroup(
            string title,
            byte currentPeriod,
            FundingDataSource fundModel,
            IEnumerable<string> fundLines,
            IPeriodisedValuesLookup periodisedValues)
        {
            CurrentPeriod = currentPeriod;
            ContractAllocationNumber = null;
            _fundModel = fundModel;
            _fundLines = fundLines;
            _periodisedValues = periodisedValues;
            Title = title;
        }

        public int CurrentPeriod { get; set; }

        public string ContractAllocationNumber { get; set; }

        public IList<FundLine> FundLines { get; set; } = new List<FundLine>();

        public string Title { get; set; }

        public decimal Period1 => FundLinesForTotal.Sum(fl => fl.Period1);

        public decimal Period2 => FundLinesForTotal.Sum(fl => fl.Period2);

        public decimal Period3 => FundLinesForTotal.Sum(fl => fl.Period3);

        public decimal Period4 => FundLinesForTotal.Sum(fl => fl.Period4);

        public decimal Period5 => FundLinesForTotal.Sum(fl => fl.Period5);

        public decimal Period6 => FundLinesForTotal.Sum(fl => fl.Period6);

        public decimal Period7 => FundLinesForTotal.Sum(fl => fl.Period7);

        public decimal Period8 => FundLinesForTotal.Sum(fl => fl.Period8);

        public decimal Period9 => FundLinesForTotal.Sum(fl => fl.Period9);

        public decimal Period10 => FundLinesForTotal.Sum(fl => fl.Period10);

        public decimal Period11 => FundLinesForTotal.Sum(fl => fl.Period11);

        public decimal Period12 => FundLinesForTotal.Sum(fl => fl.Period12);

        public decimal Period1To8 => FundLinesForTotal.Sum(fl => fl.Period1To8);

        public decimal Period9To12 => FundLinesForTotal.Sum(fl => fl.Period9To12);

        public decimal YearToDate => FundLinesForTotal.Sum(fl => fl.YearToDate);

        public decimal Total => FundLinesForTotal.Sum(fl => fl.Total);

        private IEnumerable<FundLine> FundLinesForTotal => FundLines.Where(fl => fl.IncludeInTotals);

        public FundLineGroup WithFundLine(string title, IEnumerable<string> fundLines, IEnumerable<string> attributes, bool includeInTotals = true)
        {
            var fundLine = BuildFundLine(title, attributes, fundLines, includeInTotals);

            FundLines.Add(fundLine);

            return this;
        }

        public FundLineGroup WithFundLine(string title, IEnumerable<string> attributes, bool includeInTotals = true)
        {
            var fundLine = BuildFundLine(title, attributes, includeInTotals: includeInTotals);

            FundLines.Add(fundLine);

            return this;
        }

        public FundLineGroup WithFundLine(string title, IEnumerable<byte> fundingSources, IEnumerable<int> transactionTypes)
        {
            var fundLine = BuildFundLine(title, fundingSources, transactionTypes);

            FundLines.Add(fundLine);

            return this;
        }

        public FundLineGroup WithFundLine(string title, IEnumerable<string> fundLines, IEnumerable<byte> fundingSources, IEnumerable<int> transactionTypes)
        {
            var fundLine = BuildFundLine(title, fundingSources, transactionTypes, fundLines);

            FundLines.Add(fundLine);

            return this;
        }

        public FundLine BuildFundLine(string title, IEnumerable<byte> fundingSources, IEnumerable<int> transactionTypes, IEnumerable<string> fundLines = null, bool includeInTotals = true)
        {
            var periodisedValuesList = _periodisedValues.GetPeriodisedValues(_fundModel, fundLines ?? _fundLines, fundingSources, transactionTypes);

            return BuildFundLineFromPeriodisedValues(periodisedValuesList, title, includeInTotals);
        }

        public FundLine BuildFundLine(string title, IEnumerable<string> attributes, IEnumerable<string> fundLines = null, bool includeInTotals = true)
        {
            var periodisedValuesList = _periodisedValues.GetPeriodisedValues(_fundModel, fundLines ?? _fundLines, attributes);

            return BuildFundLineFromPeriodisedValues(periodisedValuesList, title, includeInTotals);
        }

        public FundLine BuildFundLineFromPeriodisedValues(IReadOnlyCollection<decimal?[]> periodisedValuesList, string title, bool includeInTotals)
        {
            FundLine fundLine = null;
            if (periodisedValuesList != null)
            {
                fundLine = new FundLine(
                    CurrentPeriod,
                    ContractAllocationNumber,
                    title,
                    GetTotalFrom(periodisedValuesList, 0),
                    GetTotalFrom(periodisedValuesList, 1),
                    GetTotalFrom(periodisedValuesList, 2),
                    GetTotalFrom(periodisedValuesList, 3),
                    GetTotalFrom(periodisedValuesList, 4),
                    GetTotalFrom(periodisedValuesList, 5),
                    GetTotalFrom(periodisedValuesList, 6),
                    GetTotalFrom(periodisedValuesList, 7),
                    GetTotalFrom(periodisedValuesList, 8),
                    GetTotalFrom(periodisedValuesList, 9),
                    GetTotalFrom(periodisedValuesList, 10),
                    GetTotalFrom(periodisedValuesList, 11),
                    includeInTotals);
            }
            else
            {
                fundLine = new FundLine(CurrentPeriod, ContractAllocationNumber, title, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            }

            return fundLine;
        }

        public decimal GetTotalFrom(IReadOnlyCollection<decimal?[]> values, byte index) =>
            values.Where(pv => pv[index].HasValue).Sum(pv => pv[index]) ?? 0m;
    }
}
