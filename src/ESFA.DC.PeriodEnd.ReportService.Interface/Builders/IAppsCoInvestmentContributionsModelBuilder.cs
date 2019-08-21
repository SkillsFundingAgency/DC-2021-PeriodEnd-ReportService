using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestmentContributions;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Builders
{
    public interface IAppsCoInvestmentContributionsModelBuilder
    {
        IEnumerable<AppsCoInvestmentContributionsModel> BuildModel(
            AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
            AppsCoInvestmentPaymentsInfo appsCoInvestmentPaymentsInfo);
    }
}