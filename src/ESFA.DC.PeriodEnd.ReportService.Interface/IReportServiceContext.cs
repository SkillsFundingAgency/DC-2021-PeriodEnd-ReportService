﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Interface
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

        DateTime SubmissionDateTimeUtc { get; }

        string CollectionName { get; }
    }
}
