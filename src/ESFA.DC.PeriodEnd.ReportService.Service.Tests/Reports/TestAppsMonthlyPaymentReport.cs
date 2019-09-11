using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Service.Builders;
using ESFA.DC.PeriodEnd.ReportService.Service.Mapper;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports;
using ESFA.DC.PeriodEnd.ReportService.Service.Service;
using ESFA.DC.PeriodEnd.ReportService.Service.Tests.Helpers;
using ESFA.DC.PeriodEnd.ReportService.Service.Tests.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Tests.Reports
{
    public sealed class TestAppsMonthlyPaymentReport
    {
        [Fact]
        public async Task TestAppsMonthlyPaymentReportGeneration()
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            int ukPrn = 10036143;
            string filename = $"R01_10036143_Apps Monthly Payment Report {dateTime:yyyyMMdd-HHmmss}";

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10036143);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(1);

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IIlrPeriodEndProviderService> IlrPeriodEndProviderServiceMock =
                new Mock<IIlrPeriodEndProviderService>();
            Mock<IDASPaymentsProviderService> dasPaymentProviderMock = new Mock<IDASPaymentsProviderService>();
            Mock<IFM36PeriodEndProviderService> fm36ProviderServiceMock = new Mock<IFM36PeriodEndProviderService>();
            Mock<ILarsProviderService> larsProviderServiceMock = new Mock<ILarsProviderService>();
            Mock<IFCSProviderService> fcsProviderServiceMock = new Mock<IFCSProviderService>();
            IValueProvider valueProvider = new ValueProvider();
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((key, value, ct) => csv = value)
                .Returns(Task.CompletedTask);

            var appsMonthlyPaymentIlrInfo = BuildILRModel(ukPrn);
            var appsMonthlyPaymentRulebaseInfo = BuildRulebaseModel(ukPrn);
            var appsMonthlyPaymentDasInfo = BuildDasPaymentsModel(ukPrn);
            var appsMonthlyPaymentDasEarningsInfo = BuildDasEarningsModel(ukPrn);
            var appsMonthlyPaymentFcsInfo = BuildFcsModel(ukPrn);
            var larsDeliveryInfoModel = BuildLarsDeliveryInfoModel();

            IlrPeriodEndProviderServiceMock
                .Setup(
                    x => x.GetILRInfoForAppsMonthlyPaymentReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appsMonthlyPaymentIlrInfo);
            fm36ProviderServiceMock
                .Setup(x => x.GetRulebaseDataForAppsMonthlyPaymentReportAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(appsMonthlyPaymentRulebaseInfo);
            dasPaymentProviderMock
                .Setup(x => x.GetPaymentsInfoForAppsMonthlyPaymentReportAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(appsMonthlyPaymentDasInfo);

            dasPaymentProviderMock
                .Setup(x => x.GetEarningsInfoForAppsMonthlyPaymentReportAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(appsMonthlyPaymentDasEarningsInfo);

            larsProviderServiceMock
                .Setup(x => x.GetLarsLearningDeliveryInfoForAppsMonthlyPaymentReportAsync(
                    It.IsAny<string[]>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(larsDeliveryInfoModel);
            fcsProviderServiceMock.Setup(x => x.GetFcsInfoForAppsMonthlyPaymentReportAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(appsMonthlyPaymentFcsInfo);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);
            var appsMonthlyPaymentModelBuilder = new AppsMonthlyPaymentModelBuilder();

            var report = new AppsMonthlyPaymentReport(
                logger.Object,
                storage.Object,
                IlrPeriodEndProviderServiceMock.Object,
                fm36ProviderServiceMock.Object,
                dasPaymentProviderMock.Object,
                larsProviderServiceMock.Object,
                fcsProviderServiceMock.Object,
                dateTimeProviderMock.Object,
                valueProvider,
                appsMonthlyPaymentModelBuilder);

            await report.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            File.WriteAllText($"{filename}.csv", csv);
            List<AppsMonthlyPaymentModel> result;
            TestCsvHelper.CheckCsv(csv, new CsvEntry(new AppsMonthlyPaymentMapper(), 1));
            using (var reader = new StreamReader($"{filename}.csv"))
            {
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.RegisterClassMap<AppsMonthlyPaymentMapper>();
                    result = csvReader.GetRecords<AppsMonthlyPaymentModel>().ToList();
                }
            }

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(4);

            result[0].PaymentLearnerReferenceNumber.Should().Be("A12345");
            result[0].PaymentUniqueLearnerNumber.Should().Be(12345);
            result[0].LearnerCampusIdentifier.Should().Be("camp101");
            result[0].ProviderSpecifiedLearnerMonitoringA.Should().Be("T180400007");
            result[0].ProviderSpecifiedLearnerMonitoringB.Should().Be("150563");
            result[0].PaymentEarningEventAimSeqNumber.Should().Be(1);
            result[0].PaymentLearningAimReference.Should().Be("50117889");
            result[0].LarsLearningDeliveryLearningAimTitle.Should().Be("Maths & English");
            result[0].LearningDeliveryOriginalLearningStartDate.Should().Be(new DateTime(2019, 08, 28));
            result[0].PaymentLearningStartDate.Should().Be(new DateTime(2019, 08, 28));
            result[0].LearningDeliveryLearningPlannedEndDate.Should().Be(new DateTime(2020, 07, 31));
            result[0].LearningDeliveryCompletionStatus.Should().Be(2);
            result[0].LearningDeliveryLearningActualEndDate.Should().Be(new DateTime(2020, 07, 30));
            result[0].LearningDeliveryAchievementDate.Should().Be(new DateTime(2020, 07, 30));
            result[0].LearningDeliveryOutcome.Should().Be(4);
            result[0].PaymentProgrammeType.Should().Be(1);
            result[0].PaymentStandardCode.Should().Be(1);
            result[0].PaymentFrameworkCode.Should().Be(1);
            result[0].PaymentPathwayCode.Should().Be(1);
            result[0].LearningDeliveryAimType.Should().Be(3);
            result[0].LearningDeliverySoftwareSupplierAimIdentifier.Should().Be("SwSup50117889");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringA.Should().Be("001");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringB.Should().Be("002");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringC.Should().Be("003");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringD.Should().Be("004");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringE.Should().Be("005");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringF.Should().Be("006");
            result[0].ProviderSpecifiedDeliveryMonitoringA.Should().Be("A000406");
            result[0].ProviderSpecifiedDeliveryMonitoringB.Should().Be("B002902");
            result[0].ProviderSpecifiedDeliveryMonitoringC.Should().Be("C004402");
            result[0].ProviderSpecifiedDeliveryMonitoringD.Should().Be("D006801");
            result[0].LearningDeliveryEndPointAssessmentOrganisation.Should().Be("9876543210");
            result[0].RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim.Should().Be(2);
            result[0].LearningDeliverySubContractedOrPartnershipUkprn.Should().Be(10000001);
            result[0].PaymentPriceEpisodeStartDate.Should().Be("28/08/2019");
            result[0].RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate.Should()
                .Be(new DateTime(2019, 08, 2));
            result[0].FcsContractContractAllocationContractAllocationNumber.Should().Be("YNLP-1503");
            result[0].PaymentFundingLineType.Should().Be("16-18 Apprenticeship Non-Levy Contract (procured)");
            result[0].PaymentApprenticeshipContractType.Should().Be(2);
            result[0].LearnerEmploymentStatusEmployerId.Should().Be(56789);
            result[0].RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier.Should().Be("PA102");
            result[0].LearnerEmploymentStatus.Should().Be(10);
            result[0].LearnerEmploymentStatusDate.Should().Be(new DateTime(2019, 08, 27));

            // payments, Learner 1, Aim 1

            // August
            result[0].AugustLevyPayments.Should().Be(null);
            result[0].AugustCoInvestmentPayments.Should().Be(null);
            result[0].AugustCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].AugustEmployerAdditionalPayments.Should().Be(null);
            result[0].AugustProviderAdditionalPayments.Should().Be(null);
            result[0].AugustApprenticeAdditionalPayments.Should().Be(null);
            result[0].AugustEnglishAndMathsPayments.Should().Be(17);
            result[0].AugustLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].AugustTotalPayments.Should().Be(17);

            // September
            result[0].SeptemberLevyPayments.Should().Be(null);
            result[0].SeptemberCoInvestmentPayments.Should().Be(null);
            result[0].SeptemberCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].SeptemberEmployerAdditionalPayments.Should().Be(null);
            result[0].SeptemberProviderAdditionalPayments.Should().Be(null);
            result[0].SeptemberApprenticeAdditionalPayments.Should().Be(null);
            result[0].SeptemberEnglishAndMathsPayments.Should().Be(17);
            result[0].SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].SeptemberTotalPayments.Should().Be(17);

            // October
            result[0].OctoberLevyPayments.Should().Be(null);
            result[0].OctoberCoInvestmentPayments.Should().Be(null);
            result[0].OctoberCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].OctoberEmployerAdditionalPayments.Should().Be(null);
            result[0].OctoberProviderAdditionalPayments.Should().Be(null);
            result[0].OctoberApprenticeAdditionalPayments.Should().Be(null);
            result[0].OctoberEnglishAndMathsPayments.Should().Be(17);
            result[0].OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].OctoberTotalPayments.Should().Be(17);

            // November
            result[0].NovemberLevyPayments.Should().Be(null);
            result[0].NovemberCoInvestmentPayments.Should().Be(null);
            result[0].NovemberCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].NovemberEmployerAdditionalPayments.Should().Be(null);
            result[0].NovemberProviderAdditionalPayments.Should().Be(null);
            result[0].NovemberApprenticeAdditionalPayments.Should().Be(null);
            result[0].NovemberEnglishAndMathsPayments.Should().Be(17);
            result[0].NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].NovemberTotalPayments.Should().Be(17);

            // December
            result[0].DecemberLevyPayments.Should().Be(null);
            result[0].DecemberCoInvestmentPayments.Should().Be(null);
            result[0].DecemberCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].DecemberEmployerAdditionalPayments.Should().Be(null);
            result[0].DecemberProviderAdditionalPayments.Should().Be(null);
            result[0].DecemberApprenticeAdditionalPayments.Should().Be(null);
            result[0].DecemberEnglishAndMathsPayments.Should().Be(17);
            result[0].DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].DecemberTotalPayments.Should().Be(17);

            // January
            result[0].JanuaryLevyPayments.Should().Be(null);
            result[0].JanuaryCoInvestmentPayments.Should().Be(null);
            result[0].JanuaryCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].JanuaryEmployerAdditionalPayments.Should().Be(null);
            result[0].JanuaryProviderAdditionalPayments.Should().Be(null);
            result[0].JanuaryApprenticeAdditionalPayments.Should().Be(null);
            result[0].JanuaryEnglishAndMathsPayments.Should().Be(17);
            result[0].JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].JanuaryTotalPayments.Should().Be(17);

            // February
            result[0].FebruaryLevyPayments.Should().Be(null);
            result[0].FebruaryCoInvestmentPayments.Should().Be(null);
            result[0].FebruaryCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].FebruaryEmployerAdditionalPayments.Should().Be(null);
            result[0].FebruaryProviderAdditionalPayments.Should().Be(null);
            result[0].FebruaryApprenticeAdditionalPayments.Should().Be(null);
            result[0].FebruaryEnglishAndMathsPayments.Should().Be(17);
            result[0].FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].FebruaryTotalPayments.Should().Be(17);

            // March
            result[0].MarchLevyPayments.Should().Be(null);
            result[0].MarchCoInvestmentPayments.Should().Be(null);
            result[0].MarchCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].MarchEmployerAdditionalPayments.Should().Be(null);
            result[0].MarchProviderAdditionalPayments.Should().Be(null);
            result[0].MarchApprenticeAdditionalPayments.Should().Be(null);
            result[0].MarchEnglishAndMathsPayments.Should().Be(17);
            result[0].MarchLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].MarchTotalPayments.Should().Be(17);

            // April
            result[0].AprilLevyPayments.Should().Be(null);
            result[0].AprilCoInvestmentPayments.Should().Be(null);
            result[0].AprilCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].AprilEmployerAdditionalPayments.Should().Be(null);
            result[0].AprilProviderAdditionalPayments.Should().Be(null);
            result[0].AprilApprenticeAdditionalPayments.Should().Be(null);
            result[0].AprilEnglishAndMathsPayments.Should().Be(17);
            result[0].AprilLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].AprilTotalPayments.Should().Be(17);

            // May
            result[0].MayLevyPayments.Should().Be(null);
            result[0].MayCoInvestmentPayments.Should().Be(null);
            result[0].MayCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].MayEmployerAdditionalPayments.Should().Be(null);
            result[0].MayProviderAdditionalPayments.Should().Be(null);
            result[0].MayApprenticeAdditionalPayments.Should().Be(null);
            result[0].MayEnglishAndMathsPayments.Should().Be(17);
            result[0].MayLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].MayTotalPayments.Should().Be(17);

            // June
            result[0].JuneLevyPayments.Should().Be(null);
            result[0].JuneCoInvestmentPayments.Should().Be(null);
            result[0].JuneCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].JuneEmployerAdditionalPayments.Should().Be(null);
            result[0].JuneProviderAdditionalPayments.Should().Be(null);
            result[0].JuneApprenticeAdditionalPayments.Should().Be(null);
            result[0].JuneEnglishAndMathsPayments.Should().Be(17);
            result[0].JuneLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].JuneTotalPayments.Should().Be(17);

            // July
            result[0].JulyLevyPayments.Should().Be(null);
            result[0].JulyCoInvestmentPayments.Should().Be(null);
            result[0].JulyCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].JulyEmployerAdditionalPayments.Should().Be(null);
            result[0].JulyProviderAdditionalPayments.Should().Be(null);
            result[0].JulyApprenticeAdditionalPayments.Should().Be(null);
            result[0].JulyEnglishAndMathsPayments.Should().Be(17);
            result[0].JulyLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].JulyTotalPayments.Should().Be(17);

            // R13
            result[0].R13LevyPayments.Should().Be(null);
            result[0].R13CoInvestmentPayments.Should().Be(null);
            result[0].R13CoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].R13EmployerAdditionalPayments.Should().Be(null);
            result[0].R13ProviderAdditionalPayments.Should().Be(null);
            result[0].R13ApprenticeAdditionalPayments.Should().Be(null);
            result[0].R13EnglishAndMathsPayments.Should().Be(17);
            result[0].R13LearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].R13TotalPayments.Should().Be(17);

            // R14
            result[0].R14LevyPayments.Should().Be(null);
            result[0].R14CoInvestmentPayments.Should().Be(null);
            result[0].R14CoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].R14EmployerAdditionalPayments.Should().Be(null);
            result[0].R14ProviderAdditionalPayments.Should().Be(null);
            result[0].R14ApprenticeAdditionalPayments.Should().Be(null);
            result[0].R14EnglishAndMathsPayments.Should().Be(17);
            result[0].R14LearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].R14TotalPayments.Should().Be(17);

            result[0].TotalLevyPayments.Should().Be(null);
            result[0].TotalCoInvestmentPayments.Should().Be(null);
            result[0].TotalCoInvestmentDueFromEmployerPayments.Should().Be(null);
            result[0].TotalEmployerAdditionalPayments.Should().Be(null);
            result[0].TotalProviderAdditionalPayments.Should().Be(null);
            result[0].TotalApprenticeAdditionalPayments.Should().Be(null);
            result[0].TotalEnglishAndMathsPayments.Should().Be(238);
            result[0].TotalLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(null);
            result[0].TotalPayments.Should().Be(238);

            // August
            result[1].AugustLevyPayments.Should().Be(22);
            result[1].AugustCoInvestmentPayments.Should().Be(24);
            result[1].AugustCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].AugustEmployerAdditionalPayments.Should().Be(28);
            result[1].AugustProviderAdditionalPayments.Should().Be(30);
            result[1].AugustApprenticeAdditionalPayments.Should().Be(32);
            result[1].AugustEnglishAndMathsPayments.Should().Be(null);
            result[1].AugustLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].AugustTotalPayments.Should().Be(198);

            // September
            result[1].SeptemberLevyPayments.Should().Be(22);
            result[1].SeptemberCoInvestmentPayments.Should().Be(24);
            result[1].SeptemberCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].SeptemberEmployerAdditionalPayments.Should().Be(28);
            result[1].SeptemberProviderAdditionalPayments.Should().Be(30);
            result[1].SeptemberApprenticeAdditionalPayments.Should().Be(32);
            result[1].SeptemberEnglishAndMathsPayments.Should().Be(null);
            result[1].SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].SeptemberTotalPayments.Should().Be(198);

            // October
            result[1].OctoberLevyPayments.Should().Be(22);
            result[1].OctoberCoInvestmentPayments.Should().Be(24);
            result[1].OctoberCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].OctoberEmployerAdditionalPayments.Should().Be(28);
            result[1].OctoberProviderAdditionalPayments.Should().Be(30);
            result[1].OctoberApprenticeAdditionalPayments.Should().Be(32);
            result[1].OctoberEnglishAndMathsPayments.Should().Be(null);
            result[1].OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].OctoberTotalPayments.Should().Be(198);

            // November
            result[1].NovemberLevyPayments.Should().Be(22);
            result[1].NovemberCoInvestmentPayments.Should().Be(24);
            result[1].NovemberCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].NovemberEmployerAdditionalPayments.Should().Be(28);
            result[1].NovemberProviderAdditionalPayments.Should().Be(30);
            result[1].NovemberApprenticeAdditionalPayments.Should().Be(32);
            result[1].NovemberEnglishAndMathsPayments.Should().Be(null);
            result[1].NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].NovemberTotalPayments.Should().Be(198);

            // December
            result[1].DecemberLevyPayments.Should().Be(22);
            result[1].DecemberCoInvestmentPayments.Should().Be(24);
            result[1].DecemberCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].DecemberEmployerAdditionalPayments.Should().Be(28);
            result[1].DecemberProviderAdditionalPayments.Should().Be(30);
            result[1].DecemberApprenticeAdditionalPayments.Should().Be(32);
            result[1].DecemberEnglishAndMathsPayments.Should().Be(null);
            result[1].DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].DecemberTotalPayments.Should().Be(198);

            // January
            result[1].JanuaryLevyPayments.Should().Be(22);
            result[1].JanuaryCoInvestmentPayments.Should().Be(24);
            result[1].JanuaryCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].JanuaryEmployerAdditionalPayments.Should().Be(28);
            result[1].JanuaryProviderAdditionalPayments.Should().Be(30);
            result[1].JanuaryApprenticeAdditionalPayments.Should().Be(32);
            result[1].JanuaryEnglishAndMathsPayments.Should().Be(null);
            result[1].JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].JanuaryTotalPayments.Should().Be(198);

            // February
            result[1].FebruaryLevyPayments.Should().Be(22);
            result[1].FebruaryCoInvestmentPayments.Should().Be(24);
            result[1].FebruaryCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].FebruaryEmployerAdditionalPayments.Should().Be(28);
            result[1].FebruaryProviderAdditionalPayments.Should().Be(30);
            result[1].FebruaryApprenticeAdditionalPayments.Should().Be(32);
            result[1].FebruaryEnglishAndMathsPayments.Should().Be(null);
            result[1].FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].FebruaryTotalPayments.Should().Be(198);

            // March
            result[1].MarchLevyPayments.Should().Be(22);
            result[1].MarchCoInvestmentPayments.Should().Be(24);
            result[1].MarchCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].MarchEmployerAdditionalPayments.Should().Be(28);
            result[1].MarchProviderAdditionalPayments.Should().Be(30);
            result[1].MarchApprenticeAdditionalPayments.Should().Be(32);
            result[1].MarchEnglishAndMathsPayments.Should().Be(null);
            result[1].MarchLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].MarchTotalPayments.Should().Be(198);

            // April
            result[1].AprilLevyPayments.Should().Be(22);
            result[1].AprilCoInvestmentPayments.Should().Be(24);
            result[1].AprilCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].AprilEmployerAdditionalPayments.Should().Be(28);
            result[1].AprilProviderAdditionalPayments.Should().Be(30);
            result[1].AprilApprenticeAdditionalPayments.Should().Be(32);
            result[1].AprilEnglishAndMathsPayments.Should().Be(null);
            result[1].AprilLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].AprilTotalPayments.Should().Be(198);

            // May
            result[1].MayLevyPayments.Should().Be(22);
            result[1].MayCoInvestmentPayments.Should().Be(24);
            result[1].MayCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].MayEmployerAdditionalPayments.Should().Be(28);
            result[1].MayProviderAdditionalPayments.Should().Be(30);
            result[1].MayApprenticeAdditionalPayments.Should().Be(32);
            result[1].MayEnglishAndMathsPayments.Should().Be(null);
            result[1].MayLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].MayTotalPayments.Should().Be(198);

            // June
            result[1].JuneLevyPayments.Should().Be(22);
            result[1].JuneCoInvestmentPayments.Should().Be(24);
            result[1].JuneCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].JuneEmployerAdditionalPayments.Should().Be(28);
            result[1].JuneProviderAdditionalPayments.Should().Be(30);
            result[1].JuneApprenticeAdditionalPayments.Should().Be(32);
            result[1].JuneEnglishAndMathsPayments.Should().Be(null);
            result[1].JuneLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].JuneTotalPayments.Should().Be(198);

            // July
            result[1].JulyLevyPayments.Should().Be(22);
            result[1].JulyCoInvestmentPayments.Should().Be(24);
            result[1].JulyCoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].JulyEmployerAdditionalPayments.Should().Be(28);
            result[1].JulyProviderAdditionalPayments.Should().Be(30);
            result[1].JulyApprenticeAdditionalPayments.Should().Be(32);
            result[1].JulyEnglishAndMathsPayments.Should().Be(null);
            result[1].JulyLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].JulyTotalPayments.Should().Be(198);

            // R13
            result[1].R13LevyPayments.Should().Be(22);
            result[1].R13CoInvestmentPayments.Should().Be(24);
            result[1].R13CoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].R13EmployerAdditionalPayments.Should().Be(28);
            result[1].R13ProviderAdditionalPayments.Should().Be(30);
            result[1].R13ApprenticeAdditionalPayments.Should().Be(32);
            result[1].R13EnglishAndMathsPayments.Should().Be(null);
            result[1].R13LearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].R13TotalPayments.Should().Be(198);

            // R14
            result[1].R14LevyPayments.Should().Be(22);
            result[1].R14CoInvestmentPayments.Should().Be(24);
            result[1].R14CoInvestmentDueFromEmployerPayments.Should().Be(26);
            result[1].R14EmployerAdditionalPayments.Should().Be(28);
            result[1].R14ProviderAdditionalPayments.Should().Be(30);
            result[1].R14ApprenticeAdditionalPayments.Should().Be(32);
            result[1].R14EnglishAndMathsPayments.Should().Be(null);
            result[1].R14LearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(36);
            result[1].R14TotalPayments.Should().Be(198);

            result[1].TotalLevyPayments.Should().Be(308);
            result[1].TotalCoInvestmentPayments.Should().Be(336);
            result[1].TotalCoInvestmentDueFromEmployerPayments.Should().Be(364);
            result[1].TotalEmployerAdditionalPayments.Should().Be(392);
            result[1].TotalProviderAdditionalPayments.Should().Be(420);
            result[1].TotalApprenticeAdditionalPayments.Should().Be(448);
            result[1].TotalEnglishAndMathsPayments.Should().Be(null);
            result[1].TotalLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(504);
            result[1].TotalPayments.Should().Be(2772);
        }

        private List<AppsMonthlyPaymentLarsLearningDeliveryInfo> BuildLarsDeliveryInfoModel()
        {
            return new List<AppsMonthlyPaymentLarsLearningDeliveryInfo>()
            {
                new AppsMonthlyPaymentLarsLearningDeliveryInfo()
                {
                    LearnAimRef = "123456789",
                    LearningAimTitle = "Diploma in Sports Therapy"
                },
                new AppsMonthlyPaymentLarsLearningDeliveryInfo()
                {
                    LearnAimRef = "50117889",
                    LearningAimTitle = "Maths & English"
                },
            };
        }

        private AppsMonthlyPaymentILRInfo BuildILRModel(int ukPrn)
        {
            return new AppsMonthlyPaymentILRInfo()
            {
                UkPrn = ukPrn,
                Learners = new List<AppsMonthlyPaymentLearnerModel>()
                {
                    new AppsMonthlyPaymentLearnerModel()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "A12345",
                        UniqueLearnerNumber = 12345,
                        CampId = "camp101",

                        ProviderSpecLearnerMonitorings = new List<AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo>()
                        {
                            new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                ProvSpecLearnMonOccur = "A",
                                ProvSpecLearnMon = "T180400007"
                            },
                            new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                ProvSpecLearnMonOccur = "B",
                                ProvSpecLearnMon = "150563"
                            }
                        },

                        LearnerEmploymentStatus = new List<AppsMonthlyPaymentLearnerEmploymentStatusInfo>()
                        {
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                // DateEmpStatApp is after the LearnStartDate so this record should not be assigned
                                DateEmpStatApp = new DateTime(2019, 09, 26),
                                EmpStat = 10, // 10 In paid employment
                                EmpdId = 56789,
                                AgreeId = "9876"
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                // this is the one that should be assigned to the AppsMonthlyPaymentReport
                                // as it's the latest status prior to LearnStartDate
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                // DateEmpStatApp must precede the LearningStartDate
                                DateEmpStatApp = new DateTime(2019, 08, 27),
                                EmpStat = 10, // 10 In paid employment
                                EmpdId = 56789,
                                AgreeId = "7755"
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                DateEmpStatApp = new DateTime(2019, 08, 26),
                                EmpStat = 11, // 11 Not in paid employment, looking for work and available to start work
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                DateEmpStatApp = new DateTime(2019, 08, 25),
                                EmpStat = 12, // 12 Not in paid employment, not looking for work and/ or not available to start work
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                DateEmpStatApp = new DateTime(2019, 08, 24),
                                EmpStat = 98, // /98 Not known / not provided
                            }
                        },

                        LearningDeliveries = new List<AppsMonthlyPaymentLearningDeliveryModel>
                        {
                            new AppsMonthlyPaymentLearningDeliveryModel()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                LearnAimRef = "50117889",
                                AimType = 3,
                                AimSeqNumber = 1,
                                LearnStartDate = new DateTime(2019, 08, 28),
                                OrigLearnStartDate = new DateTime(2019, 08, 28),
                                LearnPlanEndDate = new DateTime(2020, 07, 31),
                                FundModel = 36,
                                ProgType = 1,
                                StdCode = 1,
                                FworkCode = 1,
                                PwayCode = 1,
                                PartnerUkprn = 10000001,
                                ConRefNumber = "NLAP-1503",
                                EpaOrgId = "9876543210",
                                SwSupAimId = "SwSup50117889",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2020, 07, 30),
                                Outcome = 4,
                                AchDate = new DateTime(2020, 07, 30),
                                ProviderSpecDeliveryMonitorings =
                                    new List<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo>()
                                    {
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "A12345",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "A000406"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "A12345",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "B002902"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "A12345",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "C004402"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "A12345",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        }
                                    },
                                LearningDeliveryFams = new List<AppsMonthlyPaymentLearningDeliveryFAMInfo>()
                                {
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM1",
                                        LearnDelFAMCode = "001"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM2",
                                        LearnDelFAMCode = "002"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM3",
                                        LearnDelFAMCode = "003"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM4",
                                        LearnDelFAMCode = "004"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM5",
                                        LearnDelFAMCode = "005"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM6",
                                        LearnDelFAMCode = "006"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private AppsMonthlyPaymentRulebaseInfo BuildRulebaseModel(int ukPrn)
        {
            return new AppsMonthlyPaymentRulebaseInfo()
            {
                UkPrn = ukPrn,
                LearnRefNumber = "A12345",
                AecApprenticeshipPriceEpisodeInfoList = new List<AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo>()
                {
                    new AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo()
                    {
                        // This is the one that should be selected
                        Ukprn = ukPrn,
                        LearnRefNumber = "A12345",
                        AimSequenceNumber = 1,
                        PriceEpisodeIdentifier = "123428/08/2019",
                        EpisodeStartDate = new DateTime(2019, 08, 27),
                        PriceEpisodeAgreeId = "PA102",
                        PriceEpisodeActualEndDate = new DateTime(2019, 08, 01),
                        PriceEpisodeActualEndDateIncEPA = new DateTime(2019, 08, 02)
                    },
                    new AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "A12345",
                        AimSequenceNumber = 1,
                        PriceEpisodeIdentifier = "123428/08/2019",
                        EpisodeStartDate = new DateTime(2019, 08, 01),
                        PriceEpisodeAgreeId = "PA101",
                        PriceEpisodeActualEndDate = new DateTime(2019, 08, 13),
                        PriceEpisodeActualEndDateIncEPA = new DateTime(2019, 08, 02),
                    },
                },
                AecLearningDeliveryInfoList = new List<AppsMonthlyPaymentAECLearningDeliveryInfo>()
                {
                    new AppsMonthlyPaymentAECLearningDeliveryInfo()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "A12345",
                        AimSequenceNumber = 1,
                        LearnAimRef = "50117889",
                        PlannedNumOnProgInstalm = 2
                    },
                    new AppsMonthlyPaymentAECLearningDeliveryInfo()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "A12345",
                        AimSequenceNumber = 2,
                        LearnAimRef = "50117889",
                        PlannedNumOnProgInstalm = 3
                    }
                }
            };
        }

        private AppsMonthlyPaymentFcsInfo BuildFcsModel(int ukPrn)
        {
            return new AppsMonthlyPaymentFcsInfo()
            {
                UkPrn = ukPrn,
                Contracts = new List<AppsMonthlyPaymentContractInfo>()
                {
                    new AppsMonthlyPaymentContractInfo()
                    {
                        ContractNumber = "NLAP-1503",
                        ContractVersionNumber = "2",
                        StartDate = new DateTime(2019, 08, 13),
                        EndDate = new DateTime(2020, 7, 31),
                        Provider = new AppsMonthlyPaymentContractorInfo()
                        {
                            UkPrn = ukPrn,
                            OrganisationIdentifier = "Manchester College",
                            LegalName = "Manchester College Ltd",
                        },
                        ContractAllocations = new List<AppsMonthlyPaymentContractAllocation>()
                        {
                            new AppsMonthlyPaymentContractAllocation()
                            {
                                ContractAllocationNumber = "YNLP-1503",
                                FundingStreamPeriodCode = "16-18NLAP2018",
                                FundingStreamCode = "16-18NLA",
                                Period = "1",
                                PeriodTypeCode = "NONLEVY"
                            }
                        }
                    }
                }
            };
        }

        private AppsMonthlyPaymentDasEarningsInfo BuildDasEarningsModel(int ukPrn)
        {
            var appsMonthlyPaymentDasEarningsInfo = new AppsMonthlyPaymentDasEarningsInfo()
            {
                UkPrn = ukPrn
            };

            appsMonthlyPaymentDasEarningsInfo.Earnings = new EditableList<AppsMonthlyPaymentDasEarningEventModel>()
            {
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    EventId = new Guid("BF23F6A8-0B15-42AA-B045-E417E9F0E4C9"),
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    AcademicYear = 1920,
                    CollectionPeriod = 1,
                    LearningAimSequenceNumber = 1
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    // This is the earning that should be selected for the aim seq num
                    EventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    AcademicYear = 1920,
                    CollectionPeriod = 2,
                    LearningAimSequenceNumber = 1
                }
            };

            return appsMonthlyPaymentDasEarningsInfo;
        }

        private AppsMonthlyPaymentDASInfo BuildDasPaymentsModel(int ukPrn)
        {
            var appsMonthlyPaymentDasInfo = new AppsMonthlyPaymentDASInfo()
            {
                UkPrn = ukPrn
            };

            appsMonthlyPaymentDasInfo.Payments = new List<AppsMonthlyPaymentDasPaymentModel>();

            /*
                        -------------------------------------------------------------------------------------------------------------------------------------------
                        *** There should be a new row on the report where the data is different for any of the following fields in the Payments2.Payment table: ***
                        -------------------------------------------------------------------------------------------------------------------------------------------
                            • LearnerReferenceNumber
                            • LearnerUln
                            • LearningAimReference
                            • LearningStartDate
                            • LearningAimProgrammeType
                            • LearningAimStandardCode
                            • LearningAimFrameworkCode
                            • LearningAimPathwayCode
                            • ReportingAimFundingLineType
                            • PriceEpisodeIdentifier(note that only programme aims(LearningAimReference = ZPROG001) have PriceEpisodeIdentifiers; maths and English aims do not)
            */

            // learner 1, first payments
            for (byte i = 1; i < 15; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 11m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearningAimReference = "50117889",
                    LearnerUln = 12345,
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",
                    ContractType = 2,

                    TransactionType = 2,
                    FundingSource = 2,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 12m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",
                    ContractType = 2,

                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 13m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",
                    ContractType = 2,

                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 14m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 15m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 16m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 17m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 18m
                };

                appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            }

            // learner 1, second payments
            for (byte i = 1; i < 15; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 22m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 2,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 24m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 26m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 28m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 30m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 32m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 34m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 36m
                };

                appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            }

            // Learner 2, first payments
            for (byte i = 1; i < 15; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 11m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 12m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",
                    ContractType = 2,

                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 13m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 14m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 15m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 16m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 17m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 18m
                };

                appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            }

            // learner 2, second payments
            for (byte i = 1; i < 15; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 22m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 2,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 24m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 26m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 28m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 30m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 32m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 34m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 36m
                };

                appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            }

            return appsMonthlyPaymentDasInfo;
        }
    }
}