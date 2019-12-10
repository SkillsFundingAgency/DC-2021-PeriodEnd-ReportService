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
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Builders;
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
    public sealed class TestLearnerLevelViewReport
    {
        [Fact]
        public async Task TestLearnerLevelViewReportGeneration()
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            int ukPrn = 10036143;
            string filename = $"R01_10036143_10036143 Learner Level View Report {dateTime:yyyyMMdd-HHmmss}";

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10036143);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(1);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriodName).Returns("R01");

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IIlrPeriodEndProviderService> ilrPeriodEndProviderServiceMock =
                new Mock<IIlrPeriodEndProviderService>();
            Mock<IDASPaymentsProviderService> dasPaymentProviderMock = new Mock<IDASPaymentsProviderService>();
            Mock<IFM36PeriodEndProviderService> fm36ProviderServiceMock = new Mock<IFM36PeriodEndProviderService>();
            Mock<ILarsProviderService> larsProviderServiceMock = new Mock<ILarsProviderService>();
            Mock<IFCSProviderService> fcsProviderServiceMock = new Mock<IFCSProviderService>();
            IValueProvider valueProvider = new ValueProvider();
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((key, value, ct) => csv = value)
                .Returns(Task.CompletedTask);

            var appsMonthlyPaymentIlrInfo = BuildILRModel(ukPrn);
            var appsCoInvestIlrInfo = BuildILRCoInvestModel(ukPrn);
            var appsMonthlyPaymentRulebaseInfo = BuildRulebaseModel(ukPrn);
            var appsMonthlyPaymentDasInfo = BuildDasPaymentsModel(ukPrn);
            var appsMonthlyPaymentDasEarningsInfo = BuildDasEarningsModel(ukPrn);
            var appsMonthlyPaymentFcsInfo = BuildFcsModel(ukPrn);
            var larsDeliveryInfoModel = BuildLarsDeliveryInfoModel();
            var learnerLevelViewFM36Info = BuildLearnerLevelViewFM36InfoModel(ukPrn);

            ilrPeriodEndProviderServiceMock
                .Setup(
                    x => x.GetILRInfoForAppsMonthlyPaymentReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appsMonthlyPaymentIlrInfo);
            ilrPeriodEndProviderServiceMock
                .Setup(
                    x => x.GetILRInfoForAppsCoInvestmentReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appsCoInvestIlrInfo);

            fm36ProviderServiceMock
                .Setup(x => x.GetRulebaseDataForAppsMonthlyPaymentReportAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(appsMonthlyPaymentRulebaseInfo);

            fm36ProviderServiceMock
                    .Setup(x => x.GetFM36DataForLearnerLevelView(
                        It.IsAny<int>(),
                        It.IsAny<CancellationToken>())).ReturnsAsync(learnerLevelViewFM36Info);

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
            LLVPaymentRecordKeyEqualityComparer lLVPaymentRecordKeyEqualityComparer = new LLVPaymentRecordKeyEqualityComparer();
            LLVPaymentRecordLRefOnlyKeyEqualityComparer lLVPaymentRecordLRefOnlyKeyEqualityComparer = new LLVPaymentRecordLRefOnlyKeyEqualityComparer();
            var learnerLevelViewModelBuilder = new LearnerLevelViewModelBuilder(logger.Object, lLVPaymentRecordKeyEqualityComparer, lLVPaymentRecordLRefOnlyKeyEqualityComparer);

            var report = new LearnerLevelViewReport(
                logger.Object,
                storage.Object,
                ilrPeriodEndProviderServiceMock.Object,
                fm36ProviderServiceMock.Object,
                dasPaymentProviderMock.Object,
                dateTimeProviderMock.Object,
                valueProvider,
                learnerLevelViewModelBuilder);

            await report.GenerateReport(reportServiceContextMock.Object, null, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            File.WriteAllText($"{filename}.csv", csv);
            List<LearnerLevelViewModel> result;
            TestCsvHelper.CheckCsv(csv, new CsvEntry(new LearnerLevelViewMapper(), 1));
            using (var reader = new StreamReader($"{filename}.csv"))
            {
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.RegisterClassMap<LearnerLevelViewMapper>();
                    result = csvReader.GetRecords<LearnerLevelViewModel>().ToList();
                }
            }

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(2);

            result[0].PaymentLearnerReferenceNumber.Should().Be("A12345");
            result[0].PaymentUniqueLearnerNumber.Should().Be(12345);
            result[0].LearnerEmploymentStatusEmployerId.Should().Be(56789);
            result[0].FamilyName.Should().Be("Banner");
            result[0].GivenNames.Should().Be("Bruce");
            result[0].IssuesAmount.Should().Be(226);
            result[0].LearnerEmploymentStatusEmployerId.Should().Be(56789);
            result[0].PaymentFundingLineType.Should().Be("16-18 Apprenticeship Non-Levy Contract (procured)");
            result[0].ESFAPlannedPaymentsThisPeriod.Should().Be(206);
            result[0].PlannedPaymentsToYouToDate.Should().Be(206);
            result[0].TotalCoInvestmentCollectedToDate.Should().Be(100);
            result[0].CoInvestmentOutstandingFromEmplToDate.Should().Be(-74);
        }

        private LearnerLevelViewFM36Info BuildLearnerLevelViewFM36InfoModel(int ukPrn)
        {
            var learnerLevelInfo = new LearnerLevelViewFM36Info()
            {
                UkPrn = ukPrn,
                AECApprenticeshipPriceEpisodePeriodisedValues = new List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>()
                {
                    new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = "A12345",
                        Periods = new decimal?[12] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                        AttributeName = "PriceEpisodeBalancePayment",
                        AimSeqNumber = 1
                    },
                    new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = "A12345",
                        Periods = new decimal?[12] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                        AttributeName = "PriceEpisodeLSFCash",
                        AimSeqNumber = 2
                    }
                },
                AECLearningDeliveryPeriodisedValuesInfo = new List<AECLearningDeliveryPeriodisedValuesInfo>()
                {
                    new AECLearningDeliveryPeriodisedValuesInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = "A12345",
                        Periods = new decimal?[12] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                        AttributeName = "MathEngBalPayment",
                        LearnDelMathEng = true,
                        AimSeqNumber = 3
                    }
                }
            };

            return learnerLevelInfo;
        }

        private List<AppsMonthlyPaymentLarsLearningDeliveryInfo> BuildLarsDeliveryInfoModel()
        {
            return new List<AppsMonthlyPaymentLarsLearningDeliveryInfo>()
            {
                new AppsMonthlyPaymentLarsLearningDeliveryInfo()
                {
                    LearnAimRef = "123456789",
                    LearningAimTitle = "Diploma in Sports Therapy"
                },
                new AppsMonthlyPaymentLarsLearningDeliveryInfo()
                {
                    LearnAimRef = "50117889",
                    LearningAimTitle = "Maths & English"
                },
            };
        }

        private AppsCoInvestmentILRInfo BuildILRCoInvestModel(int ukPrn)
        {
            return new AppsCoInvestmentILRInfo()
            {
                UkPrn = ukPrn,
                Learners = new List<LearnerInfo>()
                {
                    new LearnerInfo()
                    {
                        LearnRefNumber = "A12345",
                        LearningDeliveries = new List<LearningDeliveryInfo>()
                        {
                            new LearningDeliveryInfo()
                            {
                                UKPRN = ukPrn,
                                LearnRefNumber = "A12345",
                                ProgType = 1,
                                StdCode = 1,
                                FworkCode = 1,
                                PwayCode = 1,
                                LearnStartDate = new DateTime(2019, 8, 28),
                                LearnAimRef = "ZPROG001",
                                AppFinRecords = new List<AppFinRecordInfo>()
                                {
                                    new AppFinRecordInfo()
                                    {
                                        LearnRefNumber = "A12345",
                                        AFinType = "PMR",
                                        AFinCode = 1,
                                        AFinAmount = 100,
                                        AFinDate = new DateTime(2019, 8, 28)
                                    }
                                }
                            }
                        },
                        LearnerEmploymentStatus = new List<LearnerEmploymentStatusInfo>()
                        {
                            new LearnerEmploymentStatusInfo()
                            {
                                LearnRefNumber = "A12345"
                            }
                        }
                    }
                }
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
                        LearnRefNumber = "A12345",
                        UniqueLearnerNumber = 12345,
                        CampId = "camp101",
                        FamilyName = "Banner",
                        GivenNames = "Bruce",

                        ProviderSpecLearnerMonitorings = new List<AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo>()
                        {
                            new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                ProvSpecLearnMonOccur = "A",
                                ProvSpecLearnMon = "T180400007"
                            },
                            new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                ProvSpecLearnMonOccur = "B",
                                ProvSpecLearnMon = "150563"
                            }
                        },

                        LearnerEmploymentStatus = new List<AppsMonthlyPaymentLearnerEmploymentStatusInfo>()
                        {
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                // DateEmpStatApp is after the LearnStartDate so this record should not be assigned
                                DateEmpStatApp = new DateTime(2019, 09, 26),
                                EmpStat = 10, // 10 In paid employment
                                EmpdId = 56789,
                                AgreeId = "9876"
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                // this is the one that should be assigned to the AppsMonthlyPaymentReport
                                // as it's the latest status prior to LearnStartDate
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                // DateEmpStatApp must precede the LearningStartDate
                                DateEmpStatApp = new DateTime(2019, 08, 27),
                                EmpStat = 10, // 10 In paid employment
                                EmpdId = 56789,
                                AgreeId = "7755"
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                DateEmpStatApp = new DateTime(2019, 08, 26),
                                EmpStat = 11, // 11 Not in paid employment, looking for work and available to start work
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                DateEmpStatApp = new DateTime(2019, 08, 25),
                                EmpStat = 12, // 12 Not in paid employment, not looking for work and/ or not available to start work
                            },
                            new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                DateEmpStatApp = new DateTime(2019, 08, 24),
                                EmpStat = 98, // /98 Not known / not provided
                            }
                        },

                        LearningDeliveries = new List<AppsMonthlyPaymentLearningDeliveryModel>
                        {
                            new AppsMonthlyPaymentLearningDeliveryModel()
                            {
                                Ukprn = ukPrn,
                                LearnRefNumber = "A12345",
                                LearnAimRef = "50117889",
                                AimType = 3,
                                AimSeqNumber = 1,
                                LearnStartDate = new DateTime(2019, 08, 28),
                                OrigLearnStartDate = new DateTime(2019, 08, 28),
                                LearnPlanEndDate = new DateTime(2020, 07, 31),
                                FundModel = 36,
                                ProgType = 1,
                                StdCode = 1,
                                FworkCode = 1,
                                PwayCode = 1,
                                PartnerUkprn = 10000001,
                                ConRefNumber = "NLAP-1503",
                                EpaOrgId = "9876543210",
                                SwSupAimId = "SwSup50117889",
                                CompStatus = 2,
                                LearnActEndDate = new DateTime(2020, 07, 30),
                                Outcome = 4,
                                AchDate = new DateTime(2020, 07, 30),
                                ProviderSpecDeliveryMonitorings =
                                    new List<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo>()
                                    {
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "A12345",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "A",
                                            ProvSpecDelMon = "A000406"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "A12345",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "B",
                                            ProvSpecDelMon = "B002902"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "A12345",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "C",
                                            ProvSpecDelMon = "C004402"
                                        },
                                        new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                                        {
                                            Ukprn = ukPrn,
                                            LearnRefNumber = "A12345",
                                            AimSeqNumber = 1,
                                            ProvSpecDelMonOccur = "D",
                                            ProvSpecDelMon = "D006801"
                                        }
                                    },
                                LearningDeliveryFams = new List<AppsMonthlyPaymentLearningDeliveryFAMInfo>()
                                {
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM1",
                                        LearnDelFAMCode = "001"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM2",
                                        LearnDelFAMCode = "002"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM3",
                                        LearnDelFAMCode = "003"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM4",
                                        LearnDelFAMCode = "004"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM5",
                                        LearnDelFAMCode = "005"
                                    },
                                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                                    {
                                        Ukprn = ukPrn,
                                        LearnRefNumber = "A12345",
                                        AimSeqNumber = 1,
                                        LearnDelFAMType = "LDM6",
                                        LearnDelFAMCode = "006"
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
                LearnRefNumber = "A12345",
                AecApprenticeshipPriceEpisodeInfoList = new List<AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo>()
                {
                    new AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo()
                    {
                        // This is the one that should be selected
                        Ukprn = ukPrn,
                        LearnRefNumber = "A12345",
                        AimSequenceNumber = 1,
                        PriceEpisodeIdentifier = "123428/08/2019",
                        EpisodeStartDate = new DateTime(2019, 08, 27),
                        PriceEpisodeAgreeId = "PA102",
                        PriceEpisodeActualEndDate = new DateTime(2019, 08, 01),
                        PriceEpisodeActualEndDateIncEPA = new DateTime(2019, 08, 02)
                    },
                    new AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "A12345",
                        AimSequenceNumber = 1,
                        PriceEpisodeIdentifier = "123428/08/2019",
                        EpisodeStartDate = new DateTime(2019, 08, 01),
                        PriceEpisodeAgreeId = "PA101",
                        PriceEpisodeActualEndDate = new DateTime(2019, 08, 13),
                        PriceEpisodeActualEndDateIncEPA = new DateTime(2019, 08, 02),
                    },
                },
                AecLearningDeliveryInfoList = new List<AppsMonthlyPaymentAECLearningDeliveryInfo>()
                {
                    new AppsMonthlyPaymentAECLearningDeliveryInfo()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "A12345",
                        AimSequenceNumber = 1,
                        LearnAimRef = "50117889",
                        PlannedNumOnProgInstalm = 2
                    },
                    new AppsMonthlyPaymentAECLearningDeliveryInfo()
                    {
                        Ukprn = ukPrn,
                        LearnRefNumber = "A12345",
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
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    EventId = new Guid("BF23F6A8-0B15-42AA-B045-E417E9F0E4C9"),
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    AcademicYear = 1920,
                    CollectionPeriod = 1,
                    LearningAimSequenceNumber = 1,
                    LearnerUln = 12345
                },
                new AppsMonthlyPaymentDasEarningEventModel()
                {
                    // This is the earning that should be selected for the aim seq num
                    EventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    AcademicYear = 1920,
                    CollectionPeriod = 1,
                    LearningAimSequenceNumber = 2,
                    LearnerUln = 12345
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

            appsMonthlyPaymentDasInfo.Payments = new List<AppsMonthlyPaymentDasPaymentModel>();

            /*
                        -------------------------------------------------------------------------------------------------------------------------------------------
                        *** There should be a new row on the report where the data is different for any of the following fields in the Payments2.Payment table: ***
                        -------------------------------------------------------------------------------------------------------------------------------------------
                            • LearnerReferenceNumber
                            • LearnerUln
                            • LearningAimReference
                            • LearningStartDate
                            • LearningAimProgrammeType
                            • LearningAimStandardCode
                            • LearningAimFrameworkCode
                            • LearningAimPathwayCode
                            • ReportingAimFundingLineType
                            • PriceEpisodeIdentifier(note that only programme aims(LearningAimReference = ZPROG001) have PriceEpisodeIdentifiers; maths and English aims do not)
            */

            // learner 1, first payments
            for (byte i = 1; i < 15; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = -11m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearningAimReference = "50117889",
                    LearnerUln = 12345,
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",
                    ContractType = 2,

                    TransactionType = 2,
                    FundingSource = 2,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = -12m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",
                    ContractType = 2,

                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = -13m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",
                    ContractType = 2,

                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = -14m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = -15m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = -16m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = -17m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = -18m
                };

                appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            }

            // learner 1, second payments
            for (byte i = 1; i < 15; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 22m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 2,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 24m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 26m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 28m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 30m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 32m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 34m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "A12345",
                    LearnerUln = 12345,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 36m
                };

                appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            }

            // Learner 2, first payments
            for (byte i = 1; i < 15; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 11m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 12m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",
                    ContractType = 2,

                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 13m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 14m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 15m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 16m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 17m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "50117889",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 18m
                };

                appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            }

            // learner 2, second payments
            for (byte i = 1; i < 15; i++)
            {
                var levyPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 1,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 22m
                };

                var coInvestmentPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 2,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 24m
                };

                var coInvestmentDueFromEmployerPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 2,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 26m
                };

                var employerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 4,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 28m
                };

                var providerAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 5,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 30m
                };

                var apprenticeAdditionalPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 16,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 32m
                };

                var englishAndMathsPayments = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 13,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 34m
                };

                var paymentsForLearningSupport = new AppsMonthlyPaymentDasPaymentModel()
                {
                    AcademicYear = 1920,
                    Ukprn = ukPrn,
                    LearnerReferenceNumber = "B12345",
                    LearnerUln = 54321,
                    LearningAimReference = "ZPROG001",
                    LearningStartDate = new DateTime(2019, 08, 28),
                    EarningEventId = new Guid("4A2BC8C3-A646-4DE7-A253-410B6825C815"),
                    LearningAimProgrammeType = 1,
                    LearningAimStandardCode = 1,
                    LearningAimFrameworkCode = 1,
                    LearningAimPathwayCode = 1,
                    LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    ReportingAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                    PriceEpisodeIdentifier = "123428/08/2019",

                    ContractType = 2,
                    TransactionType = 8,
                    FundingSource = 3,
                    DeliveryPeriod = 1,
                    CollectionPeriod = i,

                    Amount = 36m
                };

                appsMonthlyPaymentDasInfo.Payments.Add(levyPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(coInvestmentDueFromEmployerPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(employerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(providerAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(apprenticeAdditionalPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(englishAndMathsPayments);
                appsMonthlyPaymentDasInfo.Payments.Add(paymentsForLearningSupport);
            }

            return appsMonthlyPaymentDasInfo;
        }
    }
}