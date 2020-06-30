using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment
{
    public class AppsCoInvestment : IReport
    {
        private readonly ICsvFileService _csvFileService;
        private readonly IFileNameService _fileNameService;
        private readonly IAppsCoInvestmentDataProvider _appsCoInvestmentDataProvider;
        private readonly IAppsCoInvestmentModelBuilder _appsCoInvestmentModelBuilder;
        public string ReportTaskName => "TaskGenerateAppsCoInvestmentContributionsReport";

        private string ReportFileName => "Apps Co-Investment Contributions Report";

        public AppsCoInvestment(
            ICsvFileService csvFileService,
            IFileNameService fileNameService,
            IAppsCoInvestmentDataProvider appsCoInvestmentDataProvider,
            IAppsCoInvestmentModelBuilder appsCoInvestmentModelBuilder)
        {
            _csvFileService = csvFileService;
            _fileNameService = fileNameService;
            _appsCoInvestmentDataProvider = appsCoInvestmentDataProvider;
            _appsCoInvestmentModelBuilder = appsCoInvestmentModelBuilder;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var ukprn = reportServiceContext.Ukprn;
            var collectionYear = reportServiceContext.CollectionYear;

            var fileName = _fileNameService.GetFilename(reportServiceContext, ReportFileName, OutputTypes.Csv);
            var paymentsTask = _appsCoInvestmentDataProvider.GetPaymentsAsync(ukprn, cancellationToken);
            var learnersTask = _appsCoInvestmentDataProvider.GetLearnersAsync(ukprn, cancellationToken);
            var priceEpisodePeriodisedValuesTask = _appsCoInvestmentDataProvider.GetAecPriceEpisodePeriodisedValuesAsync(ukprn, cancellationToken);

            await Task.WhenAll(paymentsTask, learnersTask, priceEpisodePeriodisedValuesTask);

            var appsCoInvestmentRecords = _appsCoInvestmentModelBuilder.Build(learnersTask.Result, paymentsTask.Result, priceEpisodePeriodisedValuesTask.Result, collectionYear).ToList();

            await _csvFileService.WriteAsync<AppsCoInvestmentRecord, AppsCoInvestmentClassMap>(appsCoInvestmentRecords, fileName, reportServiceContext.Container, cancellationToken);
        }
    }
}
