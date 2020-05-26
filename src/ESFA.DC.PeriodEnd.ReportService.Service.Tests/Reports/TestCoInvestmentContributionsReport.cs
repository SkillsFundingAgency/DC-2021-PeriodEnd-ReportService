using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR2021.DataStore.EF.Valid;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Builders;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Mapper;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Reports;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Service.Tests.Helpers;
using ESFA.DC.PeriodEnd.ReportService.Service.Tests.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Tests.Reports
{
    public sealed class TestCoInvestmentContributionsReport
    {
        //[Theory]
        //[InlineData("EMPLOYER2", "EMPLOYER2", "A12345", "ZPROG001", "A12345", "ZPROG001")]
        public async Task TestCoInvestmentContributionsReportGeneration(
            string employerName,
            string employerNameExpected,
            string ilrLearnRefNumber,
            string ilrLearnAimRef,
            string dasLearnRefNumber,
            string dasLearnAimRef)
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10036143 Apps Co-Investment Contributions Report {dateTime:yyyyMMdd-HHmmss}";
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
            Mock<IPersistReportData> persistReportDataMock = new Mock<IPersistReportData>();

            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((key, value, ct) => csv = value)
                .Returns(Task.CompletedTask);

            var appsCoInvestmentIlrInfo = BuildILRModel(ukPrn, ilrLearnRefNumber, ilrLearnAimRef, 1);
            var appsCoInvestmentRulebaseInfo = BuildFm36Model(ukPrn, ilrLearnRefNumber, 1, ilrLearnAimRef);
            var appsCoInvestmentDasPaymentsInfo = BuildDasPaymentsModel(ukPrn, employerName, dasLearnRefNumber, dasLearnAimRef);

            ilrPeriodEndProviderServiceMock.Setup(x => x.GetILRInfoForAppsCoInvestmentReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appsCoInvestmentIlrInfo);
            fm36ProviderServiceMock.Setup(x => x.GetFM36DataForAppsCoInvestmentReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appsCoInvestmentRulebaseInfo);
            dasPaymentProviderMock.Setup(x => x.GetPaymentsInfoForAppsCoInvestmentReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appsCoInvestmentDasPaymentsInfo);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);
            var appsCoInvestmentContributionsModelBuilder = new AppsCoInvestmentContributionsModelBuilder(new AppsCoInvestmentRecordKeyEqualityComparer(), logger.Object);

            var report = new AppsCoInvestmentContributionsReport(
                logger.Object,
                storage.Object,
                dateTimeProviderMock.Object,
                ilrPeriodEndProviderServiceMock.Object,
                dasPaymentProviderMock.Object,
                fm36ProviderServiceMock.Object,
                appsCoInvestmentContributionsModelBuilder,
                persistReportDataMock.Object);

            await report.GenerateReport(reportServiceContextMock.Object, null, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            File.WriteAllText($"{filename}.csv", csv);
            TestCsvHelper.CheckCsv(csv, new CsvEntry(new AppsCoInvestmentContributionsMapper(), 1));
            IEnumerable<AppsCoInvestmentContributionsModel> result;

            using (var csvReader = new CsvReader(new StringReader(csv)))
            {
                csvReader.Configuration.RegisterClassMap<AppsCoInvestmentContributionsMapper>();
                result = csvReader.GetRecords<AppsCoInvestmentContributionsModel>().ToList();
            }

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(1);
            result.First().CoInvestmentDueFromEmployerForAugust.Should().Be(20);
            result.First().CompletionPaymentsThisFundingYear.Should().Be(20);

            result.First().UniqueLearnerNumber.Should().Be(12345);
            result.First().EmployerNameFromApprenticeshipService.Should().Be(employerNameExpected);
        }

        [Fact]
        public void RecordKeysUnion_Test()
        {
            List<string> learnRefNumbers = new List<string>() { "055300807083", "055300807081" };

            List<AppsCoInvestmentRecordKey> appsKeys = new List<AppsCoInvestmentRecordKey>()
            {
               new AppsCoInvestmentRecordKey() { LearnerReferenceNumber = "055300807083", LearningAimReference = "ZPROG001", LearningStartDate = null, LearningAimProgrammeType = 3, LearningAimStandardCode = 0, LearningAimFrameworkCode = 462, LearningAimPathwayCode = 1 },
               new AppsCoInvestmentRecordKey() { LearnerReferenceNumber = "055300807083", LearningAimReference = "ZPROG001", LearningStartDate = null, LearningAimProgrammeType = 3, LearningAimStandardCode = 0, LearningAimFrameworkCode = 466, LearningAimPathwayCode = 1 },
               new AppsCoInvestmentRecordKey() { LearnerReferenceNumber = "055300807083", LearningAimReference = "ZPROG001", LearningStartDate = new DateTime(2019, 2, 21), LearningAimProgrammeType = 3, LearningAimStandardCode = 0, LearningAimFrameworkCode = 462, LearningAimPathwayCode = 1 },
            };

            List<AppsCoInvestmentRecordKey> ilrKeys = new List<AppsCoInvestmentRecordKey>()
            {
               new AppsCoInvestmentRecordKey() { LearnerReferenceNumber = "055300807083", LearningAimReference = "ZPROG001", LearningStartDate = new DateTime(2019, 2, 21), LearningAimProgrammeType = 3, LearningAimStandardCode = 0, LearningAimFrameworkCode = 462, LearningAimPathwayCode = 1 }
            };

            Mock<ILogger> logger = new Mock<ILogger>();
            var appsCoInvestmentContributionsModelBuilder = new AppsCoInvestmentContributionsModelBuilder(new AppsCoInvestmentRecordKeyEqualityComparer(), logger.Object);

            var result = appsCoInvestmentContributionsModelBuilder.UnionKeys(learnRefNumbers, ilrKeys, appsKeys);

            result.Count().Should().Be(3);
        }

        [Fact]
        public void GetLearnerForRecord_Test()
        {
            IDictionary<string, LearnerInfo> learnerDictionary = new Dictionary<string, LearnerInfo>(StringComparer.OrdinalIgnoreCase);
            learnerDictionary.Add("learnref1", new LearnerInfo() { LearnRefNumber = "learnref1" });
            learnerDictionary.Add("LearnRef2", new LearnerInfo() { LearnRefNumber = "LearnRef2" });
            learnerDictionary.Add("LEARNREF3", new LearnerInfo() { LearnRefNumber = "LEARNREF3" });

            var appsCoInvestmentContributionsModelBuilder = new AppsCoInvestmentContributionsModelBuilder(new AppsCoInvestmentRecordKeyEqualityComparer(), null);

            var result = appsCoInvestmentContributionsModelBuilder.GetLearnerForRecord(learnerDictionary, new AppsCoInvestmentRecordKey() { LearnerReferenceNumber = "LeaRnrEf1" });

            result.LearnRefNumber.Should().Be("learnref1");

            result = appsCoInvestmentContributionsModelBuilder.GetLearnerForRecord(learnerDictionary, new AppsCoInvestmentRecordKey() { LearnerReferenceNumber = "LearnRef2" });

            result.LearnRefNumber.Should().Be("LearnRef2");

            result = appsCoInvestmentContributionsModelBuilder.GetLearnerForRecord(learnerDictionary, new AppsCoInvestmentRecordKey() { LearnerReferenceNumber = "LEARNREF3" });

            result.LearnRefNumber.Should().Be("LEARNREF3");
        }

        [Fact]
        public void BuildPaymentInfoDictionary_MixedCaseLearnRefNumber()
        {
            var paymentInfo = new AppsCoInvestmentPaymentsInfo()
            {
                Payments = new List<PaymentInfo>()
                {
                    new PaymentInfo()
                    {
                        LearnerReferenceNumber = "MiXeD",
                    },
                    new PaymentInfo()
                    {
                        LearnerReferenceNumber = "mixed",
                    },
                    new PaymentInfo()
                    {
                        LearnerReferenceNumber = "MIXED",
                    }
                }
            };

            var builder = new AppsCoInvestmentContributionsModelBuilder(new AppsCoInvestmentRecordKeyEqualityComparer(), null);

            var result = builder.BuildPaymentInfoDictionary(paymentInfo);

            var key = new AppsCoInvestmentRecordKey("MIXED", null, 0, 0, 0, 0);

            result.Should().HaveCount(1);

            result.TryGetValue(key, out var check);

            check.Should().NotBeNull();
            check.Count.Should().Be(3);
        }

        private AppsCoInvestmentILRInfo BuildILRModel(int ukPrn, string ilrLearnRefNumber, string ilrLearnAimRef, int aimSeqNumber)
        {
            return new AppsCoInvestmentILRInfo()
            {
                UkPrn = ukPrn,
                Learners = new List<LearnerInfo>()
                {
                    new LearnerInfo()
                    {
                        LearnRefNumber = ilrLearnRefNumber,
                        LearningDeliveries = new List<LearningDeliveryInfo>()
                        {
                            new LearningDeliveryInfo()
                            {
                                UKPRN = ukPrn,
                                LearnRefNumber = ilrLearnRefNumber,
                                LearnAimRef = ilrLearnAimRef,
                                AimType = 3,
                                AimSeqNumber = aimSeqNumber,
                                LearnStartDate = new DateTime(2017, 06, 28),
                                ProgType = 1,
                                StdCode = 1,
                                FworkCode = 1,
                                PwayCode = 1,
                                SWSupAimId = "123",
                                AppFinRecords = new List<AppFinRecordInfo>()
                                {
                                    new AppFinRecordInfo()
                                    {
                                        LearnRefNumber = ilrLearnRefNumber,
                                        AimSeqNumber = aimSeqNumber,
                                        AFinType = "PMR",
                                        AFinCode = 1,
                                        AFinDate = new DateTime(2017, 07, 28),
                                        AFinAmount = 100
                                    }
                                },
                                LearningDeliveryFAMs = new List<LearningDeliveryFAM>()
                                {
                                    new LearningDeliveryFAM()
                                    {
                                        UKPRN = ukPrn,
                                        LearnRefNumber = ilrLearnRefNumber,
                                        AimSeqNumber = aimSeqNumber,
                                        LearnDelFAMType = "LDM",
                                        LearnDelFAMCode = "356"
                                    }
                                }
                            }
                        },
                        LearnerEmploymentStatus = new List<LearnerEmploymentStatusInfo>()
                        {
                            new LearnerEmploymentStatusInfo()
                            {
                                LearnRefNumber = ilrLearnRefNumber,
                                DateEmpStatApp = new DateTime(2017, 06, 28),
                                EmpId = 123
                            }
                        }
                    }
                }
            };
        }

        private AppsCoInvestmentRulebaseInfo BuildFm36Model(int ukPrn, string ilrLearnRefNumber, int aimSeqNumber, string learnAimRef)
        {
            return new AppsCoInvestmentRulebaseInfo()
            {
                UkPrn = ukPrn,
                AECLearningDeliveries = new List<AECLearningDeliveryInfo>()
                {
                    new AECLearningDeliveryInfo()
                    {
                        LearnRefNumber = ilrLearnRefNumber,
                        AimSeqNumber = aimSeqNumber,
                        AppAdjLearnStartDate = new DateTime(2018, 06, 28),
                        UKPRN = ukPrn,
                        LearningDeliveryValues = new AECLearningDeliveryValuesInfo()
                        {
                            LearnDelMathEng = false,
                        }
                    }
                },
                AECApprenticeshipPriceEpisodePeriodisedValues = new List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>()
                {
                    new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = ilrLearnRefNumber,
                        AimSeqNumber = aimSeqNumber,
                        PriceEpisodeIdentifier = "ABC-123",
                        Periods = new decimal?[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140 },
                        AttributeName = "PriceEpisodeCompletionPayment"
                    },
                    new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = ilrLearnRefNumber,
                        AimSeqNumber = aimSeqNumber,
                        PriceEpisodeIdentifier = "ABC-123",
                        Periods = new decimal?[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140 },
                        AttributeName = "PriceEpisodeCompletionPayment"
                    }
                }
            };
        }

        private AppsCoInvestmentPaymentsInfo BuildDasPaymentsModel(int ukPrn, string employerName, string dasLearnRefNumber, string dasLearnAimRef)
        {
            return new AppsCoInvestmentPaymentsInfo()
            {
                UkPrn = ukPrn,
                Payments = new List<PaymentInfo>()
                {
                    new PaymentInfo()
                    {
                        UkPrn = ukPrn,
                        LearnerReferenceNumber = dasLearnRefNumber,
                        LearningAimReference = dasLearnAimRef,
                        LearnerUln = 12345,
                        LearningStartDate = new DateTime(2017, 06, 28),
                        LearningAimProgrammeType = 1,
                        LearningAimStandardCode = 1,
                        LearningAimFrameworkCode = 1,
                        LearningAimPathwayCode = 1,
                        FundingSource = 3,
                        TransactionType = 3,
                        AcademicYear = 2021,
                        Amount = 10,
                        ContractType = 2,
                        CollectionPeriod = 1,
                        DeliveryPeriod = 1,
                        LegalEntityName = employerName,
                        SfaContributionPercentage = new decimal(0.9D),
                        PriceEpisodeIdentifier = "ABC-123"
                    },
                    new PaymentInfo()
                    {
                        UkPrn = ukPrn,
                        LearnerReferenceNumber = dasLearnRefNumber,
                        LearningAimReference = "ABC001",
                        LearnerUln = 12345,
                        LearningStartDate = new DateTime(2017, 06, 28),
                        LearningAimProgrammeType = 1,
                        LearningAimStandardCode = 1,
                        LearningAimFrameworkCode = 1,
                        LearningAimPathwayCode = 1,
                        FundingSource = 3,
                        TransactionType = 3,
                        AcademicYear = 2021,
                        Amount = 10,
                        ContractType = 2,
                        CollectionPeriod = 1,
                        DeliveryPeriod = 1,
                        LegalEntityName = employerName,
                        SfaContributionPercentage = new decimal(0.95D),
                        PriceEpisodeIdentifier = "ABC-234"
                    }
                }
            };
        }
    }
}