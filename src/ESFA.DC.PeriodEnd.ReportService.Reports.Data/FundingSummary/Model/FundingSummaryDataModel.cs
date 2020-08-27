using System;
using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Data;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Model
{
    public class FundingSummaryDataModel : IFundingSummaryDataModel
    {
        public IPeriodisedValuesLookup PeriodisedValuesLookup { get; set; }

        public IDictionary<string, string> FcsDictionary { get; set; }

        public string OrganisationName { get; set; }

        public string EasFileName { get; set; }

        public DateTime? LastEasUpdate { get; set; }

        public string IlrFileName { get; set; }

        public DateTime IlrSubmittedDateTime { get; set; }
    }
}
