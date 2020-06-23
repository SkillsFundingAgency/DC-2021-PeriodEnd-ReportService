﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly
{
    public class AppsMonthly : IReport
    {
        private readonly ICsvFileService _csvFileService;
        private readonly IFileNameService _fileNameService;
        private readonly IAppsMonthlyPaymentsDataProvider _appsMonthlyPaymentsDataProvider;
        private readonly IAppsMonthlyModelBuilder _appsMonthlyPaymentModelBuilder;
        public string ReportTaskName => "TaskGenerateAppsMonthlyPaymentReport";
        
        private string ReportFileName => "Apps Monthly Payment Report";

        public AppsMonthly(
            ICsvFileService csvFileService,
            IFileNameService fileNameService,
            IAppsMonthlyPaymentsDataProvider appsMonthlyPaymentsDataProvider,
            IAppsMonthlyModelBuilder appsMonthlyPaymentModelBuilder)
        {
            _csvFileService = csvFileService;
            _fileNameService = fileNameService;
            _appsMonthlyPaymentsDataProvider = appsMonthlyPaymentsDataProvider;
            _appsMonthlyPaymentModelBuilder = appsMonthlyPaymentModelBuilder;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var ukprn = reportServiceContext.Ukprn;
            var collectionYear = reportServiceContext.CollectionYear;

            var fileName = _fileNameService.GetFilename(reportServiceContext, ReportFileName, OutputTypes.Csv);

            var paymentsTask = _appsMonthlyPaymentsDataProvider.GetPaymentsAsync(ukprn, collectionYear, cancellationToken);
            var learnersTask = _appsMonthlyPaymentsDataProvider.GetLearnersAsync(ukprn, cancellationToken);
            var contractAllocationsTask = _appsMonthlyPaymentsDataProvider.GetContractAllocationsAsync(ukprn, cancellationToken);
            var earningsTask = _appsMonthlyPaymentsDataProvider.GetEarningsAsync(ukprn, cancellationToken);
            var priceEpisodesTask = _appsMonthlyPaymentsDataProvider.GetPriceEpisodesAsync(ukprn, cancellationToken);

            await Task.WhenAll(paymentsTask, learnersTask, contractAllocationsTask, earningsTask, priceEpisodesTask);

            var larsLearningDeliveries = await _appsMonthlyPaymentsDataProvider.GetLarsLearningDeliveriesAsync(learnersTask.Result, cancellationToken);

            var models = _appsMonthlyPaymentModelBuilder.Build(
                paymentsTask.Result,
                learnersTask.Result,
                contractAllocationsTask.Result,
                earningsTask.Result,
                larsLearningDeliveries,
                priceEpisodesTask.Result);

            await _csvFileService.WriteAsync<AppsMonthlyRecord, AppsMonthlyClassMap>(models, fileName, reportServiceContext.Container, cancellationToken);
        }
    }
}
