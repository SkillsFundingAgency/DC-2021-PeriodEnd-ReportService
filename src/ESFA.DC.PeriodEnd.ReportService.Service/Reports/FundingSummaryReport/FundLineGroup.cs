using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport
{
    public class FundLineGroup : IFundLineGroup
    {
        private readonly FundingDataSources _fundModel;
        private readonly IEnumerable<string> _fundLines;
        private readonly IPeriodisedValuesLookup _periodisedValues;

        public FundLineGroup(
            string title,
            int currentPeriod,
            FundingDataSources fundModel,
            IEnumerable<string> fundLines,
            IPeriodisedValuesLookup periodisedValues)
        {
            CurrentPeriod = currentPeriod;
            _fundModel = fundModel;
            _fundLines = fundLines;
            _periodisedValues = periodisedValues;
            Title = title;
        }

        public IList<IFundLine> FundLines { get; set; } = new List<IFundLine>();

        public string Title { get; }

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

        public int CurrentPeriod { get; }

        private IEnumerable<IFundLine> FundLinesForTotal => FundLines.Where(fl => fl.IncludeInTotals);

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

        public IFundLine BuildFundLine(string title, IEnumerable<string> attributes, IEnumerable<string> fundLines = null, bool includeInTotals = true)
        {
            var periodisedValuesList = _periodisedValues.GetPeriodisedValues(_fundModel, fundLines ?? _fundLines, attributes);

            if (periodisedValuesList != null)
            {
                return new FundLine(
                    CurrentPeriod,
                    title,
                    periodisedValuesList.Where(pv => pv[0].HasValue).Sum(pv => pv[0].Value),
                    periodisedValuesList.Where(pv => pv[1].HasValue).Sum(pv => pv[1].Value),
                    periodisedValuesList.Where(pv => pv[2].HasValue).Sum(pv => pv[2].Value),
                    periodisedValuesList.Where(pv => pv[3].HasValue).Sum(pv => pv[3].Value),
                    periodisedValuesList.Where(pv => pv[4].HasValue).Sum(pv => pv[4].Value),
                    periodisedValuesList.Where(pv => pv[5].HasValue).Sum(pv => pv[5].Value),
                    periodisedValuesList.Where(pv => pv[6].HasValue).Sum(pv => pv[6].Value),
                    periodisedValuesList.Where(pv => pv[7].HasValue).Sum(pv => pv[7].Value),
                    periodisedValuesList.Where(pv => pv[8].HasValue).Sum(pv => pv[8].Value),
                    periodisedValuesList.Where(pv => pv[9].HasValue).Sum(pv => pv[9].Value),
                    periodisedValuesList.Where(pv => pv[10].HasValue).Sum(pv => pv[10].Value),
                    periodisedValuesList.Where(pv => pv[11].HasValue).Sum(pv => pv[11].Value),
                    includeInTotals);
            }

            return BuildEmptyFundLine(title);
        }

        private IFundLine BuildEmptyFundLine(string title)
        {
            return new FundLine(CurrentPeriod, title, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
    }
}