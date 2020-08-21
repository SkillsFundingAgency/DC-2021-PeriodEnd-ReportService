using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model.Comparer
{
    public class PriceEpisodeEarningComparer : IEqualityComparer<PriceEpisodeEarning>, IPriceEpisodeEarningComparer
    {
        public bool Equals(PriceEpisodeEarning x, PriceEpisodeEarning y)
        {
            return (x.LearnRefNumber == y.LearnRefNumber) && (x.AimSequenceNumber == y.AimSequenceNumber);
        }

        public int GetHashCode(PriceEpisodeEarning obj)
        {
            return obj.GetHashCode();
        }
    }
}
