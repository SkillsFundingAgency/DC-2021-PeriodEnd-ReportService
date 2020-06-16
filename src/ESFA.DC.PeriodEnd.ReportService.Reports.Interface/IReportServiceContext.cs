using System;
using System.Collections.Generic;
using ESFA.DC.CollectionsManagement.Models;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface
{
    public interface IReportServiceContext
    {
        long JobId { get; }

        string Filename { get; }

        int Ukprn { get; }

        string Container { get; }

        IEnumerable<string> Tasks { get; }

        int CollectionYear { get; }

        int ReturnPeriod { get; }

        string ReturnPeriodName { get; }

        DateTime SubmissionDateTimeUtc { get; }

        string CollectionName { get; }

        string CollectionReturnCodeDC { get; }

        string CollectionReturnCodeESF { get; }

        string CollectionReturnCodeApp { get; }

        IEnumerable<ReturnPeriod> ILRPeriods { get; }

        IEnumerable<ReturnPeriod> ILRPeriodsAdjustedTimes { get; }

        string ReportDataConnectionString { get; }

        bool DataPersistFeatureEnabled { get; }
    }
}