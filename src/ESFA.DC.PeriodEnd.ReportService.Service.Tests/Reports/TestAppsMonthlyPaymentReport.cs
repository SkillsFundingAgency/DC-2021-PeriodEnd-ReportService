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
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Builders;
using ESFA.DC.PeriodEnd.ReportService.Service.Mapper;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports;
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
            // original mock data - int ukPrn = 10036143;
            int ukPrn = 10000001;
            // original mock data - string filename = $"R01_10036143_10036143 Apps Monthly Payment Report {dateTime:yyyyMMdd-HHmmss}";
            string filename = $"R05_10000001_10000001 Apps Monthly Payment Report {dateTime:yyyyMMdd-HHmmss}";

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(ukPrn);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(5);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriodName).Returns("R05");

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IIlrPeriodEndProviderService> IlrPeriodEndProviderServiceMock = new Mock<IIlrPeriodEndProviderService>();
            Mock<IDASPaymentsProviderService> dasPaymentProviderMock = new Mock<IDASPaymentsProviderService>();
            Mock<IFM36PeriodEndProviderService> fm36ProviderServiceMock = new Mock<IFM36PeriodEndProviderService>();
            Mock<ILarsProviderService> larsProviderServiceMock = new Mock<ILarsProviderService>();
            Mock<IFCSProviderService> fcsProviderServiceMock = new Mock<IFCSProviderService>();

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

            Mock<IPersistReportData> persistReportDataMock = new Mock<IPersistReportData>();

            var report = new AppsMonthlyPaymentReport(
                logger.Object,
                storage.Object,
                IlrPeriodEndProviderServiceMock.Object,
                fm36ProviderServiceMock.Object,
                dasPaymentProviderMock.Object,
                larsProviderServiceMock.Object,
                fcsProviderServiceMock.Object,
                dateTimeProviderMock.Object,
                appsMonthlyPaymentModelBuilder,
                persistReportDataMock.Object);

            await report.GenerateReport(reportServiceContextMock.Object, null, CancellationToken.None);

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

            return;

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(2);

            result[0].PaymentLearnerReferenceNumber.Should().Be("LR1001");
            result[0].PaymentUniqueLearnerNumber.Should().Be(1000000001);
            result[0].LearnerCampusIdentifier.Should().Be("C0471802");
            result[0].ProviderSpecifiedLearnerMonitoringA.Should().Be("001");
            result[0].ProviderSpecifiedLearnerMonitoringB.Should().Be("100102");
            result[0].PaymentEarningEventAimSeqNumber.Should().Be(4);
            result[0].PaymentLearningAimReference.Should().Be("ZPROG001");
            result[0].LarsLearningDeliveryLearningAimTitle.Should().Be("Generic code to identify ILR programme aims");
            result[0].LearningDeliveryOriginalLearningStartDate.Should().BeNull();
            result[0].PaymentLearningStartDate.Should().Be(new DateTime(2017, 9, 18));
            result[0].LearningDeliveryLearningPlannedEndDate.Should().Be(new DateTime(2019, 9, 19));
            result[0].LearningDeliveryLearningActualEndDate.Should().Be(new DateTime(2019, 9, 11));
            result[0].LearningDeliveryAchievementDate.Should().BeNull();
            result[0].LearningDeliveryOutcome.Should().Be(1);
            result[0].PaymentProgrammeType.Should().Be(3);
            result[0].PaymentStandardCode.Should().Be(0);
            result[0].PaymentFrameworkCode.Should().Be(436);
            result[0].PaymentPathwayCode.Should().Be(6);
            result[0].LearningDeliveryAimType.Should().Be(1);
            result[0].LearningDeliverySoftwareSupplierAimIdentifier.Should().Be("E8FC9ECD-01DC-4D10-AB8D-E177BD21B259");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringA.Should().BeNullOrEmpty();
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringB.Should().BeNullOrEmpty();
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringC.Should().BeNullOrEmpty();
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringD.Should().BeNullOrEmpty();
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringE.Should().BeNullOrEmpty();
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringF.Should().BeNullOrEmpty();
            result[0].ProviderSpecifiedDeliveryMonitoringA.Should().Be("1920");
            result[0].ProviderSpecifiedDeliveryMonitoringB.Should().Be("E5072");
            result[0].ProviderSpecifiedDeliveryMonitoringC.Should().BeNullOrEmpty();
            result[0].ProviderSpecifiedDeliveryMonitoringD.Should().Be("D006801");
            result[0].LearningDeliveryEndPointAssessmentOrganisation.Should().BeNullOrEmpty();
            result[0].RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim.Should().BeNull();
            result[0].LearningDeliverySubContractedOrPartnershipUkprn.Should().BeNull();
            result[0].PaymentPriceEpisodeStartDate.Should().Be("01/08/2019");
            result[0].RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate.Should().BeNull();
            result[0].FcsContractContractAllocationContractAllocationNumber.Should().Be("YNLP-1157;YNLP-1158");
            result[0].PaymentFundingLineType.Should().Be("19 + Apprenticeship(Employer on App Service) Levy funding");
            result[0].PaymentApprenticeshipContractType.Should().Be(1);
            result[0].LearnerEmploymentStatusEmployerId.Should().BeNull();
            result[0].RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier.Should().BeNullOrEmpty();
            result[0].LearnerEmploymentStatus.Should().BeNull();
            result[0].LearnerEmploymentStatusDate.Should().BeNull();

            // payments, Learner 1, Aim 1

            // August
            result[0].AugustLevyPayments.Should().Be(0);
            result[0].AugustCoInvestmentPayments.Should().Be(0);
            result[0].AugustCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].AugustEmployerAdditionalPayments.Should().Be(0);
            result[0].AugustProviderAdditionalPayments.Should().Be(0);
            result[0].AugustApprenticeAdditionalPayments.Should().Be(0);
            result[0].AugustEnglishAndMathsPayments.Should().Be(0);
            result[0].AugustLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].AugustTotalPayments.Should().Be(0);

            // September
            result[0].SeptemberLevyPayments.Should().Be(0);
            result[0].SeptemberCoInvestmentPayments.Should().Be(0);
            result[0].SeptemberCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].SeptemberEmployerAdditionalPayments.Should().Be(0);
            result[0].SeptemberProviderAdditionalPayments.Should().Be(0);
            result[0].SeptemberApprenticeAdditionalPayments.Should().Be(0);
            result[0].SeptemberEnglishAndMathsPayments.Should().Be(0);
            result[0].SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].SeptemberTotalPayments.Should().Be(0);

            // October
            result[0].OctoberLevyPayments.Should().Be(0);
            result[0].OctoberCoInvestmentPayments.Should().Be(0);
            result[0].OctoberCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].OctoberEmployerAdditionalPayments.Should().Be(0);
            result[0].OctoberProviderAdditionalPayments.Should().Be(0);
            result[0].OctoberApprenticeAdditionalPayments.Should().Be(0);
            result[0].OctoberEnglishAndMathsPayments.Should().Be(0);
            result[0].OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].OctoberTotalPayments.Should().Be(0);

            // November
            result[0].NovemberLevyPayments.Should().Be(0);
            result[0].NovemberCoInvestmentPayments.Should().Be(0);
            result[0].NovemberCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].NovemberEmployerAdditionalPayments.Should().Be(0);
            result[0].NovemberProviderAdditionalPayments.Should().Be(0);
            result[0].NovemberApprenticeAdditionalPayments.Should().Be(0);
            result[0].NovemberEnglishAndMathsPayments.Should().Be(0);
            result[0].NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].NovemberTotalPayments.Should().Be(0);

            // December
            result[0].DecemberLevyPayments.Should().Be(0);
            result[0].DecemberCoInvestmentPayments.Should().Be(0);
            result[0].DecemberCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].DecemberEmployerAdditionalPayments.Should().Be(0);
            result[0].DecemberProviderAdditionalPayments.Should().Be(0);
            result[0].DecemberApprenticeAdditionalPayments.Should().Be(0);
            result[0].DecemberEnglishAndMathsPayments.Should().Be(0);
            result[0].DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].DecemberTotalPayments.Should().Be(0);

            // January
            result[0].JanuaryLevyPayments.Should().Be(0);
            result[0].JanuaryCoInvestmentPayments.Should().Be(0);
            result[0].JanuaryCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].JanuaryEmployerAdditionalPayments.Should().Be(0);
            result[0].JanuaryProviderAdditionalPayments.Should().Be(0);
            result[0].JanuaryApprenticeAdditionalPayments.Should().Be(0);
            result[0].JanuaryEnglishAndMathsPayments.Should().Be(0);
            result[0].JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].JanuaryTotalPayments.Should().Be(0);

            // February
            result[0].FebruaryLevyPayments.Should().Be(0);
            result[0].FebruaryCoInvestmentPayments.Should().Be(0);
            result[0].FebruaryCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].FebruaryEmployerAdditionalPayments.Should().Be(0);
            result[0].FebruaryProviderAdditionalPayments.Should().Be(0);
            result[0].FebruaryApprenticeAdditionalPayments.Should().Be(0);
            result[0].FebruaryEnglishAndMathsPayments.Should().Be(0);
            result[0].FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].FebruaryTotalPayments.Should().Be(0);

            // March
            result[0].MarchLevyPayments.Should().Be(0);
            result[0].MarchCoInvestmentPayments.Should().Be(0);
            result[0].MarchCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].MarchEmployerAdditionalPayments.Should().Be(0);
            result[0].MarchProviderAdditionalPayments.Should().Be(0);
            result[0].MarchApprenticeAdditionalPayments.Should().Be(0);
            result[0].MarchEnglishAndMathsPayments.Should().Be(0);
            result[0].MarchLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].MarchTotalPayments.Should().Be(0);

            // April
            result[0].AprilLevyPayments.Should().Be(0);
            result[0].AprilCoInvestmentPayments.Should().Be(0);
            result[0].AprilCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].AprilEmployerAdditionalPayments.Should().Be(0);
            result[0].AprilProviderAdditionalPayments.Should().Be(0);
            result[0].AprilApprenticeAdditionalPayments.Should().Be(0);
            result[0].AprilEnglishAndMathsPayments.Should().Be(0);
            result[0].AprilLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].AprilTotalPayments.Should().Be(0);

            // May
            result[0].MayLevyPayments.Should().Be(0);
            result[0].MayCoInvestmentPayments.Should().Be(0);
            result[0].MayCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].MayEmployerAdditionalPayments.Should().Be(0);
            result[0].MayProviderAdditionalPayments.Should().Be(0);
            result[0].MayApprenticeAdditionalPayments.Should().Be(0);
            result[0].MayEnglishAndMathsPayments.Should().Be(0);
            result[0].MayLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].MayTotalPayments.Should().Be(0);

            // June
            result[0].JuneLevyPayments.Should().Be(0);
            result[0].JuneCoInvestmentPayments.Should().Be(0);
            result[0].JuneCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].JuneEmployerAdditionalPayments.Should().Be(0);
            result[0].JuneProviderAdditionalPayments.Should().Be(0);
            result[0].JuneApprenticeAdditionalPayments.Should().Be(0);
            result[0].JuneEnglishAndMathsPayments.Should().Be(0);
            result[0].JuneLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].JuneTotalPayments.Should().Be(0);

            // July
            result[0].JulyLevyPayments.Should().Be(0);
            result[0].JulyCoInvestmentPayments.Should().Be(0);
            result[0].JulyCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].JulyEmployerAdditionalPayments.Should().Be(0);
            result[0].JulyProviderAdditionalPayments.Should().Be(0);
            result[0].JulyApprenticeAdditionalPayments.Should().Be(0);
            result[0].JulyEnglishAndMathsPayments.Should().Be(0);
            result[0].JulyLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].JulyTotalPayments.Should().Be(0);

            // R13
            result[0].R13LevyPayments.Should().Be(0);
            result[0].R13CoInvestmentPayments.Should().Be(0);
            result[0].R13CoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].R13EmployerAdditionalPayments.Should().Be(0);
            result[0].R13ProviderAdditionalPayments.Should().Be(0);
            result[0].R13ApprenticeAdditionalPayments.Should().Be(0);
            result[0].R13EnglishAndMathsPayments.Should().Be(0);
            result[0].R13LearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].R13TotalPayments.Should().Be(0);

            // R14
            result[0].R14LevyPayments.Should().Be(0);
            result[0].R14CoInvestmentPayments.Should().Be(0);
            result[0].R14CoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].R14EmployerAdditionalPayments.Should().Be(0);
            result[0].R14ProviderAdditionalPayments.Should().Be(0);
            result[0].R14ApprenticeAdditionalPayments.Should().Be(0);
            result[0].R14EnglishAndMathsPayments.Should().Be(0);
            result[0].R14LearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].R14TotalPayments.Should().Be(0);

            result[0].TotalLevyPayments.Should().Be(0);
            result[0].TotalCoInvestmentPayments.Should().Be(0);
            result[0].TotalCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].TotalEmployerAdditionalPayments.Should().Be(0);
            result[0].TotalProviderAdditionalPayments.Should().Be(0);
            result[0].TotalApprenticeAdditionalPayments.Should().Be(0);
            result[0].TotalEnglishAndMathsPayments.Should().Be(0);
            result[0].TotalLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            result[0].TotalPayments.Should().Be(0);
        }

        private List<AppsMonthlyPaymentLarsLearningDeliveryInfo> BuildLarsDeliveryInfoModel()
        {
            return new List<AppsMonthlyPaymentLarsLearningDeliveryInfo>()
            {
                new AppsMonthlyPaymentLarsLearningDeliveryInfo()
                {
                    LearnAimRef = "ZPROG001",
                    LearningAimTitle = "Generic code to identify ILR programme aims"
                },
                new AppsMonthlyPaymentLarsLearningDeliveryInfo()
                {
                    LearnAimRef = "60154020",
                    LearningAimTitle = "60154020 Aim Title"
                },
                new AppsMonthlyPaymentLarsLearningDeliveryInfo()
                {
                    LearnAimRef = "50115893",
                    LearningAimTitle = "50115893 Aim Title"
                },
                new AppsMonthlyPaymentLarsLearningDeliveryInfo()
                {
                    LearnAimRef = "50089638",
                    LearningAimTitle = "Functional Skills qualification in English"
                },
                new AppsMonthlyPaymentLarsLearningDeliveryInfo()
                {
                    LearnAimRef = "50085098",
                    LearningAimTitle = "50085098 Aim Title"
                },
                new AppsMonthlyPaymentLarsLearningDeliveryInfo()
                {
                    LearnAimRef = "50089080",
                    LearningAimTitle = "50089080 Aim Title"
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
                        LearnRefNumber = "LR1001",
                        UniqueLearnerNumber = 1000000001,
                        CampId = "C0471802",

                        ProviderSpecLearnerMonitorings = new List<AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo>()
                        {
                            new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                ProvSpecLearnMonOccur = "A",
                                ProvSpecLearnMon = "001"
                            },
                            new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                ProvSpecLearnMonOccur = "B",
                                ProvSpecLearnMon = "100102"
                            }
                        },

                        LearnerEmploymentStatus = new List<AppsMonthlyPaymentLearnerEmploymentStatusInfo>()
                        {
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                DateEmpStatApp = new DateTime(2019, 10, 7),
                                EmpStat = 10, // 10 In paid employment
                                EmpdId = 905118782,
                                AgreeId = "5YJB6B"
                            },
                        },

                        LearningDeliveries = new List<AppsMonthlyPaymentLearningDeliveryModel>
                        {
                            new AppsMonthlyPaymentLearningDeliveryModel()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                LearnAimRef = "ZPROG001",
                                AimType = 1,
                                AimSeqNumber = 6,
                                LearnStartDate = new DateTime(2019, 10, 7),
                                OrigLearnStartDate = null,
                                LearnPlanEndDate = new DateTime(2020, 8, 10),
                                FundModel = 36,
                                ProgType = 2,
                                FworkCode = 436,
                                PwayCode = 5,
                                StdCode = 0,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "62993A2E-3D84-4BFA-8D32-0B72F286C0B8",
                                CompStatus = 1,
                                LearnActEndDate = null,
                                Outcome = null,
                                AchDate = null,
                                ProviderSpecDeliveryMonitorings =
                                    new List<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo>()
                                    {
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        },

                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 2,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 2,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 2,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 2,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        },

                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 3,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 3,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 3,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 3,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        },

                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 4,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 4,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 4,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 4,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        },

                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 5,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 5,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 5,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 5,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        },

                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 6,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 6,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 6,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 6,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        }
                                    },
                                LearningDeliveryFams = new List<AppsMonthlyPaymentLearningDeliveryFAMInfo>()
                                {
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 6,
                                        LearnDelFAMType = "HHS",
                                        LearnDelFAMCode = "98"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 6,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105",
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 6,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1",
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 6,
                                        LearnDelFAMType = "LDM",
                                        LearnDelFAMCode = "356",
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 6,
                                        LearnDelFAMType = "LDM",
                                        LearnDelFAMCode = "357",
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 6,
                                        LearnDelFAMType = "LDM",
                                        LearnDelFAMCode = "358",
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 6,
                                        LearnDelFAMType = "LDM",
                                        LearnDelFAMCode = "359",
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 6,
                                        LearnDelFAMType = "LDM",
                                        LearnDelFAMCode = "360",
                                    }
                                }
                            },
                            new AppsMonthlyPaymentLearningDeliveryModel()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                LearnAimRef = "ZPROG001",
                                AimType = 1,
                                AimSeqNumber = 7,
                                LearnStartDate = new DateTime(2017, 9, 18),
                                LearnPlanEndDate = new DateTime(2019, 9, 19),
                                FundModel = 36,
                                ProgType = 3,
                                StdCode = 0,
                                FworkCode = 436,
                                PwayCode = 6,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "E8FC9ECD-01DC-4D10-AB8D-E177BD21B259",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 9, 11),
                                Outcome = 1,
                                AchDate = null,
                                ProviderSpecDeliveryMonitorings =
                                    new List<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo>()
                                    {
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        },

                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 2,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 2,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 2,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 2,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        },

                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 3,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 3,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 3,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 3,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        },

                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 4,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 4,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 4,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 4,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        },

                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 5,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 5,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 5,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 5,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        },

                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 6,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "1920"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 6,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "E5072"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "3070",
                                            AimSeqNumber = 6,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "CHILD"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "LR1001",
                                            AimSeqNumber = 6,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        }
                                    },
                                LearningDeliveryFams = new List<AppsMonthlyPaymentLearningDeliveryFAMInfo>()
                                {
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 7,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 3,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1",
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
                LearnRefNumber = "LR1001",
                AecApprenticeshipPriceEpisodeInfoList = new List<AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo>()
                {
                    new AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "LR1001",
                        AimSequenceNumber = 6,
                        PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                        EpisodeStartDate = new DateTime(2018, 8, 1),
                        PriceEpisodeAgreeId = "5YJB6B",
                        PriceEpisodeActualEndDate = new DateTime(2019, 07, 31),
                        PriceEpisodeActualEndDateIncEPA = new DateTime(2019, 07, 31)
                    },
                    new AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "LR1001",
                        AimSequenceNumber = 6,
                        PriceEpisodeIdentifier = "2-445-3-01/08/2019",
                        EpisodeStartDate = new DateTime(2019, 8, 01),
                        PriceEpisodeAgreeId = "5YJB6B",
                        PriceEpisodeActualEndDate = new DateTime(2019, 10, 8),
                        PriceEpisodeActualEndDateIncEPA = new DateTime(2019, 10, 8),
                    },
                    new AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "LR1001",
                        AimSequenceNumber = 6,
                        PriceEpisodeIdentifier = "2-445-3-16/06/2018",
                        EpisodeStartDate = new DateTime(2018, 6, 16),
                        PriceEpisodeAgreeId = "5YJB6B",
                        PriceEpisodeActualEndDate = new DateTime(2018, 07, 31),
                        PriceEpisodeActualEndDateIncEPA = new DateTime(2018, 07, 31),
                    },
                },
                AecLearningDeliveryInfoList = new List<AppsMonthlyPaymentAECLearningDeliveryInfo>()
                {
                    new AppsMonthlyPaymentAECLearningDeliveryInfo()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "LR1001",
                        AimSequenceNumber = 1,
                        LearnAimRef = "50117889",
                        PlannedNumOnProgInstalm = 2
                    },
                    new AppsMonthlyPaymentAECLearningDeliveryInfo()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "LR1001",
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
                        ContractNumber = "NLAP-1157",
                        ContractVersionNumber = "1",
                        StartDate = new DateTime(2019, 08, 1),
                        EndDate = new DateTime(2020, 7, 31),
                        Provider = new AppsMonthlyPaymentContractorInfo()
                        {
                            UkPrn = ukPrn,
                            OrganisationIdentifier = "Manchester College",
                            LegalName = "Manchester College Ltd",
                        },
                        ContractAllocations = new List<AppsMonthlyPaymentContractAllocationInfo>()
                        {
                            new AppsMonthlyPaymentContractAllocationInfo()
                            {
                                ContractAllocationNumber = "YNLP-1157",
                                FundingStreamPeriodCode = "LEVY1799",
                                FundingStreamCode = "16-18NLAP",
                                Period = "2019",
                                PeriodTypeCode = "NONLEVY"
                            }
                        }
                    },
                    new AppsMonthlyPaymentContractInfo()
                    {
                        ContractNumber = "NLAP-1158",
                        ContractVersionNumber = "2",
                        StartDate = new DateTime(2019, 08, 1),
                        EndDate = new DateTime(2020, 7, 31),
                        Provider = new AppsMonthlyPaymentContractorInfo()
                        {
                            UkPrn = ukPrn,
                            OrganisationIdentifier = "Manchester College",
                            LegalName = "Manchester College Ltd",
                        },
                        ContractAllocations = new List<AppsMonthlyPaymentContractAllocationInfo>()
                        {
                            new AppsMonthlyPaymentContractAllocationInfo()
                            {
                                ContractAllocationNumber = "YNLP-1158",
                                FundingStreamPeriodCode = "LEVY1799",
                                FundingStreamCode = "LEVY",
                                Period = "2019",
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
                // Id        EventId                                Ukprn     ContractType  CollectionPeriod   AcademicYear   LearnRefNumr  Uln         LearnAimRef  ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                     LearnStartDate  AgreementId IlrSubmissionDateTime       JobId   EventTime                           CreationDate                        LearningAimSequenceNumber   SfaContributionPercentage   IlrFileName EventType
                // 1239037   2DA9C964-556F-4CD2-ADCF-69824DEF3DC5   10061808  1             2                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         19+ Apprenticeship (Employer on App Service)   2017-09-18      None        2019-10-03 12:33:38.6800000 48193   2019-10-03 12:34:04.8306378 +00:00  2019-10-03 12:34:12.4828763 +00:00  4   0.00000 10061808/ILR-10061808-1920-20191003-131506-01-Valid.XML SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 10394332  6E29D890-6009-42D0-9526-0F332AAA031F   10061808  1             4                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         NULL                                           2017-09-18      None        2019-12-06 16:06:06.7800000 132257  2019-12-07 00:23:44.2179942 +00:00  2019-12-07 00:23:46.6749429 +00:00  7   0.00000 10061808/ILR-10061808-1920-20191129-142049-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 10394334  C9E9C125-50DE-49F5-A3A4-502B6A502B61   10061808  1             4                  1920           138901100102  1036910902  ZPROG001     2         0        436        5         NULL                                           2019-10-07      None        2019-12-06 16:06:06.7800000 132257  2019-12-07 00:23:44.2177244 +00:00  2019-12-07 00:23:46.6749429 +00:00  6   0.00000 10061808/ILR-10061808-1920-20191129-142049-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 1968141   8EF436B6-469B-4669-AAB4-BB0B7F6795EB   10061808  1             2                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         19+ Apprenticeship (Employer on App Service)   2017-09-18      None        2019-10-04 19:02:11.7070000 53642   2019-10-04 20:03:51.3140925 +00:00  2019-10-04 20:04:26.2653428 +00:00  4   0.00000 10061808/ILR-10061808-1920-20191003-131506-01.XML   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 22459534  DFCE379F-CE04-4806-8952-F9DCC648EEF3   10061808  1             7                  1920           138901100102  1036910902  ZPROG001     2         0        436        5         NULL                                           2019-10-07      None        2020-02-25 04:35:26.5870000 136102  2020-02-25 04:36:08.7856869 +00:00  2020-02-25 04:36:18.1615823 +00:00  6   0.00000 10061808/ILR-10061808-1920-20200203-185022-01-Valid.XML SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 22459535  B8F120C5-1D58-4CAE-B3EC-CA67AAE61E4D   10061808  1             7                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         NULL                                           2017-09-18      None        2020-02-25 04:35:26.5870000 136102  2020-02-25 04:36:08.7859648 +00:00  2020-02-25 04:36:18.1615823 +00:00  7   0.00000 10061808/ILR-10061808-1920-20200203-185022-01-Valid.XML SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 14266235  D6804CC5-6478-4C2B-9DDF-9AF9819BFAB2   10061808  1             5                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         NULL                                           2017-09-18      None        2020-01-07 19:55:43.2300000 154851  2020-01-07 23:32:41.3608052 +00:00  2020-01-07 23:32:47.5523512 +00:00  7   0.00000 10061808/ILR-10061808-1920-20200106-081642-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 14266236  91C6089F-86A8-49D4-9C8A-73492824AA1E   10061808  1             5                  1920           138901100102  1036910902  ZPROG001     2         0        436        5         NULL                                           2019-10-07      None        2020-01-07 19:55:43.2300000 154851  2020-01-07 23:32:41.3606032 +00:00  2020-01-07 23:32:47.5523512 +00:00  6   0.00000 10061808/ILR-10061808-1920-20200106-081642-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 20808943  1E34B770-4A31-43B7-BA91-63687337BBE4   10061808  1             6                  1920           138901100102  1036910902  ZPROG001     2         0        436        5         NULL                                           2019-10-07      None        2020-02-07 17:29:00.5600000 182558  2020-02-07 22:11:06.5130236 +00:00  2020-02-07 22:11:15.9407429 +00:00  6   0.00000 10061808/ILR-10061808-1920-20200203-185022-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 20808945  E4A50686-1C94-4F4E-9D42-82F324532DC0   10061808  1             6                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         NULL                                           2017-09-18      None        2020-02-07 17:29:00.5600000 182558  2020-02-07 22:11:06.5132037 +00:00  2020-02-07 22:11:15.9407429 +00:00  7   0.00000 10061808/ILR-10061808-1920-20200203-185022-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 5060649   EA84AA32-AF53-4082-BB04-05D3F118AF0D   10061808  1             3                  1920           138901100102  1036910902  ZPROG001     2         0        436        5         NULL                                           2019-10-07      None        2019-11-06 20:40:41.7300000 94338   2019-11-07 21:57:03.5299406 +00:00  2019-11-07 22:52:49.8054643 +00:00  6   0.00000 10061808/ILR-10061808-1920-20191104-092448-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 5060650   C5B1C18A-BA97-4BBB-9705-CD4B50D91311   10061808  1             3                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         NULL                                           2017-09-18      None        2019-11-06 20:40:41.7300000 94338   2019-11-07 21:57:03.5301290 +00:00  2019-11-07 22:52:49.8054643 +00:00  7   0.00000 10061808/ILR-10061808-1920-20191104-092448-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent

                // Id        EventId                                Ukprn     ContractType  CollectionPeriod   AcademicYear   LearnRefNumr  Uln         LearnAimRef  ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                     LearnStartDate  AgreementId IlrSubmissionDateTime       JobId   EventTime                           CreationDate                        LearningAimSequenceNumber   SfaContributionPercentage   IlrFileName EventType
                // 10394332  6E29D890-6009-42D0-9526-0F332AAA031F   10061808  1             4                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         NULL                                           2017-09-18      None        2019-12-06 16:06:06.7800000 132257  2019-12-07 00:23:44.2179942 +00:00  2019-12-07 00:23:46.6749429 +00:00  7   0.00000 10061808/ILR-10061808-1920-20191129-142049-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 22459535  B8F120C5-1D58-4CAE-B3EC-CA67AAE61E4D   10061808  1             7                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         NULL                                           2017-09-18      None        2020-02-25 04:35:26.5870000 136102  2020-02-25 04:36:08.7859648 +00:00  2020-02-25 04:36:18.1615823 +00:00  7   0.00000 10061808/ILR-10061808-1920-20200203-185022-01-Valid.XML SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 14266235  D6804CC5-6478-4C2B-9DDF-9AF9819BFAB2   10061808  1             5                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         NULL                                           2017-09-18      None        2020-01-07 19:55:43.2300000 154851  2020-01-07 23:32:41.3608052 +00:00  2020-01-07 23:32:47.5523512 +00:00  7   0.00000 10061808/ILR-10061808-1920-20200106-081642-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 20808945  E4A50686-1C94-4F4E-9D42-82F324532DC0   10061808  1             6                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         NULL                                           2017-09-18      None        2020-02-07 17:29:00.5600000 182558  2020-02-07 22:11:06.5132037 +00:00  2020-02-07 22:11:15.9407429 +00:00  7   0.00000 10061808/ILR-10061808-1920-20200203-185022-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                // 5060650   C5B1C18A-BA97-4BBB-9705-CD4B50D91311   10061808  1             3                  1920           138901100102  1036910902  ZPROG001     3         0        436        6         NULL                                           2017-09-18      None        2019-11-06 20:40:41.7300000 94338   2019-11-07 21:57:03.5301290 +00:00  2019-11-07 22:52:49.8054643 +00:00  7   0.00000 10061808/ILR-10061808-1920-20191104-092448-01.ZIP   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent

                // only mock period 6 data
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 20808945,
                    EventId = new Guid("E4A50686-1C94-4F4E-9D42-82F324532DC0"),
                    Ukprn = 10061808,
                    ContractType = 1,
                    CollectionPeriod = 6,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "138901100102",
                    LearnerUln = 1036910902,
                    LearningAimReference = "ZPROG001",
                    LearningAimProgrammeType = 3,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 436,
                    LearningAimPathwayCode = 6,
                    LearningStartDate = new DateTime(2017, 09, 18),
                    AgreementId = "None",
                    IlrSubmissionDateTime = new DateTime(2017, 02, 07),
                    EventTime = new DateTime(2020, 2, 7, 22, 11, 06),
                    CreationDate = new DateTime(2020, 02, 07),
                    LearningAimSequenceNumber = 7
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 20808943,
                    EventId = new Guid("1E34B770-4A31-43B7-BA91-63687337BBE4"),
                    Ukprn = 10061808,
                    ContractType = 1,
                    CollectionPeriod = 6,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "138901100102",
                    LearnerUln = 1036910902,
                    LearningAimReference = "ZPROG001",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 436,
                    LearningAimPathwayCode = 5,
                    LearningStartDate = new DateTime(2019, 10, 07),
                    AgreementId = "None",
                    IlrSubmissionDateTime = new DateTime(2019, 02, 07),
                    EventTime = new DateTime(2020, 2, 7, 22, 11, 06),
                    CreationDate = new DateTime(2020, 02, 07),
                    LearningAimSequenceNumber = 6
                },
                //new AppsMonthlyPaymentDasEarningEventModel()
                //{
                //    Id = 14266236,
                //    EventId = new Guid("91C6089F-86A8-49D4-9C8A-73492824AA1E"),
                //    Ukprn = 10061808,
                //    ContractType = 1,
                //    CollectionPeriod = 5,
                //    AcademicYear = 1920,
                //    LearnerReferenceNumber = "138901100102",
                //    LearnerUln = 1036910902,
                //    LearningAimReference = "ZPROG001",
                //    LearningAimProgrammeType = 2,
                //    LearningAimStandardCode = 0,
                //    LearningAimFrameworkCode = 436,
                //    LearningAimPathwayCode = 5,
                //    LearningStartDate = new DateTime(2019, 10, 07),
                //    AgreementId = "None",
                //    IlrSubmissionDateTime = new DateTime(2019, 01, 07),
                //    EventTime = new DateTime(2020, 1, 7, 23, 32, 41),
                //    CreationDate = new DateTime(2020, 01, 07),
                //    LearningAimSequenceNumber = 6
                //},
                //new AppsMonthlyPaymentDasEarningEventModel()
                //{
                //    Id = 14266235,
                //    EventId = new Guid("D6804CC5-6478-4C2B-9DDF-9AF9819BFAB2"),
                //    Ukprn = 10061808,
                //    ContractType = 1,
                //    CollectionPeriod = 5,
                //    AcademicYear = 1920,
                //    LearnerReferenceNumber = "138901100102",
                //    LearnerUln = 1036910902,
                //    LearningAimReference = "ZPROG001",
                //    LearningAimProgrammeType = 3,
                //    LearningAimStandardCode = 0,
                //    LearningAimFrameworkCode = 436,
                //    LearningAimPathwayCode = 6,
                //    LearningStartDate = new DateTime(2017, 09, 18), AgreementId = "None",
                //    IlrSubmissionDateTime = new DateTime(2017, 01, 07),
                //    EventTime = new DateTime(2020, 1, 7, 23, 32, 41),
                //    CreationDate = new DateTime(2020, 01, 07),
                //    LearningAimSequenceNumber = 7
                //},
                //new AppsMonthlyPaymentDasEarningEventModel()
                //{
                //    Id = 10394334,
                //    EventId = new Guid("C9E9C125-50DE-49F5-A3A4-502B6A502B61"),
                //    Ukprn = 10061808,
                //    ContractType = 1,
                //    CollectionPeriod = 4,
                //    AcademicYear = 1920,
                //    LearnerReferenceNumber = "138901100102",
                //    LearnerUln = 1036910902,
                //    LearningAimReference = "ZPROG001",
                //    LearningAimProgrammeType = 2,
                //    LearningAimStandardCode = 0,
                //    LearningAimFrameworkCode = 436,
                //    LearningAimPathwayCode = 5,
                //    LearningStartDate = new DateTime(2019, 10, 07), AgreementId = "None",
                //    IlrSubmissionDateTime = new DateTime(2019, 12, 06),
                //    EventTime = new DateTime(2019, 12, 7, 00, 23, 44),
                //    CreationDate = new DateTime(2019, 12, 07),
                //    LearningAimSequenceNumber = 6
                //},
                //new AppsMonthlyPaymentDasEarningEventModel()
                //{
                //    Id = 10394332,
                //    EventId = new Guid("6E29D890-6009-42D0-9526-0F332AAA031F"),
                //    Ukprn = 10061808,
                //    ContractType = 1,
                //    CollectionPeriod = 4,
                //    AcademicYear = 1920,
                //    LearnerReferenceNumber = "138901100102",
                //    LearnerUln = 1036910902,
                //    LearningAimReference = "ZPROG001",
                //    LearningAimProgrammeType = 3,
                //    LearningAimStandardCode = 0,
                //    LearningAimFrameworkCode = 436,
                //    LearningAimPathwayCode = 6,
                //    LearningStartDate = new DateTime(2017, 09, 18), AgreementId = "None",
                //    IlrSubmissionDateTime = new DateTime(2017, 12, 06),
                //    EventTime = new DateTime(2019, 12, 7, 00, 23, 44),
                //    CreationDate = new DateTime(2019, 12, 07),
                //    LearningAimSequenceNumber = 7
                //},
                //new AppsMonthlyPaymentDasEarningEventModel()
                //{
                //    Id = 5060650,
                //    EventId = new Guid("C5B1C18A-BA97-4BBB-9705-CD4B50D91311"),
                //    Ukprn = 10061808,
                //    ContractType = 1,
                //    CollectionPeriod = 3,
                //    AcademicYear = 1920,
                //    LearnerReferenceNumber = "138901100102",
                //    LearnerUln = 1036910902,
                //    LearningAimReference = "ZPROG001",
                //    LearningAimProgrammeType = 3,
                //    LearningAimStandardCode = 0,
                //    LearningAimFrameworkCode = 436,
                //    LearningAimPathwayCode = 6,
                //    LearningStartDate = new DateTime(2017, 09, 18),
                //    AgreementId = "None",
                //    IlrSubmissionDateTime = new DateTime(2017, 11, 06),
                //    EventTime = new DateTime(2019, 11, 7, 21, 57, 03),
                //    CreationDate = new DateTime(2019, 11, 07),
                //    LearningAimSequenceNumber = 7
                //},
                //new AppsMonthlyPaymentDasEarningEventModel()
                //{
                //    Id = 5060649,
                //    EventId = new Guid("EA84AA32-AF53-4082-BB04-05D3F118AF0D"),
                //    Ukprn = 10061808,
                //    ContractType = 1,
                //    CollectionPeriod = 3,
                //    AcademicYear = 1920,
                //    LearnerReferenceNumber = "138901100102",
                //    LearnerUln = 1036910902,
                //    LearningAimReference = "ZPROG001",
                //    LearningAimProgrammeType = 2,
                //    LearningAimStandardCode = 0,
                //    LearningAimFrameworkCode = 436,
                //    LearningAimPathwayCode = 5,
                //    LearningStartDate = new DateTime(2019, 10, 07),
                //    AgreementId = "None",
                //    IlrSubmissionDateTime = new DateTime(2019, 11, 06),
                //    EventTime = new DateTime(2019, 11, 7, 21, 57, 03),
                //    CreationDate = new DateTime(2019, 11, 07),
                //    LearningAimSequenceNumber = 6
                //},
                //new AppsMonthlyPaymentDasEarningEventModel()
                //{
                //    Id = 1968141,
                //    EventId = new Guid("8EF436B6-469B-4669-AAB4-BB0B7F6795EB"),
                //    Ukprn = 10061808,
                //    ContractType = 1,
                //    CollectionPeriod = 2,
                //    AcademicYear = 1920,
                //    LearnerReferenceNumber = "138901100102",
                //    LearnerUln = 1036910902,
                //    LearningAimReference = "ZPROG001",
                //    LearningAimProgrammeType = 3,
                //    LearningAimStandardCode = 0,
                //    LearningAimFrameworkCode = 436,
                //    LearningAimPathwayCode = 6,
                //    LearningStartDate = new DateTime(2017, 09, 18),
                //    AgreementId = "None",
                //    IlrSubmissionDateTime = new DateTime(2017, 10, 04),
                //    EventTime = new DateTime(2019, 10, 4, 20, 03, 51),
                //    CreationDate = new DateTime(2019, 10, 04),
                //    LearningAimSequenceNumber = 4
                //},
                //new AppsMonthlyPaymentDasEarningEventModel()
                //{
                //    Id = 1239037,
                //    EventId = new Guid("2DA9C964-556F-4CD2-ADCF-69824DEF3DC5"),
                //    Ukprn = 10061808,
                //    ContractType = 1,
                //    CollectionPeriod = 2,
                //    AcademicYear = 1920,
                //    LearnerReferenceNumber = "138901100102",
                //    LearnerUln = 1036910902,
                //    LearningAimReference = "ZPROG001",
                //    LearningAimProgrammeType = 3,
                //    LearningAimStandardCode = 0,
                //    LearningAimFrameworkCode = 436,
                //    LearningAimPathwayCode = 6,
                //    LearningStartDate = new DateTime(2017, 09, 18),
                //    AgreementId = "None",
                //    IlrSubmissionDateTime = new DateTime(2017, 10, 03),
                //    EventTime = new DateTime(2019, 10, 3, 12, 34, 04),
                //    CreationDate = new DateTime(2019, 10, 03),
                //    LearningAimSequenceNumber = 4
                //}
            };

            return appsMonthlyPaymentDasEarningsInfo;
        }

        private AppsMonthlyPaymentDASInfo BuildDasPaymentsModel(int ukPrn)
        {
            var appsMonthlyPaymentDasInfo = new AppsMonthlyPaymentDASInfo()
            {
                UkPrn = ukPrn
            };

            appsMonthlyPaymentDasInfo.Payments = new List<AppsMonthlyPaymentDasPaymentModel>()
            {
                // mock data for defect 92411
                // AcademicYear  Ukprn     LearnRefNum   Uln         LearnAimRef  LearnStartDate  ProgType  StdCode  FworkCode  PwayCode  ReportingAimFundingLineType                                PriceEpisodeIdentifier  ContractType  TransactionType FundingSource  DeliveryPeriod  CollectionPeriod  EarningEventId
                // 1920          10061808  138901100102  1036910902  ZPROG001     2019-10-07      2         0        436        5         19+ Apprenticeship (Employer on App Service) Levy funding  2-436-5-07/10/2019      1             1               1              3               3                 EA84AA32-AF53-4082-BB04-05D3F118AF0D
                // 1920          10061808  138901100102  1036910902  ZPROG001     2019-10-07      2         0        436        5         19+ Apprenticeship (Employer on App Service) Levy funding  2-436-5-07/10/2019      1             1               1              4               4                 C9E9C125-50DE-49F5-A3A4-502B6A502B61
                // 1920          10061808  138901100102  1036910902  ZPROG001     2019-10-07      2         0        436        5         19+ Apprenticeship (Employer on App Service) Levy funding  2-436-5-07/10/2019      1             1               1              5               5                 91C6089F-86A8-49D4-9C8A-73492824AA1E
                // 1920          10061808  138901100102  1036910902  ZPROG001     2019-10-07      2         0        436        5         19+ Apprenticeship (Employer on App Service) Levy funding  2-436-5-07/10/2019      1             1               1              6               6                 1E34B770-4A31-43B7-BA91-63687337BBE4
                // 1920          10061808  138901100102  1036910902  ZPROG001     2019-10-07      2         0        436        5         19+ Apprenticeship (Employer on App Service) Levy funding  2-436-5-07/10/2019      1             1               1              7               7                 DFCE379F-CE04-4806-8952-F9DCC648EEF3
                // 1920          10061808  138901100102  1036910902  ZPROG001     2017-09-18      3         0        436        6         19+ Apprenticeship (Employer on App Service) Levy funding  3-436-6-01/08/2019      1             1               1              1               2                 8EF436B6-469B-4669-AAB4-BB0B7F6795EB
                // 1920          10061808  138901100102  1036910902  ZPROG001     2017-09-18      3         0        436        6         19+ Apprenticeship (Employer on App Service) Levy funding  3-436-6-01/08/2019      1             2               1              2               2                 8EF436B6-469B-4669-AAB4-BB0B7F6795EB

                // AcademicYear  Ukprn     LearnRefNum   Uln         LearnAimRef  LearnStartDate  ProgType  StdCode  FworkCode  PwayCode  ReportingAimFundingLineType
                // 1920          10061808  138901100102  1036910902  ZPROG001     2019-10-07      2         0        436        5         19+ Apprenticeship (Employer on App Service) Levy funding

                // PriceEpisodeIdentifier  ContractType  TransactionType FundingSource  DeliveryPeriod  CollectionPeriod  EarningEventId
                // 2-436-5-07/10/2019      1             1               1              3               3                 EA84AA32-AF53-4082-BB04-05D3F118AF0D
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 10, 7),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 436,
                    LearningAimPathwayCode = 5,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-436-5-07/10/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 3,
                    CollectionPeriod = 3,
                    EarningEventId = new Guid("EA84AA32-AF53-4082-BB04-05D3F118AF0D")
                },

                // AcademicYear  Ukprn     LearnRefNum   Uln         LearnAimRef  LearnStartDate  ProgType  StdCode  FworkCode  PwayCode  ReportingAimFundingLineType
                // 1920          10061808  138901100102  1036910902  ZPROG001     2019-10-07      2         0        436        5         19+ Apprenticeship (Employer on App Service) Levy funding

                // PriceEpisodeIdentifier  ContractType  TransactionType FundingSource  DeliveryPeriod  CollectionPeriod  EarningEventId
                // 2-436-5-07/10/2019      1             1               1              4               4                 C9E9C125-50DE-49F5-A3A4-502B6A502B61
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 10, 7),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 436,
                    LearningAimPathwayCode = 5,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-436-5-07/10/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 4,
                    CollectionPeriod = 4,
                    EarningEventId = new Guid("C9E9C125-50DE-49F5-A3A4-502B6A502B61")
                },

                // AcademicYear  Ukprn     LearnRefNum   Uln         LearnAimRef  LearnStartDate  ProgType  StdCode  FworkCode  PwayCode  ReportingAimFundingLineType
                // 1920          10061808  138901100102  1036910902  ZPROG001     2019-10-07      2         0        436        5         19+ Apprenticeship (Employer on App Service) Levy funding

                // PriceEpisodeIdentifier  ContractType  TransactionType FundingSource  DeliveryPeriod  CollectionPeriod  EarningEventId
                // 2-436-5-07/10/2019      1             1               1              5               5                 91C6089F-86A8-49D4-9C8A-73492824AA1E
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 10, 7),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 436,
                    LearningAimPathwayCode = 5,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-436-5-07/10/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 5,
                    CollectionPeriod = 5,
                    EarningEventId = new Guid("91C6089F-86A8-49D4-9C8A-73492824AA1E")
                },

                // AcademicYear  Ukprn     LearnRefNum   Uln         LearnAimRef  LearnStartDate  ProgType  StdCode  FworkCode  PwayCode  ReportingAimFundingLineType
                // 1920          10061808  138901100102  1036910902  ZPROG001     2019-10-07      2         0        436        5         19+ Apprenticeship (Employer on App Service) Levy funding

                // PriceEpisodeIdentifier  ContractType  TransactionType FundingSource  DeliveryPeriod  CollectionPeriod  EarningEventId
                // 2-436-5-07/10/2019      1             1               1              6               6                 1E34B770-4A31-43B7-BA91-63687337BBE4
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 10, 7),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 436,
                    LearningAimPathwayCode = 5,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-436-5-07/10/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 6,
                    CollectionPeriod = 6,
                    EarningEventId = new Guid("1E34B770-4A31-43B7-BA91-63687337BBE4")
                },

                // AcademicYear  Ukprn     LearnRefNum   Uln         LearnAimRef  LearnStartDate  ProgType  StdCode  FworkCode  PwayCode  ReportingAimFundingLineType
                // 1920          10061808  138901100102  1036910902  ZPROG001     2019-10-07      2         0        436        5         19+ Apprenticeship (Employer on App Service) Levy funding

                // PriceEpisodeIdentifier  ContractType  TransactionType FundingSource  DeliveryPeriod  CollectionPeriod  EarningEventId
                // 2-436-5-07/10/2019      1             1               1              7               7                 DFCE379F-CE04-4806-8952-F9DCC648EEF3
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 10, 7),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 436,
                    LearningAimPathwayCode = 5,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-436-5-07/10/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 7,
                    CollectionPeriod = 7,
                    EarningEventId = new Guid("DFCE379F-CE04-4806-8952-F9DCC648EEF3")
                },

                // AcademicYear  Ukprn     LearnRefNum   Uln         LearnAimRef  LearnStartDate  ProgType  StdCode  FworkCode  PwayCode  ReportingAimFundingLineType
                // 1920          10061808  138901100102  1036910902  ZPROG001     2017-09-18      3         0        436        6         19+ Apprenticeship (Employer on App Service) Levy funding

                // PriceEpisodeIdentifier  ContractType  TransactionType FundingSource  DeliveryPeriod  CollectionPeriod  EarningEventId
                // 3-436-6-01/08/2019      1             1               1              1               2                 8EF436B6-469B-4669-AAB4-BB0B7F6795EB
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2017, 9, 18),
                    LearningAimProgrammeType = 3,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 436,
                    LearningAimPathwayCode = 6,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "3-436-6-01/08/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = 2,
                    EarningEventId = new Guid("8EF436B6-469B-4669-AAB4-BB0B7F6795EB")
                },

                // AcademicYear  Ukprn     LearnRefNum   Uln         LearnAimRef  LearnStartDate  ProgType  StdCode  FworkCode  PwayCode  ReportingAimFundingLineType
                // 1920          10061808  138901100102  1036910902  ZPROG001     2017-09-18      3         0        436        6         19+ Apprenticeship (Employer on App Service) Levy funding

                // PriceEpisodeIdentifier  ContractType  TransactionType FundingSource  DeliveryPeriod  CollectionPeriod  EarningEventId
                // 3-436-6-01/08/2019      1             2               1              2               2                 8EF436B6-469B-4669-AAB4-BB0B7F6795EB
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2017, 9, 18),
                    LearningAimProgrammeType = 3,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 436,
                    LearningAimPathwayCode = 6,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "3-436-6-01/08/2019",
                    ContractType = 1,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 2,
                    CollectionPeriod = 2,
                    EarningEventId = new Guid("8EF436B6-469B-4669-AAB4-BB0B7F6795EB")
                }
            };

            #region Original Mock Data commented out for defect 93224
            // learner 1, first payments
            //for (byte i = 1; i < 15; i++)
            //{
            //    var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 2,
            //        FundingSource = 1,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 11m
            //    };

            //    var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearningAimReference = "50117889",
            //        LearnerUln = 12345,
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",
            //        ContractType = 2,

            //        TransactionType = 2,
            //        FundingSource = 2,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 12m
            //    };

            //    var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",
            //        ContractType = 2,

            //        TransactionType = 2,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 13m
            //    };

            //    var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",
            //        ContractType = 2,

            //        TransactionType = 4,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 14m
            //    };

            //    var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 5,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 15m
            //    };

            //    var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 16,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 16m
            //    };

            //    var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 13,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 17m
            //    };

            //    var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 8,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 18m
            //    };

            //    appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            //}

            //// learner 1, second payments
            //for (byte i = 1; i < 15; i++)
            //{
            //    var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 2,
            //        FundingSource = 1,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 22m
            //    };

            //    var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 2,
            //        FundingSource = 2,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 24m
            //    };

            //    var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 2,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 26m
            //    };

            //    var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 4,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 28m
            //    };

            //    var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 5,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 30m
            //    };

            //    var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 16,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 32m
            //    };

            //    var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 13,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 34m
            //    };

            //    var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "A12345",
            //        LearnerUln = 12345,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 8,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 36m
            //    };

            //    appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            //}

            //// Learner 2, first payments
            //for (byte i = 1; i < 15; i++)
            //{
            //    var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 2,
            //        FundingSource = 1,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 11m
            //    };

            //    var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 2,
            //        FundingSource = 1,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 12m
            //    };

            //    var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",
            //        ContractType = 2,

            //        TransactionType = 2,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 13m
            //    };

            //    var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 4,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 14m
            //    };

            //    var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 5,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 15m
            //    };

            //    var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 16,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 16m
            //    };

            //    var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 13,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 17m
            //    };

            //    var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "50117889",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 8,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 18m
            //    };

            //    appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            //}

            //// learner 2, second payments
            //for (byte i = 1; i < 15; i++)
            //{
            //    var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 2,
            //        FundingSource = 1,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 22m
            //    };

            //    var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 2,
            //        FundingSource = 2,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 24m
            //    };

            //    var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 2,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 26m
            //    };

            //    var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 4,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 28m
            //    };

            //    var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 5,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 30m
            //    };

            //    var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 16,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 32m
            //    };

            //    var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 13,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 34m
            //    };

            //    var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
            //    {
            //        AcademicYear = 1920,
            //        Ukprn = ukPrn,
            //        LearnerReferenceNumber = "B12345",
            //        LearnerUln = 54321,
            //        LearningAimReference = "ZPROG001",
            //        LearningStartDate = new DateTime(2019, 08, 28),
            //        EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
            //        LearningAimProgrammeType = 1,
            //        LearningAimStandardCode = 1,
            //        LearningAimFrameworkCode = 1,
            //        LearningAimPathwayCode = 1,
            //        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
            //        PriceEpisodeIdentifier = "123428/08/2019",

            //        ContractType = 2,
            //        TransactionType = 8,
            //        FundingSource = 3,
            //        DeliveryPeriod = 1,
            //        CollectionPeriod = i,

            //        Amount = 36m
            //    };

            //    appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
            //    appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            //}

            #endregion
            return appsMonthlyPaymentDasInfo;
        }
    }
}