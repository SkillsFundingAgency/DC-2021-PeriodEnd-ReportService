using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders
{
    public interface IAppsCoInvestmentModelBuilder
    {
        IEnumerable<AppsCoInvestmentRecord> Build(
            ICollection<Learner> learners,
            ICollection<Payment> payments,
            ICollection<AECApprenticeshipPriceEpisodePeriodisedValues> aecPriceEpisodePeriodisedValues,
            int currentAcademicYear,
            int previousYearClosedReturnPeriod);

    }
}
