using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Builders.PeriodEnd;
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
    public sealed class TestAppsAdditionalPaymentsReport
    {
        [Theory]
        [InlineData("employer1", "employer1", "A12345", "ZPROG001", "A12345", "ZPROG001", "T180400007")]
        [InlineData(null, "", "A12345", "ZPROG001", "A12345", "ZPROG001", "T180400007")]
        [InlineData("EMPLOYER2", "employer2", "A12345", "ZPROG001", "A12345", "ZPROG001", "T180400007")]
        [InlineData("EMPLOYER2", "employer2", "A12345", "zprog001", "A12345", "ZPROG001", "T180400007")]
        [InlineData("EMPLOYER2", "employer2", "a12345", "zprog001", "A12345", "ZPROG001", "T180400007")]
        [InlineData("EMPLOYER2", "employer2", "a12345", "zprog001", "a12345", "ZPROG001", "T180400007")]
        [InlineData("EMPLOYER2", "employer2", "a12345", "zprog001", "A12345", "zprog001", "T180400007")]
        [InlineData("EMPLOYER2", "employer2", "a12345", "zprog001", "A12345", "ZPROG001", "t180400007")]
        public async Task TestAppsAdditionalPaymentsReportGeneration(
            string employerName,
            string employerNameExpected,
            string ilrLearnRefNumber,
            string ilrLearnAimRef,
            string dasLearnRefNumber,
            string dasLearnAimRef,
            string provSpecLearnMonOccur)
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"R01_10036143_Apps Additional Payments Report {dateTime:yyyyMMdd-HHmmss}";
            int ukPrn = 10036143;
            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(ukPrn);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(1);

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IIlrPeriodEndProviderService> ilrPeriodEndProviderServiceMock = new Mock<IIlrPeriodEndProviderService>();
            Mock<IDASPaymentsProviderService> dasPaymentProviderMock = new Mock<IDASPaymentsProviderService>();
            Mock<IRulebaseProviderService> fm36ProviderServiceMock = new Mock<IRulebaseProviderService>();
            IValueProvider valueProvider = new ValueProvider();
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((key, value, ct) => csv = value)
                .Returns(Task.CompletedTask);

            var appsAdditionalPaymentIlrInfo = BuildILRModel(ukPrn, ilrLearnRefNumber, ilrLearnAimRef, provSpecLearnMonOccur);
            var appsAdditionalPaymentRulebaseInfo = BuildFm36Model(ukPrn);
            var appsAdditionalPaymentDasPaymentsInfo = BuildDasPaymentsModel(ukPrn, employerName, dasLearnRefNumber, dasLearnAimRef);

            ilrPeriodEndProviderServiceMock.Setup(x => x.GetILRInfoForAppsAdditionalPaymentsReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appsAdditionalPaymentIlrInfo);
            fm36ProviderServiceMock.Setup(x => x.GetFM36DataForAppsAdditionalPaymentReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appsAdditionalPaymentRulebaseInfo);
            dasPaymentProviderMock.Setup(x => x.GetPaymentsInfoForAppsAdditionalPaymentsReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appsAdditionalPaymentDasPaymentsInfo);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);
            var appsAdditionalPaymentsModelBuilder = new AppsAdditionalPaymentsModelBuilder();

            var report = new AppsAdditionalPaymentsReport(
                logger.Object,
                storage.Object,
                ilrPeriodEndProviderServiceMock.Object,
                fm36ProviderServiceMock.Object,
                dateTimeProviderMock.Object,
                valueProvider,
                dasPaymentProviderMock.Object,
                appsAdditionalPaymentsModelBuilder);

            await report.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            TestCsvHelper.CheckCsv(csv, new CsvEntry(new AppsAdditionalPaymentsMapper(), 1));
            IEnumerable<AppsAdditionalPaymentsModel> result;

            using (var csvReader = new CsvReader(new StringReader(csv)))
            {
                csvReader.Configuration.RegisterClassMap<AppsAdditionalPaymentsMapper>();
                result = csvReader.GetRecords<AppsAdditionalPaymentsModel>().ToList();
            }

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(1);
            result.First().AprilEarnings.Should().Be(180);
            result.First().JulyEarnings.Should().Be(240);
            result.First().DecemberEarnings.Should().Be(100);
            result.First().TotalEarnings.Should().Be(1560);
            result.First().TotalPaymentsYearToDate.Should().Be(20);
            result.First().UniqueLearnerNumber.Should().Be(12345);
            result.First().EmployerNameFromApprenticeshipService.Should().Be(employerNameExpected);
        }

        private AppsAdditionalPaymentILRInfo BuildILRModel(int ukPrn, string ilrLearnRefNumber, string ilrLearnAimRef, string provSpecLearnMonOccur)
        {
            return new AppsAdditionalPaymentILRInfo()
            {
                UkPrn = ukPrn,
                Learners = new List<AppsAdditionalPaymentLearnerInfo>()
            {
                new AppsAdditionalPaymentLearnerInfo()
                {
                    LearnRefNumber = ilrLearnRefNumber,
                    ULN = 12345,
                    LearningDeliveries = new List<AppsAdditionalPaymentLearningDeliveryInfo>()
                    {
                        new AppsAdditionalPaymentLearningDeliveryInfo()
                        {
                            UKPRN = ukPrn,
                            LearnRefNumber = ilrLearnRefNumber,
                            LearnAimRef = ilrLearnAimRef,
                            AimType = 3,
                            AimSeqNumber = 1,
                            LearnStartDate = new DateTime(2019, 08, 28),
                            FundModel = 36,
                            ProgType = 1,
                            StdCode = 1,
                            FworkCode = 1,
                            PwayCode = 1
                        }
                    },
                    ProviderSpecLearnerMonitorings =
                        new List<AppsAdditionalPaymentProviderSpecLearnerMonitoringInfo>()
                        {
                            new AppsAdditionalPaymentProviderSpecLearnerMonitoringInfo()
                            {
                                UKPRN = ukPrn,
                                LearnRefNumber = "1",
                                ProvSpecLearnMon = "A",
                                ProvSpecLearnMonOccur = provSpecLearnMonOccur
                            },
                            new AppsAdditionalPaymentProviderSpecLearnerMonitoringInfo()
                            {
                                UKPRN = ukPrn,
                                LearnRefNumber = "1",
                                ProvSpecLearnMon = "B",
                                ProvSpecLearnMonOccur = "150563"
                            }
                        }
                }
            }
            };
        }

        private AppsAdditionalPaymentRulebaseInfo BuildFm36Model(int ukPrn)
        {
            return new AppsAdditionalPaymentRulebaseInfo()
            {
                UkPrn = ukPrn,
                AECLearningDeliveries = new List<AECLearningDeliveryInfo>(),
                AECApprenticeshipPriceEpisodePeriodisedValues =
                    new List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>()
                    {
                    new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = "A12345",
                        AimSeqNumber = 1,
                        PriceEpisodeIdentifier = "1",
                        Periods = new decimal[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120 },
                        AttributeName = "PriceEpisodeFirstEmp1618Pay"
                    },
                    new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = "A12345",
                        AimSeqNumber = 1,
                        PriceEpisodeIdentifier = "1",
                        Periods = new decimal[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120 },
                        AttributeName = "PriceEpisodeSecondProv1618Pay"
                    }
                    }
            };
        }

        private AppsAdditionalPaymentDasPaymentsInfo BuildDasPaymentsModel(int ukPrn, string employerName, string dasLearnRefNumber, string dasLearnAimRef)
        {
            return new AppsAdditionalPaymentDasPaymentsInfo()
            {
                UkPrn = ukPrn,
                Payments = new List<DASPaymentInfo>()
            {
                new DASPaymentInfo()
                {
                    UkPrn = ukPrn,
                    LearnerReferenceNumber = dasLearnRefNumber,
                    LearningAimReference = dasLearnAimRef,
                    LearnerUln = 12345,
                    LearningStartDate = new DateTime(2019, 08, 28),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    FundingSource = 1,
                    TransactionType = 4,
                    AcademicYear = 1920,
                    Amount = 10,
                    ContractType = 2,
                    CollectionPeriod = 1,
                    DeliveryPeriod = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy",
                    TypeOfAdditionalPayment = "Apprentice",
                    EmployerName = employerName
                },
                new DASPaymentInfo()
                {
                    UkPrn = ukPrn,
                    LearnerReferenceNumber = dasLearnRefNumber,
                    LearningAimReference = dasLearnAimRef,
                    LearnerUln = 12345,
                    LearningStartDate = new DateTime(2019, 08, 28),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    FundingSource = 1,
                    TransactionType = 6,
                    AcademicYear = 1920,
                    Amount = 10,
                    ContractType = 2,
                    CollectionPeriod = 1,
                    DeliveryPeriod = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy",
                    TypeOfAdditionalPayment = "Apprentice",
                    EmployerName = employerName
                }
            }
            };
        }
    }
}