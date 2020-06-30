using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR2021.DataStore.EF.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using Learner = ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model.Learner;
using LearningDelivery = ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model.LearningDelivery;
using LearnerEmploymentStatus = ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model.LearnerEmploymentStatus;
using LearningDeliveryFam = ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model.LearningDeliveryFam;
using Microsoft.EntityFrameworkCore;


namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsCoInvestment.Ilr
{
    public class LearnerDataProvider : ILearnerDataProvider
    {
        private readonly Func<IIlr2021Context> _ilrContext;
        private const int FundModel = 36;

        public LearnerDataProvider(Func<IIlr2021Context> ilrContext)
        {
            _ilrContext = ilrContext;
        }

        public async Task<ICollection<Learner>> GetLearnersAsync(int ukprn, CancellationToken cancellationToken)
        {
            List<Learner> learners;
            using (var ilrContext = _ilrContext())
            {
                
                learners = await ilrContext
                    .Learners
                    .Where(x => x.UKPRN == ukprn && x.LearningDeliveries.Any(ld => ld.FundModel == FundModel && ld.LearnAimRef == LearnAimRefConstants.ZPROG001))
                    .Select(learner =>
                        new Learner()
                        {
                            LearnRefNumber = learner.LearnRefNumber,
                            FamilyName = learner.FamilyName,
                            GivenNames = learner.GivenNames,
                            LearningDeliveries = learner
                                .LearningDeliveries
                                .Where(ld => ld.FundModel == FundModel && ld.LearnAimRef == LearnAimRefConstants.ZPROG001)
                                .Select(ld => new LearningDelivery()
                                {
                                    LearnRefNumber = ld.LearnRefNumber,
                                    LearnAimRef = ld.LearnAimRef,
                                    AimType = ld.AimType,
                                    AimSeqNumber = ld.AimSeqNumber,
                                    LearnStartDate = ld.LearnStartDate,
                                    FundModel = ld.FundModel,
                                    // Note: Payments default to zero instead of null for no value so we need to cater for that here so that joins work correctly
                                    ProgType = ld.ProgType ?? 0,
                                    StdCode = ld.StdCode ?? 0,
                                    FworkCode = ld.FworkCode ?? 0,
                                    PwayCode = ld.PwayCode ?? 0,
                                    SWSupAimId = ld.SWSupAimId,
                                    AppFinRecords = ld.AppFinRecords
                                        .Where(afr => afr.AFinType == FinTypes.PMR)
                                        .Select(afr => new AppFinRecord()
                                        {
                                            LearnRefNumber = afr.LearnRefNumber,
                                            AimSeqNumber = afr.AimSeqNumber,
                                            AFinType = afr.AFinType,
                                            AFinCode = afr.AFinCode,
                                            AFinDate = afr.AFinDate,
                                            AFinAmount = afr.AFinAmount
                                        }).ToList(),
                                    LearningDeliveryFams = ld.LearningDeliveryFAMs
                                        .Where(fam => fam.LearnDelFAMType == LearnDelFamTypeConstants.LDM)
                                        .Select(ldfam => new LearningDeliveryFam()
                                        {
                                            Type = ldfam.LearnDelFAMType,
                                            Code = ldfam.LearnDelFAMCode
                                        }).ToList(),
                                    AECLearningDelivery = new AECLearningDelivery()
                                    {
                                        AppAdjLearnStartDate = ld.AEC_LearningDelivery.AppAdjLearnStartDate
                                    }
                                }).ToList(),
                            LearnerEmploymentStatuses = learner.LearnerEmploymentStatuses.Select(x => new LearnerEmploymentStatus()
                            {
                                LearnRefNumber = x.LearnRefNumber,
                                DateEmpStatApp = x.DateEmpStatApp,
                                EmpId = x.EmpId
                            }).ToList(),
                            
                        }).ToListAsync(cancellationToken);
            }

            return learners;
        }

    }
}
