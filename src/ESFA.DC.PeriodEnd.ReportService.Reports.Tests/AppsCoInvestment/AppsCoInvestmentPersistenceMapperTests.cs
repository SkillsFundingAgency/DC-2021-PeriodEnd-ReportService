using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment
{
    public class AppsCoInvestmentPersistenceMapperTests
    {
        [Fact]
        public void Map_Returns_Valid_Persist_Model()
        {
            var reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.Setup(x => x.Ukprn).Returns(123456);
            reportServiceContextMock.Setup(x => x.ReturnPeriod).Returns(10);

            var record = new AppsCoInvestmentRecord
            {
                RecordKey = new AppsCoInvestmentRecordKey(
                    "LearnRefNumber",
                    new DateTime(2020,08,02),
                    10,
                    20,
                    30,
                    40),
                FamilyName = "FamilyName",
                GivenNames = "Given names",
                UniqueLearnerNumber = 12345678,
                LearningDelivery = new LearningDelivery()
                {
                    SWSupAimId = "SW Sup Aim Id",
                    AECLearningDelivery = new AECLearningDelivery()
                    {
                        AppAdjLearnStartDate = new DateTime(2019,11,12)
                    }
                },
                ApprenticeshipContractType = 1,
                EmployerIdentifierAtStartOfLearning = 10,
                EmployerNameFromApprenticeshipService = "Employer name",
                EarningsAndPayments = new EarningsAndPayments()
                {
                    TotalPMRPreviousFundingYears = 10,
                    TotalCoInvestmentDueFromEmployerInPreviousFundingYears = 20,
                    TotalPMRThisFundingYear = 30,
                    TotalCoInvestmentDueFromEmployerThisFundingYear = 40,
                    PercentageOfCoInvestmentCollected = 50,
                    CompletionEarningThisFundingYear = 60,
                    CompletionPaymentsThisFundingYear = 70,
                    EmployerCoInvestmentPercentage = 10.00m,
                    CoInvestmentPaymentsDueFromEmployer = new CoInvestmentPaymentsDueFromEmployer
                    {
                        August = 1,
                        September = 2,
                        October = 3,
                        November = 4,
                        December = 5,
                        January = 6,
                        February = 7,
                        March = 8,
                        April = 9,
                        May = 10,
                        June = 11,
                        July = 12,
                        R13 = 13,
                        R14 = 14
                    }
                },
                LDM356Or361 = "Yes"
            };

            var appsCoInvestmentPersistModels = new AppsCoInvestmentPersistenceMapper().Map(reportServiceContextMock.Object, new List<AppsCoInvestmentRecord>() {record}, CancellationToken.None).ToList();

            var persistModel = appsCoInvestmentPersistModels[0];

            persistModel.Ukprn.Should().Be(reportServiceContextMock.Object.Ukprn);
            persistModel.ReturnPeriod.Should().Be(reportServiceContextMock.Object.ReturnPeriod);
            persistModel.LearnRefNumber.Should().Be(record.RecordKey.LearnerReferenceNumber);
            persistModel.FamilyName.Should().Be(record.FamilyName);
            persistModel.GivenNames.Should().Be(record.GivenNames);
            persistModel.UniqueLearnerNumber.Should().Be(record.UniqueLearnerNumber);
            persistModel.LearningStartDate.Should().Be(record.RecordKey.LearningStartDate);
            persistModel.ProgType.Should().Be(record.RecordKey.ProgrammeType);
            persistModel.StandardCode.Should().Be(record.RecordKey.StandardCode);
            persistModel.FrameworkCode.Should().Be(record.RecordKey.FrameworkCode);
            persistModel.ApprenticeshipPathway.Should().Be(record.RecordKey.PathwayCode);
            persistModel.SoftwareSupplierAimIdentifier.Should().Be(record.LearningDelivery.SWSupAimId);
            persistModel.LearningDeliveryFAMTypeApprenticeshipContractType.Should().Be(record.ApprenticeshipContractType);
            persistModel.EmployerIdentifierAtStartOfLearning.Should().Be(record.EmployerIdentifierAtStartOfLearning);
            persistModel.EmployerNameFromApprenticeshipService.Should().Be(record.EmployerNameFromApprenticeshipService);
            persistModel.TotalPMRPreviousFundingYears.Should().Be(record.EarningsAndPayments.TotalPMRPreviousFundingYears);
            persistModel.TotalCoInvestmentDueFromEmployerInPreviousFundingYears.Should().Be(record.EarningsAndPayments.TotalCoInvestmentDueFromEmployerInPreviousFundingYears);
            persistModel.TotalPMRThisFundingYear.Should().Be(record.EarningsAndPayments.TotalPMRThisFundingYear);
            persistModel.TotalCoInvestmentDueFromEmployerThisFundingYear.Should().Be(record.EarningsAndPayments.TotalCoInvestmentDueFromEmployerThisFundingYear);
            persistModel.PercentageOfCoInvestmentCollected.Should().Be(record.EarningsAndPayments.PercentageOfCoInvestmentCollected);
            persistModel.LDM356Or361.Should().Be(record.LDM356Or361);
            persistModel.CoInvestmentDueFromEmployerForAugust.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.August);
            persistModel.CoInvestmentDueFromEmployerForSeptember.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.September);
            persistModel.CoInvestmentDueFromEmployerForOctober.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.October);
            persistModel.CoInvestmentDueFromEmployerForNovember.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.November);
            persistModel.CoInvestmentDueFromEmployerForDecember.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.December);
            persistModel.CoInvestmentDueFromEmployerForJanuary.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.January);
            persistModel.CoInvestmentDueFromEmployerForFebruary.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.February);
            persistModel.CoInvestmentDueFromEmployerForMarch.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.March);
            persistModel.CoInvestmentDueFromEmployerForApril.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.April);
            persistModel.CoInvestmentDueFromEmployerForMay.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.May);
            persistModel.CoInvestmentDueFromEmployerForJune.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.June);
            persistModel.CoInvestmentDueFromEmployerForJuly.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.July);
            persistModel.CoInvestmentDueFromEmployerForR13.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.R13);
            persistModel.CoInvestmentDueFromEmployerForR14.Should().Be(record.EarningsAndPayments.CoInvestmentPaymentsDueFromEmployer.R14);
            persistModel.CompletionEarningThisFundingYear.Should().Be(record.EarningsAndPayments.CompletionEarningThisFundingYear);
            persistModel.ApplicableProgrammeStartDate.Should().Be(record.LearningDelivery.AECLearningDelivery.AppAdjLearnStartDate);
        }
    }
}
