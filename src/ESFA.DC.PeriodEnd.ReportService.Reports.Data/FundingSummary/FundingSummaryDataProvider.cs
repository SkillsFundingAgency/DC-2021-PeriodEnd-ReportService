using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.FundingSummary
{
    public class FundingSummaryDataProvider : IFundingSummaryDataProvider
    {
        private readonly IFm25DataProvider _fm25DataProvider;
        private readonly IFm35DataProvider _fm35DataProvider;
        private readonly IFm81DataProvider _fm81DataProvider;
        private readonly IFm99DataProvider _fm99DataProvider;
        private readonly IEasDataProvider _easDataProvider;
        private readonly IDasDataProvider _dasDataProvider;
        private readonly IDasEasDataProvider _dasEasDataProvider;
        private readonly IFcsDataProvider _fcsDataProvider;

        public FundingSummaryDataProvider(
            IFm25DataProvider fm25DataProvider,
            IFm35DataProvider fm35DataProvider,
            IFm81DataProvider fm81DataProvider,
            IFm99DataProvider fm99DataProvider,
            IEasDataProvider easDataProvider,
            IDasDataProvider dasDataProvider,
            IDasEasDataProvider dasEasDataProvider,
            IFcsDataProvider fcsDataProvider)
        {
            _fm25DataProvider = fm25DataProvider;
            _fm35DataProvider = fm35DataProvider;
            _fm81DataProvider = fm81DataProvider;
            _fm99DataProvider = fm99DataProvider;
            _easDataProvider = easDataProvider;
            _dasDataProvider = dasDataProvider;
            _dasEasDataProvider = dasEasDataProvider;
            _fcsDataProvider = fcsDataProvider;
        }

        public async Task<IFundingSummaryDataModel> ProvideAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            return new FundingSummaryDataModel
            {
                PeriodisedValuesLookup = await ProvidePeriodisedValuesAsync(reportServiceContext, cancellationToken),
                FcsDictionary = await ProvideFcsAsync(reportServiceContext, cancellationToken)
            };
        }

        private async Task<IDictionary<string, string>> ProvideFcsAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            return await _fcsDataProvider.Provide(reportServiceContext.Ukprn, cancellationToken);
        }

        private async Task<IPeriodisedValuesLookup> ProvidePeriodisedValuesAsync(
            IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var fm35 = _fm35DataProvider.Provide(reportServiceContext.Ukprn, cancellationToken);
            var fm25 = _fm25DataProvider.Provide(reportServiceContext.Ukprn, cancellationToken);
            var fm81 = _fm81DataProvider.Provide(reportServiceContext.Ukprn, cancellationToken);
            var fm99 = _fm99DataProvider.Provide(reportServiceContext.Ukprn, cancellationToken);
            var eas = _easDataProvider.Provide(reportServiceContext.Ukprn, cancellationToken);
            var das = _dasDataProvider.ProvideAsync(reportServiceContext.CollectionYear, reportServiceContext.Ukprn, cancellationToken);
            var easdas = _dasEasDataProvider.Provide(reportServiceContext.Ukprn, cancellationToken);

            await Task.WhenAll(fm35, fm25, fm81, fm99, eas, das, easdas);

            var periodisedValuesLookup = new PeriodisedValuesLookup();

            periodisedValuesLookup.Add(FundingDataSource.FM35, fm35.Result);
            periodisedValuesLookup.Add(FundingDataSource.FM25, fm25.Result);
            periodisedValuesLookup.Add(FundingDataSource.FM81, fm81.Result);
            periodisedValuesLookup.Add(FundingDataSource.FM99, fm99.Result);
            periodisedValuesLookup.Add(FundingDataSource.EAS, eas.Result);
            periodisedValuesLookup.Add(FundingDataSource.DAS, das.Result);
            periodisedValuesLookup.Add(FundingDataSource.EASDAS, easdas.Result);

            return periodisedValuesLookup;
        }
    }
}
