using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment.Comparer
{
    public class AECApprenticeshipPriceEpisodePeriodisedValuesInfoComparer : IEqualityComparer<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>, IAECApprenticeshipPriceEpisodePeriodisedValuesInfoyComparer
    {
        public bool Equals(AECApprenticeshipPriceEpisodePeriodisedValuesInfo x, AECApprenticeshipPriceEpisodePeriodisedValuesInfo y)
        {
            return (x.LearnRefNumber == y.LearnRefNumber) && (x.AimSeqNumber == y.AimSeqNumber);
        }

        public int GetHashCode(AECApprenticeshipPriceEpisodePeriodisedValuesInfo obj)
        {
            return obj.GetHashCode();
        }
    }
}
