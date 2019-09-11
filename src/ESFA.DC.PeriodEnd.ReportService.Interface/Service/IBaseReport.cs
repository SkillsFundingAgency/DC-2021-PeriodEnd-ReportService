using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IBaseReport
    {
        string TaskName { get; }

        IEnumerable<Type> DependsOn { get; }

        Task<IEnumerable<string>> GenerateAsync(IReportServiceContext reportServiceContext, IReportServiceDependentData reportsDependentData, CancellationToken cancellationToken);
    }
}