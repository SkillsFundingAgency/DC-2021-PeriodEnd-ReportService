using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Data
{
    public interface IFundingSummaryDataModel
    {
        IPeriodisedValuesLookup PeriodisedValuesLookup { get; set; }

        IDictionary<string, string> FcsDictionary { get; set; }

        string OrganisationName { get; set; }

        DateTime? LastEasUpdate { get; set; }

        string IlrFileName { get; set; }

        DateTime IlrSubmittedDateTime { get; set; }
    }
}
