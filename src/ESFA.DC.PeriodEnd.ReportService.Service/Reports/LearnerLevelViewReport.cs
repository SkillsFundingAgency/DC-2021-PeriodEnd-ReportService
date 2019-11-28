﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Mapper;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports
{
    public class LearnerLevelViewReport : AbstractReport
    {
        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;
        private readonly IFM36PeriodEndProviderService _fm36ProviderService;
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly ILarsProviderService _larsProviderService;
        private readonly IFCSProviderService _fcsProviderService;
        private readonly ILearnerLevelViewModelBuilder _modelBuilder;

        public LearnerLevelViewReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IIlrPeriodEndProviderService ilrPeriodEndProviderService,
            IFM36PeriodEndProviderService fm36ProviderService,
            IDASPaymentsProviderService dasPaymentsProviderService,
            ILarsProviderService larsProviderService,
            IFCSProviderService fcsProviderService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            ILearnerLevelViewModelBuilder modelBuilder)
        : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
            _fm36ProviderService = fm36ProviderService;
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _larsProviderService = larsProviderService;
            _fcsProviderService = fcsProviderService;
            _modelBuilder = modelBuilder;
        }

        public override string ReportFileName => "Learner Level View Report";

        public override string ReportTaskName => ReportTaskNameConstants.LearnerLevelViewReport;

        public override async Task GenerateReport(
            IReportServiceContext reportServiceContext,
            ZipArchive archive,
            CancellationToken cancellationToken)
        {
            var externalFileName = GetFilename(reportServiceContext);
            var fileName = GetZipFilename(reportServiceContext);

            // get the main base DAS payments data
            var appsMonthlyPaymentDasInfo =
                await _dasPaymentsProviderService.GetPaymentsInfoForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn, cancellationToken);

            // get the DAS Earnings Event data
            var appsMonthlyPaymentDasEarningsInfo =
                await _dasPaymentsProviderService.GetEarningsInfoForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn, cancellationToken);

            // get the ILR data
            var appsMonthlyPaymentIlrInfo =
                await _ilrPeriodEndProviderService.GetILRInfoForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn, cancellationToken);

            // Get the name's of the learning aims
            IEnumerable<string> learnAimRefs = appsMonthlyPaymentIlrInfo.Learners.SelectMany(x => x.LearningDeliveries)
                .Select(x => x.LearnAimRef).Distinct().ToList();
            var appsMonthlyPaymentLarsLearningDeliveryInfos =
                await _larsProviderService.GetLarsLearningDeliveryInfoForAppsMonthlyPaymentReportAsync(
                    learnAimRefs.ToArray(),
                    cancellationToken);

            // Get the earning data
            var learnerLevelViewFM36Info = await _fm36ProviderService.GetFM36DataForLearnerLevelView(
                    reportServiceContext.Ukprn,
                    cancellationToken);

            // Get the employer investment info
            var appsCoInvestmentIlrInfo = await _ilrPeriodEndProviderService.GetILRInfoForAppsCoInvestmentReportAsync(
                    reportServiceContext.Ukprn,
                    cancellationToken);

            // Build the actual Apps Monthly Payment Report
            var learnerLevelViewModel = _modelBuilder.BuildLearnerLevelViewModelList(
                appsMonthlyPaymentIlrInfo,
                appsMonthlyPaymentDasInfo,
                appsMonthlyPaymentDasEarningsInfo,
                appsCoInvestmentIlrInfo,
                appsMonthlyPaymentLarsLearningDeliveryInfos,
                learnerLevelViewFM36Info,
                reportServiceContext.ReturnPeriod);

            string learnerLevelViewCSV = await GetLearnerLevelViewCsv(learnerLevelViewModel, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", learnerLevelViewCSV, cancellationToken);

            string learnerLevelFinancialsRemovedCSV = await GetLearnerLevelFinancialsRemovedViewCsv(learnerLevelViewModel, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", learnerLevelFinancialsRemovedCSV);
        }

        private async Task<string> GetLearnerLevelViewCsv(IReadOnlyList<LearnerLevelViewModel> learnerLevelViewModel, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<LearnerLevelViewMapper, LearnerLevelViewModel>(csvWriter, learnerLevelViewModel);

                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        private async Task<string> GetLearnerLevelFinancialsRemovedViewCsv(IReadOnlyList<LearnerLevelViewModel> learnerLevelViewModel, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<LearnerLevelViewFinancialsRemovedMapper, LearnerLevelViewModel>(csvWriter, learnerLevelViewModel);

                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}