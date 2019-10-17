using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Builders;
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
        [InlineData("EMPLOYER2", "EMPLOYER2", "A12345", "ZPROG001", "A12345", "ZPROG001", "T100", "T200")]
        public async Task TestAppsAdditionalPaymentsReportGeneration(
            string employerName,
            string employerNameExpected,
            string ilrLearnRefNumber,
            string ilrLearnAimRef,
            string dasLearnRefNumber,
            string dasLearnAimRef,
            string provSpecLearnMonOccurA,
            string provSpecLearnMonOccurB)
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10036143 Apps Additional Payments Report {dateTime:yyyyMMdd-HHmmss}";
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
            Mock<IFM36PeriodEndProviderService> fm36ProviderServiceMock = new Mock<IFM36PeriodEndProviderService>();
            IValueProvider valueProvider = new ValueProvider();
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((key, value, ct) => csv = value)
                .Returns(Task.CompletedTask);

            var appsAdditionalPaymentIlrInfo = BuildILRModel(ukPrn, ilrLearnRefNumber, ilrLearnAimRef, provSpecLearnMonOccurA, provSpecLearnMonOccurB);
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
            result.First().AugustEarnings.Should().Be(40);
            result.First().JulyEarnings.Should().Be(0);
            result.First().TotalEarnings.Should().Be(40);
            result.First().TotalPaymentsYearToDate.Should().Be(20);
            result.First().UniqueLearnerNumber.Should().Be(12345);
            result.First().EmployerNameFromApprenticeshipService.Should().Be(employerNameExpected);
        }

        private AppsAdditionalPaymentILRInfo BuildILRModel(int ukPrn, string ilrLearnRefNumber, string ilrLearnAimRef, string provSpecLearnMonOccurA, string provSpecLearnMonOccurB)
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
                                ProvSpecLearnMonOccur = provSpecLearnMonOccurA
                            },
                            new AppsAdditionalPaymentProviderSpecLearnerMonitoringInfo()
                            {
                                UKPRN = ukPrn,
                                LearnRefNumber = "1",
                                ProvSpecLearnMon = "B",
                                ProvSpecLearnMonOccur = provSpecLearnMonOccurB
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
                            Periods = new decimal?[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120 },
                            AttributeName = "PriceEpisodeFirstEmp1618Pay"
                        },
                        new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                        {
                            UKPRN = ukPrn,
                            LearnRefNumber = "A12345",
                            AimSeqNumber = 1,
                            PriceEpisodeIdentifier = "1",
                            Periods = new decimal?[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120 },
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