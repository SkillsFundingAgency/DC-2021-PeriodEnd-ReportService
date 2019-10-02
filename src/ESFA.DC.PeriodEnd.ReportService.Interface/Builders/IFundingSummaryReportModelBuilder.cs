using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Builders
{
    public interface IFundingSummaryReportModelBuilder
    {
        IReadOnlyList<IFundingCategory> BuildFundingSummaryReportModel(
            IReportServiceContext reportServiceContext,
            IReportServiceDependentData reportServiceDependentData,
            Dictionary<string, Dictionary<string, decimal?[][]>> fm35LearningDeliveryPeriodisedValues,
            IList<ProviderEasInfo> providerEasInfo,
            AppsMonthlyPaymentFcsInfo appsMonthlyPaymentFcsInfo);
    }
}