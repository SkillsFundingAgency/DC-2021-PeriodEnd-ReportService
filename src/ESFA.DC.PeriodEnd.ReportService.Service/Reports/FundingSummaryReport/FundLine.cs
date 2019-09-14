using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport
{
    public class FundLine : IFundLine
    {
        private const int _periodCount = 12;
        private readonly decimal[] _periods;

        public FundLine(
            int currentPeriod,
            string title,
            decimal period1,
            decimal period2,
            decimal period3,
            decimal period4,
            decimal period5,
            decimal period6,
            decimal period7,
            decimal period8,
            decimal period9,
            decimal period10,
            decimal period11,
            decimal period12,
            bool includeInTotals = true)
        {
            CurrentPeriod = currentPeriod;
            Title = title;
            _periods = new[]
            {
                period1,
                period2,
                period3,
                period4,
                period5,
                period6,
                period7,
                period8,
                period9,
                period10,
                period11,
                period12,
            };

            IncludeInTotals = includeInTotals;
        }

        public string Title { get; }

        public decimal Period1 => _periods[0];

        public decimal Period2 => _periods[1];

        public decimal Period3 => _periods[2];

        public decimal Period4 => _periods[3];

        public decimal Period5 => _periods[4];

        public decimal Period6 => _periods[5];

        public decimal Period7 => _periods[6];

        public decimal Period8 => _periods[7];

        public decimal Period9 => _periods[8];

        public decimal Period10 => _periods[9];

        public decimal Period11 => _periods[10];

        public decimal Period12 => _periods[11];

        public decimal Period1To8 => SumPeriods(1, 8);

        public decimal Period9To12 => SumPeriods(9, 12);

        public decimal YearToDate => SumPeriods(1, CurrentPeriod);

        public decimal Total => SumPeriods(1, 12);

        public int CurrentPeriod { get; }

        public bool IncludeInTotals { get; }

        private decimal SumPeriods(int start, int end)
        {
            decimal total = 0;

            var endIndex = end > _periodCount ? _periodCount : end;

            for (var index = start - 1; index < endIndex; index++)
            {
                total += _periods[index];
            }

            return total;
        }
    }
}