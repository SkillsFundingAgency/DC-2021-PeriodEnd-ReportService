using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider
{
    public interface IOrganisationDataProvider
    {
        Task<ICollection<Organisation>> ProvideAsync(ICollection<long> ukprns);
    }
}
