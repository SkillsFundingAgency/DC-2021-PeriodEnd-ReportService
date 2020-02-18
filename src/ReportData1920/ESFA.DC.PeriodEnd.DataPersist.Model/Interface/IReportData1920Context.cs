using System;
using System.Linq;

namespace ESFA.DC.PeriodEnd.DataPersist.Model.Interface
{
    public interface IReportData1920Context : IDisposable
    {
        IQueryable<McaGlaDevolvedOccupancyReportV2> McaGlaDevolvedOccupancyReportV2s { get; }
    }
}
