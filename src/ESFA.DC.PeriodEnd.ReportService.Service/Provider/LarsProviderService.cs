using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Data.LARS.Model;
using ESFA.DC.Data.LARS.Model.Interfaces;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.Lars;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public sealed class LarsProviderService : ILarsProviderService
    {
        private readonly ILogger _logger;
        private readonly IReportServiceConfiguration _reportServiceConfiguration;

        private readonly SemaphoreSlim _getLearningDeliveriesLock;

        private readonly SemaphoreSlim _getFrameworkAimsLock;

        private readonly SemaphoreSlim _getVersionLock;

        private readonly SemaphoreSlim _getStandardsLock;

        private readonly Dictionary<int, string> _loadedStandards;

        private Dictionary<string, LarsLearningDelivery> _loadedLearningDeliveries;

        private List<LearnerAndDeliveries> _loadedFrameworkAims;

        private string _version;

        public LarsProviderService(ILogger logger, IReportServiceConfiguration reportServiceConfiguration)
        {
            _logger = logger;
            _reportServiceConfiguration = reportServiceConfiguration;
            _loadedLearningDeliveries = null;
            _loadedFrameworkAims = null;
            _version = null;

            _loadedStandards = new Dictionary<int, string>();
            _getLearningDeliveriesLock = new SemaphoreSlim(1, 1);
            _getFrameworkAimsLock = new SemaphoreSlim(1, 1);
            _getVersionLock = new SemaphoreSlim(1, 1);
            _getStandardsLock = new SemaphoreSlim(1, 1);
        }

        public async Task<Dictionary<string, LarsLearningDelivery>> GetLearningDeliveriesAsync(
            string[] validLearnerAimRefs,
            CancellationToken cancellationToken)
        {
            await _getLearningDeliveriesLock.WaitAsync(cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (_loadedLearningDeliveries == null)
                {
                    ILARS larsContext = new LARS(_reportServiceConfiguration.LarsConnectionString);
                    _loadedLearningDeliveries = await larsContext.LARS_LearningDelivery
                        .Where(
                            x => validLearnerAimRefs.Contains(x.LearnAimRef))
                        .ToDictionaryAsync(
                            k => k.LearnAimRef,
                            v => new LarsLearningDelivery
                            {
                                LearningAimTitle = v.LearnAimRefTitle,
                                NotionalNvqLevel = v.NotionalNVQLevelv2,
                                Tier2SectorSubjectArea = v.SectorSubjectAreaTier2,
                                FrameworkCommonComponent = v.FrameworkCommonComponent
                            },
                            StringComparer.InvariantCultureIgnoreCase,
                            cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get LARS learning deliveries", ex);
            }
            finally
            {
                _getLearningDeliveriesLock.Release();
            }

            return _loadedLearningDeliveries;
        }

        public async Task<string> GetStandardAsync(
            int learningDeliveryStandardCode,
            CancellationToken cancellationToken)
        {
            await _getStandardsLock.WaitAsync(cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (!_loadedStandards.ContainsKey(learningDeliveryStandardCode))
                {
                    ILARS larsContext = new LARS(_reportServiceConfiguration.LarsConnectionString);
                    LARS_Standard larsStandard = await larsContext.LARS_Standard
                        .SingleOrDefaultAsync(l => l.StandardCode == learningDeliveryStandardCode, cancellationToken);
                    _loadedStandards[learningDeliveryStandardCode] = larsStandard?.NotionalEndLevel ?? "NA";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get LARS standards", ex);
            }
            finally
            {
                _getStandardsLock.Release();
            }

            return _loadedStandards[learningDeliveryStandardCode];
        }

        public async Task<List<LearnerAndDeliveries>> GetFrameworkAimsAsync(
            string[] learnAimRefs,
            List<ILearner> learners,
            CancellationToken cancellationToken)
        {
            await _getFrameworkAimsLock.WaitAsync(cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (_loadedFrameworkAims == null)
                {
                    _loadedFrameworkAims = new List<LearnerAndDeliveries>();
                    foreach (ILearner learner in learners)
                    {
                        List<LearningDelivery> learningDeliveries = new List<LearningDelivery>();
                        foreach (ILearningDelivery learningDelivery in learner.LearningDeliveries)
                        {
                            learningDeliveries.Add(new LearningDelivery(learningDelivery.LearnAimRef, learningDelivery.AimSeqNumber, learningDelivery.FworkCodeNullable, learningDelivery.ProgTypeNullable, learningDelivery.PwayCodeNullable, learningDelivery.LearnStartDate));
                        }

                        _loadedFrameworkAims.Add(new LearnerAndDeliveries(learner.LearnRefNumber, learningDeliveries));
                    }

                    ILARS larsContext = new LARS(_reportServiceConfiguration.LarsConnectionString);
                    LarsFrameworkAim[] res = await larsContext.LARS_FrameworkAims
                        .Where(x => learnAimRefs.Contains(x.LearnAimRef))
                        .Select(x =>
                            new LarsFrameworkAim
                            {
                                FworkCode = x.FworkCode,
                                ProgType = x.ProgType,
                                PwayCode = x.PwayCode,
                                LearnAimRef = x.LearnAimRef,
                                EffectiveFrom = x.EffectiveFrom,
                                EffectiveTo = x.EffectiveTo ?? DateTime.MaxValue,
                                FrameworkComponentType = x.FrameworkComponentType
                            })
                        .OrderByDescending(x => x.EffectiveTo)
                        .ToArrayAsync(cancellationToken);

                    foreach (LearnerAndDeliveries learnerAndDelivery in _loadedFrameworkAims)
                    {
                        foreach (LearningDelivery learningDelivery in learnerAndDelivery.LearningDeliveries)
                        {
                            learningDelivery.FrameworkComponentType = res.FirstOrDefault(x =>
                                (learningDelivery.FworkCode == null || x.FworkCode == learningDelivery.FworkCode) &&
                                (learningDelivery.ProgType == null || x.ProgType == learningDelivery.ProgType) &&
                                (learningDelivery.PwayCode == null || x.PwayCode == learningDelivery.PwayCode) &&
                                string.Equals(x.LearnAimRef, learningDelivery.LearningDeliveryLearnAimRef, StringComparison.OrdinalIgnoreCase) &&
                                x.EffectiveFrom < learningDelivery.LearningDeliveryLearnStartDate &&
                                x.EffectiveTo > learningDelivery.LearningDeliveryLearnStartDate)?.FrameworkComponentType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get LARS framework aims", ex);
            }
            finally
            {
                _getFrameworkAimsLock.Release();
            }

            return _loadedFrameworkAims;
        }

        public async Task<string> GetVersionAsync(CancellationToken cancellationToken)
        {
            await _getVersionLock.WaitAsync(cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(_version))
                {
                    ILARS larsContext = new LARS(_reportServiceConfiguration.LarsConnectionString);
                    _version = (await larsContext.Current_Version.SingleAsync(cancellationToken)).CurrentVersion;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get LARS version information", ex);
            }
            finally
            {
                _getVersionLock.Release();
            }

            return _version;
        }

        public async Task<List<AppsMonthlyPaymentLarsLearningDeliveryInfo>> GetLarsLearningDeliveryInfoForAppsMonthlyPaymentReportAsync(string[] learnerAimRefs, CancellationToken cancellationToken)
        {
            var appsMonthlyPaymentLarsLearningDeliveryInfoList = new List<AppsMonthlyPaymentLarsLearningDeliveryInfo>();

            cancellationToken.ThrowIfCancellationRequested();

            using (var larsContext = new LARS(_reportServiceConfiguration.LarsConnectionString))
            {
                var larsLearningDeliveries = await larsContext.LARS_LearningDelivery.Where(x => learnerAimRefs.Contains(x.LearnAimRef)).ToListAsync(cancellationToken);

                foreach (var learningDelivery in larsLearningDeliveries)
                {
                    appsMonthlyPaymentLarsLearningDeliveryInfoList.Add(new AppsMonthlyPaymentLarsLearningDeliveryInfo()
                    {
                        LearnAimRef = learningDelivery.LearnAimRef,
                        LearningAimTitle = learningDelivery.LearnAimRefTitle
                    });
                }
            }

            return appsMonthlyPaymentLarsLearningDeliveryInfoList;
        }
    }
}
