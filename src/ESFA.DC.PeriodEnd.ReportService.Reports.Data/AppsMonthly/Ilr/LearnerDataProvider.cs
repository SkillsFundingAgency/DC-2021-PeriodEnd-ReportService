using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR2021.DataStore.EF.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;
using Learner = ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model.Learner;
using LearningDelivery = ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model.LearningDelivery;
using LearnerEmploymentStatus = ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model.LearnerEmploymentStatus;
using LearningDeliveryFam = ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model.LearningDeliveryFam;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly.Ilr
{
    public class LearnerDataProvider : ILearnerDataProvider
    {
        private readonly Func<IIlr2021Context> _ilr;

        public LearnerDataProvider(Func<IIlr2021Context> ilr)
        {
            _ilr = ilr;
        }

        public async Task<ICollection<Learner>> GetLearnersAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var ilrContext = _ilr())
            {
                return await ilrContext.Learners.Where(wl => wl.UKPRN == ukprn)
                    .Select(l => new Learner
                    {
                        LearnRefNumber = l.LearnRefNumber,
                        FamilyName = l.FamilyName,
                        GivenNames = l.GivenNames,
                        CampusIdentifier = l.CampId,
                        LearningDeliveries = l.LearningDeliveries
                            .Select(ld => new LearningDelivery
                            {
                                LearnAimRef = ld.LearnRefNumber,
                                LearnStartDate = ld.LearnStartDate,
                                ProgType = ld.ProgType ?? 0,
                                StdCode = ld.StdCode ?? 0,
                                FworkCode = ld.FworkCode ?? 0,
                                PwayCode = ld.PwayCode ?? 0,
                                AimSequenceNumber = ld.AimSeqNumber,
                                OrigLearnStartDate = ld.OrigLearnStartDate,
                                LearnPlanEndDate = ld.LearnPlanEndDate,
                                CompStatus = ld.CompStatus,
                                LearnActEndDate = ld.LearnActEndDate,
                                AchDate = ld.AchDate,
                                Outcome = ld.Outcome,
                                AimType = ld.AimType,
                                SWSupAimId = ld.SWSupAimId,
                                EPAOrgId = ld.EPAOrgID,
                                PartnerUkprn = ld.PartnerUKPRN,
                                LearningDeliveryFams = ld.LearningDeliveryFAMs
                                    .Select(ldfam => new LearningDeliveryFam
                                    {
                                        Type = ldfam.LearnDelFAMType,
                                        Code = ldfam.LearnDelFAMCode
                                    } ).ToList(),
                                ProviderSpecDeliveryMonitorings = ld.ProviderSpecDeliveryMonitorings
                                    .Select(psdm => new ProviderMonitoring
                                    {
                                        Occur = psdm.ProvSpecDelMonOccur,
                                        Mon = psdm.ProvSpecDelMon
                                    }).ToList(),
                                AecLearningDelivery = new AecLearningDelivery()
                                {
                                    PlannedNumOnProgInstalm = ld.AEC_LearningDelivery.PlannedNumOnProgInstalm
                                }
                            }).ToList(),
                        ProviderSpecLearnMonitorings = l.ProviderSpecLearnerMonitorings
                            .Select(psm => new ProviderMonitoring
                            {
                                Occur = psm.ProvSpecLearnMonOccur,
                                Mon =  psm.ProvSpecLearnMon
                            }).ToList(),
                        LearnerEmploymentStatuses = l.LearnerEmploymentStatuses
                            .Select(les => new LearnerEmploymentStatus
                            {
                                EmpId = les.EmpId,
                                EmpStat = les.EmpStat,
                                DateEmpStatApp = les.DateEmpStatApp
                            }).ToList()
                    }).ToListAsync(cancellationToken);
            }
        }
    }
}
