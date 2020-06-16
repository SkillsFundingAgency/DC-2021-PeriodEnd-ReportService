using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.FundingSummary.Builders
{
    public class FundLineGroupBuilder : AbstractBuilder<FundLineGroup>
    {
        private const int CurrentPeriod = 1;

        public FundLineGroupBuilder()
        {
            modelObject = new FundLineGroup
            {
                FundLines = new List<FundLine>
                {
                    new FundLineBuilder()
                        .With(b => b.CurrentPeriod, CurrentPeriod)
                        .With(b => b.IncludeInTotals, true).Build()
                }
            };
        }
    }
}
