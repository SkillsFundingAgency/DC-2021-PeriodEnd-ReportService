using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.FileService.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView;
using ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView.Interface;
using ESFA.DC.ReportData.Model;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView;
using Moq;
using Xunit;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;
using LearnerLevelViewSummaryModel = ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model.LearnerLevelViewSummaryModel;
using CsvHelper;
using CsvHelper.Configuration;
using System.Linq;
using FluentAssertions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Persistence;
using ESFA.DC.Serialization.Json;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.UYPSummaryView
{
    public class UYPSummaryViewTests
    {
        [Fact]
        public async Task GenerateReport()
        {
            DateTime dateTime = DateTime.UtcNow;
            int ukPrn = 10036143;
            string filename = $"R01_10036143_10036143 Learner Level View Report {dateTime:yyyyMMdd-HHmmss}";
            int academicYear = 2021;

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10036143);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(1);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriodName).Returns("R01");

            Mock<ILogger> loggerMock = new Mock<ILogger>();
            Mock<ICsvFileService> csvFileServiceMock = new Mock<ICsvFileService>();
            Mock<IFileNameService> fileNameServiceMock = new Mock<IFileNameService>();
            Mock<IUYPSummaryViewDataProvider> uypSummaryViewDataProviderMock = new Mock<IUYPSummaryViewDataProvider>();
            Mock<IUYPSummaryViewModelBuilder> uypSummaryViewModelBuilderMock = new Mock<IUYPSummaryViewModelBuilder>();
            Mock<IJsonSerializationService> jsonSerializationServiceMock = new Mock<IJsonSerializationService>();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            Mock<IFileService> fileServiceMock = new Mock<IFileService>();
            Mock<IReportDataPersistanceService<LearnerLevelViewReport>> reportDataPersistanceServiceMock = 
                new Mock<IReportDataPersistanceService<LearnerLevelViewReport>>();
            Mock<IUYPSummaryViewPersistenceMapper> uypSummaryViewPersistenceMapper = new Mock<IUYPSummaryViewPersistenceMapper>();

            uypSummaryViewPersistenceMapper.Setup(x => x.Map(
                It.IsAny<IReportServiceContext>(), 
                It.IsAny<IEnumerable<LearnerLevelViewModel>>(), 
                It.IsAny<CancellationToken>())).Returns(new List<LearnerLevelViewReport>());

            reportDataPersistanceServiceMock.Setup(x => x.PersistAsync(
                It.IsAny<IReportServiceContext>(), 
                It.IsAny<IEnumerable<LearnerLevelViewReport>>(), 
                It.IsAny<CancellationToken>()));

            jsonSerializationServiceMock.Setup(x => x.Serialize<IEnumerable<LearnerLevelViewSummaryModel>>(
                It.IsAny<IEnumerable<LearnerLevelViewSummaryModel>>())).Returns(string.Empty);

            // We need three streams
            FileStream[] fs = new FileStream[3] { new FileStream("1.json", FileMode.Create), new FileStream("2.csv", FileMode.Create), new FileStream("3.csv", FileMode.Create) };
            var index = 0;
            fileServiceMock.Setup(x => x.OpenWriteStreamAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>())).ReturnsAsync(() => fs[index++]);

            // Return filenames
            fileNameServiceMock.Setup(x => x.GetFilename(It.IsAny<IReportServiceContext>(), It.IsAny<string>(), It.IsAny<OutputTypes>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(filename);

            // Setup data return objects
            ICollection<CoInvestmentInfo> coInvestmentInfo = new CoInvestmentInfo[1] {
                BuildILRCoInvestModel(ukPrn)
            };
            uypSummaryViewDataProviderMock.Setup(x => x.GetCoinvestmentsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(coInvestmentInfo);

            ICollection<Payment> payments = BuildDasPaymentsModel(ukPrn, academicYear);
            uypSummaryViewDataProviderMock.Setup(x => x.GetDASPaymentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(payments);

            ICollection<DataLock> dataLocks = new DataLock[1] { new DataLock() { DataLockFailureId = 1, DeliveryPeriod = 1, LearnerReferenceNumber = "A12345" } };
            uypSummaryViewDataProviderMock.Setup(x => x.GetDASDataLockAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(dataLocks);

            ICollection<HBCPInfo> hbcpInfo = new HBCPInfo[1] { new HBCPInfo() { LearnerReferenceNumber = "A12345", DeliveryPeriod = 1, NonPaymentReason = 1 } };
            uypSummaryViewDataProviderMock.Setup(x => x.GetHBCPInfoAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(hbcpInfo);

            ICollection<Learner> learners = BuildILRModel(ukPrn);
            uypSummaryViewDataProviderMock.Setup(x => x.GetILRLearnerInfoAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(learners);

            ICollection<LearningDeliveryEarning> ldEarnings = BuildLDEarningsModel(ukPrn);
            uypSummaryViewDataProviderMock.Setup(x => x.GetLearnerDeliveryEarningsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(ldEarnings);

            IDictionary<long, string> legalEntityNames = BuildLegalEntityNames();
            uypSummaryViewDataProviderMock.Setup(x => x.GetLegalEntityNameAsync(It.IsAny<int>(), It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>())).ReturnsAsync(legalEntityNames);

            ICollection<PriceEpisodeEarning> peEarnings = BuildPEEarningsModel(ukPrn);
            uypSummaryViewDataProviderMock.Setup(x => x.GetPriceEpisodeEarningsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(peEarnings);

            ICollection<LearnerLevelViewModel> results = new LearnerLevelViewModel[1] { new LearnerLevelViewModel()
                    {
                        Ukprn = ukPrn,
                        PaymentLearnerReferenceNumber = "A12345",
                        PaymentUniqueLearnerNumber = 12345,
                        FamilyName = "Banner",
                        GivenNames = "Bruce",
                        LearnerEmploymentStatusEmployerId = 1,
                        EmployerName = "Employer Name",
                        TotalEarningsToDate = 1m,
                        PlannedPaymentsToYouToDate = 2m,
                        TotalCoInvestmentCollectedToDate = 3m,
                        CoInvestmentOutstandingFromEmplToDate = 4m,
                        TotalEarningsForPeriod = 5m,
                        ESFAPlannedPaymentsThisPeriod = 6m,
                        CoInvestmentPaymentsToCollectThisPeriod = 7m,
                        IssuesAmount = 8m,
                        ReasonForIssues = "Borked",
                        PaymentFundingLineType = "12345",
                        RuleDescription = "Rule X"
                    }
                };
            uypSummaryViewModelBuilderMock.Setup(x => x.Build(
                                                            It.IsAny<ICollection<Payment>>(),
                                                            It.IsAny<ICollection<Learner>>(),
                                                            It.IsAny<ICollection<LearningDeliveryEarning>>(),
                                                            It.IsAny<ICollection<PriceEpisodeEarning>>(),
                                                            It.IsAny<ICollection<CoInvestmentInfo>>(),
                                                            It.IsAny<ICollection<DataLock>>(),
                                                            It.IsAny<ICollection<HBCPInfo>>(),
                                                            It.IsAny<IDictionary<long, string>>(),
                                                            It.IsAny<int>(),
                                                            It.IsAny<int>())).Returns(results);

            // Create and invoke the view
            ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView.UYPSummaryView report
                = new ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView.UYPSummaryView(
                    csvFileServiceMock.Object, fileNameServiceMock.Object, uypSummaryViewDataProviderMock.Object,
                    uypSummaryViewModelBuilderMock.Object, jsonSerializationService,
                    fileServiceMock.Object, reportDataPersistanceServiceMock.Object, uypSummaryViewPersistenceMapper.Object, 
                    loggerMock.Object);
            await report.GenerateReport(reportServiceContextMock.Object, CancellationToken.None);

            List<LearnerLevelViewSummaryModel> result;
            using (var reader = new StreamReader(fs[0].Name))
            {
                string fileData = reader.ReadToEnd();
                result = jsonSerializationService.Deserialize<IEnumerable<LearnerLevelViewSummaryModel>>(fileData).ToList();
            }

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(1);

            result[0].CoInvestmentPaymentsToCollectForThisPeriod.Should().Be(7);
            result[0].ESFAPlannedPaymentsForThisPeriod.Should().Be(6);
            result[0].NumberofLearners.Should().Be(1);
            result[0].TotalCoInvestmentCollectedToDate.Should().Be(3);
            result[0].NumberofClawbacks.Should().Be(0);
        }

        private IDictionary<long, string> BuildLegalEntityNames()
        {
            return new Dictionary<long, string>() { { 1, "Employer Name" } };
        }

        private CoInvestmentInfo BuildILRCoInvestModel(int ukPrn)
        {
            return new CoInvestmentInfo()
            {
                LearnRefNumber = "A12345",
                LearnAimRef = "ZPROG001",
                AFinDate = new DateTime(2019, 8, 28),
                AFinType = "PMR",
                AFinCode = 1,
                AFinAmount = 100
            };
        }

        private ICollection<Learner> BuildILRModel(int ukPrn)
        {
            return new List<Learner>() {
                new Learner()
                {
                    LearnRefNumber = "A12345",
                    FamilyName = "Banner",
                    GivenNames = "Bruce",
                    UniqueLearnerNumber = 12345,
                    LearnerEmploymentStatuses = new List<LearnerEmploymentStatus>()
                       { new LearnerEmploymentStatus() { LearnRefNumber = "A12345", EmpId = 10, EmpStat = 56789, DateEmpStatApp = new DateTime(2020, 09, 26) } }
                },
                new Learner()
                {
                    LearnRefNumber = "B12345",
                    FamilyName = "Peter",
                    GivenNames = "Parker",
                    UniqueLearnerNumber = 54321,
                    LearnerEmploymentStatuses = new List<LearnerEmploymentStatus>()
                       { new LearnerEmploymentStatus() { LearnRefNumber = "A12345", EmpId = 10, EmpStat = 56789, DateEmpStatApp = new DateTime(2020, 09, 26) } }
                }
            };
        }

        private ICollection<LearningDeliveryEarning> BuildLDEarningsModel(int ukPrn)
        {
            return new List<LearningDeliveryEarning>() {
                new LearningDeliveryEarning() {
                    LearnRefNumber = "A12345",
                    AimSequenceNumber = 3,
                    AttributeName = "MathEngBalPayment",
                    Period_1 = 1m,
                    Period_2 = 1m,
                    Period_3 = 1m,
                    Period_4 = 1m,
                    Period_5 = 1m,
                    Period_6 = 1m,
                    Period_7 = 1m,
                    Period_8 = 1m,
                    Period_9 = 1m,
                    Period_10 = 1m,
                    Period_11 = 1m,
                    Period_12 = 1m,
                    LearnDelMathEng = true
                }
            };
        }

        private ICollection<PriceEpisodeEarning> BuildPEEarningsModel(int ukPrn)
        {
            return new List<PriceEpisodeEarning>() {
                new PriceEpisodeEarning() {
                    LearnRefNumber = "A12345",
                    AttributeName = "PriceEpisodeBalancePayment",
                    Period_1 = 1m,
                    Period_2 = 1m,
                    Period_3 = 1m,
                    Period_4 = 1m,
                    Period_5 = 1m,
                    Period_6 = 1m,
                    Period_7 = 1m,
                    Period_8 = 1m,
                    Period_9 = 1m,
                    Period_10 = 1m,
                    Period_11 = 1m,
                    Period_12 = 1m
                },
                new PriceEpisodeEarning() {
                    LearnRefNumber = "A12345",
                    AttributeName = "PriceEpisodeLSFCash",
                    Period_1 = 1m,
                    Period_2 = 1m,
                    Period_3 = 1m,
                    Period_4 = 1m,
                    Period_5 = 1m,
                    Period_6 = 1m,
                    Period_7 = 1m,
                    Period_8 = 1m,
                    Period_9 = 1m,
                    Period_10 = 1m,
                    Period_11 = 1m,
                    Period_12 = 1m
                }
            };
        }

        private void AddPayments(
                ref List<Payment> payments, 
                int ukPrn, 
                int academicYear, 
                string learningAimRef, 
                decimal amount, 
                decimal amountIncr,
                string learnerReferenceNumber,
                int LearnerUln)
        {
            decimal amountCounter = amount;

            for (byte i = 1; i < 15; i++)
            {
                // Levy payments
                payments.Add(new Payment()
                {
                    AcademicYear = academicYear,
                    LearnerReferenceNumber = learnerReferenceNumber,
                    LearnerUln = LearnerUln,
                    LearningAimReference = learningAimRef,
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    CollectionPeriod = i,
                    Amount = amountCounter,
                    TransactionType = 2,
                    FundingSource = 1,
                    ApprenticeshipId = 1
                });

                // Coinvestment payments
                payments.Add(new Payment()
                {
                    AcademicYear = academicYear,
                    LearnerReferenceNumber = learnerReferenceNumber,
                    LearnerUln = LearnerUln,
                    LearningAimReference = learningAimRef,
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    CollectionPeriod = i,
                    Amount = (amountCounter += amountIncr),
                    TransactionType = 2,
                    FundingSource = 2,
                    ApprenticeshipId = 1
                });

                // CoInvestment Due From Employer Payments
                payments.Add(new Payment()
                {
                    AcademicYear = academicYear,
                    LearnerReferenceNumber = learnerReferenceNumber,
                    LearnerUln = LearnerUln,
                    LearningAimReference = learningAimRef,
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    CollectionPeriod = i,
                    Amount = (amountCounter += amountIncr),
                    TransactionType = 2,
                    FundingSource = 3,
                    ApprenticeshipId = 1
                });

                // Employer Additional Payments
                payments.Add(new Payment()
                {
                    AcademicYear = academicYear,
                    LearnerReferenceNumber = learnerReferenceNumber,
                    LearnerUln = LearnerUln,
                    LearningAimReference = learningAimRef,
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    CollectionPeriod = i,
                    Amount = (amountCounter += amountIncr),
                    TransactionType = 4,
                    FundingSource = 3,
                    ApprenticeshipId = 1
                });

                // Provider Additional Payments
                payments.Add(new Payment()
                {
                    AcademicYear = academicYear,
                    LearnerReferenceNumber = learnerReferenceNumber,
                    LearnerUln = LearnerUln,
                    LearningAimReference = learningAimRef,
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    CollectionPeriod = i,
                    Amount = (amountCounter += amountIncr),
                    TransactionType = 5,
                    FundingSource = 3,
                    ApprenticeshipId = 1
                });

                // Apprentice Additional Payments
                payments.Add(new Payment()
                {
                    AcademicYear = academicYear,
                    LearnerReferenceNumber = learnerReferenceNumber,
                    LearnerUln = LearnerUln,
                    LearningAimReference = learningAimRef,
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    CollectionPeriod = i,
                    Amount = (amountCounter += amountIncr),
                    TransactionType = 16,
                    FundingSource = 3,
                    ApprenticeshipId = 1
                });

                // English and Maths Payments
                payments.Add(new Payment()
                {
                    AcademicYear = academicYear,
                    LearnerReferenceNumber = learnerReferenceNumber,
                    LearnerUln = LearnerUln,
                    LearningAimReference = learningAimRef,
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    CollectionPeriod = i,
                    Amount = (amountCounter += amountIncr),
                    TransactionType = 13,
                    FundingSource = 3,
                    ApprenticeshipId = 1
                });

                // Payments for Learning Support 
                payments.Add(new Payment()
                {
                    AcademicYear = academicYear,
                    LearnerReferenceNumber = learnerReferenceNumber,
                    LearnerUln = LearnerUln,
                    LearningAimReference = learningAimRef,
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    CollectionPeriod = i,
                    Amount = (amountCounter += amountIncr),
                    TransactionType = 8,
                    FundingSource = 3,
                    ApprenticeshipId = 1
                });
            }
        }

        private Payment[] BuildDasPaymentsModel(int ukPrn, int academicYear)
        {
            var payments = new List<Payment>();

            // Learner 1
            AddPayments(ref payments, ukPrn, academicYear, "50117889", 11m, 1m, "A12345", 12345);
            AddPayments(ref payments, ukPrn, academicYear, "ZPROG001", 22m, 2m, "A12345", 12345);

            // Learner 2
            AddPayments(ref payments, ukPrn, academicYear, "50117889", 11m, 1m, "B12345", 54321);
            AddPayments(ref payments, ukPrn, academicYear, "ZPROG001", 22m, 2m, "B12345", 54321);

            return payments.ToArray();
        }
    }

    public class LearnerLevelViewMapper : ClassMap<LearnerLevelViewModel>
    {
        public LearnerLevelViewMapper()
        {
            int i = 0;

            Map(m => m.PaymentLearnerReferenceNumber).Index(i++).Name("Learner reference number");
            Map(m => m.PaymentUniqueLearnerNumber).Index(i++).Name("Unique learner number");

            Map(m => m.PaymentFundingLineType).Index(i++).Name("Funding line type");
            Map(m => m.LearnerEmploymentStatusEmployerId).Index(i++).Name("Employer identifier on employment status date");
            Map(m => m.EmployerName).Index(i++).Name("Latest employer name");

            Map(m => m.Ukprn).Index(i++).Name("UKPRN");
            Map(m => m.FamilyName).Index(i++).Name("Family Name");
            Map(m => m.GivenNames).Index(i++).Name("Given Names");

            Map(m => m.TotalEarningsToDate).Index(i++).Name("Total earnings to date");
            Map(m => m.PlannedPaymentsToYouToDate).Index(i++).Name("ESFA planned payments to you to date");
            Map(m => m.TotalCoInvestmentCollectedToDate).Index(i++).Name("Total Co-Investment collected to date");
            Map(m => m.CoInvestmentOutstandingFromEmplToDate).Index(i++).Name("Co-Investment outstanding from employer to date");
            Map(m => m.TotalEarningsForPeriod).Index(i++).Name("Total earnings for this period");
            Map(m => m.ESFAPlannedPaymentsThisPeriod).Index(i++).Name("ESFA planned payments for this period");
            Map(m => m.CoInvestmentPaymentsToCollectThisPeriod).Index(i++).Name("Co-investment payments to collect for this period");
            Map(m => m.IssuesAmount).Index(i++).Name("Issues amount");
            Map(m => m.ReasonForIssues).Index(i++).Name("Reasons for issues");
        }
    }
}