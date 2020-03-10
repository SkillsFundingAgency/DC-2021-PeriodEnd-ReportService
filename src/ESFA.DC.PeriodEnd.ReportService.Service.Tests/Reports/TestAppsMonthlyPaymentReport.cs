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

            fcsProviderServiceMock.Setup(x => x.GetContractAllocationNumberFSPCodeLookupAsync(
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

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(2);

            result[0].PaymentLearnerReferenceNumber.Should().Be("LR1001");
            result[0].PaymentUniqueLearnerNumber.Should().Be(1000000001);
            result[0].LearnerCampusIdentifier.Should().Be("C0471802");
            result[0].ProviderSpecifiedLearnerMonitoringA.Should().Be("001");
            result[0].ProviderSpecifiedLearnerMonitoringB.Should().Be("100102");
            result[0].PaymentEarningEventAimSeqNumber.Should().Be(6);
            result[0].PaymentLearningAimReference.Should().Be("ZPROG001");
            result[0].LarsLearningDeliveryLearningAimTitle.Should().Be("Generic code to identify ILR programme aims");
            result[0].LearningDeliveryOriginalLearningStartDate.Should().BeNull();
            result[0].PaymentLearningStartDate.Should().Be(new DateTime(2019, 4, 13));
            result[0].LearningDeliveryLearningPlannedEndDate.Should().Be(new DateTime(2020, 8, 10));
            result[0].LearningDeliveryLearningActualEndDate.Should().BeNull();
            result[0].LearningDeliveryAchievementDate.Should().BeNull();
            result[0].LearningDeliveryOutcome.Should().BeNull();
            result[0].PaymentProgrammeType.Should().Be(2);
            result[0].PaymentStandardCode.Should().Be(0);
            result[0].PaymentFrameworkCode.Should().Be(436);
            result[0].PaymentPathwayCode.Should().Be(5);
            result[0].LearningDeliveryAimType.Should().Be(1);
            result[0].LearningDeliverySoftwareSupplierAimIdentifier.Should().Be("62993A2E-3D84-4BFA-8D32-0B72F286C0B8");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringA.Should().Be("356");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringB.Should().Be("357");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringC.Should().Be("358");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringD.Should().Be("359");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringE.Should().Be("360");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringF.Should().BeNullOrEmpty();
            result[0].ProviderSpecifiedDeliveryMonitoringA.Should().Be("1920");
            result[0].ProviderSpecifiedDeliveryMonitoringB.Should().Be("E5072");
            result[0].ProviderSpecifiedDeliveryMonitoringC.Should().BeNullOrEmpty();
            result[0].ProviderSpecifiedDeliveryMonitoringD.Should().Be("D006801");
            result[0].LearningDeliveryEndPointAssessmentOrganisation.Should().BeNullOrEmpty();
            result[0].RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim.Should().BeNull();
            result[0].LearningDeliverySubContractedOrPartnershipUkprn.Should().BeNull();
            result[0].PaymentPriceEpisodeStartDate.Should().Be("07/10/2019");
            result[0].RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate.Should().BeNull();
            result[0].FcsContractContractAllocationContractAllocationNumber.Should().Be("YNLP-1157;YNLP-1158");
            result[0].PaymentFundingLineType.Should().Be("19 + Apprenticeship(Employer on App Service) Levy funding");
            result[0].PaymentApprenticeshipContractType.Should().Be(1);
            result[0].LearnerEmploymentStatusEmployerId.Should().Be(905118782);
            result[0].RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier.Should().BeNullOrEmpty();
            result[0].LearnerEmploymentStatus.Should().Be(10);
            result[0].LearnerEmploymentStatusDate.Should().Be(new DateTime(2019, 10, 7));

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
                            // Mock data for 93224
                            new AppsMonthlyPaymentLearningDeliveryModel()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                LearnAimRef = "60154020",
                                AimType = 3,
                                AimSeqNumber = 1,
                                LearnStartDate = new DateTime(2018, 6, 16),
                                OrigLearnStartDate = null,
                                LearnPlanEndDate = new DateTime(2020, 4, 16),
                                FundModel = 36,
                                ProgType = 2,
                                FworkCode = 445,
                                PwayCode = 3,
                                StdCode = 0,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "62993A2E-3D84-4BFA-8D32-0B72F286C0B8",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 10, 8),
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
                                LearnAimRef = "50115893",
                                AimType = 4,
                                AimSeqNumber = 2,
                                LearnStartDate = new DateTime(2018, 6, 16),
                                LearnPlanEndDate = new DateTime(2019, 6, 16),
                                FundModel = 99,
                                ProgType = 0,
                                StdCode = 0,
                                FworkCode = 0,
                                PwayCode = 0,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "E8FC9ECD-01DC-4D10-AB8D-E177BD21B259",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 5, 7),
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
                            },
                            new AppsMonthlyPaymentLearningDeliveryModel()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                LearnAimRef = "50089638",
                                AimType = 3,
                                AimSeqNumber = 3,
                                LearnStartDate = new DateTime(2019, 4, 14),
                                OrigLearnStartDate = null,
                                LearnPlanEndDate = new DateTime(2019, 10, 13),
                                FundModel = 36,
                                ProgType = 2,
                                FworkCode = 445,
                                PwayCode = 3,
                                StdCode = 0,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "62993A2E-3D84-4BFA-8D32-0B72F286C0B8",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 10, 8),
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
                                LearnAimRef = "50085098",
                                AimType = 3,
                                AimSeqNumber = 4,
                                LearnStartDate = new DateTime(2019, 3, 17),
                                OrigLearnStartDate = null,
                                LearnPlanEndDate = new DateTime(2020, 4, 16),
                                FundModel = 36,
                                ProgType = 2,
                                FworkCode = 445,
                                PwayCode = 3,
                                StdCode = 0,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "62993A2E-3D84-4BFA-8D32-0B72F286C0B8",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 9, 16),
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
                                LearnAimRef = "50089080",
                                AimType = 3,
                                AimSeqNumber = 5,
                                LearnStartDate = new DateTime(2018, 6, 16),
                                OrigLearnStartDate = null,
                                LearnPlanEndDate = new DateTime(2019, 6, 16),
                                FundModel = 36,
                                ProgType = 2,
                                FworkCode = 445,
                                PwayCode = 3,
                                StdCode = 0,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "62993A2E-3D84-4BFA-8D32-0B72F286C0B8",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 3, 27),
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
                                AimSeqNumber = 6,
                                LearnStartDate = new DateTime(2018, 6, 16),
                                OrigLearnStartDate = null,
                                LearnPlanEndDate = new DateTime(2020, 4, 16),
                                FundModel = 36,
                                ProgType = 2,
                                FworkCode = 445,
                                PwayCode = 3,
                                StdCode = 0,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "62993A2E-3D84-4BFA-8D32-0B72F286C0B8",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 10, 8),
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

        private IDictionary<string, string> BuildFcsModel(int ukPrn)
        {
            IDictionary<string, string> allocationNumbers = null;

            allocationNumbers = new Dictionary<string, string>
            {
                { "LEVY1799", "YNLP-1157;YNLP-1158" }
            };

            return allocationNumbers;
        }

        private AppsMonthlyPaymentDasEarningsInfo BuildDasEarningsModel(int ukPrn)
        {
            var appsMonthlyPaymentDasEarningsInfo = new AppsMonthlyPaymentDasEarningsInfo()
            {
                UkPrn = ukPrn
            };

            appsMonthlyPaymentDasEarningsInfo.Earnings = new EditableList<AppsMonthlyPaymentDasEarningEventModel>()
            {
                // mock data for defect 93224

                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 9875758, EventId = new Guid("660E861F-1520-4F49-B84E-D75099483B1F"), Ukprn = 10000001,
                    ContractType = 1, CollectionPeriod = 4, AcademicYear = 1920, LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001, LearningAimReference = "50089638", LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0, LearningAimFrameworkCode = 445, LearningAimPathwayCode = 3,
                    LearningStartDate = new DateTime(2019, 04, 13), AgreementId = string.Empty,
                    IlrSubmissionDateTime = new DateTime(2019, 12, 06),
                    EventTime = new DateTime(2019, 12, 6, 20, 56, 7),
                    CreationDate = new DateTime(2019, 12, 06), LearningAimSequenceNumber = 3
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 14658992, EventId = new Guid("F65B7E77-C958-430A-8247-81A503B93B19"), Ukprn = 10000001,
                    ContractType = 1, CollectionPeriod = 5, AcademicYear = 1920, LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001, LearningAimReference = "50089638", LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0, LearningAimFrameworkCode = 445, LearningAimPathwayCode = 3,
                    LearningStartDate = new DateTime(2019, 04, 13), AgreementId = string.Empty,
                    IlrSubmissionDateTime = new DateTime(2019, 01, 08),
                    EventTime = new DateTime(2020, 1, 8, 14, 56, 48),
                    CreationDate = new DateTime(2020, 01, 08), LearningAimSequenceNumber = 3
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 20249225, EventId = new Guid("39048CD9-67E9-4AE7-8723-173094DB1B5C"), Ukprn = 10000001,
                    ContractType = 1, CollectionPeriod = 6, AcademicYear = 1920, LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001, LearningAimReference = "50089638", LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0, LearningAimFrameworkCode = 445, LearningAimPathwayCode = 3,
                    LearningStartDate = new DateTime(2019, 04, 13), AgreementId = string.Empty,
                    IlrSubmissionDateTime = new DateTime(2019, 02, 07),
                    EventTime = new DateTime(2020, 2, 7, 18, 56, 52),
                    CreationDate = new DateTime(2020, 02, 07), LearningAimSequenceNumber = 3
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 1376649, EventId = new Guid("01C050C2-9C0B-4F03-A2C9-C4BD08CEB9FE"), Ukprn = 10000001,
                    ContractType = 1, CollectionPeriod = 2, AcademicYear = 1920, LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001, LearningAimReference = "ZPROG001", LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0, LearningAimFrameworkCode = 445, LearningAimPathwayCode = 3,
                    LearningStartDate = new DateTime(2018, 06, 16), AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2018, 10, 04),
                    EventTime = new DateTime(2019, 10, 4, 9, 43, 4),
                    CreationDate = new DateTime(2019, 10, 04), LearningAimSequenceNumber = 6
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 1443478, EventId = new Guid("79F8C510-3E59-45E9-A615-E302F4DFCAE1"), Ukprn = 10000001,
                    ContractType = 1, CollectionPeriod = 2, AcademicYear = 1920, LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001, LearningAimReference = "ZPROG001", LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0, LearningAimFrameworkCode = 445, LearningAimPathwayCode = 3,
                    LearningStartDate = new DateTime(2018, 06, 16), AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2018, 10, 04),
                    EventTime = new DateTime(2019, 10, 4, 11, 24, 40),
                    CreationDate = new DateTime(2019, 10, 04), LearningAimSequenceNumber = 6
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 1618582, EventId = new Guid("23F84DCA-ADD4-4D9A-9551-C4B328F7214C"), Ukprn = 10000001,
                    ContractType = 1, CollectionPeriod = 2, AcademicYear = 1920, LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001, LearningAimReference = "ZPROG001", LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0, LearningAimFrameworkCode = 445, LearningAimPathwayCode = 3,
                    LearningStartDate = new DateTime(2018, 06, 16), AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2018, 10, 04),
                    EventTime = new DateTime(2019, 10, 4, 15, 3, 22),
                    CreationDate = new DateTime(2019, 10, 04), LearningAimSequenceNumber = 6
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 1776657, EventId = new Guid("BC7FADB1-D943-454F-8591-809BD4A30E02"), Ukprn = 10000001,
                    ContractType = 1, CollectionPeriod = 2, AcademicYear = 1920, LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001, LearningAimReference = "ZPROG001", LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0, LearningAimFrameworkCode = 445, LearningAimPathwayCode = 3,
                    LearningStartDate = new DateTime(2018, 06, 16), AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2018, 10, 04),
                    EventTime = new DateTime(2019, 10, 4, 19, 22, 29),
                    CreationDate = new DateTime(2019, 10, 04), LearningAimSequenceNumber = 6
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 4677582, EventId = new Guid("6D022948-C5E2-491C-9301-E858D7A99A9E"), Ukprn = 10000001,
                    ContractType = 1, CollectionPeriod = 3, AcademicYear = 1920, LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001, LearningAimReference = "ZPROG001", LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0, LearningAimFrameworkCode = 445, LearningAimPathwayCode = 3,
                    LearningStartDate = new DateTime(2018, 06, 16), AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2018, 11, 06),
                    EventTime = new DateTime(2019, 11, 7,  21, 17, 48),
                    CreationDate = new DateTime(2019, 11, 07), LearningAimSequenceNumber = 6
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 9875760, EventId = new Guid("5C60B697-C04F-43CA-9E03-5E3B008A4189"), Ukprn = 10000001,
                    ContractType = 1, CollectionPeriod = 4, AcademicYear = 1920, LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001, LearningAimReference = "ZPROG001", LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0, LearningAimFrameworkCode = 445, LearningAimPathwayCode = 3,
                    LearningStartDate = new DateTime(2018, 06, 16), AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2018, 12, 06),
                    EventTime = new DateTime(2019, 12, 6, 20, 56, 7),
                    CreationDate = new DateTime(2019, 12, 06), LearningAimSequenceNumber = 6
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 14658993, EventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597"), Ukprn = 10000001,
                    ContractType = 1, CollectionPeriod = 5, AcademicYear = 1920, LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001, LearningAimReference = "ZPROG001", LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0, LearningAimFrameworkCode = 445, LearningAimPathwayCode = 3,
                    LearningStartDate = new DateTime(2018, 06, 16), AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2018, 01, 08),
                    EventTime = new DateTime(2020, 1, 8, 14, 56, 48),
                    CreationDate = new DateTime(2020, 01, 08), LearningAimSequenceNumber = 6
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 20249229, EventId = new Guid("5AC5396C-B665-4084-9134-E7550546433A"), Ukprn = 10000001,
                    ContractType = 1, CollectionPeriod = 6, AcademicYear = 1920, LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001, LearningAimReference = "ZPROG001", LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0, LearningAimFrameworkCode = 445, LearningAimPathwayCode = 3,
                    LearningStartDate = new DateTime(2018, 06, 16), AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2018, 02, 07),
                    EventTime = new DateTime(2020, 2, 7, 18, 56, 52),
                    CreationDate = new DateTime(2020, 02, 07), LearningAimSequenceNumber = 6
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

            appsMonthlyPaymentDasInfo.Payments = new List<AppsMonthlyPaymentDasPaymentModel>()
            {
                // mock data for defect 93224
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920, Ukprn = 10000001, LearnerReferenceNumber = "LR1001", LearnerUln = 1000000001,
                    LearningAimReference = "50089638", LearningStartDate = new DateTime(2019, 04, 13),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = string.Empty, ContractType = 1, TransactionType = 13, FundingSource = 4,
                    DeliveryPeriod = 2, CollectionPeriod = 4,
                    EarningEventId = new Guid("660E861F-1520-4F49-B84E-D75099483B1F")
                },
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920, Ukprn = 10000001, LearnerReferenceNumber = "LR1001", LearnerUln = 1000000001,
                    LearningAimReference = "50089638", LearningStartDate = new DateTime(2019, 04, 13),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = string.Empty, ContractType = 1, TransactionType = 13, FundingSource = 4,
                    DeliveryPeriod = 1, CollectionPeriod = 4,
                    EarningEventId = new Guid("660E861F-1520-4F49-B84E-D75099483B1F")
                },
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920, Ukprn = 10000001, LearnerReferenceNumber = "LR1001", LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001", LearningStartDate = new DateTime(2018, 06, 16),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019", ContractType = 1, TransactionType = 1,
                    FundingSource = 1, DeliveryPeriod = 2, CollectionPeriod = 2,
                    EarningEventId = new Guid("BC7FADB1-D943-454F-8591-809BD4A30E02")
                },
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920, Ukprn = 10000001, LearnerReferenceNumber = "LR1001", LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001", LearningStartDate = new DateTime(2018, 06, 16),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019", ContractType = 1, TransactionType = 1,
                    FundingSource = 1, DeliveryPeriod = 1, CollectionPeriod = 2,
                    EarningEventId = new Guid("BC7FADB1-D943-454F-8591-809BD4A30E02")
                },
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920, Ukprn = 10000001, LearnerReferenceNumber = "LR1001", LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001", LearningStartDate = new DateTime(2018, 06, 16),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019", ContractType = 1, TransactionType = 1,
                    FundingSource = 1, DeliveryPeriod = 3, CollectionPeriod = 3,
                    EarningEventId = new Guid("6D022948-C5E2-491C-9301-E858D7A99A9E")
                },
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920, Ukprn = 10000001, LearnerReferenceNumber = "LR1001", LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001", LearningStartDate = new DateTime(2018, 06, 16),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019", ContractType = 1, TransactionType = 1,
                    FundingSource = 1, DeliveryPeriod = 4, CollectionPeriod = 4,
                    EarningEventId = new Guid("5C60B697-C04F-43CA-9E03-5E3B008A4189")
                },
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920, Ukprn = 10000001, LearnerReferenceNumber = "LR1001", LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001", LearningStartDate = new DateTime(2018, 06, 16),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019", ContractType = 1, TransactionType = 2,
                    FundingSource = 1, DeliveryPeriod = 3, CollectionPeriod = 5,
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597")
                },
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920, Ukprn = 10000001, LearnerReferenceNumber = "LR1001", LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001", LearningStartDate = new DateTime(2018, 06, 16),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019", ContractType = 1, TransactionType = 1,
                    FundingSource = 1, DeliveryPeriod = 4, CollectionPeriod = 5,
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597")
                },
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920, Ukprn = 10000001, LearnerReferenceNumber = "LR1001", LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001", LearningStartDate = new DateTime(2018, 06, 16),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019", ContractType = 1, TransactionType = 3,
                    FundingSource = 1, DeliveryPeriod = 3, CollectionPeriod = 5,
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597")
                },
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920, Ukprn = 10000001, LearnerReferenceNumber = "LR1001", LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001", LearningStartDate = new DateTime(2018, 06, 16),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019", ContractType = 1, TransactionType = 1,
                    FundingSource = 1, DeliveryPeriod = 3, CollectionPeriod = 5,
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597")
                }
            };

            return appsMonthlyPaymentDasInfo;
        }
    }
}