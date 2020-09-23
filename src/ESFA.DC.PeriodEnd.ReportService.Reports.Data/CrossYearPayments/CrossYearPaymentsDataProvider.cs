using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.CrossYearPayments
{
    public class CrossYearPaymentsDataProvider : ICrossYearDataProvider
    {
        private readonly IOrgDataProvider _orgDataProvider;
        private readonly IFcsDataProvider _fcsDataProvider;
        private readonly IAppsDataProvider _appsDataProvider;

        public CrossYearPaymentsDataProvider(IOrgDataProvider orgDataProvider, IFcsDataProvider fcsDataProvider, IAppsDataProvider appsDataProvider)
        {
            _orgDataProvider = orgDataProvider;
            _fcsDataProvider = fcsDataProvider;
            _appsDataProvider = appsDataProvider;
        }

        public async Task<CrossYearDataModel> ProvideAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var orgNameTask = _orgDataProvider.ProvideAsync(reportServiceContext.Ukprn);

            var fcsAllocationsTask = _fcsDataProvider.ProvideAllocationsAsync(reportServiceContext.Ukprn);
            var fcsPaymentsTask = _fcsDataProvider.ProvidePaymentsAsync(reportServiceContext.Ukprn);
            var fcsContractsTask = _fcsDataProvider.ProviderContractsAsync(reportServiceContext.Ukprn);
            var appsPaymentTask = _appsDataProvider.ProvidePaymentsAsync(reportServiceContext.Ukprn);
            var appsAdjustmentPaymentsTask = _appsDataProvider.ProvideAdjustmentPaymentsAsync(reportServiceContext.Ukprn);

            await Task.WhenAll(orgNameTask, fcsAllocationsTask, fcsPaymentsTask, fcsContractsTask, appsPaymentTask, appsAdjustmentPaymentsTask);

            var model = new CrossYearDataModel
            {
                OrgName = orgNameTask.Result,
                Payments = appsPaymentTask.Result,
                AdjustmentPayments = appsAdjustmentPaymentsTask.Result,
                FcsAllocations = fcsAllocationsTask.Result,
                FcsPayments = fcsPaymentsTask.Result,
                FcsContracts = fcsContractsTask.Result
            };

            return model;
        }
    }
}
