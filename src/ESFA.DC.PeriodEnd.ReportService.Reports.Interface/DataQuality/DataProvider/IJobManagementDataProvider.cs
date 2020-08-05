using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider
{
    public interface IJobManagementDataProvider
    {
        Task<int> ProvideCollectionIdAsync(string collectionType);

        Task<ICollection<FilePeriodInfo>> ProvideFilePeriodInfoForCollectionAsync(int collectionId);
    }
}
