using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Builders;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Tests.Reports.AppsMonthlyPaymentReport
{
    public class AppsMonthlyPaymentReport_MethodTests
    {
        private AppsMonthlyPaymentModelBuilder _modelBuilder;
        private int _ukprn;
        private string _learnerReferenceNumber;
        private string _learningAimReferenceNumber;
        private byte _learningAimSequenceNumber;
        private DateTime _learningStartDate;
        private string _priceEpisodeIdentifier;
        private int _programmeType;
        private int _standardCode;
        private int _frameworkCode;
        private int _pathwayCode;

        private AppsMonthlyPaymentILRInfo _ilrData;
        private AppsMonthlyPaymentRulebaseInfo _rulebaseData;
        private AppsMonthlyPaymentDASInfo _paymentsData;
        private AppsMonthlyPaymentDasEarningsInfo _earningsData;
        private IDictionary<string, string> _fcsData;
        private IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo> _larsData;

        public AppsMonthlyPaymentReport_MethodTests()
        {
            _modelBuilder = new AppsMonthlyPaymentModelBuilder();
            _ukprn = 10000001;
            _learnerReferenceNumber = "LR1001";
            _learningAimReferenceNumber = Generics.ZPROG001;
            _learningAimSequenceNumber = 6;
            _learningStartDate = new DateTime(2018, 6, 16);
            _priceEpisodeIdentifier = "2-445-3-01/08/2019";
            _programmeType = 2;
            _standardCode = 0;
            _frameworkCode = 445;
            _pathwayCode = 3;

            _ilrData = BuildILRModel(_ukprn);
            _rulebaseData = BuildRulebaseModel(_ukprn);
            _paymentsData = BuildDasPaymentsModel(_ukprn);
            _earningsData = BuildDasEarningsModel(_ukprn);
            _fcsData = BuildFcsModel(_ukprn);
            _larsData = BuildLarsDeliveryInfoModel();
        }

        [Fact]
        public void TestLookupPriceEpisodeStartDate()
        {
            string priceEpisodeStartDate = _modelBuilder.LookupPriceEpisodeStartDate(_priceEpisodeIdentifier);

            priceEpisodeStartDate.Should().Be("01/08/2019");
        }

        [Fact]
        public void TestGetPaymentAimSequenceNumber()
        {
            var reportRowModels = _paymentsData.Payments?
                .GroupBy(r => new
                {
                    r.LearnerReferenceNumber,
                    r.LearnerUln,
                    r.LearningAimReference,
                    r.LearningStartDate,
                    r.LearningAimProgrammeType,
                    r.LearningAimStandardCode,
                    r.LearningAimFrameworkCode,
                    r.LearningAimPathwayCode,
                    r.ReportingAimFundingLineType,
                    r.PriceEpisodeIdentifier
                })
                .Select(g =>
                {
                    var aimSequenceNumber = _modelBuilder.GetPaymentAimSequenceNumber(g, _earningsData);

                    return new AppsMonthlyPaymentReportRowModel
                    {
                        PaymentLearningAimReference = g.Key.LearningAimReference,
                        PaymentEarningEventAimSeqNumber = aimSequenceNumber
                    };
                }).ToList();

            reportRowModels?.FirstOrDefault(r => r.PaymentLearningAimReference.Equals(Generics.ZPROG001))?.PaymentEarningEventAimSeqNumber?.Should().Be(6);
        }

        [Fact]
        public void TestLookupAimTitle()
        {
            var aimTitle = _modelBuilder.LookupAimTitle(_learningAimReferenceNumber, _larsData);

            aimTitle.Should().Be("Generic code to identify ILR programme aims");
        }

        [Fact]
        public void TestLookupContractAllocationNumber()
        {
            var contractAllocationNumber = _modelBuilder.LookupContractAllocationNumber("19+ Apprenticeship (Employer on App Service) Levy funding", _fcsData);

            contractAllocationNumber.Should().Be("YNLP-1157;YNLP-1158");
        }

        [Fact]
        public void TestLookupLearner()
        {
            var ilrLearnerForThisPayment = _modelBuilder.LookupLearner(_learnerReferenceNumber, _ilrData);

            ilrLearnerForThisPayment.Should().NotBeNull();
            ilrLearnerForThisPayment.LearnRefNumber.Should().Be(_learnerReferenceNumber);
            ilrLearnerForThisPayment.UniqueLearnerNumber.Should().Be(1000000001);
        }

        [Fact]
        public void TestLookupProvSpecLearnMon()
        {
            const string provSpecLearnMonA = "101";
            const string provSpecLearnMonB = "102";

            var providerSpecLearnerMonitorings = new List<AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo>()
            {
                new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                {
                    Ukprn = _ukprn,
                    LearnRefNumber = _learnerReferenceNumber,
                    ProvSpecLearnMonOccur = Generics.ProviderSpecifiedLearnerMonitoringA,
                    ProvSpecLearnMon = provSpecLearnMonA
                },
                new AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo()
                {
                    Ukprn = _ukprn,
                    LearnRefNumber = _learnerReferenceNumber,
                    ProvSpecLearnMonOccur = Generics.ProviderSpecifiedLearnerMonitoringB,
                    ProvSpecLearnMon = provSpecLearnMonB
                }
            };

            var providerSpecifiedLearnerMonitoringA = _modelBuilder.LookupProvSpecLearnMon(providerSpecLearnerMonitorings, Generics.ProviderSpecifiedLearnerMonitoringA);
            var providerSpecifiedLearnerMonitoringB = _modelBuilder.LookupProvSpecLearnMon(providerSpecLearnerMonitorings, Generics.ProviderSpecifiedLearnerMonitoringB);

            providerSpecifiedLearnerMonitoringA.Should().Be(provSpecLearnMonA);
            providerSpecifiedLearnerMonitoringB.Should().Be(provSpecLearnMonB);
        }

        [Fact]
        public void TestLookupLearningDelivery()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                Ukprn = _ukprn,
                PaymentLearnerReferenceNumber = _learnerReferenceNumber,
                PaymentLearningAimReference = _learningAimReferenceNumber,
                PaymentLearningStartDate = _learningStartDate,
                PaymentProgrammeType = _programmeType,
                PaymentStandardCode = _standardCode,
                PaymentFrameworkCode = _frameworkCode,
                PaymentPathwayCode = _pathwayCode
            };
            var learner = _modelBuilder.LookupLearner(_learnerReferenceNumber, _ilrData);

            var learningDelivery = _modelBuilder.LookupLearningDelivery(reportRowModel, learner);

            learningDelivery?.Ukprn.Should().Be(_ukprn);
            learningDelivery?.LearnRefNumber.Should().Be(_learnerReferenceNumber);
            learningDelivery?.LearnAimRef.Should().Be(_learningAimReferenceNumber);
            learningDelivery?.LearnStartDate.Should().Be(_learningStartDate);
            learningDelivery?.ProgType.Should().Be(_programmeType);
            learningDelivery?.StdCode.Should().Be(_standardCode);
            learningDelivery?.FworkCode.Should().Be(_frameworkCode);
            learningDelivery?.PwayCode.Should().Be(_pathwayCode);
        }

        [Fact]
        public void TestLookupLearningDeliveryLdmFams()
        {
            List<AppsMonthlyPaymentLearningDeliveryFAMInfo> ldmFamData =
                new List<AppsMonthlyPaymentLearningDeliveryFAMInfo>()
                {
                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                    {
                        Ukprn = _ukprn,
                        LearnRefNumber = _learnerReferenceNumber,
                        AimSeqNumber = 6,
                        LearnDelFAMType = Generics.LearningDeliveryFAMCodeLDM,
                        LearnDelFAMCode = "101",
                    },
                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                    {
                        Ukprn = _ukprn,
                        LearnRefNumber = _learnerReferenceNumber,
                        AimSeqNumber = 6,
                        LearnDelFAMType = Generics.LearningDeliveryFAMCodeLDM,
                        LearnDelFAMCode = "102",
                    },
                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                    {
                        Ukprn = _ukprn,
                        LearnRefNumber = _learnerReferenceNumber,
                        AimSeqNumber = 6,
                        LearnDelFAMType = Generics.LearningDeliveryFAMCodeLDM,
                        LearnDelFAMCode = "103",
                    },
                    new AppsMonthlyPaymentLearningDeliveryFAMInfo()
                    {
                        Ukprn = _ukprn,
                        LearnRefNumber = _learnerReferenceNumber,
                        AimSeqNumber = 6,
                        LearnDelFAMType = Generics.LearningDeliveryFAMCodeLDM,
                        LearnDelFAMCode = "104",
                    }
                };

            var ldmFams = _modelBuilder.LookupLearningDeliveryLdmFams(ldmFamData, Generics.LearningDeliveryFAMCodeLDM);

            ldmFams?.Length.Should().Be(6);
            ldmFams?[0]?.LearnDelFAMCode.Should().Be("101");
            ldmFams?[1]?.LearnDelFAMCode.Should().Be("102");
            ldmFams?[2]?.LearnDelFAMCode.Should().Be("103");
            ldmFams?[3]?.LearnDelFAMCode.Should().Be("104");
            ldmFams?[4]?.LearnDelFAMCode.Should().BeNullOrEmpty();
            ldmFams?[5].Should().BeNull();
        }

        [Fact]
        public void TestLookupProvSpecDelMon()
        {
            var provSpecDelMons = new List<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo>()
            {
                new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                {
                    Ukprn = _ukprn,
                    LearnRefNumber = _learnerReferenceNumber,
                    AimSeqNumber = 1,
                    ProvSpecDelMonOccur = Generics.ProviderSpecifiedDeliveryMonitoringA,
                    ProvSpecDelMon = "1920"
                },
                new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                {
                    Ukprn = _ukprn,
                    LearnRefNumber = _learnerReferenceNumber,
                    AimSeqNumber = 1,
                    ProvSpecDelMonOccur = Generics.ProviderSpecifiedDeliveryMonitoringB,
                    ProvSpecDelMon = "E5072"
                },
                new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                {
                    Ukprn = _ukprn,
                    LearnRefNumber = _learnerReferenceNumber,
                    AimSeqNumber = 1,
                    ProvSpecDelMonOccur = Generics.ProviderSpecifiedDeliveryMonitoringC,
                    ProvSpecDelMon = "CHILD"
                },
                new AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo()
                {
                    Ukprn = _ukprn,
                    LearnRefNumber = _learnerReferenceNumber,
                    AimSeqNumber = 1,
                    ProvSpecDelMonOccur = Generics.ProviderSpecifiedDeliveryMonitoringD,
                    ProvSpecDelMon = "D006801"
                }
            };

            var provSpecDelMonA = _modelBuilder.LookupProvSpecDelMon(provSpecDelMons, Generics.ProviderSpecifiedDeliveryMonitoringA);
            var provSpecDelMonB = _modelBuilder.LookupProvSpecDelMon(provSpecDelMons, Generics.ProviderSpecifiedDeliveryMonitoringB);
            var provSpecDelMonC = _modelBuilder.LookupProvSpecDelMon(provSpecDelMons, Generics.ProviderSpecifiedDeliveryMonitoringC);
            var provSpecDelMonD = _modelBuilder.LookupProvSpecDelMon(provSpecDelMons, Generics.ProviderSpecifiedDeliveryMonitoringD);

            provSpecDelMonA?.Should().Be("1920");
            provSpecDelMonB?.Should().Be("E5072");
            provSpecDelMonC?.Should().Be("CHILD");
            provSpecDelMonD?.Should().Be("D006801");
        }

        [Fact]
        public void TestLookupAecPriceEpisode()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                Ukprn = _ukprn,
                PaymentLearnerReferenceNumber = _learnerReferenceNumber,
                PaymentPriceEpisodeIdentifier = _priceEpisodeIdentifier
            };

            var aecPriceEpisode = _modelBuilder.LookupAecPriceEpisode(reportRowModel, _rulebaseData);

            aecPriceEpisode?.Ukprn.Should().Be(_ukprn);
            aecPriceEpisode?.LearnRefNumber.Should().Be(_learnerReferenceNumber);
            aecPriceEpisode?.PriceEpisodeIdentifier.Should().Be(_priceEpisodeIdentifier);
        }

        [Fact]
        public void TestLookupAecLearningDelivery()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                Ukprn = _ukprn,
                PaymentLearnerReferenceNumber = _learnerReferenceNumber,
                PaymentEarningEventAimSeqNumber = _learningAimSequenceNumber,
                PaymentLearningAimReference = _learningAimReferenceNumber,
            };

            var aecLearningDelivery = _modelBuilder.LookupAecLearningDelivery(reportRowModel, _rulebaseData, _learningAimSequenceNumber);

            aecLearningDelivery?.Ukprn.Should().Be(_ukprn);
            aecLearningDelivery?.LearnRefNumber.Should().Be(_learnerReferenceNumber);
            aecLearningDelivery?.AimSequenceNumber.Should().Be(_learningAimSequenceNumber);
            aecLearningDelivery?.LearnAimRef.Should().Be(_learningAimReferenceNumber);
        }

        [Fact]
        public void TestLookupLearnerEmploymentStatus()
        {
            var learnerEmploymentStatusData = new List<AppsMonthlyPaymentLearnerEmploymentStatusInfo>()
            {
                new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                {
                    Ukprn = _ukprn,
                    LearnRefNumber = _learnerReferenceNumber,
                    DateEmpStatApp = new DateTime(2018, 8, 1),
                    EmpStat = 10, // 10 In paid employment
                    EmpdId = 905118782,
                    AgreeId = "5YJB5B"
                },
                new AppsMonthlyPaymentLearnerEmploymentStatusInfo()
                {
                    Ukprn = _ukprn,
                    LearnRefNumber = _learnerReferenceNumber,
                    DateEmpStatApp = new DateTime(2019, 10, 7),
                    EmpStat = 10, // 10 In paid employment
                    EmpdId = 905118782,
                    AgreeId = "5YJB6B"
                }
            };

            var employmentStatus = _modelBuilder.LookupLearnerEmploymentStatus(learnerEmploymentStatusData, _learningStartDate);

            employmentStatus?.Ukprn.Should().Be(_ukprn);
            employmentStatus?.LearnRefNumber.Should().Be(_learnerReferenceNumber);
            employmentStatus?.DateEmpStatApp.Should().Be(new DateTime(2018, 8, 1));
            employmentStatus?.EmpStat.Should().Be(10);
            employmentStatus?.EmpdId.Should().Be(905118782);
            employmentStatus?.AgreeId.Should().Be("5YJB5B");
        }

        [Fact]
        public void TestGetPaymentTypeTotals()
        {
            var total = _modelBuilder.GetPaymentTypeTotals(1, 2, 3, 4, 5, 6, 7);

            total.Should().Be(28);
        }

        [Fact]
        public void TestGetAugPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                AugustLevyPayments = 1,
                AugustCoInvestmentPayments = 2,
                AugustEmployerAdditionalPayments = 3,
                AugustProviderAdditionalPayments = 4,
                AugustApprenticeAdditionalPayments = 5,
                AugustEnglishAndMathsPayments = 6,
                AugustLearningSupportDisadvantageAndFrameworkUpliftPayments = 7
            };

            reportRowModel.AugustTotalPayments = _modelBuilder.GetAugPaymentTypeTotals(reportRowModel);

            reportRowModel?.AugustTotalPayments.Should().Be(28);
        }

        [Fact]
        public void TestGetSepPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                SeptemberLevyPayments = 2,
                SeptemberCoInvestmentPayments = 3,
                SeptemberEmployerAdditionalPayments = 4,
                SeptemberProviderAdditionalPayments = 5,
                SeptemberApprenticeAdditionalPayments = 6,
                SeptemberEnglishAndMathsPayments = 7,
                SeptemberLearningSupportDisadvantageAndFrameworkUpliftPayments = 8
            };

            reportRowModel.SeptemberTotalPayments = _modelBuilder.GetSepPaymentTypeTotals(reportRowModel);

            reportRowModel?.SeptemberTotalPayments.Should().Be(35);
        }

        [Fact]
        public void TestGetOctPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                OctoberLevyPayments = 3,
                OctoberCoInvestmentPayments = 4,
                OctoberEmployerAdditionalPayments = 5,
                OctoberProviderAdditionalPayments = 6,
                OctoberApprenticeAdditionalPayments = 7,
                OctoberEnglishAndMathsPayments = 8,
                OctoberLearningSupportDisadvantageAndFrameworkUpliftPayments = 9
            };

            reportRowModel.OctoberTotalPayments = _modelBuilder.GetOctPaymentTypeTotals(reportRowModel);

            reportRowModel?.OctoberTotalPayments.Should().Be(42);
        }

        [Fact]
        public void TestGetNovPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                NovemberLevyPayments = 4,
                NovemberCoInvestmentPayments = 5,
                NovemberEmployerAdditionalPayments = 6,
                NovemberProviderAdditionalPayments = 7,
                NovemberApprenticeAdditionalPayments = 8,
                NovemberEnglishAndMathsPayments = 9,
                NovemberLearningSupportDisadvantageAndFrameworkUpliftPayments = 10
            };

            reportRowModel.NovemberTotalPayments = _modelBuilder.GetNovPaymentTypeTotals(reportRowModel);

            reportRowModel?.NovemberTotalPayments.Should().Be(49);
        }

        [Fact]
        public void TestGetDecPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                DecemberLevyPayments = 5,
                DecemberCoInvestmentPayments = 6,
                DecemberEmployerAdditionalPayments = 7,
                DecemberProviderAdditionalPayments = 8,
                DecemberApprenticeAdditionalPayments = 9,
                DecemberEnglishAndMathsPayments = 10,
                DecemberLearningSupportDisadvantageAndFrameworkUpliftPayments = 11
            };

            reportRowModel.DecemberTotalPayments = _modelBuilder.GetDecPaymentTypeTotals(reportRowModel);

            reportRowModel?.DecemberTotalPayments.Should().Be(56);
        }

        [Fact]
        public void TestGetJanPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                JanuaryLevyPayments = 6,
                JanuaryCoInvestmentPayments = 7,
                JanuaryEmployerAdditionalPayments = 8,
                JanuaryProviderAdditionalPayments = 9,
                JanuaryApprenticeAdditionalPayments = 10,
                JanuaryEnglishAndMathsPayments = 11,
                JanuaryLearningSupportDisadvantageAndFrameworkUpliftPayments = 12
            };

            reportRowModel.JanuaryTotalPayments = _modelBuilder.GetJanPaymentTypeTotals(reportRowModel);

            reportRowModel?.JanuaryTotalPayments.Should().Be(63);
        }

        [Fact]
        public void TestGetFebPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                FebruaryLevyPayments = 7,
                FebruaryCoInvestmentPayments = 8,
                FebruaryEmployerAdditionalPayments = 9,
                FebruaryProviderAdditionalPayments = 10,
                FebruaryApprenticeAdditionalPayments = 11,
                FebruaryEnglishAndMathsPayments = 12,
                FebruaryLearningSupportDisadvantageAndFrameworkUpliftPayments = 13
            };

            reportRowModel.FebruaryTotalPayments = _modelBuilder.GetFebPaymentTypeTotals(reportRowModel);

            reportRowModel?.FebruaryTotalPayments.Should().Be(70);
        }

        [Fact]
        public void TestGetMarPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                MarchLevyPayments = 8,
                MarchCoInvestmentPayments = 9,
                MarchEmployerAdditionalPayments = 10,
                MarchProviderAdditionalPayments = 11,
                MarchApprenticeAdditionalPayments = 12,
                MarchEnglishAndMathsPayments = 13,
                MarchLearningSupportDisadvantageAndFrameworkUpliftPayments = 14
            };

            reportRowModel.MarchTotalPayments = _modelBuilder.GetMarPaymentTypeTotals(reportRowModel);

            reportRowModel?.MarchTotalPayments.Should().Be(77);
        }

        [Fact]
        public void TestGetAprPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                AprilLevyPayments = 9,
                AprilCoInvestmentPayments = 10,
                AprilEmployerAdditionalPayments = 11,
                AprilProviderAdditionalPayments = 12,
                AprilApprenticeAdditionalPayments = 13,
                AprilEnglishAndMathsPayments = 14,
                AprilLearningSupportDisadvantageAndFrameworkUpliftPayments = 15
            };

            reportRowModel.AprilTotalPayments = _modelBuilder.GetAprPaymentTypeTotals(reportRowModel);

            reportRowModel?.AprilTotalPayments.Should().Be(84);
        }

        [Fact]
        public void TestGetMayPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                MayLevyPayments = 10,
                MayCoInvestmentPayments = 11,
                MayEmployerAdditionalPayments = 12,
                MayProviderAdditionalPayments = 13,
                MayApprenticeAdditionalPayments = 14,
                MayEnglishAndMathsPayments = 15,
                MayLearningSupportDisadvantageAndFrameworkUpliftPayments = 16
            };

            reportRowModel.MayTotalPayments = _modelBuilder.GetMayPaymentTypeTotals(reportRowModel);

            reportRowModel?.MayTotalPayments.Should().Be(91);
        }

        [Fact]
        public void TestGetJunPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                JuneLevyPayments = 11,
                JuneCoInvestmentPayments = 12,
                JuneEmployerAdditionalPayments = 13,
                JuneProviderAdditionalPayments = 14,
                JuneApprenticeAdditionalPayments = 15,
                JuneEnglishAndMathsPayments = 16,
                JuneLearningSupportDisadvantageAndFrameworkUpliftPayments = 17
            };

            reportRowModel.JuneTotalPayments = _modelBuilder.GetJunPaymentTypeTotals(reportRowModel);

            reportRowModel?.JuneTotalPayments.Should().Be(98);
        }

        [Fact]
        public void TestGetJulPaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                JulyLevyPayments = 12,
                JulyCoInvestmentPayments = 13,
                JulyEmployerAdditionalPayments = 14,
                JulyProviderAdditionalPayments = 15,
                JulyApprenticeAdditionalPayments = 16,
                JulyEnglishAndMathsPayments = 17,
                JulyLearningSupportDisadvantageAndFrameworkUpliftPayments = 18
            };

            reportRowModel.JulyTotalPayments = _modelBuilder.GetJulPaymentTypeTotals(reportRowModel);

            reportRowModel?.JulyTotalPayments.Should().Be(105);
        }

        [Fact]
        public void TestGetR13PaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                R13LevyPayments = 13,
                R13CoInvestmentPayments = 14,
                R13EmployerAdditionalPayments = 15,
                R13ProviderAdditionalPayments = 16,
                R13ApprenticeAdditionalPayments = 17,
                R13EnglishAndMathsPayments = 18,
                R13LearningSupportDisadvantageAndFrameworkUpliftPayments = 19
            };

            reportRowModel.R13TotalPayments = _modelBuilder.GetR13PaymentTypeTotals(reportRowModel);

            reportRowModel?.R13TotalPayments.Should().Be(112);
        }

        [Fact]
        public void TestGetR14PaymentTypeTotals()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                R14LevyPayments = 14,
                R14CoInvestmentPayments = 15,
                R14EmployerAdditionalPayments = 16,
                R14ProviderAdditionalPayments = 17,
                R14ApprenticeAdditionalPayments = 18,
                R14EnglishAndMathsPayments = 19,
                R14LearningSupportDisadvantageAndFrameworkUpliftPayments = 20
            };

            reportRowModel.R14TotalPayments = _modelBuilder.GetR14PaymentTypeTotals(reportRowModel);

            reportRowModel?.R14TotalPayments.Should().Be(119);
        }

        [Fact]
        public void TestCalculateTotalPayments()
        {
            var reportRowModel = new AppsMonthlyPaymentReportRowModel()
            {
                AugustTotalPayments = 28,
                SeptemberTotalPayments = 35,
                OctoberTotalPayments = 42,
                NovemberTotalPayments = 49,
                DecemberTotalPayments = 56,
                JanuaryTotalPayments = 63,
                FebruaryTotalPayments = 70,
                MarchTotalPayments = 77,
                AprilTotalPayments = 84,
                MayTotalPayments = 91,
                JuneTotalPayments = 98,
                JulyTotalPayments = 105,
                R13TotalPayments = 112,
                R14TotalPayments = 119
            };

            reportRowModel.TotalPayments = _modelBuilder.CalculateTotalPayments(reportRowModel);
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
                    LearningAimReference = "50089638", LearningStartDate = new DateTime(2019, 04, 13),
                    LearningAimProgrammeType = 2, LearningAimStandardCode = 0, LearningAimFrameworkCode = 445,
                    LearningAimPathwayCode = 3,
                    ReportingAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding",
                    PriceEpisodeIdentifier = string.Empty, ContractType = 1, TransactionType = 13, FundingSource = 4,
                    DeliveryPeriod = 1, CollectionPeriod = 4,
                    EarningEventId = new Guid("00000000-0000-0000-0000-000000000000")
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

