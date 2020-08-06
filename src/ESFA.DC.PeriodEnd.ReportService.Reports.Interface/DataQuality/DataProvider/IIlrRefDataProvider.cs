using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider
{
    public interface IIlrRefDataProvider
    {
        Task<ICollection<ValidationRule>> ProvideAsync(CancellationToken cancellationToken);
    }
}
