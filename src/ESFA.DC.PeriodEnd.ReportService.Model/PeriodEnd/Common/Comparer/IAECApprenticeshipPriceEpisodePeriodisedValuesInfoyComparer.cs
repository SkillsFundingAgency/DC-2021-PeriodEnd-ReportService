using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment.Comparer
{
    public interface IAECApprenticeshipPriceEpisodePeriodisedValuesInfoyComparer
    {
        bool Equals(AECApprenticeshipPriceEpisodePeriodisedValuesInfo x, AECApprenticeshipPriceEpisodePeriodisedValuesInfo y);

        int GetHashCode(AECApprenticeshipPriceEpisodePeriodisedValuesInfo obj);
    }
}
