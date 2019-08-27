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
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
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
            string filename = $"10036143_1_Apps Monthly Payment Report {dateTime:yyyyMMdd-HHmmss}";

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10036143);

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IIlrPeriodEndProviderService> IlrPeriodEndProviderServiceMock =
                new Mock<IIlrPeriodEndProviderService>();
            Mock<IDASPaymentsProviderService> dasPaymentProviderMock = new Mock<IDASPaymentsProviderService>();
//            Mock<IDASPaymentsProviderService> dasEarningsProviderMock = new Mock<IDASPaymentsProviderService>();
            Mock<IRulebaseProviderService> fm36ProviderServiceMock = new Mock<IRulebaseProviderService>();
            Mock<ILarsProviderService> larsProviderServiceMock = new Mock<ILarsProviderService>();
            Mock<IFcsProviderService> fcsProviderServiceMock = new Mock<IFcsProviderService>();
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

            //dasEarningsProviderMock
            //    .Setup(x => x.GetEarningsInfoForAppsMonthlyPaymentReportAsync(
            //        It.IsAny<int>(),
            //        It.IsAny<CancellationToken>())).ReturnsAsync(appsMonthlyPaymentDasEarningsInfo);

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
//                dasEarningsProviderMock.Object,
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
            result.Count().Should().Be(1);
            result.First().PaymentLearnerReferenceNumber.Should().Be("A12345");
            result.First().PaymentUniqueLearnerNumber.Should().Be("12345");
            result.First().LearnerCampusIdentifier.Should().Be("camp101");

            // TODO: setup test data and add expected result values here
            //result.First().ProviderSpecifiedLearnerMonitoringA.Should().Be(1);
            //result.First().ProviderSpecifiedLearnerMonitoringB.Should().Be(1);
            //result.First().PaymentEarningEventAimSeqNumber.Should().Be(1);
            //result.First().PaymentLearningAimReference.Should().Be(1);
            //result.First().LarsLearningDeliveryLearningAimTitle.Should().Be(1);
            //result.First().LearningDeliveryOriginalLearningStartDate.Should().Be(1);
            //result.First().PaymentLearningStartDate.Should().Be(1);
            //result.First().LearningDeliveryLearningPlannedEndData.Should().Be(1);
            //result.First().LearningDeliveryCompletionStatus.Should().Be(1);
            //result.First().LearningDeliveryLearningActualEndDate.Should().Be(1);
            //result.First().LearningDeliveryAchievementDate.Should().Be(1);
            //result.First().LearningDeliveryOutcome.Should().Be(1);
            //result.First().PaymentProgrammeType.Should().Be(1);
            //result.First().PaymentStandardCode.Should().Be(1);
            //result.First().PaymentFrameworkCode.Should().Be(1);
            //result.First().PaymentPathwayCode.Should().Be(1);
            //result.First().LearningDeliveryAimType.Should().Be(1);
            //result.First().LearningDeliverySoftwareSupplierAimIdentifier.Should().Be(1);
            //result.First().LearningDeliveryFamTypeLearningDeliveryMonitoringA.Should().Be(1);
            //result.First().LearningDeliveryFamTypeLearningDeliveryMonitoringB.Should().Be(1);
            //result.First().LearningDeliveryFamTypeLearningDeliveryMonitoringC.Should().Be(1);
            //result.First().LearningDeliveryFamTypeLearningDeliveryMonitoringD.Should().Be(1);
            //result.First().LearningDeliveryFamTypeLearningDeliveryMonitoringE.Should().Be(1);
            //result.First().LearningDeliveryFamTypeLearningDeliveryMonitoringF.Should().Be(1);
            //result.First().ProviderSpecifiedDeliveryMonitoringA.Should().Be(1);
            //result.First().ProviderSpecifiedDeliveryMonitoringB.Should().Be(1);
            //result.First().ProviderSpecifiedDeliveryMonitoringC.Should().Be(1);
            //result.First().ProviderSpecifiedDeliveryMonitoringD.Should().Be(1);
            //result.First().LearningDeliveryEndPointAssessmentOrganisation.Should().Be(1);
            //result.First().RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim.Should().Be(1);
            //result.First().LearningDeliverySubContractedOrPartnershipUkprn.Should().Be(1);
            //result.First().PaymentPriceEpisodeStartDate.Should().Be(1);
            //result.First().RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate.Should().Be(1);
            //result.First().FcsContractContractAllocationContractAllocationNumber.Should().Be(1);
            //result.First().PaymentFundingLineType.Should().Be(1);
            //result.First().PaymentApprenticeshipContractType.Should().Be(1);
            //result.First().LearnerEmploymentStatusEmployerId.Should().Be(1);
            //result.First().RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier.Should().Be(1);
            //result.First().LearnerEmploymentStatus.Should().Be(1);
            //result.First().LearnerEmploymentStatusDate.Should().Be(1);

            // Levy payments array
            result.First().LevyPayments[0].Should().Be(45);
            result.First().LevyPayments[1].Should().Be(45);
            result.First().LevyPayments[2].Should().Be(45);
            result.First().LevyPayments[3].Should().Be(45);
            result.First().LevyPayments[4].Should().Be(45);
            result.First().LevyPayments[5].Should().Be(45);
            result.First().LevyPayments[6].Should().Be(45);
            result.First().LevyPayments[7].Should().Be(45);
            result.First().LevyPayments[8].Should().Be(45);
            result.First().LevyPayments[9].Should().Be(45);
            result.First().LevyPayments[10].Should().Be(45);
            result.First().LevyPayments[11].Should().Be(45);
            result.First().LevyPayments[12].Should().Be(45);
            result.First().LevyPayments[13].Should().Be(45);

            // CoInvestment payments array
            result.First().CoInvestmentPayments[0].Should().Be(36);
            result.First().CoInvestmentPayments[1].Should().Be(36);
            result.First().CoInvestmentPayments[2].Should().Be(36);
            result.First().CoInvestmentPayments[3].Should().Be(36);
            result.First().CoInvestmentPayments[4].Should().Be(36);
            result.First().CoInvestmentPayments[5].Should().Be(36);
            result.First().CoInvestmentPayments[6].Should().Be(36);
            result.First().CoInvestmentPayments[7].Should().Be(36);
            result.First().CoInvestmentPayments[8].Should().Be(36);
            result.First().CoInvestmentPayments[9].Should().Be(36);
            result.First().CoInvestmentPayments[10].Should().Be(36);
            result.First().CoInvestmentPayments[11].Should().Be(36);
            result.First().CoInvestmentPayments[12].Should().Be(36);
            result.First().CoInvestmentPayments[13].Should().Be(36);

            // CoInvestment due from employer array
            result.First().CoInvestmentDueFromEmployerPayments[0].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[1].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[2].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[3].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[4].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[5].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[6].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[7].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[8].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[9].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[10].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[11].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[12].Should().Be(39);
            result.First().CoInvestmentDueFromEmployerPayments[13].Should().Be(39);

            // Employer additional payments array
            result.First().EmployerAdditionalPayments[0].Should().Be(42);
            result.First().EmployerAdditionalPayments[1].Should().Be(42);
            result.First().EmployerAdditionalPayments[2].Should().Be(42);
            result.First().EmployerAdditionalPayments[3].Should().Be(42);
            result.First().EmployerAdditionalPayments[4].Should().Be(42);
            result.First().EmployerAdditionalPayments[5].Should().Be(42);
            result.First().EmployerAdditionalPayments[6].Should().Be(42);
            result.First().EmployerAdditionalPayments[7].Should().Be(42);
            result.First().EmployerAdditionalPayments[8].Should().Be(42);
            result.First().EmployerAdditionalPayments[9].Should().Be(42);
            result.First().EmployerAdditionalPayments[10].Should().Be(42);
            result.First().EmployerAdditionalPayments[11].Should().Be(42);
            result.First().EmployerAdditionalPayments[12].Should().Be(42);
            result.First().EmployerAdditionalPayments[13].Should().Be(42);

            // Provider additional payments array
            result.First().ProviderAdditionalPayments[0].Should().Be(45);
            result.First().ProviderAdditionalPayments[1].Should().Be(45);
            result.First().ProviderAdditionalPayments[2].Should().Be(45);
            result.First().ProviderAdditionalPayments[3].Should().Be(45);
            result.First().ProviderAdditionalPayments[4].Should().Be(45);
            result.First().ProviderAdditionalPayments[5].Should().Be(45);
            result.First().ProviderAdditionalPayments[6].Should().Be(45);
            result.First().ProviderAdditionalPayments[7].Should().Be(45);
            result.First().ProviderAdditionalPayments[8].Should().Be(45);
            result.First().ProviderAdditionalPayments[9].Should().Be(45);
            result.First().ProviderAdditionalPayments[10].Should().Be(45);
            result.First().ProviderAdditionalPayments[11].Should().Be(45);
            result.First().ProviderAdditionalPayments[12].Should().Be(45);
            result.First().ProviderAdditionalPayments[13].Should().Be(45);

            // Apprenticeship additional payments array
            result.First().ApprenticeAdditionalPayments[0].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[1].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[2].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[3].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[4].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[5].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[6].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[7].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[8].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[9].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[10].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[11].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[12].Should().Be(48);
            result.First().ApprenticeAdditionalPayments[13].Should().Be(48);

            // English and Maths payments array
            result.First().EnglishAndMathsPayments[0].Should().Be(551);
            result.First().EnglishAndMathsPayments[1].Should().Be(51);
            result.First().EnglishAndMathsPayments[2].Should().Be(51);
            result.First().EnglishAndMathsPayments[3].Should().Be(51);
            result.First().EnglishAndMathsPayments[4].Should().Be(51);
            result.First().EnglishAndMathsPayments[5].Should().Be(51);
            result.First().EnglishAndMathsPayments[6].Should().Be(51);
            result.First().EnglishAndMathsPayments[7].Should().Be(51);
            result.First().EnglishAndMathsPayments[8].Should().Be(51);
            result.First().EnglishAndMathsPayments[9].Should().Be(51);
            result.First().EnglishAndMathsPayments[10].Should().Be(51);
            result.First().EnglishAndMathsPayments[11].Should().Be(51);
            result.First().EnglishAndMathsPayments[12].Should().Be(51);
            result.First().EnglishAndMathsPayments[13].Should().Be(51);

            // Learning support, disadvantage and framework uplift payments array
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[0].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[1].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[2].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[3].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[4].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[5].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[6].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[7].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[8].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[9].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[10].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[11].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[12].Should().Be(54);
            result.First().LearningSupportDisadvantageAndFrameworkUpliftPayments[13].Should().Be(54);

            result.First().AugustLevyPayments.Should().Be(45);
            result.First().AugustCoInvestmentPayments.Should().Be(36);
            result.First().AugustCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().AugustEmployerAdditionalPayments.Should().Be(42);
            result.First().AugustProviderAdditionalPayments.Should().Be(45);
            result.First().AugustApprenticeAdditionalPayments.Should().Be(48);
            result.First().AugustEnglishAndMathsPayments.Should().Be(51);
            result.First().AugustLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().AugustTotalPayments.Should().Be(1);

            result.First().SeptemberLevyPayments.Should().Be(45);
            result.First().SeptemberCoInvestmentPayments.Should().Be(36);
            result.First().SeptemberCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().SeptemberEmployerAdditionalPayments.Should().Be(42);
            result.First().SeptemberProviderAdditionalPayments.Should().Be(45);
            result.First().SeptemberApprenticeAdditionalPayments.Should().Be(48);
            result.First().SeptemberEnglishAndMathsPayments.Should().Be(51);
            result.First().SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().SeptemberTotalPayments.Should().Be(1);

            result.First().OctoberLevyPayments.Should().Be(45);
            result.First().OctoberCoInvestmentPayments.Should().Be(36);
            result.First().OctoberCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().OctoberEmployerAdditionalPayments.Should().Be(42);
            result.First().OctoberProviderAdditionalPayments.Should().Be(45);
            result.First().OctoberApprenticeAdditionalPayments.Should().Be(48);
            result.First().OctoberEnglishAndMathsPayments.Should().Be(51);
            result.First().OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().OctoberTotalPayments.Should().Be(1);

            result.First().NovemberLevyPayments.Should().Be(45);
            result.First().NovemberCoInvestmentPayments.Should().Be(36);
            result.First().NovemberCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().NovemberEmployerAdditionalPayments.Should().Be(42);
            result.First().NovemberProviderAdditionalPayments.Should().Be(45);
            result.First().NovemberApprenticeAdditionalPayments.Should().Be(48);
            result.First().NovemberEnglishAndMathsPayments.Should().Be(51);
            result.First().NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().NovemberTotalPayments.Should().Be(1);

            result.First().DecemberLevyPayments.Should().Be(45);
            result.First().DecemberCoInvestmentPayments.Should().Be(36);
            result.First().DecemberCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().DecemberEmployerAdditionalPayments.Should().Be(42);
            result.First().DecemberProviderAdditionalPayments.Should().Be(45);
            result.First().DecemberApprenticeAdditionalPayments.Should().Be(48);
            result.First().DecemberEnglishAndMathsPayments.Should().Be(51);
            result.First().DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().DecemberTotalPayments.Should().Be(1);

            result.First().JanuaryLevyPayments.Should().Be(45);
            result.First().JanuaryCoInvestmentPayments.Should().Be(36);
            result.First().JanuaryCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().JanuaryEmployerAdditionalPayments.Should().Be(42);
            result.First().JanuaryProviderAdditionalPayments.Should().Be(45);
            result.First().JanuaryApprenticeAdditionalPayments.Should().Be(48);
            result.First().JanuaryEnglishAndMathsPayments.Should().Be(51);
            result.First().JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().JanuaryTotalPayments.Should().Be(1);

            result.First().FebruaryLevyPayments.Should().Be(45);
            result.First().FebruaryCoInvestmentPayments.Should().Be(36);
            result.First().FebruaryCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().FebruaryEmployerAdditionalPayments.Should().Be(42);
            result.First().FebruaryProviderAdditionalPayments.Should().Be(45);
            result.First().FebruaryApprenticeAdditionalPayments.Should().Be(48);
            result.First().FebruaryEnglishAndMathsPayments.Should().Be(51);
            result.First().FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().FebruaryTotalPayments.Should().Be(1);

            result.First().MarchLevyPayments.Should().Be(45);
            result.First().MarchCoInvestmentPayments.Should().Be(36);
            result.First().MarchCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().MarchEmployerAdditionalPayments.Should().Be(42);
            result.First().MarchProviderAdditionalPayments.Should().Be(45);
            result.First().MarchApprenticeAdditionalPayments.Should().Be(48);
            result.First().MarchEnglishAndMathsPayments.Should().Be(51);
            result.First().MarchLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().MarchTotalPayments.Should().Be(1);

            result.First().AprilLevyPayments.Should().Be(45);
            result.First().AprilCoInvestmentPayments.Should().Be(36);
            result.First().AprilCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().AprilEmployerAdditionalPayments.Should().Be(42);
            result.First().AprilProviderAdditionalPayments.Should().Be(45);
            result.First().AprilApprenticeAdditionalPayments.Should().Be(48);
            result.First().AprilEnglishAndMathsPayments.Should().Be(51);
            result.First().AprilLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().AprilTotalPayments.Should().Be(1);

            result.First().MayLevyPayments.Should().Be(45);
            result.First().MayCoInvestmentPayments.Should().Be(36);
            result.First().MayCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().MayEmployerAdditionalPayments.Should().Be(42);
            result.First().MayProviderAdditionalPayments.Should().Be(45);
            result.First().MayApprenticeAdditionalPayments.Should().Be(48);
            result.First().MayEnglishAndMathsPayments.Should().Be(51);
            result.First().MayLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().MayTotalPayments.Should().Be(1);

            result.First().JuneLevyPayments.Should().Be(45);
            result.First().JuneCoInvestmentPayments.Should().Be(36);
            result.First().JuneCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().JuneEmployerAdditionalPayments.Should().Be(42);
            result.First().JuneProviderAdditionalPayments.Should().Be(45);
            result.First().JuneApprenticeAdditionalPayments.Should().Be(48);
            result.First().JuneEnglishAndMathsPayments.Should().Be(51);
            result.First().JuneLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().JuneTotalPayments.Should().Be(1);

            result.First().JulyLevyPayments.Should().Be(45);
            result.First().JulyCoInvestmentPayments.Should().Be(36);
            result.First().JulyCoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().JulyEmployerAdditionalPayments.Should().Be(42);
            result.First().JulyProviderAdditionalPayments.Should().Be(45);
            result.First().JulyApprenticeAdditionalPayments.Should().Be(48);
            result.First().JulyEnglishAndMathsPayments.Should().Be(51);
            result.First().JulyLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().JulyTotalPayments.Should().Be(1);

            result.First().R13LevyPayments.Should().Be(45);
            result.First().R13CoInvestmentPayments.Should().Be(36);
            result.First().R13CoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().R13EmployerAdditionalPayments.Should().Be(42);
            result.First().R13ProviderAdditionalPayments.Should().Be(45);
            result.First().R13ApprenticeAdditionalPayments.Should().Be(48);
            result.First().R13EnglishAndMathsPayments.Should().Be(51);
            result.First().R13LearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().R13TotalPayments.Should().Be(1);

            result.First().R14LevyPayments.Should().Be(45);
            result.First().R14CoInvestmentPayments.Should().Be(36);
            result.First().R14CoInvestmentDueFromEmployerPayments.Should().Be(39);
            result.First().R14EmployerAdditionalPayments.Should().Be(42);
            result.First().R14ProviderAdditionalPayments.Should().Be(45);
            result.First().R14ApprenticeAdditionalPayments.Should().Be(48);
            result.First().R14EnglishAndMathsPayments.Should().Be(51);
            result.First().R14LearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(54);
            result.First().R14TotalPayments.Should().Be(1);

            result.First().TotalLevyPayments.Should().Be(1);
            result.First().TotalCoInvestmentPayments.Should().Be(1);
            result.First().TotalCoInvestmentDueFromEmployerPayments.Should().Be(1);
            result.First().TotalEmployerAdditionalPayments.Should().Be(1);
            result.First().TotalProviderAdditionalPayments.Should().Be(1);
            result.First().TotalApprenticeAdditionalPayments.Should().Be(1);
            result.First().TotalEnglishAndMathsPayments.Should().Be(1);
            result.First().TotalLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(1);
            result.First().TotalPayments.Should().Be(1);
            result.First().OfficialSensitive.Should().Be(1);
/*
            result.First().LearningAimReference.Should().Be("50117889");
            result.First().LearningStartDate.Should().Be("28/08/2019");
            result.First().LearningAimProgrammeType.Should().Be(1);
            result.First().LearningAimStandardCode.Should().Be(1);
            result.First().LearningAimFrameworkCode.Should().Be(1);
            result.First().LearningAimPathwayCode.Should().Be(1);
            result.First().AimType.Should().Be(3);
            result.First().FundingLineType.Should().Be("16-18 Apprenticeship Non-Levy");
            result.First().PaymentApprenticeshipContractType.Should().Be(2);
            result.First().AugustLevyPayments.Should().Be(11);
            result.First().AugustCoInvestmentPayments.Should().Be(12);
            result.First().AugustTotalPayments.Should().Be(116);
            result.First().TotalLevyPayments.Should().Be(143);
            result.First().TotalCoInvestmentPayments.Should().Be(156);
            result.First().TotalCoInvestmentDueFromEmployerPayments.Should().Be(169);
            result.First().TotalEmployerAdditionalPayments.Should().Be(182);
            result.First().TotalProviderAdditionalPayments.Should().Be(195);
            result.First().TotalApprenticeAdditionalPayments.Should().Be(208);
            result.First().TotalEnglishAndMathsPayments.Should().Be(221);
            result.First().TotalPaymentsForLearningSupportDisadvantageAndFrameworkUplifts.Should().Be(234);
            result.First().TotalPayments.Should().Be(1508);
            result.First().LearningAimTitle.Should().Be("Maths & English");
*/
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
                Learners = new List<AppsMonthlyPaymentLearnerInfo>()
                {
                    new AppsMonthlyPaymentLearnerInfo()
                    {
                        Ukprn = ukPrn.ToString(),
                        LearnRefNumber = "A12345",
                        UniqueLearnerNumber = "12345",
                        CampId = "camp101",

                        ProviderSpecLearnerMonitorings = new List<AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo>()
                        {
                            new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                            {
                                Ukprn = ukPrn.ToString(),
                                LearnRefNumber = "1",
                                ProvSpecLearnMon = "A",
                                ProvSpecLearnMonOccur = "T180400007"
                            },
                            new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                            {
                                Ukprn = ukPrn.ToString(),
                                LearnRefNumber = "1",
                                ProvSpecLearnMon = "B",
                                ProvSpecLearnMonOccur = "150563"
                            }
                        },

                        LearnerEmploymentStatus = new List<AppsMonthlyPaymentLearnerEmploymentStatusInfo>()
                        {
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn.ToString(),
                                LearnRefNumber = "A12345",
                                // DateEmpStatApp is after the LearnStartDate so this record should not be assigned
                                DateEmpStatApp = new DateTime(2019, 09, 26),
                                EmpStat = "10",         // 10 In paid employment
                                EmpdId = "56789",
                                AgreeId = "9876"
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                // this is the one that should be assigned to the AppsMonthlyPaymentReport
                                // as it's the latest status prior to LearnStartDate
                                Ukprn = ukPrn.ToString(),
                                LearnRefNumber = "A12345",
                                // DateEmpStatApp must precede the LearningStartDate
                                DateEmpStatApp = new DateTime(2019, 08, 27),
                                EmpStat = "10",         // 10 In paid employment
                                EmpdId = "56789",
                                AgreeId = "7755"
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn.ToString(),
                                LearnRefNumber = "A12345",
                                DateEmpStatApp = new DateTime(2019, 08, 26),
                                EmpStat = "11",          // 11 Not in paid employment, looking for work and available to start work
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn.ToString(),
                                LearnRefNumber = "A12345",
                                DateEmpStatApp = new DateTime(2019, 08, 25),
                                EmpStat = "12",         // 12 Not in paid employment, not looking for work and/ or not available to start work
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn.ToString(),
                                LearnRefNumber = "A12345",
                                DateEmpStatApp = new DateTime(2019, 08, 24),
                                EmpStat = "98",         // /98 Not known / not provided
                            }
                        },

                        LearningDeliveries = new List<AppsMonthlyPaymentLearningDeliveryInfo>
                        {
                            new AppsMonthlyPaymentLearningDeliveryInfo()
                            {
                                Ukprn = ukPrn.ToString(),
                                LearnRefNumber = "A12345",
                                LearnAimRef = "50117889",
                                AimType = "3",
                                AimSeqNumber = "1",
                                LearnStartDate = new DateTime(2019, 08, 28),
                                OrigLearnStartDate = null,
                                LearnPlanEndDate = "2019-07-31",
                                FundModel = "36",
                                ProgType = "1",
                                StdCode = "1",
                                FworkCode = "1",
                                PwayCode = "1",
                                PartnerUkprn = null,
                                ConRefNumber = "NLAP-1503",
                                EpaOrgId = "9876543210",
                                SwSupAimId = "SwSup50117889",
                                CompStatus = "2",
                                LearnActEndDate = "2019-07-31",
                                Outcome = "4",
                                AchDate = "2019-07-30",
                                ProviderSpecDeliveryMonitorings = new List<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo>()
                                {
                                    new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                    {
                                        Ukprn = ukPrn.ToString(),
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = "1",
                                        ProvSpecDelMonOccur = "A",
                                        ProvSpecDelMon = "A000406"
                                    },
                                    new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                    {
                                        Ukprn = ukPrn.ToString(),
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = "1",
                                        ProvSpecDelMonOccur = "B",
                                        ProvSpecDelMon = "B002902"
                                    },
                                    new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                    {
                                        Ukprn = ukPrn.ToString(),
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = "1",
                                        ProvSpecDelMonOccur = "C",
                                        ProvSpecDelMon = "C004402"
                                    },
                                    new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                    {
                                        Ukprn = ukPrn.ToString(),
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = "1",
                                        ProvSpecDelMonOccur = "D",
                                        ProvSpecDelMon = "D006801"
                                    }
                                },
                                LearningDeliveryFams = new List<AppsMonthlyPaymentLearningDeliveryFAMInfo>()
                                {
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn.ToString(),
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = "1",
                                        LearnDelFAMType = "LDM1",
                                        LearnDelFAMCode = "001"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn.ToString(),
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = "1",
                                        LearnDelFAMType = "LDM2",
                                        LearnDelFAMCode = "002"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn.ToString(),
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = "1",
                                        LearnDelFAMType = "LDM3",
                                        LearnDelFAMCode = "003"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn.ToString(),
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = "1",
                                        LearnDelFAMType = "LDM4",
                                        LearnDelFAMCode = "004"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn.ToString(),
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = "1",
                                        LearnDelFAMType = "LDM5",
                                        LearnDelFAMCode = "005"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn.ToString(),
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = "1",
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
                AECApprenticeshipPriceEpisodes = new List<AECApprenticeshipPriceEpisodeInfo>()
                {
                    new AECApprenticeshipPriceEpisodeInfo()
                    {
                        Ukprn = ukPrn.ToString(),
                        LearnRefNumber = "A12345",
                        AimSequenceNumber = "1",
                        EpisodeStartDate = new DateTime(2019, 08,  27),
                        PriceEpisodeAgreeId = "PA102",
                        PriceEpisodeActualEndDate = new DateTime(2019, 08, 01),
                        PriceEpisodeActualEndDateIncEPA = new DateTime(2019, 08, 02),
                    },                    new AECApprenticeshipPriceEpisodeInfo()
                    {
                        Ukprn = ukPrn.ToString(),
                        LearnRefNumber = "A12345",
                        AimSequenceNumber = "1",
                        EpisodeStartDate = new DateTime(2019, 08,  01),
                        PriceEpisodeAgreeId = "PA101",
                        PriceEpisodeActualEndDate = new DateTime(2019, 08, 13),
                        PriceEpisodeActualEndDateIncEPA = new DateTime(2019, 08, 02),
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
                        StartDate = "2019-08-13", //new DateTime(2019, 08, 13),
                        EndDate = "2019-07-31", // new DateTime(2020, 7, 31),
                        Provider = new AppsMonthlyPaymentContractorInfo()
                        {
                            UkPrn = ukPrn.ToString(),
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

            appsMonthlyPaymentDasEarningsInfo.Earnings = new EditableList<AppsMonthlyPaymentDasEarningEventInfo>()
            {
                new AppsMonthlyPaymentDasEarningEventInfo()
                {
                    EventId = new Guid("BF23F6A8-0B15-42AA-B045-E417E9F0E4C9"),
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    AcademicYear = 1920,
                    CollectionPeriod = 1,
                    LearningAimSequenceNumber = "1"
                },
                new AppsMonthlyPaymentDasEarningEventInfo()
                {
                    // This is the earning that should be selected for the aim seq num
                    EventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    AcademicYear = 1920,
                    CollectionPeriod = 2,
                    LearningAimSequenceNumber = "1"
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

            appsMonthlyPaymentDasInfo.Payments = new List<AppsMonthlyPaymentDasPayments2Payment>();

            /*
                        ------------------------------------------------------------------------------------------------------------------------------------------
                            ***There should be a new row on the report where the data is different for any of the following fields in the Payments2.Payment table:***
                                ------------------------------------------------------------------------------------------------------------------------------------------
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
            for (byte i = 1; i < 14; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 11m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearningAimReference = "50117889",
                    LearnerUln = "12345",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",
                    ContractType = "2",

                    TransactionType = 2,
                    FundingSource = 2,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 12m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",
                    ContractType = "2",

                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 13m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",
                    ContractType = "2",

                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 14m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 15m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 16m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 17m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
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
            for (byte i = 1; i < 14; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 22m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 2,
                    FundingSource = 2,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 24m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 26m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 28m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 30m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 32m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 34m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = "12345",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
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
            for (byte i = 1; i < 14; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 11m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 12m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",
                    ContractType = "2",

                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 13m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 14m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 15m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 16m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 17m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
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
            for (byte i = 1; i < 14; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 22m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 2,
                    FundingSource = 2,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 24m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 26m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 28m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 30m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 32m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
                    CollectionPeriod = i,

                    Amount = 34m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPayments2Payment()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn.ToString(),
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = "54321",
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = "1",
                    LearningAimStandardCode = "1",
                    LearningAimFrameworkCode = "1",
                    LearningAimPathwayCode = "1",
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "12342019-08-28",

                    ContractType = "2",
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = "1",
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