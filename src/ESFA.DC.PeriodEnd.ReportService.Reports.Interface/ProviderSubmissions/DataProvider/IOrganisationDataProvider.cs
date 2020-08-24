using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.DataProvider
{
    public interface IOrganisationDataProvider
    {
        Task<ICollection<Organisation>> ProvideAsync(ICollection<long> ukprns);
    }
}
