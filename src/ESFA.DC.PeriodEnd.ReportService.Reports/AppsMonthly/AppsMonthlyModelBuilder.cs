using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly
{
    public class AppsMonthlyModelBuilder
    {
        private const string NoContract = "No contract";

        private IDictionary<string, string> _fundingStreamPeriodLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017LevyContract1618] = FundingStreamPeriodCodeConstants.LEVY1799,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipEmployerOnAppServiceLevyFunding1618] = FundingStreamPeriodCodeConstants.LEVY1799,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017LevyContract19Plus] = FundingStreamPeriodCodeConstants.LEVY1799,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipEmployerOnAppServiceLevyFunding19Plus] = FundingStreamPeriodCodeConstants.LEVY1799,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017NonLevyContract1618] = FundingStreamPeriodCodeConstants.APPS2021,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017NonLevyContractNonProcured1618] = FundingStreamPeriodCodeConstants.APPS2021,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipNonLevyContractProcured1618] = FundingStreamPeriodCodeConstants.C1618NLAP2018,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipEmployerOnAppServiceNonLevyFunding1618] = FundingStreamPeriodCodeConstants.NONLEVY2019,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017NonLevyContract19Plus] = FundingStreamPeriodCodeConstants.APPS2021,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017NonLevyContractNonProcured19Plus] = FundingStreamPeriodCodeConstants.APPS2021,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipNonLevyContractProcured19Plus] = FundingStreamPeriodCodeConstants.ANLAP2018,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipEmployerOnAppServiceNonLevyFunding19Plus] = FundingStreamPeriodCodeConstants.NONLEVY2019,
        };

        public IEnumerable<AppsMonthlyRecord> Build(IEnumerable<Payment> payments, IEnumerable<Learner> learners, IEnumerable<ContractAllocation> contractAllocations)
        {
            var contractNumberLookup = BuildContractNumbersLookup(contractAllocations);

            return payments
                .GroupBy(
                    p =>
                        new RecordKey(p.LearnerReferenceNumber,
                            p.LearnerUln,
                            p.LearningAimReference,
                            p.LearningStartDate,
                            p.LearningAimProgrammeType,
                            p.LearningAimStandardCode,
                            p.LearningAimFrameworkCode,
                            p.LearningAimPathwayCode,
                            p.ReportingAimFundingLineType,
                            p.PriceEpisodeIdentifier))
                .Select(k =>
                {
                    var learner = GetLearnerForRecord(learners, k.Key);
                    var learningDelivery = GetLearnerLearningDeliveryForRecord(learner, k.Key);

                    var fundingStreamPeriod = FundingStreamPeriodForReportingAimFundingLineType(k.Key.ReportingAimFundingLineType);

                    var contractNumber = contractNumberLookup.GetValueOrDefault(fundingStreamPeriod, NoContract);

                    return new AppsMonthlyRecord()
                    {
                        RecordKey = k.Key,
                        Learner = learner,
                        LearningDelivery = learningDelivery,
                        ContractNumber = contractNumber,
                    };
                });
        }

        public Learner GetLearnerForRecord(IEnumerable<Learner> learners, RecordKey recordKey)
        {
            return learners.FirstOrDefault(l => l.LearnRefNumber.CaseInsensitiveEquals(recordKey.LearnerReferenceNumber));
        }

        public LearningDelivery GetLearnerLearningDeliveryForRecord(Learner learner, RecordKey recordKey)
        {
            // BR2 - UKPRN and LearnRefNumber is implicitly matched, not included on models
            return learner?
                .LearningDeliveries?
                .FirstOrDefault(ld =>
                    ld.ProgType == recordKey.ProgrammeType
                    && ld.StdCode == recordKey.StandardCode
                    && ld.FworkCode == recordKey.FrameworkCode
                    && ld.PwayCode == recordKey.PathwayCode
                    && ld.LearnStartDate == recordKey.LearnStartDate
                    && ld.LearnAimRef.CaseInsensitiveEquals(recordKey.LearningAimReference));
        }

        public string FundingStreamPeriodForReportingAimFundingLineType(string reportingAimFundingLineType) =>  _fundingStreamPeriodLookup.GetValueOrDefault(reportingAimFundingLineType);

        public IDictionary<string, string> BuildContractNumbersLookup(IEnumerable<ContractAllocation> contractAllocations)
        {
            return contractAllocations
                .GroupBy(ca => ca.FundingStreamPeriod, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    k => k.Key,
                    v => string.Join("; ", v.Select(ca => ca.ContractAllocationNumber).OrderBy(ca => ca)));
        }
    }
}
