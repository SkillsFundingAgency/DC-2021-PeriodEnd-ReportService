using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.EAS1920.EF.Interface;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR1920.DataStore.EF.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Model;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class PeriodisedValuesLookupProviderService : IPeriodisedValuesLookupProviderService
    {
        private readonly Func<IIlr1920RulebaseContext> _rulebaseContextFactory;
        private readonly Func<IEasdbContext> _easContextFactory;
        private readonly Func<IDASPaymentsContext> _dasContextFactory;

        public PeriodisedValuesLookupProviderService(
            Func<IIlr1920RulebaseContext> rulebaseContextFactory,
            Func<IEasdbContext> easContextFactory,
            Func<IDASPaymentsContext> dasContextFactory)
        {
            _rulebaseContextFactory = rulebaseContextFactory;
            _easContextFactory = easContextFactory;
            _dasContextFactory = dasContextFactory;
        }

        public async Task<IPeriodisedValuesLookup> ProvideAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var ukprn = reportServiceContext.Ukprn;

            var fm35 = BuildFm35DictionaryAsync(ukprn, cancellationToken);
            var fm25 = BuildFm25DictionaryAsync(ukprn, cancellationToken);
            var fm81 = BuildFm81DictionaryAsync(ukprn, cancellationToken);
            var fm99 = BuildFm99DictionaryAsync(ukprn, cancellationToken);
            var eas = BuildEasDictionaryAsync(ukprn, cancellationToken);
            var das = BuildDasDictionaryAsync(ukprn, cancellationToken);

            await Task.WhenAll(fm35, fm25, fm81, fm99, eas, das);

            var periodisedValuesLookup = new PeriodisedValuesLookup();

            periodisedValuesLookup.Add(FundingDataSource.FM35, fm35.Result);
            periodisedValuesLookup.Add(FundingDataSource.FM25, fm25.Result);
            periodisedValuesLookup.Add(FundingDataSource.FM81, fm81.Result);
            periodisedValuesLookup.Add(FundingDataSource.FM99, fm99.Result);
            periodisedValuesLookup.Add(FundingDataSource.EAS, eas.Result);
            periodisedValuesLookup.Add(FundingDataSource.DAS, das.Result);

            return periodisedValuesLookup;
        }

        public async Task<Dictionary<string, Dictionary<string, decimal?[][]>>> BuildFm35DictionaryAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var context = _rulebaseContextFactory())
            {
                var periodisedValues = await context
                    .FM35_LearningDelivery_PeriodisedValues
                    .Where(
                        u => u.UKPRN == ukprn)
                    .GroupBy(pv => new { pv.FM35_LearningDelivery.FundLine, pv.AttributeName })
                    .Select(
                        pv =>
                            new PeriodisedValuesFlattened()
                            {
                                FundLine = pv.Key.FundLine,
                                AttributeName = pv.Key.AttributeName,
                                Periods = new decimal?[]
                                    {
                                        pv.Sum(a => a.Period_1),
                                        pv.Sum(a => a.Period_2),
                                        pv.Sum(a => a.Period_3),
                                        pv.Sum(a => a.Period_4),
                                        pv.Sum(a => a.Period_5),
                                        pv.Sum(a => a.Period_6),
                                        pv.Sum(a => a.Period_7),
                                        pv.Sum(a => a.Period_8),
                                        pv.Sum(a => a.Period_9),
                                        pv.Sum(a => a.Period_10),
                                        pv.Sum(a => a.Period_11),
                                        pv.Sum(a => a.Period_12),
                                    }
                            }).ToListAsync(cancellationToken);

                return BuildDictionary(periodisedValues);
            }
        }

        public async Task<Dictionary<string, Dictionary<string, decimal?[][]>>> BuildFm25DictionaryAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var context = _rulebaseContextFactory())
            {
                var periodisedValues = await context
                    .FM25_FM35_Learner_PeriodisedValues
                    .Where(
                        u => u.UKPRN == ukprn)
                    .GroupBy(pv => new { pv.FM25_Learner.FundLine, pv.AttributeName })
                    .Select(
                        pv =>
                            new PeriodisedValuesFlattened()
                            {
                                FundLine = pv.Key.FundLine,
                                AttributeName = pv.Key.AttributeName,
                                Periods = new decimal?[]
                                {
                                    pv.Sum(a => a.Period_1),
                                    pv.Sum(a => a.Period_2),
                                    pv.Sum(a => a.Period_3),
                                    pv.Sum(a => a.Period_4),
                                    pv.Sum(a => a.Period_5),
                                    pv.Sum(a => a.Period_6),
                                    pv.Sum(a => a.Period_7),
                                    pv.Sum(a => a.Period_8),
                                    pv.Sum(a => a.Period_9),
                                    pv.Sum(a => a.Period_10),
                                    pv.Sum(a => a.Period_11),
                                    pv.Sum(a => a.Period_12),
                                }
                            }).ToListAsync(cancellationToken);

                return BuildDictionary(periodisedValues);
            }
        }

        public async Task<Dictionary<string, Dictionary<string, decimal?[][]>>> BuildFm81DictionaryAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var context = _rulebaseContextFactory())
            {
                var periodisedValues = await context
                    .TBL_LearningDelivery_PeriodisedValues
                    .Where(
                        u => u.UKPRN == ukprn)
                    .GroupBy(pv => new { pv.TBL_LearningDelivery.FundLine, pv.AttributeName })
                    .Select(
                        pv =>
                            new PeriodisedValuesFlattened()
                            {
                                FundLine = pv.Key.FundLine,
                                AttributeName = pv.Key.AttributeName,
                                Periods = new decimal?[]
                                {
                                    pv.Sum(a => a.Period_1),
                                    pv.Sum(a => a.Period_2),
                                    pv.Sum(a => a.Period_3),
                                    pv.Sum(a => a.Period_4),
                                    pv.Sum(a => a.Period_5),
                                    pv.Sum(a => a.Period_6),
                                    pv.Sum(a => a.Period_7),
                                    pv.Sum(a => a.Period_8),
                                    pv.Sum(a => a.Period_9),
                                    pv.Sum(a => a.Period_10),
                                    pv.Sum(a => a.Period_11),
                                    pv.Sum(a => a.Period_12),
                                }
                            }).ToListAsync(cancellationToken);

                return BuildDictionary(periodisedValues);
            }
        }

        public async Task<Dictionary<string, Dictionary<string, decimal?[][]>>> BuildFm99DictionaryAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var context = _rulebaseContextFactory())
            {
                var periodisedValues = await context
                    .ALB_LearningDelivery_PeriodisedValues
                    .Where(
                        u => u.UKPRN == ukprn)
                    .GroupBy(pv => new { pv.ALB_LearningDelivery.FundLine, pv.AttributeName })
                    .Select(
                        pv =>
                            new PeriodisedValuesFlattened()
                            {
                                FundLine = pv.Key.FundLine,
                                AttributeName = pv.Key.AttributeName,
                                Periods = new decimal?[]
                                {
                                    pv.Sum(a => a.Period_1),
                                    pv.Sum(a => a.Period_2),
                                    pv.Sum(a => a.Period_3),
                                    pv.Sum(a => a.Period_4),
                                    pv.Sum(a => a.Period_5),
                                    pv.Sum(a => a.Period_6),
                                    pv.Sum(a => a.Period_7),
                                    pv.Sum(a => a.Period_8),
                                    pv.Sum(a => a.Period_9),
                                    pv.Sum(a => a.Period_10),
                                    pv.Sum(a => a.Period_11),
                                    pv.Sum(a => a.Period_12),
                                }
                            }).ToListAsync(cancellationToken);

                return BuildDictionary(periodisedValues);
            }
        }

        public async Task<Dictionary<string, Dictionary<string, decimal?[][]>>> BuildEasDictionaryAsync(int ukprn, CancellationToken cancellationToken)
        {
            var ukprnString = ukprn.ToString();

            using (var context = _easContextFactory())
            {
                var periodisedValues = await context
                    .EasSubmissionValues
                    .Where(v => v.EasSubmission.Ukprn == ukprnString)
                    .GroupBy(pv => new { FundLine = pv.Payment.FundingLine.Name, AttributeName = pv.Payment.AdjustmentType.Name })
                    .Select(
                        pv =>
                            new PeriodisedValuesFlattened()
                            {
                                FundLine = pv.Key.FundLine,
                                AttributeName = pv.Key.AttributeName,
                                Periods = new decimal?[]
                                {
                                    pv.Where(v => v.CollectionPeriod == 1).Sum(a => a.PaymentValue),
                                    pv.Where(v => v.CollectionPeriod == 2).Sum(a => a.PaymentValue),
                                    pv.Where(v => v.CollectionPeriod == 3).Sum(a => a.PaymentValue),
                                    pv.Where(v => v.CollectionPeriod == 4).Sum(a => a.PaymentValue),
                                    pv.Where(v => v.CollectionPeriod == 5).Sum(a => a.PaymentValue),
                                    pv.Where(v => v.CollectionPeriod == 6).Sum(a => a.PaymentValue),
                                    pv.Where(v => v.CollectionPeriod == 7).Sum(a => a.PaymentValue),
                                    pv.Where(v => v.CollectionPeriod == 8).Sum(a => a.PaymentValue),
                                    pv.Where(v => v.CollectionPeriod == 9).Sum(a => a.PaymentValue),
                                    pv.Where(v => v.CollectionPeriod == 10).Sum(a => a.PaymentValue),
                                    pv.Where(v => v.CollectionPeriod == 11).Sum(a => a.PaymentValue),
                                    pv.Where(v => v.CollectionPeriod == 12).Sum(a => a.PaymentValue),
                                }
                            }).ToListAsync(cancellationToken);

                return BuildDictionary(periodisedValues);
            }
        }

        public async Task<Dictionary<string, Dictionary<int, Dictionary<int, decimal?[][]>>>> BuildDasDictionaryAsync(int ukprn, CancellationToken cancellationToken)
        {
            var ukprnString = ukprn.ToString();

            using (var context = _dasContextFactory())
            {
                var periodisedValues = await context
                    .Payments
                    .Where(p => p.Ukprn == ukprn)
                    .GroupBy(pv => new { FundLine = pv.ReportingAimFundingLineType, pv.FundingSource, pv.TransactionType })
                    .Select(
                        pv =>
                            new
                            {
                                pv.Key.FundLine,
                                pv.Key.FundingSource,
                                pv.Key.TransactionType,
                                Periods = new decimal?[]
                                {
                                    pv.Where(v => v.CollectionPeriod == 1).Sum(a => a.Amount),
                                    pv.Where(v => v.CollectionPeriod == 2).Sum(a => a.Amount),
                                    pv.Where(v => v.CollectionPeriod == 3).Sum(a => a.Amount),
                                    pv.Where(v => v.CollectionPeriod == 4).Sum(a => a.Amount),
                                    pv.Where(v => v.CollectionPeriod == 5).Sum(a => a.Amount),
                                    pv.Where(v => v.CollectionPeriod == 6).Sum(a => a.Amount),
                                    pv.Where(v => v.CollectionPeriod == 7).Sum(a => a.Amount),
                                    pv.Where(v => v.CollectionPeriod == 8).Sum(a => a.Amount),
                                    pv.Where(v => v.CollectionPeriod == 9).Sum(a => a.Amount),
                                    pv.Where(v => v.CollectionPeriod == 10).Sum(a => a.Amount),
                                    pv.Where(v => v.CollectionPeriod == 11).Sum(a => a.Amount),
                                    pv.Where(v => v.CollectionPeriod == 12).Sum(a => a.Amount),
                                }
                            }).ToListAsync(cancellationToken);

                return periodisedValues.ToList()
                    .GroupBy(p => p.FundLine, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(k => k.Key,
                        v => v
                            .GroupBy(fsg => (int)fsg.FundingSource)
                            .ToDictionary(fsgk => fsgk.Key,
                                fs =>
                                    fs
                                    .GroupBy(ttg => (int)ttg.TransactionType)
                                    .ToDictionary(k => k.Key,
                                        tt => tt.Select(pvGroup => pvGroup.Periods).ToArray())),
                        StringComparer.OrdinalIgnoreCase);
            }
        }

        private Dictionary<string, Dictionary<string, decimal?[][]>> BuildDictionary(IEnumerable<PeriodisedValuesFlattened> periodisedValues)
        {
            return periodisedValues
                .GroupBy(pv => pv.FundLine, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k.Key,
                    v => v
                        .GroupBy(ldpv => ldpv.AttributeName, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(k => k.Key, value =>
                                value.Select(pvGroup => pvGroup.Periods).ToArray(),
                            StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}