using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public struct ReturnPeriod
    {
        public string Name { get; set; }

        public int Period { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }
}