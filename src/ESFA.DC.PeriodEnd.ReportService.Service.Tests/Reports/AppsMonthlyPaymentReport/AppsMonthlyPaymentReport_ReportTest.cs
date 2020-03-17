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
    public sealed class AppsMonthlyPaymentReport_ReportTest
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
            Mock<IIlrPeriodEndProviderService> IlrPeriodEndProviderServiceMock =
                new Mock<IIlrPeriodEndProviderService>();
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

            var report = new ReportService.Service.Reports.AppsMonthlyPaymentReport(
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
            List<AppsMonthlyPaymentReportRowModel> result;
            TestCsvHelper.CheckCsv(csv, new CsvEntry(new AppsMonthlyPaymentMapper(), 1));
            using (var reader = new StreamReader($"{filename}.csv"))
            {
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.RegisterClassMap<AppsMonthlyPaymentMapper>();
                    result = csvReader.GetRecords<AppsMonthlyPaymentReportRowModel>().ToList();
                }
            }

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(2);

            var testValue = result.FirstOrDefault(r => r.PaymentLearningAimReference.Equals("ZPROG001"));

            testValue?.PaymentLearnerReferenceNumber.Should().Be("LR1001");
            testValue?.PaymentUniqueLearnerNumber.Should().Be(1000000001);
            testValue?.LearnerCampusIdentifier.Should().Be("C0471802");
            testValue?.ProviderSpecifiedLearnerMonitoringA.Should().Be("001");
            testValue?.ProviderSpecifiedLearnerMonitoringB.Should().Be("100102");
            testValue?.PaymentEarningEventAimSeqNumber.Should().Be(6);
            testValue?.PaymentLearningAimReference.Should().Be("ZPROG001");
            testValue?.LarsLearningDeliveryLearningAimTitle.Should().Be("Generic code to identify ILR programme aims");
            testValue?.LearningDeliveryOriginalLearningStartDate.Should().BeNull();
            testValue?.PaymentLearningStartDate.Should().Be(new DateTime(2018, 6, 16));
            testValue?.LearningDeliveryLearningPlannedEndDate.Should().Be(new DateTime(2020, 4, 16));
            testValue?.LearningDeliveryLearningActualEndDate.Should().Be(new DateTime(2019, 10, 8));
            testValue?.LearningDeliveryAchievementDate.Should().BeNull();
            testValue?.LearningDeliveryOutcome.Should().Be(1);
            testValue?.PaymentProgrammeType.Should().Be(2);
            testValue?.PaymentStandardCode.Should().Be(0);
            testValue?.PaymentFrameworkCode.Should().Be(445);
            testValue?.PaymentPathwayCode.Should().Be(3);
            testValue?.LearningDeliveryAimType.Should().Be(1);
            testValue?.LearningDeliverySoftwareSupplierAimIdentifier.Should().Be("62993A2E-3D84-4BFA-8D32-0B72F286C0B8");
            testValue?.LearningDeliveryFamTypeLearningDeliveryMonitoringA.Should().Be("356");
            testValue?.LearningDeliveryFamTypeLearningDeliveryMonitoringB.Should().Be("357");
            testValue?.LearningDeliveryFamTypeLearningDeliveryMonitoringC.Should().Be("358");
            testValue?.LearningDeliveryFamTypeLearningDeliveryMonitoringD.Should().Be("359");
            testValue?.LearningDeliveryFamTypeLearningDeliveryMonitoringE.Should().Be("360");
            testValue?.LearningDeliveryFamTypeLearningDeliveryMonitoringF.Should().BeNullOrEmpty();
            testValue?.ProviderSpecifiedDeliveryMonitoringA.Should().Be("1920");
            testValue?.ProviderSpecifiedDeliveryMonitoringB.Should().Be("E5072");
            testValue?.ProviderSpecifiedDeliveryMonitoringC.Should().Be("CHILD");
            testValue?.ProviderSpecifiedDeliveryMonitoringD.Should().Be("D006801");
            testValue?.LearningDeliveryEndPointAssessmentOrganisation.Should().BeNullOrEmpty();
            testValue?.RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim.Should().BeNull();
            testValue?.LearningDeliverySubContractedOrPartnershipUkprn.Should().BeNull();
            testValue?.PaymentPriceEpisodeStartDate.Should().Be("01/08/2019");
            testValue?.RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate.Should().Be(new DateTime(2019, 10, 8));
            testValue?.FcsContractContractAllocationContractAllocationNumber.Should().Be("YNLP-1157;YNLP-1158");
            testValue?.PaymentFundingLineType.Should().Be("19+ Apprenticeship (Employer on App Service) Levy funding");
            testValue?.PaymentApprenticeshipContractType.Should().Be(1);
            testValue?.LearnerEmploymentStatusEmployerId.Should().BeNull();
            testValue?.RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier.Should().Be("5YJB6B");
            testValue?.LearnerEmploymentStatus.Should().BeNull();
            testValue?.LearnerEmploymentStatusDate.Should().BeNull();

            // payments, Learner 1, Aim 1

            // August
            testValue?.AugustLevyPayments.Should().Be(0);
            testValue?.AugustCoInvestmentPayments.Should().Be(0);
            testValue?.AugustCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.AugustEmployerAdditionalPayments.Should().Be(0);
            testValue?.AugustProviderAdditionalPayments.Should().Be(0);
            testValue?.AugustApprenticeAdditionalPayments.Should().Be(0);
            testValue?.AugustEnglishAndMathsPayments.Should().Be(0);
            testValue?.AugustLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.AugustTotalPayments.Should().Be(0);

            // September
            testValue?.SeptemberLevyPayments.Should().Be(38);
            testValue?.SeptemberCoInvestmentPayments.Should().Be(0);
            testValue?.SeptemberCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.SeptemberEmployerAdditionalPayments.Should().Be(0);
            testValue?.SeptemberProviderAdditionalPayments.Should().Be(0);
            testValue?.SeptemberApprenticeAdditionalPayments.Should().Be(0);
            testValue?.SeptemberEnglishAndMathsPayments.Should().Be(0);
            testValue?.SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.SeptemberTotalPayments.Should().Be(38);

            // October
            testValue?.OctoberLevyPayments.Should().Be(22);
            testValue?.OctoberCoInvestmentPayments.Should().Be(0);
            testValue?.OctoberCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.OctoberEmployerAdditionalPayments.Should().Be(0);
            testValue?.OctoberProviderAdditionalPayments.Should().Be(0);
            testValue?.OctoberApprenticeAdditionalPayments.Should().Be(0);
            testValue?.OctoberEnglishAndMathsPayments.Should().Be(0);
            testValue?.OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.OctoberTotalPayments.Should().Be(22);

            // November
            testValue?.NovemberLevyPayments.Should().Be(24);
            testValue?.NovemberCoInvestmentPayments.Should().Be(0);
            testValue?.NovemberCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.NovemberEmployerAdditionalPayments.Should().Be(0);
            testValue?.NovemberProviderAdditionalPayments.Should().Be(0);
            testValue?.NovemberApprenticeAdditionalPayments.Should().Be(0);
            testValue?.NovemberEnglishAndMathsPayments.Should().Be(0);
            testValue?.NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.NovemberTotalPayments.Should().Be(24);

            // December
            testValue?.DecemberLevyPayments.Should().Be(116);
            testValue?.DecemberCoInvestmentPayments.Should().Be(0);
            testValue?.DecemberCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.DecemberEmployerAdditionalPayments.Should().Be(0);
            testValue?.DecemberProviderAdditionalPayments.Should().Be(0);
            testValue?.DecemberApprenticeAdditionalPayments.Should().Be(0);
            testValue?.DecemberEnglishAndMathsPayments.Should().Be(0);
            testValue?.DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.DecemberTotalPayments.Should().Be(116);

            // January
            testValue?.JanuaryLevyPayments.Should().Be(0);
            testValue?.JanuaryCoInvestmentPayments.Should().Be(0);
            testValue?.JanuaryCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.JanuaryEmployerAdditionalPayments.Should().Be(0);
            testValue?.JanuaryProviderAdditionalPayments.Should().Be(0);
            testValue?.JanuaryApprenticeAdditionalPayments.Should().Be(0);
            testValue?.JanuaryEnglishAndMathsPayments.Should().Be(0);
            testValue?.JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.JanuaryTotalPayments.Should().Be(0);

            // February
            testValue?.FebruaryLevyPayments.Should().Be(0);
            testValue?.FebruaryCoInvestmentPayments.Should().Be(0);
            testValue?.FebruaryCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.FebruaryEmployerAdditionalPayments.Should().Be(0);
            testValue?.FebruaryProviderAdditionalPayments.Should().Be(0);
            testValue?.FebruaryApprenticeAdditionalPayments.Should().Be(0);
            testValue?.FebruaryEnglishAndMathsPayments.Should().Be(0);
            testValue?.FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.FebruaryTotalPayments.Should().Be(0);

            // March
            testValue?.MarchLevyPayments.Should().Be(0);
            testValue?.MarchCoInvestmentPayments.Should().Be(0);
            testValue?.MarchCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.MarchEmployerAdditionalPayments.Should().Be(0);
            testValue?.MarchProviderAdditionalPayments.Should().Be(0);
            testValue?.MarchApprenticeAdditionalPayments.Should().Be(0);
            testValue?.MarchEnglishAndMathsPayments.Should().Be(0);
            testValue?.MarchLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.MarchTotalPayments.Should().Be(0);

            // April
            testValue?.AprilLevyPayments.Should().Be(0);
            testValue?.AprilCoInvestmentPayments.Should().Be(0);
            testValue?.AprilCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.AprilEmployerAdditionalPayments.Should().Be(0);
            testValue?.AprilProviderAdditionalPayments.Should().Be(0);
            testValue?.AprilApprenticeAdditionalPayments.Should().Be(0);
            testValue?.AprilEnglishAndMathsPayments.Should().Be(0);
            testValue?.AprilLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.AprilTotalPayments.Should().Be(0);

            // May
            testValue?.MayLevyPayments.Should().Be(0);
            testValue?.MayCoInvestmentPayments.Should().Be(0);
            testValue?.MayCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.MayEmployerAdditionalPayments.Should().Be(0);
            testValue?.MayProviderAdditionalPayments.Should().Be(0);
            testValue?.MayApprenticeAdditionalPayments.Should().Be(0);
            testValue?.MayEnglishAndMathsPayments.Should().Be(0);
            testValue?.MayLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.MayTotalPayments.Should().Be(0);

            // June
            testValue?.JuneLevyPayments.Should().Be(0);
            testValue?.JuneCoInvestmentPayments.Should().Be(0);
            testValue?.JuneCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.JuneEmployerAdditionalPayments.Should().Be(0);
            testValue?.JuneProviderAdditionalPayments.Should().Be(0);
            testValue?.JuneApprenticeAdditionalPayments.Should().Be(0);
            testValue?.JuneEnglishAndMathsPayments.Should().Be(0);
            testValue?.JuneLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.JuneTotalPayments.Should().Be(0);

            // July
            testValue?.JulyLevyPayments.Should().Be(0);
            testValue?.JulyCoInvestmentPayments.Should().Be(0);
            testValue?.JulyCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.JulyEmployerAdditionalPayments.Should().Be(0);
            testValue?.JulyProviderAdditionalPayments.Should().Be(0);
            testValue?.JulyApprenticeAdditionalPayments.Should().Be(0);
            testValue?.JulyEnglishAndMathsPayments.Should().Be(0);
            testValue?.JulyLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.JulyTotalPayments.Should().Be(0);

            // R13
            testValue?.R13LevyPayments.Should().Be(0);
            testValue?.R13CoInvestmentPayments.Should().Be(0);
            testValue?.R13CoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.R13EmployerAdditionalPayments.Should().Be(0);
            testValue?.R13ProviderAdditionalPayments.Should().Be(0);
            testValue?.R13ApprenticeAdditionalPayments.Should().Be(0);
            testValue?.R13EnglishAndMathsPayments.Should().Be(0);
            testValue?.R13LearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.R13TotalPayments.Should().Be(0);

            // R14
            testValue?.R14LevyPayments.Should().Be(0);
            testValue?.R14CoInvestmentPayments.Should().Be(0);
            testValue?.R14CoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.R14EmployerAdditionalPayments.Should().Be(0);
            testValue?.R14ProviderAdditionalPayments.Should().Be(0);
            testValue?.R14ApprenticeAdditionalPayments.Should().Be(0);
            testValue?.R14EnglishAndMathsPayments.Should().Be(0);
            testValue?.R14LearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.R14TotalPayments.Should().Be(0);

            testValue?.TotalLevyPayments.Should().Be(0);
            testValue?.TotalCoInvestmentPayments.Should().Be(0);
            testValue?.TotalCoInvestmentDueFromEmployerPayments.Should().Be(0);
            testValue?.TotalEmployerAdditionalPayments.Should().Be(0);
            testValue?.TotalProviderAdditionalPayments.Should().Be(0);
            testValue?.TotalApprenticeAdditionalPayments.Should().Be(0);
            testValue?.TotalEnglishAndMathsPayments.Should().Be(0);
            testValue?.TotalLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
            testValue?.TotalPayments.Should().Be(200);
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
                            // UKPRN     LearnRefNumber  LearnAimRef AimType AimSeqNumber  LearnStartDate  OrigLearnStartDate  LearnPlanEndDate  FundModel  ProgType  FworkCode  PwayCode  StdCode  PartnerUKPRN  DelLocPostCode  AddHours  PriorLearnFundAdj  OtherFundAdj  ConRefNumber  EPAOrgID  EmpOutcome  CompStatus  LearnActEndDate  WithdrawReason  Outcome  AchDate OutGrade  SWSupAimId                            PHours  LSDPostcode
                            // 10004718  420614          60154020    3       1             2018-06-16      NULL                2020-04-16        36         2         445        3         NULL     NULL          LE15 7GE        NULL      NULL               NULL          NULL          NULL      NULL        2           2019-10-08       NULL            1        NULL    PA        1ca1f1e7-44d6-4d1f-b19c-6b0c25fd216e  NULL    NULL
                            // 10004718  420614          60154020    4       2             2018-06-16      NULL                2019-06-16        99         NULL      NULL       NULL      NULL     NULL          LE15 7GE        NULL      NULL               NULL          NULL          NULL      NULL        2           2019-05-07       NULL            1        NULL    PA        6099ce59-f555-4546-9fdd-d742b0d79983  NULL    NULL
                            // 10004718  420614          50089638    3       3             2019-04-13      NULL                2019-10-13        36         2         445        3         NULL     NULL          LE15 7GE        NULL      NULL               NULL          NULL          NULL      NULL        2           2019-10-08       NULL            1        NULL    PA        a3997933-c6f1-47c9-bc6f-6ded04edb093  NULL    NULL
                            // 10004718  420614          50085098    3       4             2019-03-17      NULL                2020-04-16        36         2         445        3         NULL     NULL          LE15 7GE        NULL      NULL               NULL          NULL          NULL      NULL        2           2019-09-16       NULL            1        NULL    PA        4544bf6a-79a3-458e-b64d-875513f2d803  NULL    NULL
                            // 10004718  420614          50089080    3       5             2018-06-16      NULL                2019-06-16        36         2         445        3         NULL     NULL          LE15 7GE        NULL      NULL               NULL          NULL          NULL      NULL        2           2019-03-27       NULL            1        NULL    PA        cdd29b11-3dda-4860-92ad-1f9f1d9ca1b5  NULL    NULL
                            // 10004718  420614          ZPROG001    1       6             2018-06-16      NULL                2020-04-16        36         2         445        3         NULL     NULL          LE15 7GE        NULL      NULL               NULL          NULL          NULL      NULL        2           2019-10-08       NULL            1        NULL    NULL      51d76fe6-646f-4e8c-a3e1-34c30dad4a11  NULL    NULL
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
                                SwSupAimId = "1ca1f1e7-44d6-4d1f-b19c-6b0c25fd216e",
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
                                LearnAimRef = "60154020",
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
                                LearnStartDate = new DateTime(2019, 4, 13),
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
                    EventTime = new DateTime(2019, 11, 7, 21, 17, 48),
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
                    EarningEventId = new Guid("660E861F-1520-4F49-B84E-D75099483B1F"),
                    Amount = 1
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
                    EarningEventId = new Guid("660E861F-1520-4F49-B84E-D75099483B1F"),
                    Amount = 2
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
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000"),
                    Amount = 3,
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
                    EarningEventId = new Guid("BC7FADB1-D943-454F-8591-809BD4A30E02"),
                    Amount = 4
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
                    EarningEventId = new Guid("BC7FADB1-D943-454F-8591-809BD4A30E02"),
                    Amount = 5
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
                    EarningEventId = new Guid("6D022948-C5E2-491C-9301-E858D7A99A9E"),
                    Amount = 6
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
                    EarningEventId = new Guid("5C60B697-C04F-43CA-9E03-5E3B008A4189"),
                    Amount = 7
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
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597"),
                    Amount = 8
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
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597"),
                    Amount = 9
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
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597"),
                    Amount = 10
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
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597"),
                    Amount = 11
                },
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920, Ukprn = 10000001, LearnerReferenceNumber = "LR1001", LearnerUln = 1000000001,
                    LearningAimReference = "50089638", LearningStartDate = new DateTime(2019, 04, 13),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = string.Empty, ContractType = 1, TransactionType = 13, FundingSource = 4,
                    DeliveryPeriod = 2, CollectionPeriod = 4,
                    EarningEventId = new Guid("660E861F-1520-4F49-B84E-D75099483B1F"),
                    Amount = 12
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
                    EarningEventId = new Guid("660E861F-1520-4F49-B84E-D75099483B1F"),
                    Amount = 13
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
                    EarningEventId = new Guid("BC7FADB1-D943-454F-8591-809BD4A30E02"),
                    Amount = 14
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
                    EarningEventId = new Guid("BC7FADB1-D943-454F-8591-809BD4A30E02"),
                    Amount = 15
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
                    EarningEventId = new Guid("6D022948-C5E2-491C-9301-E858D7A99A9E"),
                    Amount = 16
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
                    EarningEventId = new Guid("5C60B697-C04F-43CA-9E03-5E3B008A4189"),
                    Amount = 17
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
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597"),
                    Amount = 18
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
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597"),
                    Amount = 19
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
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597"),
                    Amount = 20
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
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597"),
                    Amount = 21
                }
            };

            return appsMonthlyPaymentDasInfo;
        }
    }
}
