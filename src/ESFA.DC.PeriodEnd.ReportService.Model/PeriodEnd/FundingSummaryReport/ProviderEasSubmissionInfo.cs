using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class ProviderEasSubmissionInfo
    {
        //public ProviderEasSubmissionInfo(string ukprn, IList<ProviderEasSubmission> easSubmissions)
        //{
        //    UKPRN = ukprn;
        //    EasSubmissions = easSubmissions;
        //}

        public string UKPRN { get; set; }

        public IList<ProviderEasSubmission> EasSubmissions { get; set; }
    }
}
