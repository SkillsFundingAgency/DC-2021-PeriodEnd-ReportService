using System.Linq;
using ESFA.DC.PeriodEnd.DataPersist.Model.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.DataPersist.Model
{
    public partial class ReportData1920Context : IReportData1920ReadWriteContext, IReportData1920Context
    {
        DbSet<McaGlaDevolvedOccupancyReportV2> IReportData1920ReadWriteContext.McaGlaDevolvedOccupancyReportV2s => McaGlaDevolvedOccupancyReportV2s;

        IQueryable<McaGlaDevolvedOccupancyReportV2> IReportData1920Context.McaGlaDevolvedOccupancyReportV2s => McaGlaDevolvedOccupancyReportV2s;
    }
}
