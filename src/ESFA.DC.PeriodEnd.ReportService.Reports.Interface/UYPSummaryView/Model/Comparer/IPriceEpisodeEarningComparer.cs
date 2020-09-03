using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model.Comparer
{
    public interface IPriceEpisodeEarningComparer
    {
        bool Equals(PriceEpisodeEarning x, PriceEpisodeEarning y);

        int GetHashCode(PriceEpisodeEarning obj);
    }
}
