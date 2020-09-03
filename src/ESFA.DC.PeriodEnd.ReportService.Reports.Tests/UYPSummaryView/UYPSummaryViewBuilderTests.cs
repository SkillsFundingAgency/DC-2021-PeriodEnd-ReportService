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
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model.Comparer;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.UYPSummaryView
{
    public class UYPSummaryViewBuilderTests
    {
        [Fact]
        public void GenerateReport()
        {
            DateTime dateTime = DateTime.UtcNow;
            int ukPrn = 10036143;
            string filename = $"R01_10036143_10036143 Learner Level View Report {dateTime:yyyyMMdd-HHmmss}";
            int academicYear = 2021;
            int returnPeriod = 1;

            Mock<ILogger> loggerMock = new Mock<ILogger>();

            LLVPaymentRecordKeyEqualityComparer lLVPaymentRecordKeyEqualityComparer = new LLVPaymentRecordKeyEqualityComparer();
            LLVPaymentRecordLRefOnlyKeyEqualityComparer lLVPaymentRecordLRefOnlyKeyEqualityComparer = new LLVPaymentRecordLRefOnlyKeyEqualityComparer();

            // Create and invoke the builder
            IUYPSummaryViewModelBuilder uypSummaryViewModelBuilder = new UYPSummaryViewModelBuilder(
                loggerMock.Object,
                lLVPaymentRecordKeyEqualityComparer,
                lLVPaymentRecordLRefOnlyKeyEqualityComparer);
            var result = uypSummaryViewModelBuilder.Build(
                BuildDasPaymentsModel(ukPrn, academicYear),
                BuildILRModel(ukPrn),
                BuildLDEarningsModel(ukPrn),
                BuildPEEarningsModel(ukPrn),
                new CoInvestmentInfo[1] { BuildILRCoInvestModel(ukPrn) },
                new DataLock[1] { new DataLock() { DataLockFailureId = 1, DeliveryPeriod = 1, LearnerReferenceNumber = "A12345" } },
                new HBCPInfo[1] { new HBCPInfo() { LearnerReferenceNumber = "A12345", DeliveryPeriod = 1, NonPaymentReason = 1 } },
                BuildLegalEntityNames(),
                returnPeriod,
                ukPrn);

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(2);

            var learnerResult = result.FirstOrDefault(x =>x.PaymentLearnerReferenceNumber == "A12345");

            learnerResult.PaymentLearnerReferenceNumber.Should().Be("A12345");
            learnerResult.PaymentUniqueLearnerNumber.Should().Be(12345);
            learnerResult.LearnerEmploymentStatusEmployerId.Should().Be(56789);
            learnerResult.FamilyName.Should().Be("Banner");
            learnerResult.GivenNames.Should().Be("Bruce");
            learnerResult.IssuesAmount.Should().Be(0);
            learnerResult.LearnerEmploymentStatusEmployerId.Should().Be(56789);
            learnerResult.PaymentFundingLineType.Should().Be("16-18 Apprenticeship Non-Levy Contract (procured)");
            learnerResult.ESFAPlannedPaymentsThisPeriod.Should().Be(189);
            learnerResult.PlannedPaymentsToYouToDate.Should().Be(189);
            learnerResult.TotalCoInvestmentCollectedToDate.Should().Be(100);
            learnerResult.CoInvestmentOutstandingFromEmplToDate.Should().Be(-74);
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
                AFinDate = new DateTime(2020, 8, 28),
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
                       { new LearnerEmploymentStatus() { LearnRefNumber = "A12345", EmpId = 56789, EmpStat = 10, DateEmpStatApp = new DateTime(2020, 09, 26) } }
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
}