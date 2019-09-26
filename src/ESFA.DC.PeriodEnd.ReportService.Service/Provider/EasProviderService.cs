using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.EAS1920.EF;
using ESFA.DC.EAS1920.EF.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using Microsoft.EntityFrameworkCore;
//using EasSubmission = ESFA.DC.EAS1920.EF.EasSubmission;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class EasProviderService : AbstractFundModelProviderService, IEasProviderService
    {
        private readonly Func<IEasdbContext> _easContextFactory;

        public EasProviderService(
            ILogger logger,
            Func<IEasdbContext> easContextFactory)
            : base(logger)
        {
            _easContextFactory = easContextFactory;
        }

        public async Task<ProviderEasSubmissionInfo> GetProviderEasSubmissions(
            int ukPrn,
            CancellationToken cancellationToken)
        {
            ProviderEasSubmissionInfo providerEasSubmissionInfo = null;
            try
            {
                providerEasSubmissionInfo = new ProviderEasSubmissionInfo()
                {
                    UKPRN = ukPrn.ToString(),
                    EasSubmissions = new List<ProviderEasSubmission>()
                };

                cancellationToken.ThrowIfCancellationRequested();

                using (var context = _easContextFactory())
                {
                    providerEasSubmissionInfo.EasSubmissions = await context.EasSubmissions
                        .Where(x => x.Ukprn == ukPrn.ToString())
                        .Select(s => new ProviderEasSubmission
                        (
                            s.SubmissionId,
                            s.Ukprn,
                            (byte?) s.CollectionPeriod
                        ))
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS data", ex);
                throw;
            }

            return providerEasSubmissionInfo;
        }

        public async Task<IList<EasSubmissionInfo>> GetEasSubmissionInfo(int ukprn, CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _easContextFactory())
                {
                    return await context.EasSubmissions
                        .Where(x => x.Ukprn == ukprn.ToString())
                        .Select(s => new EasSubmissionInfo
                        {
                            SubmissionId = s.SubmissionId,
                            UKPRN = s.Ukprn,
                            CollectionPeriod = (byte?)s.CollectionPeriod
                        })
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS Submission data", ex);
                throw;
            }
        }

        public async Task<IList<EasSubmissionValueInfo>> GetEasSubmissionValueInfo(Guid submissionId, CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _easContextFactory())
                {
                    return await context.EasSubmissionValues
                        .Where(x => x.SubmissionId == submissionId)
                        .Select(v => new EasSubmissionValueInfo
                        {
                            SubmissionId = v.SubmissionId,
                            CollectionPeriod = (byte?)v.CollectionPeriod,
                            PaymentId = v.PaymentId,
                            PaymentValue = v.PaymentValue
                        })
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS Submission Value data", ex);
                throw;
            }
        }


        public async Task<IList<EasAdjustmentTypeInfo>> GetEasAdjustmentTypeInfo(CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _easContextFactory())
                {
                    return await context.AdjustmentTypes
                        .Select(a => new EasAdjustmentTypeInfo
                        {
                            Id = a.Id,
                            Name = a.Name
                        })
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS Adjustment Type data", ex);
                throw;
            }
        }

        public async Task<IList<EasFundingLineInfo>> GetEasFundingLineInfo(CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _easContextFactory())
                {
                    return await context.FundingLines
                        .Select(a => new EasFundingLineInfo
                        {
                            Id = a.Id,
                            Name = a.Name
                        })
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS Funding Line data", ex);
                throw;
            }
        }

        public async Task<IList<EasSubmissionInfo>> GetEasSubmissions(int ukprn, CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _easContextFactory())
                {
                    return await context.EasSubmissions
                        .Where(x => x.Ukprn == ukprn.ToString())
                        .Select(s => new EasSubmissionInfo
                        {
                            SubmissionId = s.SubmissionId,
                            UKPRN = s.Ukprn,
                            CollectionPeriod = (byte?) s.CollectionPeriod
                        })
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS Submission data", ex);
                throw;
            }
        }
    }
}
