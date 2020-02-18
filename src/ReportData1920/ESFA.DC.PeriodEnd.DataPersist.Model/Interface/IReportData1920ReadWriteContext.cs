using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ESFA.DC.PeriodEnd.DataPersist.Model.Interface
{
    public interface IReportData1920ReadWriteContext : IDisposable
    {
        DbSet<McaGlaDevolvedOccupancyReportV2> McaGlaDevolvedOccupancyReportV2s { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        DatabaseFacade Database { get; }
    }
}
