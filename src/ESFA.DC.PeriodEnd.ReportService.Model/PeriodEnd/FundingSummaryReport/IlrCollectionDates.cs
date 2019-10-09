using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class IlrCollectionDates
    {
        public IReadOnlyCollection<ReturnPeriod> ReturnPeriods { get; set; }

        public IReadOnlyCollection<CensusDate> CensusDates { get; set; }
    }
}