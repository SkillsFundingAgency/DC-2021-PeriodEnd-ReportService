using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;
using ESFA.DC.ReportData.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Persistence
{
    public interface IUYPSummaryViewPersistenceMapper
    {
        IEnumerable<LearnerLevelViewReport> Map(IReportServiceContext reportServiceContext, IEnumerable<LearnerLevelViewModel> appsMonthlyRecords, CancellationToken cancellationToken);

        IEnumerable<UYPSummaryViewReport> Map(IReportServiceContext reportServiceContext, IEnumerable<LearnerLevelViewSummaryModel> summaryModels);
    }
}
