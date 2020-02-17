using System.Linq;
using ESFA.DC.PeriodEnd.DataPersist.Model.Interface;

namespace ESFA.DC.PeriodEnd.DataPersist.Model
{
    public partial class ReportData1920Context : IReportData1920Context
    {
        IQueryable<McaGlaDevelovedOccupancyReportV2> IReportData1920Context.McaGlaDevelovedOccupancyReportV2s => McaGlaDevelovedOccupancyReportV2s;
    }
}
