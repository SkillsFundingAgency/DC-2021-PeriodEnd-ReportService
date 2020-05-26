using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Reports
{
    public interface IReport
    {
        string ReportTaskName { get; }

        IEnumerable<Type> DependsOn { get; }

        Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
