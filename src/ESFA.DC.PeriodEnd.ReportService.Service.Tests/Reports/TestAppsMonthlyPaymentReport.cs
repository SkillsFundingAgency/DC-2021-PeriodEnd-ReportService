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
            // original mock data - reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10036143);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(ukPrn);
            // original mock data - reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(1);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(5);
            // original mock data - reportServiceContextMock.SetupGet(x => x.ReturnPeriodName).Returns("R01");
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

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(5);

            result[0].PaymentLearnerReferenceNumber.Should().Be("LR1001");
            result[0].PaymentUniqueLearnerNumber.Should().Be(1000000001);
            result[0].LearnerCampusIdentifier.Should().Be("C0471802");
            result[0].ProviderSpecifiedLearnerMonitoringA.Should().BeEmpty();
            result[0].ProviderSpecifiedLearnerMonitoringB.Should().BeEmpty();
            result[0].PaymentEarningEventAimSeqNumber.Should().BeNull();
            result[0].PaymentLearningAimReference.Should().Be("ZPROG001");
            result[0].LarsLearningDeliveryLearningAimTitle.Should().Be("Maths & English");
            result[0].LearningDeliveryOriginalLearningStartDate.Should().BeNull();
            result[0].PaymentLearningStartDate.Should().Be(new DateTime(2018, 6, 16));
//            result[0].LearningDeliveryLearningPlannedEndDate.Should().Be(new DateTime(2019, 10, 13));
//            result[0].LearningDeliveryLearningActualEndDate.Should().Be(new DateTime(2019, 10, 8));
//            result[0].LearningDeliveryAchievementDate.Should().BeNull();
//            result[0].LearningDeliveryOutcome.Should().Be(1);
            result[0].PaymentProgrammeType.Should().Be(2);
            result[0].PaymentStandardCode.Should().Be(0);
            result[0].PaymentFrameworkCode.Should().Be(445);
            result[0].PaymentPathwayCode.Should().Be(3);
//            result[0].LearningDeliveryAimType.Should().Be(3);
//            result[0].LearningDeliverySoftwareSupplierAimIdentifier.Should().Be("a3997933-c6f1-47c9-bc6f-6ded04edb093");
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringA.Should().BeNullOrEmpty();
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringB.Should().BeNullOrEmpty();
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringC.Should().BeNullOrEmpty();
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringD.Should().BeNullOrEmpty();
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringE.Should().BeNullOrEmpty();
            result[0].LearningDeliveryFamTypeLearningDeliveryMonitoringF.Should().BeNullOrEmpty();
            result[0].ProviderSpecifiedDeliveryMonitoringA.Should().BeNullOrEmpty();
            result[0].ProviderSpecifiedDeliveryMonitoringB.Should().BeNullOrEmpty();
            result[0].ProviderSpecifiedDeliveryMonitoringC.Should().BeNullOrEmpty();
            result[0].ProviderSpecifiedDeliveryMonitoringD.Should().BeNullOrEmpty();
            result[0].LearningDeliveryEndPointAssessmentOrganisation.Should().BeNullOrEmpty();
            //            result[0].RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim.Should().Be(2);
            //            result[0].LearningDeliverySubContractedOrPartnershipUkprn.Should().Be(10000001);
            //result[0].PaymentPriceEpisodeStartDate.Should().Be("1/8/2018");
            result[0].RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate.Should().BeNull();
//            result[0].FcsContractContractAllocationContractAllocationNumber.Should().Be("YNLP-1503");
//            result[0].PaymentFundingLineType.Should().Be("16-18 Apprenticeship Non-Levy Contract (procured)");
            result[0].PaymentApprenticeshipContractType.Should().Be(1);
//            result[0].LearnerEmploymentStatusEmployerId.Should().Be(56789);
//            result[0].RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier.Should().Be("PA102");
//            result[0].LearnerEmploymentStatus.Should().Be(10);
//            result[0].LearnerEmploymentStatusDate.Should().Be(new DateTime(2019, 08, 27));

            // payments, Learner 1, Aim 1

            // August
            result[0].AugustLevyPayments.Should().Be(0);
            result[0].AugustCoInvestmentPayments.Should().Be(0);
            result[0].AugustCoInvestmentDueFromEmployerPayments.Should().Be(0);
            result[0].AugustEmployerAdditionalPayments.Should().Be(0);
            result[0].AugustProviderAdditionalPayments.Should().Be(0);
            result[0].AugustApprenticeAdditionalPayments.Should().Be(0);
 //           result[0].AugustEnglishAndMathsPayments.Should().Be(17);
            result[0].AugustLearningSupportDisadvantageAndFrameworkUpliftPayments.Should().Be(0);
 //           result[0].AugustTotalPayments.Should().Be(17);

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
                    LearningAimTitle = "Maths & English"
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
                            // original mock data commented out for defect 93224
                            //new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                            //{
                            //    Ukprn = ukPrn,
                            //    LearnRefNumber = "A12345",
                            //    ProvSpecLearnMonOccur = "A",
                            //    ProvSpecLearnMon = "T180400007"
                            //},
                            //new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                            //{
                            //    Ukprn = ukPrn,
                            //    LearnRefNumber = "A12345",
                            //    ProvSpecLearnMonOccur = "B",
                            //    ProvSpecLearnMon = "150563"
                            //}
                        },

                        LearnerEmploymentStatus = new List<AppsMonthlyPaymentLearnerEmploymentStatusInfo>()
                        {
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                // DateEmpStatApp is after the LearnStartDate so this record should not be assigned
                                DateEmpStatApp = new DateTime(2018, 06, 15),
                                EmpStat = 10, // 10 In paid employment
                                EmpdId = 241837022,
                                AgreeId = "5YJB6B"
                            },
                        },

                        LearningDeliveries = new List<AppsMonthlyPaymentLearningDeliveryModel>
                        {
                            //UKPRN     LearnRefNumber  LearnAimRef AimType AimSeqNumber LearnStartDate  OrigLearnStartDate  LearnPlanEndDate  FundModel  ProgType  FworkCode  PwayCode
                            //10000001  LR1001          60154020    3       1            2018-06-16      NULL                2020-04-16        36         2         445        3

                            //StdCode   PartnerUKPRN    DelLocPostCode  AddHours  PriorLearnFundAdj  OtherFundAdj  ConRefNumber  EPAOrgID   EmpOutcome  CompStatus  LearnActEndDate  WithdrawReason
                            //NULL      NULL            LE15 7GE        NULL      NULL               NULL          NULL          NULL       NULL        2           2019-10-08       NULL

                            //Outcome  AchDate  OutGrade  SWSupAimId                             PHours  LSDPostcode
                            //1        NULL     PA        1ca1f1e7-44d6-4d1f-b19c-6b0c25fd216e   NULL    NULL
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
                                //PartnerUkprn = 10000001,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "1ca1f1e7-44d6-4d1f-b19c-6b0c25fd216e",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 10, 08),
                                Outcome = 1,
                                //AchDate = new DateTime(2020, 07, 30),
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
                                        AimSeqNumber = 1,
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
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 3,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    }
                                }
                            },
                            //UKPRN     LearnRefNumber  LearnAimRef AimType AimSeqNumber LearnStartDate  OrigLearnStartDate  LearnPlanEndDate  FundModel  ProgType  FworkCode  PwayCode
                            //10000001  LR1001          50115893    4       2            2018-06-16      NULL                2019-06-16        99         NULL      NULL       NULL

                            //StdCode PartnerUKPRN  DelLocPostCode  AddHours  PriorLearnFundAdj  OtherFundAdj  ConRefNumber  EPAOrgID   EmpOutcome  CompStatus  LearnActEndDate
                            //NULL    NULL          LE15 7GE        NULL      NULL               NULL          NULL          NULL       NULL        2           2019-05-07

                            //WithdrawReason  Outcome AchDate OutGrade  SWSupAimId                            PHours  LSDPostcode
                            //NULL            1       NULL    PA        6099ce59-f555-4546-9fdd-d742b0d79983  NULL    NULL
                            new AppsMonthlyPaymentLearningDeliveryModel()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                LearnAimRef = "50115893",
                                AimType = 4,
                                AimSeqNumber = 2,
                                LearnStartDate = new DateTime(2018, 6, 16),
                                //OrigLearnStartDate = new DateTime(2019, 08, 28),
                                LearnPlanEndDate = new DateTime(2019, 6, 16),
                                FundModel = 99,
                                ProgType = 0,
                                StdCode = 0,
                                FworkCode = 0,
                                PwayCode = 0,
                                //PartnerUkprn = 10000001,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "6099ce59-f555-4546-9fdd-d742b0d79983",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 5, 7),
                                Outcome = 1,
                                //AchDate = new DateTime(2020, 07, 30),
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
                                        AimSeqNumber = 1,
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
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 3,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    }
                                }
                            },
                            //UKPRN     LearnRefNumber  LearnAimRef AimType AimSeqNumber LearnStartDate  OrigLearnStartDate  LearnPlanEndDate  FundModel  ProgType  FworkCode  PwayCode
                            //10000001  LR1001          50089638    3       3            2019-04-13      NULL                2019-10-13        36         2         445        3

                            //StdCode PartnerUKPRN  DelLocPostCode  AddHours  PriorLearnFundAdj  OtherFundAdj  ConRefNumber  EPAOrgID   EmpOutcome  CompStatus  LearnActEndDate
                            //NULL    NULL          LE15 7GE        NULL      NULL               NULL          NULL          NULL       NULL        2           2019-10-08

                            //WithdrawReason  Outcome AchDate OutGrade  SWSupAimId                              PHours  LSDPostcode
                            //NULL            1       NULL    PA        a3997933-c6f1-47c9-bc6f-6ded04edb093    NULL    NULL
                            new AppsMonthlyPaymentLearningDeliveryModel()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                LearnAimRef = "50089638",
                                AimType = 3,
                                AimSeqNumber = 3,
                                LearnStartDate = new DateTime(2019, 4, 13),
                                //OrigLearnStartDate = new DateTime(2019, 08, 28),
                                LearnPlanEndDate = new DateTime(2019, 10, 13),
                                FundModel = 36,
                                ProgType = 2,
                                //StdCode = 1,
                                FworkCode = 445,
                                PwayCode = 3,
                                //PartnerUkprn = 10000001,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "a3997933-c6f1-47c9-bc6f-6ded04edb093",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 10, 08),
                                Outcome = 1,
                                //AchDate = new DateTime(2020, 07, 30),
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
                                        AimSeqNumber = 1,
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
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 3,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    }
                                }
                            },
                            //UKPRN     LearnRefNumber  LearnAimRef AimType AimSeqNumber LearnStartDate  OrigLearnStartDate  LearnPlanEndDate  FundModel  ProgType  FworkCode  PwayCode
                            //10000001  LR1001          50085098    3       4            2019-03-17      NULL                2020-04-16        36         2         445        3

                            //StdCode PartnerUKPRN  DelLocPostCode  AddHours  PriorLearnFundAdj  OtherFundAdj  ConRefNumber  EPAOrgID   EmpOutcome  CompStatus  LearnActEndDate
                            //NULL    NULL          LE15 7GE        NULL      NULL               NULL          NULL          NULL       NULL        2           2019-09-16

                            //WithdrawReason  Outcome AchDate OutGrade  SWSupAimId                              PHours  LSDPostcode
                            //NULL            1       NULL    PA        4544bf6a-79a3-458e-b64d-875513f2d803    NULL    NULL
                            new AppsMonthlyPaymentLearningDeliveryModel()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                LearnAimRef = "50085098",
                                AimType = 3,
                                AimSeqNumber = 4,
                                LearnStartDate = new DateTime(2019, 3, 17),
                                //OrigLearnStartDate = new DateTime(2019, 08, 28),
                                LearnPlanEndDate = new DateTime(2020, 4, 16),
                                FundModel = 36,
                                ProgType = 2,
                                //StdCode = 1,
                                FworkCode = 445,
                                PwayCode = 3,
                                //PartnerUkprn = 10000001,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "4544bf6a-79a3-458e-b64d-875513f2d803",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 9, 16),
                                Outcome = 1,
                                //AchDate = new DateTime(2020, 07, 30),
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
                                        AimSeqNumber = 1,
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
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 3,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    }
                                }
                            },
                            //UKPRN     LearnRefNumber  LearnAimRef AimType AimSeqNumber LearnStartDate  OrigLearnStartDate  LearnPlanEndDate  FundModel  ProgType  FworkCode  PwayCode
                            //10000001  LR1001          50089080    3       5            2018-06-16      NULL                2019-06-16        36         2         445        3

                            //StdCode PartnerUKPRN  DelLocPostCode  AddHours  PriorLearnFundAdj  OtherFundAdj  ConRefNumber  EPAOrgID   EmpOutcome  CompStatus  LearnActEndDate
                            //NULL    NULL          LE15 7GE        NULL      NULL               NULL          NULL          NULL       NULL        2           2019-03-27

                            //WithdrawReason Outcome AchDate OutGrade   SWSupAimId                            PHours  LSDPostcode
                            //NULL           1       NULL    PA         cdd29b11-3dda-4860-92ad-1f9f1d9ca1b5  NULL    NULL
                            new AppsMonthlyPaymentLearningDeliveryModel()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                LearnAimRef = "50089080",
                                AimType = 3,
                                AimSeqNumber = 5,
                                LearnStartDate = new DateTime(2018, 6, 16),
                                //OrigLearnStartDate = new DateTime(2019, 08, 28),
                                LearnPlanEndDate = new DateTime(2019, 6, 16),
                                FundModel = 36,
                                ProgType = 2,
                                //StdCode = 1,
                                FworkCode = 445,
                                PwayCode = 3,
                                //PartnerUkprn = 10000001,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "cdd29b11-3dda-4860-92ad-1f9f1d9ca1b5",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 3, 27),
                                Outcome = 1,
                                //AchDate = new DateTime(2020, 07, 30),
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
                                        AimSeqNumber = 1,
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
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 3,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    }
                                }
                            },
                            //UKPRN     LearnRefNumber  LearnAimRef AimType AimSeqNumber LearnStartDate  OrigLearnStartDate  LearnPlanEndDate  FundModel  ProgType  FworkCode  PwayCode
                            //10000001  LR1001          ZPROG001    1       6            2018-06-16      NULL                2020-04-16        36         2         445        3

                            //StdCode PartnerUKPRN  DelLocPostCode  AddHours  PriorLearnFundAdj  OtherFundAdj  ConRefNumber  EPAOrgID   EmpOutcome  CompStatus  LearnActEndDate
                            //NULL    NULL          LE15 7GE        NULL      NULL               NULL          NULL          NULL       NULL        2           2019-10-08

                            //WithdrawReason  Outcome AchDate OutGrade  SWSupAimId                              PHours  LSDPostcode
                            //NULL            8       NULL    NULL      51d76fe6-646f-4e8c-a3e1-34c30dad4a11    NULL    NULL
                            new AppsMonthlyPaymentLearningDeliveryModel()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "LR1001",
                                LearnAimRef = "ZPROG001",
                                AimType = 1,
                                AimSeqNumber = 6,
                                LearnStartDate = new DateTime(2018, 6, 16),
                                //OrigLearnStartDate = new DateTime(2019, 08, 28),
                                LearnPlanEndDate = new DateTime(2020, 4, 16),
                                FundModel = 36,
                                ProgType = 2,
                                //StdCode = 1,
                                FworkCode = 445,
                                PwayCode = 3,
                                //PartnerUkprn = 10000001,
                                ConRefNumber = string.Empty,
                                EpaOrgId = string.Empty,
                                SwSupAimId = "51d76fe6-646f-4e8c-a3e1-34c30dad4a11",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2019, 10, 08),
                                Outcome = 8,
                                //AchDate = new DateTime(2020, 07, 30),
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
                                        AimSeqNumber = 1,
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
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 3,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "LR1001",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "SOF",
                                        LearnDelFAMCode = "105"
                                    }
                                }
                            },
                            // original mock data commented out for defect 93224
                                //new AppsMonthlyPaymentLearningDeliveryModel()
                                //{
                                //    Ukprn = ukPrn,
                                //    LearnRefNumber = "A12345",
                                //    LearnAimRef = "50117889",
                                //    AimType = 3,
                                //    AimSeqNumber = 1,
                                //    LearnStartDate = new DateTime(2019, 08, 28),
                                //    OrigLearnStartDate = new DateTime(2019, 08, 28),
                                //    LearnPlanEndDate = new DateTime(2020, 07, 31),
                                //    FundModel = 36,
                                //    ProgType = 1,
                                //    StdCode = 1,
                                //    FworkCode = 1,
                                //    PwayCode = 1,
                                //    PartnerUkprn = 10000001,
                                //    ConRefNumber = "NLAP-1503",
                                //    EpaOrgId = "9876543210",
                                //    SwSupAimId = "SwSup50117889",
                                //    CompStatus = 2,
                                //    LearnActEndDate = new DateTime(2020, 07, 30),
                                //    Outcome = 4,
                                //    AchDate = new DateTime(2020, 07, 30),
                                //    ProviderSpecDeliveryMonitorings =
                                //        new List<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo>()
                                //        {
                                //            new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                //            {
                                //                Ukprn = ukPrn,
                                //                LearnRefNumber = "A12345",
                                //                AimSeqNumber = 1,
                                //                ProvSpecDelMonOccur = "A",
                                //                ProvSpecDelMon = "A000406"
                                //            },
                                //            new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                //            {
                                //                Ukprn = ukPrn,
                                //                LearnRefNumber = "A12345",
                                //                AimSeqNumber = 1,
                                //                ProvSpecDelMonOccur = "B",
                                //                ProvSpecDelMon = "B002902"
                                //            },
                                //            new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                //            {
                                //                Ukprn = ukPrn,
                                //                LearnRefNumber = "A12345",
                                //                AimSeqNumber = 1,
                                //                ProvSpecDelMonOccur = "C",
                                //                ProvSpecDelMon = "C004402"
                                //            },
                                //            new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                //            {
                                //                Ukprn = ukPrn,
                                //                LearnRefNumber = "A12345",
                                //                AimSeqNumber = 1,
                                //                ProvSpecDelMonOccur = "D",
                                //                ProvSpecDelMon = "D006801"
                                //            }
                                //        },
                                //    LearningDeliveryFams = new List<AppsMonthlyPaymentLearningDeliveryFAMInfo>()
                                //    {
                                //        new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                //        {
                                //            Ukprn = ukPrn,
                                //            LearnRefNumber = "A12345",
                                //            AimSeqNumber = 1,
                                //            LearnDelFAMType = "LDM1",
                                //            LearnDelFAMCode = "001"
                                //        },
                                //        new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                //        {
                                //            Ukprn = ukPrn,
                                //            LearnRefNumber = "A12345",
                                //            AimSeqNumber = 1,
                                //            LearnDelFAMType = "LDM2",
                                //            LearnDelFAMCode = "002"
                                //        },
                                //        new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                //        {
                                //            Ukprn = ukPrn,
                                //            LearnRefNumber = "A12345",
                                //            AimSeqNumber = 1,
                                //            LearnDelFAMType = "LDM3",
                                //            LearnDelFAMCode = "003"
                                //        },
                                //        new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                //        {
                                //            Ukprn = ukPrn,
                                //            LearnRefNumber = "A12345",
                                //            AimSeqNumber = 1,
                                //            LearnDelFAMType = "LDM4",
                                //            LearnDelFAMCode = "004"
                                //        },
                                //        new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                //        {
                                //            Ukprn = ukPrn,
                                //            LearnRefNumber = "A12345",
                                //            AimSeqNumber = 1,
                                //            LearnDelFAMType = "LDM5",
                                //            LearnDelFAMCode = "005"
                                //        },
                                //        new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                //        {
                                //            Ukprn = ukPrn,
                                //            LearnRefNumber = "A12345",
                                //            AimSeqNumber = 1,
                                //            LearnDelFAMType = "LDM6",
                                //            LearnDelFAMCode = "006"
                                //        }
                                //    }
                                //}
                            //}
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
                        ContractAllocations = new List<AppsMonthlyPaymentContractAllocationInfo>()
                        {
                            new AppsMonthlyPaymentContractAllocationInfo()
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
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //24351540   095347D4-5FB4-402B-9726-A4483FA56C78  10000001  1             7       1920          LR1001          1000000001  50089638

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                   LearningStartDate  AgreementId  IlrSubmissionDateTime            JobId
                //2         0        445        3         19 + Apprenticeship(Employer on App Service) 2019 - 04 - 13     NULL         2020 - 02 - 19 11:40:09.4070000  130173

                //EventTime                            CreationDate                         AimSeqNumber SfaContributionPercentage   IlrFileName                                                           EventType
                //2020-02-19 11:57:46.9414138 + 00:00  2020-02-19 11:58:14.8997343 + 00:00  3            NULL                        10000001 / ILR - 10000001 - 1920 - 20200206 - 121448 - 01 - Valid.XML SFA.DAS.Payments.EarningEvents.Messages.Events.Act1FunctionalSkillEarningsEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 24351540,
                    EventId = new Guid("095347D4-5FB4-402B-9726-A4483FA56C78"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 7,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089638",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = "19 + Apprenticeship(Employer on App Service)",
                    LearningStartDate = new DateTime(2018, 06, 16),
                    AgreementId = string.Empty,
                    IlrSubmissionDateTime = new DateTime(2020, 2, 19, 11, 40, 9),
                    EventTime = new DateTime(2020, 2, 19, 11, 57, 46),
                    CreationDate = new DateTime(2020, 2, 19, 11, 58, 14),
                    LearningAimSequenceNumber = 1,
                    SfaContributionPercentage = null,
                    IlrFileName = "10000001/ILR-10000001-1920-20200206-121448-01-Valid.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.Act1FunctionalSkillEarningsEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //9875758    660E861F-1520-4F49-B84E-D75099483B1F  10000001  1             4       1920          LR1001          1000000001  50089638

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType         LearningStartDate  AgreementId  IlrSubmissionDateTime            JobId
                //2         0        445 3   19 + Apprenticeship(Employer on App Service)    2019 - 04 - 13     NULL         2019 - 12 - 06 16:06:04.6830000 131060

                //EventTime                            CreationDate                         AimSeqNumber SfaContributionPercentage   IlrFileName                                                           EventType
                //2019-12-06 20:56:07.2362192 + 00:00  2019-12-06 20:57:31.0141287 + 00:00  3            NULL                        10000001 / ILR - 10000001 - 1920 - 20191205 - 152222 - 01.XML         SFA.DAS.Payments.EarningEvents.Messages.Events.Act1FunctionalSkillEarningsEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 9875758,
                    EventId = new Guid("660E861F-1520-4F49-B84E-D75099483B1F"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 4,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089638",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = "19 + Apprenticeship(Employer on App Service)",
                    LearningStartDate = new DateTime(2019, 04, 13),
                    AgreementId = string.Empty,
                    IlrSubmissionDateTime = new DateTime(2019, 12, 6, 16, 6, 4),
                    EventTime = new DateTime(2019, 12, 6, 20, 57, 7),
                    CreationDate = new DateTime(2019, 12, 6, 20, 57, 31),
                    LearningAimSequenceNumber = 3,
                    SfaContributionPercentage = null,
                    IlrFileName = "10000001/ILR-10000001-1920-20191205-152222-01.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.Act1FunctionalSkillEarningsEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //14658992   F65B7E77-C958-430A-8247-81A503B93B19  10000001  1             5       1920          LR1001          1000000001  50089638

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                    LearningStartDate            AgreementId  IlrSubmissionDateTime       JobId
                //2         0        445        3         19 + Apprenticeship(Employer on App Service)  2019-04-13 00:00:00.0000000  NULL         2020-01-08 14:13:51.7230000 155354

                //EventTime                            CreationDate                             AimSeqNumber SfaContributionPercentage   IlrFileName                                                           EventType
                //2020-01-08 14:56:48.2555543 + 00:00  2020 - 01 - 08 14:56:59.2364265 + 00:00  3            NULL                        10000001 / ILR - 10000001 - 1920 - 20200107 - 155216 - 01.XML   SFA.DAS.Payments.EarningEvents.Messages.Events.Act1FunctionalSkillEarningsEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 14658992,
                    EventId = new Guid("F65B7E77-C958-430A-8247-81A503B93B19"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 5,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089638",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = "19 + Apprenticeship(Employer on App Service)",
                    LearningStartDate = new DateTime(2019, 04, 13),
                    AgreementId = string.Empty,
                    IlrSubmissionDateTime = new DateTime(2020, 1, 8, 14, 13, 51),
                    EventTime = new DateTime(2020, 1, 8, 14, 56, 48),
                    CreationDate = new DateTime(2020, 1, 8, 14, 56, 59),
                    LearningAimSequenceNumber = 3,
                    SfaContributionPercentage = null,
                    IlrFileName = "10000001/ILR-10000001-1920-20200107-155216-01.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.Act1FunctionalSkillEarningsEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //20249225   39048CD9-67E9-4AE7-8723-173094DB1B5C  10000001  1             6       1920          LR1001          1000000001  50089638

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                    LearningStartDate           AgreementId  IlrSubmissionDateTime           JobId
                //2         0        445        3         19 + Apprenticeship(Employer on App Service)  2019-04-13 00:00:00.0000000 NULL         2020 - 02 - 07 17:28:44.8570000 181257

                //EventTime                            CreationDate                             AimSeqNumber SfaContributionPercentage   IlrFileName                                                           EventType
                //2020-02-07 18:56:52.6057454 + 00:00  2020 - 02 - 07 18:58:42.2275309 + 00:00  3            NULL                        10000001 / ILR - 10000001 - 1920 - 20200206 - 121448 - 01.XML   SFA.DAS.Payments.EarningEvents.Messages.Events.Act1FunctionalSkillEarningsEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 20249225,
                    EventId = new Guid("39048CD9-67E9-4AE7-8723-173094DB1B5C"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 6,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089638",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = "19 + Apprenticeship(Employer on App Service)",
                    LearningStartDate = new DateTime(2019, 04, 13),
                    AgreementId = string.Empty,
                    IlrSubmissionDateTime = new DateTime(2020, 2, 17, 17, 28, 44),
                    EventTime = new DateTime(2020, 2, 7, 18, 56, 52),
                    CreationDate = new DateTime(2020, 2, 7, 18, 58, 42),
                    LearningAimSequenceNumber = 3,
                    SfaContributionPercentage = null,
                    IlrFileName = "10000001/ILR-10000001-1920-20200206-121448-01.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.Act1FunctionalSkillEarningsEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //1376649    01C050C2-9C0B-4F03-A2C9-C4BD08CEB9FE  10000001  1             2       1920          LR1001          1000000001  ZPROG001

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                    LearningStartDate           AgreementId  IlrSubmissionDateTime       JobId
                //2         0        445        3         19 + Apprenticeship(Employer on App Service)  2018-06-16 00:00:00.0000000 5YJB6B       2019-10-04 09:39:58.8030000 49975

                //EventTime                            CreationDate                             AimSeqNumber SfaContributionPercentage   IlrFileName                                                           EventType
                //2019-10-04 09:43:04.2741875 + 00:00  2019 - 10 - 04 09:43:06.8690848 + 00:00  6            0.00000                     10000001 / ILR - 10000001 - 1920 - 20191004 - 101447 - 01 - Valid.XML SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 1376649,
                    EventId = new Guid("01C050C2-9C0B-4F03-A2C9-C4BD08CEB9FE"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 2,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = "19 + Apprenticeship(Employer on App Service)",
                    LearningStartDate = new DateTime(2018, 06, 16),
                    AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2019, 10, 4, 9, 39, 58),
                    EventTime = new DateTime(2019, 10, 4, 9, 34, 43),
                    CreationDate = new DateTime(2019, 10, 4, 9, 43, 6),
                    LearningAimSequenceNumber = 6,
                    SfaContributionPercentage = 0m,
                    IlrFileName = "10000001/ILR-10000001-1920-20191004-101447-01-Valid.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.Act1FunctionalSkillEarningsEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //1443478    79F8C510-3E59-45E9-A615-E302F4DFCAE1  10000001  1             2       1920          LR1001          1000000001  ZPROG001

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                    LearningStartDate           AgreementId  IlrSubmissionDateTime           JobId
                //2         0        445        3         19 + Apprenticeship(Employer on App Service)  2018-06-16 00:00:00.0000000 5YJB6B       2019 - 10 - 04 11:21:48.6870000 50663

                //EventTime                            CreationDate                         AimSeqNumber SfaContributionPercentage   IlrFileName                                                           EventType
                //2019-10-04 11:24:40.5145095 + 00:00  2019-10-04 11:24:48.6847251 + 00:00  6            0.00000                     10000001 / ILR - 10000001 - 1920 - 20191004 - 121117 - 01 - Valid.XML SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 1443478,
                    EventId = new Guid("79F8C510-3E59-45E9-A615-E302F4DFCAE1"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 2,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = "19 + Apprenticeship(Employer on App Service)",
                    LearningStartDate = new DateTime(2018, 06, 16),
                    AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2019, 10, 4, 11, 21, 48),
                    EventTime = new DateTime(2019, 10, 4, 11, 24, 40),
                    CreationDate = new DateTime(2019, 10, 4, 11, 24, 48),
                    LearningAimSequenceNumber = 6,
                    SfaContributionPercentage = 0m,
                    IlrFileName = "10000001/ILR-10000001-1920-20191004-121117-01-Valid.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //1618582    23F84DCA-ADD4-4D9A-9551-C4B328F7214C  10000001  1             2       1920          LR1001          1000000001  ZPROG001

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                    LearningStartDate           AgreementId  IlrSubmissionDateTime           JobId
                //2         0        445        3         19 + Apprenticeship(Employer on App Service)  2018-06-16 00:00:00.0000000 5YJB6B       2019 - 10 - 04 15:00:50.9670000 51878

                //EventTime                            CreationDate                         AimSeqNumber SfaContributionPercentage   IlrFileName                                              EventType
                //2019-10-04 15:03:22.6059023 + 00:00  2019-10-04 15:03:24.9272560 + 00:00  6            0.00000                     10000001/ILR-10000001-1920-20191004-155137-01-Valid.XML  SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 1618582,
                    EventId = new Guid("23F84DCA-ADD4-4D9A-9551-C4B328F7214C"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 2,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = "19 + Apprenticeship(Employer on App Service)",
                    LearningStartDate = new DateTime(2018, 06, 16),
                    AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2019, 10, 4, 11, 21, 48),
                    EventTime = new DateTime(2019, 10, 4, 15, 3, 22),
                    CreationDate = new DateTime(2019, 10, 4, 15, 3, 24),
                    LearningAimSequenceNumber = 6,
                    SfaContributionPercentage = 0m,
                    IlrFileName = "10000001/ILR-10000001-1920-20191004-155137-01-Valid.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //1776657    BC7FADB1-D943-454F-8591-809BD4A30E02  10000001  1             2       1920          LR1001          1000000001  ZPROG001

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                    LearningStartDate           AgreementId  IlrSubmissionDateTime        JobId
                //2         0        445        3         19 + Apprenticeship(Employer on App Service)  2018-06-16 00:00:00.0000000 5YJB6B       2019-10-04 19:01:53.7930000  52675

                //EventTime                            CreationDate                         AimSeqNumber SfaContributionPercentage   IlrFileName                                         EventType
                //2019-10-04 19:22:29.4501517 + 00:00  2019-10-04 19:22:42.1232356 + 00:00  6            0.00000                     10000001/ILR-10000001-1920-20191004-155137-01.XML   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 1776657,
                    EventId = new Guid("BC7FADB1-D943-454F-8591-809BD4A30E02"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 2,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = "19 + Apprenticeship(Employer on App Service)",
                    LearningStartDate = new DateTime(2018, 06, 16),
                    AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2019, 10, 4, 19, 1, 48),
                    EventTime = new DateTime(2019, 10, 4, 19, 22, 29),
                    CreationDate = new DateTime(2019, 10, 4, 19, 22, 24),
                    LearningAimSequenceNumber = 6,
                    SfaContributionPercentage = 0m,
                    IlrFileName = "10000001/ILR-10000001-1920-20191004-155137-01.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //4677582    6D022948-C5E2-491C-9301-E858D7A99A9E  10000001  1             3       1920          LR1001          1000000001  ZPROG001

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                    LearningStartDate           AgreementId  IlrSubmissionDateTime        JobId
                //2         0        445        3         NULL                                          2018-06-16 00:00:00.0000000 5YJB6B       2019-11-06 20:40:23.0930000  93290

                //EventTime                            CreationDate                         AimSeqNumber SfaContributionPercentage   IlrFileName                                         EventType
                //2019-11-07 21:17:48.8268934 + 00:00  2019-11-07 21:43:46.0646980 + 00:00  6            0.00000                     10000001/ILR-10000001-1920-20191105-211349-01.XML   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 4677582,
                    EventId = new Guid("6D022948-C5E2-491C-9301-E858D7A99A9E"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 3,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = string.Empty,
                    LearningStartDate = new DateTime(2018, 06, 16),
                    AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2019, 11, 6, 20, 40, 23),
                    EventTime = new DateTime(2019, 11, 7, 21, 17, 48),
                    CreationDate = new DateTime(2019, 11, 7, 21, 43, 46),
                    LearningAimSequenceNumber = 6,
                    SfaContributionPercentage = 0m,
                    IlrFileName = "10000001/ILR-10000001-1920-20191105-211349-01.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //24351538   CDD5249F-F265-4E0E-AF2F-DA4B0D42E3B4  10000001  1             7       1920          LR1001          1000000001  ZPROG001

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                    LearningStartDate           AgreementId  IlrSubmissionDateTime        JobId
                //2         0        445        3          NULL                                         2018-06-16 00:00:00.0000000 5YJB6B       2020-02-19 11:40:09.4070000  130173

                //EventTime                            CreationDate                         AimSeqNumber SfaContributionPercentage   IlrFileName                                             EventType
                //2020-02-19 11:57:46.9411554 + 00:00  2020-02-19 11:58:14.8997343 + 00:00  6            0.00000                     10000001/ILR-10000001-1920-20200206-121448-01-Valid.XML SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 24351538,
                    EventId = new Guid("CDD5249F-F265-4E0E-AF2F-DA4B0D42E3B4"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 7,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = string.Empty,
                    LearningStartDate = new DateTime(2018, 06, 16),
                    AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2020, 2, 19, 11, 40, 9),
                    EventTime = new DateTime(2020, 2, 19, 11, 57, 46),
                    CreationDate = new DateTime(2020, 2, 19, 11, 58, 14),
                    LearningAimSequenceNumber = 6,
                    SfaContributionPercentage = 0m,
                    IlrFileName = "10000001/ILR-10000001-1920-20200206-121448-01-Valid.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //9875760    5C60B697-C04F-43CA-9E03-5E3B008A4189  10000001  1             4       1920          LR1001          1000000001  ZPROG001

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                    LearningStartDate           AgreementId  IlrSubmissionDateTime        JobId
                //2         0        445        3         NULL                                          2018-06-16 00:00:00.0000000 5YJB6B       2019-12-06 16:06:04.6830000  131060

                //EventTime                            CreationDate                         AimSeqNumber SfaContributionPercentage   IlrFileName                                         EventType
                //2019-12-06 20:56:07.2358980 + 00:00  2019-12-06 20:57:31.0141287 + 00:00  6            0.00000                     10000001/ILR-10000001-1920-20191205-152222-01.XML   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 9875760,
                    EventId = new Guid("5C60B697-C04F-43CA-9E03-5E3B008A4189"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 4,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = string.Empty,
                    LearningStartDate = new DateTime(2018, 06, 16),
                    AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2019, 12, 6, 16, 6, 4),
                    EventTime = new DateTime(2019, 12, 16, 20, 56, 7),
                    CreationDate = new DateTime(2019, 12, 6, 20, 57, 31),
                    LearningAimSequenceNumber = 6,
                    SfaContributionPercentage = 0m,
                    IlrFileName = "10000001/ILR-10000001-1920-20191205-152222-01.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //14658993   822572E4-290E-4BBD-A414-7EDCC8D3B597  10000001  1             5       1920          LR1001          1000000001  ZPROG001

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                    LearningStartDate           AgreementId  IlrSubmissionDateTime        JobId
                //2         0        445        3         NULL                                          2018-06-16 00:00:00.0000000 5YJB6B       2020-01-08 14:13:51.7230000  155354

                //EventTime                            CreationDate                         AimSeqNumber SfaContributionPercentage   IlrFileName                                         EventType
                //2020-01-08 14:56:48.2552900 + 00:00  2020-01-08 14:56:59.2364265 + 00:00  6            0.00000                     10000001/ILR-10000001-1920-20200107-155216-01.XML   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 14658993,
                    EventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 5,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = string.Empty,
                    LearningStartDate = new DateTime(2018, 06, 16),
                    AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2020, 1, 8, 14, 13, 51),
                    EventTime = new DateTime(2020, 1, 8, 14, 56, 48),
                    CreationDate = new DateTime(2020, 1, 8, 14, 56, 59),
                    LearningAimSequenceNumber = 6,
                    SfaContributionPercentage = 0m,
                    IlrFileName = "10000001/ILR-10000001-1920-20200107-155216-01.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent"
                },
                //Id         EventId                               Ukprn     ContractType  Period  AcademicYear  LearnRefNumber  LearnerUln  LearningAimReference
                //20249229   5AC5396C-B665-4084-9134-E7550546433A  10000001  1             6       1920          LR1001          1000000001  ZPROG001

                //ProgType  StdCode  FworkCode  PwayCode  LearningAimFundingLineType                    LearningStartDate           AgreementId  IlrSubmissionDateTime        JobId
                //2         0        445        3         NULL                                          2018-06-16 00:00:00.0000000 5YJB6B       2020-02-07 17:28:44.8570000  181257

                //EventTime                            CreationDate                         AimSeqNumber SfaContributionPercentage   IlrFileName                                         EventType
                //2020-02-07 18:56:52.6054614 + 00:00  2020-02-07 18:58:42.2275309 + 00:00  6            0.00000                     10000001/ILR-10000001-1920-20200206-121448-01.XML   SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    Id = 20249229,
                    EventId = new Guid("5AC5396C-B665-4084-9134-E7550546433A"),
                    Ukprn = ukPrn,
                    ContractType = 1,
                    CollectionPeriod = 6,
                    AcademicYear = 1920,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    LearningAimFundingLineType = string.Empty,
                    LearningStartDate = new DateTime(2018, 06, 16),
                    AgreementId = "5YJB6B",
                    IlrSubmissionDateTime = new DateTime(2020, 2, 7, 17, 28, 44),
                    EventTime = new DateTime(2020, 2, 7, 18, 56, 52),
                    CreationDate = new DateTime(2020, 2, 7, 18, 58, 42),
                    LearningAimSequenceNumber = 6,
                    SfaContributionPercentage = 0m,
                    IlrFileName = "10000001/ILR-10000001-1920-20200206-121448-01.XML",
                    EventType = "SFA.DAS.Payments.EarningEvents.Messages.Events.ApprenticeshipContractType1EarningEvent"
                },
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
                //Ukprn   LearnerReferenceNumber  LearnerUln  LearningAimReference    LearningStartDate   PriceEpisodeIdentifier  PriceEpisodeStartDate   LearningAimProgrammeType    LearningAimStandardCode LearningAimFrameworkCode    LearningAimPathwayCode  ReportingAimFundingLineType EarningEventId  ContractType    TransactionType FundingSource   DeliveryPeriod  CollectionPeriod
                //10000001    LR1001  1000000001  50089638    2019-04-13 00:00:00.0000000         2   0   445 3   19+ Apprenticeship (Employer on App Service) Levy funding   660E861F-1520-4F49-B84E-D75099483B1F    1   13  4   1   4
                //10000001    LR1001  1000000001  50089638    2019-04-13 00:00:00.0000000         2   0   445 3   19+ Apprenticeship (Employer on App Service) Levy funding   660E861F-1520-4F49-B84E-D75099483B1F    1   13  4   2   4
                //10000001    LR1001  1000000001  50089080    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   14  4   8   8
                //10000001    LR1001  1000000001  50089080    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   13  4   7   7
                //10000001    LR1001  1000000001  50089080    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   13  4   6   6
                //10000001    LR1001  1000000001  50089080    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   13  4   5   5
                //10000001    LR1001  1000000001  50089080    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   13  4   4   4
                //10000001    LR1001  1000000001  50089080    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   13  4   3   3
                //10000001    LR1001  1000000001  50089080    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   13  4   1   3
                //10000001    LR1001  1000000001  50089080    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   13  4   2   3
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   12  12
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   11  11
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   7   7
                //10000001    LR1001  1000000001  50089638    2019-04-13 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   13  4   12  12
                //10000001    LR1001  1000000001  50089638    2019-04-13 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   13  4   11  11
                //10000001    LR1001  1000000001  50089638    2019-04-13 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   13  4   10  10
                //10000001    LR1001  1000000001  50089638    2019-04-13 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   13  4   9   9
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   10  10
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   9   9
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   8   8
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   6   6
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   5   5
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   4   4
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   1   3
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   2   3
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2018  01/08/2018  2   0   445 3   NULL    00000000-0000-0000-0000-000000000000    1   1   1   3   3
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2019  01/08/2019  2   0   445 3   19+ Apprenticeship (Employer on App Service) Levy funding   BC7FADB1-D943-454F-8591-809BD4A30E02    1   1   1   1   2
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2019  01/08/2019  2   0   445 3   19+ Apprenticeship (Employer on App Service) Levy funding   BC7FADB1-D943-454F-8591-809BD4A30E02    1   1   1   2   2
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2019  01/08/2019  2   0   445 3   19+ Apprenticeship (Employer on App Service) Levy funding   6D022948-C5E2-491C-9301-E858D7A99A9E    1   1   1   3   3
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2019  01/08/2019  2   0   445 3   19+ Apprenticeship (Employer on App Service) Levy funding   5C60B697-C04F-43CA-9E03-5E3B008A4189    1   1   1   4   4
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2019  01/08/2019  2   0   445 3   19+ Apprenticeship (Employer on App Service) Levy funding   822572E4-290E-4BBD-A414-7EDCC8D3B597    1   1   1   3   5
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2019  01/08/2019  2   0   445 3   19+ Apprenticeship (Employer on App Service) Levy funding   822572E4-290E-4BBD-A414-7EDCC8D3B597    1   3   1   3   5
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2019  01/08/2019  2   0   445 3   19+ Apprenticeship (Employer on App Service) Levy funding   822572E4-290E-4BBD-A414-7EDCC8D3B597    1   1   1   4   5
                //10000001    LR1001  1000000001  ZPROG001    2018-06-16 00:00:00.0000000 2-445-3-01/08/2019  01/08/2019  2   0   445 3   19+ Apprenticeship (Employer on App Service) Levy funding   822572E4-290E-4BBD-A414-7EDCC8D3B597    1   2   1   3   5

                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089638     2019-04-13                                               2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //19 + Apprenticeship(Employer on App Service) Levy funding   1             13              4             1               4
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089638",
                    LearningStartDate = new DateTime(2019, 4, 13),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = string.Empty,
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 1,
                    CollectionPeriod = 1,
                    EarningEventId = new Guid("660E861F-1520-4F49-B84E-D75099483B1F")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089638      2019-04-13                                              2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //19 + Apprenticeship(Employer on App Service) Levy funding   1             13              4             2               4
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089638",
                    LearningStartDate = new DateTime(2019, 4, 13),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = string.Empty,
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 2,
                    CollectionPeriod = 4,
                    EarningEventId = new Guid("660E861F-1520-4F49-B84E-D75099483B1F")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089080      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             14              4               8             8
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089080",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 14,
                    FundingSource = 4,
                    DeliveryPeriod = 8,
                    CollectionPeriod = 8,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089080      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             13              4               7             7
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089080",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 7,
                    CollectionPeriod = 7,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089080      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             13              4               6             6
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089080",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 6,
                    CollectionPeriod = 6,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089080      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             13              4               5             5
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089080",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 5,
                    CollectionPeriod = 5,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089080      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             13              4               4             4
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089080",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 4,
                    CollectionPeriod = 4,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089080      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             13              4               3             3
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089080",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 3,
                    CollectionPeriod = 3,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089080      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3
                //
                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             13              4               1             3
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089080",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 1,
                    CollectionPeriod = 3,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089080      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             13              4               2             3
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089080",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 2,
                    CollectionPeriod = 3,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               11            11
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 11,
                    CollectionPeriod = 11,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089638      2019-04-13      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               11            11
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089638",
                    LearningStartDate = new DateTime(2019, 4, 13),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 11,
                    CollectionPeriod = 11,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089638      2019-04-13      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             13              4               11            11
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089638",
                    LearningStartDate = new DateTime(2019, 4, 13),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 11,
                    CollectionPeriod = 11,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089638      2019-04-13      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             13              4               10            10
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089638",
                    LearningStartDate = new DateTime(2019, 4, 13),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 10,
                    CollectionPeriod = 10,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  50089638      2019-04-13      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             13              4               9             9
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "50089638",
                    LearningStartDate = new DateTime(2019, 4, 13),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 13,
                    FundingSource = 4,
                    DeliveryPeriod = 9,
                    CollectionPeriod = 9,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               5             5
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 5,
                    CollectionPeriod = 5,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               12            12
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 12,
                    CollectionPeriod = 12,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3
                //
                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               10            10
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 10,
                    CollectionPeriod = 10,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               9             9
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 9,
                    CollectionPeriod = 9,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               8             8
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 8,
                    CollectionPeriod = 8,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               7             7
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 7,
                    CollectionPeriod = 7,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               6             6
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 6,
                    CollectionPeriod = 6,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               4             4
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 4,
                    CollectionPeriod = 4,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               1             3
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = 3,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               2             3
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 2,
                    CollectionPeriod = 3,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2018    01 / 08 / 2018    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //NULL                                                        1             1               1               3             3
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = string.Empty,
                    PriceEpisodeIdentifier = "2-445-3-01/08/2018",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 3,
                    CollectionPeriod = 3,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2019    01 / 08 / 2019    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //19 + Apprenticeship(Employer on App Service) Levy funding   1             1               1               4             4
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 4,
                    CollectionPeriod = 4,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2019    01 / 08 / 2019    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //19 + Apprenticeship(Employer on App Service) Levy funding   1             1               1               3             3
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 3,
                    CollectionPeriod = 3,
                    EarningEventId = new Guid("BC7FADB1-D943-454F-8591-809BD4A30E02")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2019    01 / 08 / 2019    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //19 + Apprenticeship(Employer on App Service) Levy funding   1             1               1               3             3
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 3,
                    CollectionPeriod = 3,
                    EarningEventId = new Guid("BC7FADB1-D943-454F-8591-809BD4A30E02")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2019    01 / 08 / 2019    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //19 + Apprenticeship(Employer on App Service) Levy funding   1             1               1               1             2
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = 2,
                    EarningEventId = new Guid("6D022948-C5E2-491C-9301-E858D7A99A9E")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2019    01 / 08 / 2019    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //19 + Apprenticeship(Employer on App Service) Levy funding   1             1               1               2             2
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 2,
                    CollectionPeriod = 2,
                    EarningEventId = new Guid("5C60B697-C04F-43CA-9E03-5E3B008A4189")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2019    01 / 08 / 2019    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //19 + Apprenticeship(Employer on App Service) Levy funding   1             1               1               3             5
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 3,
                    CollectionPeriod = 5,
                    //EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2019    01 / 08 / 2019    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //19 + Apprenticeship(Employer on App Service) Levy funding   1             3               1               3             5
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019",
                    ContractType = 1,
                    TransactionType = 3,
                    FundingSource = 1,
                    DeliveryPeriod = 3,
                    CollectionPeriod = 5,
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2019    01 / 08 / 2019    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //19 + Apprenticeship(Employer on App Service) Levy funding   1             1               1               4             5
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019",
                    ContractType = 1,
                    TransactionType = 1,
                    FundingSource = 1,
                    DeliveryPeriod = 4,
                    CollectionPeriod = 5,
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597")
                },
                //Ukprn     LearnRefNum  Uln         LearngAimRef  LearnStartDate  PriceEpisodeId        PriceEpStartDate  ProgType  StdCode  FworkCode  PwayCode
                //10000001  LR1001       1000000001  ZPROG001      2018-06-16      2-445-3-01/08/2019    01 / 08 / 2019    2         0        445        3

                //ReportingAimFundingLineType                                 ContractType  TransactionType FundingSource DeliveryPeriod  CollectionPeriod
                //19 + Apprenticeship(Employer on App Service) Levy funding   1             2               1               3             5
                new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = 10000001,
                    LearnerReferenceNumber = "LR1001",
                    LearnerUln = 1000000001,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2018, 6, 16),
                    LearningAimProgrammeType = 2,
                    LearningAimStandardCode = 0,
                    LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19 + Apprenticeship(Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = "2-445-3-01/08/2019",
                    ContractType = 1,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 3,
                    CollectionPeriod = 5,
                    EarningEventId = new Guid("822572E4-290E-4BBD-A414-7EDCC8D3B597")
                },
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