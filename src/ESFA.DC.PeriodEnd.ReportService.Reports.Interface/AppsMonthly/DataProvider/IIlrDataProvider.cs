using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.DataProvider
{
    public interface IIlrDataProvider
    {
        Task<ICollection<AecApprenticeshipPriceEpisode>> GetPriceEpisodesAsync(int ukprn,
            CancellationToken cancellationToken);
    }
}
