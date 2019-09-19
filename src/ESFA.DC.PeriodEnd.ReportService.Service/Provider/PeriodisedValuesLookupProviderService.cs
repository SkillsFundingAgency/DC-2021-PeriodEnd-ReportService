using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class PeriodisedValuesLookupProviderService : IPeriodisedValuesLookupProviderService
    {
        public IPeriodisedValuesLookup Provide(IEnumerable<FundingDataSource> fundingDataSources, IReportServiceDependentData reportServiceDependentData)
        {
            var periodisedValuesLookup = new PeriodisedValuesLookup();

            if (fundingDataSources.Contains(FundingDataSource.FM35))
            {
                periodisedValuesLookup[FundingDataSource.FM35] = BuildFm35Dictionary(reportServiceDependentData.Get<FM35Global>());
            }

            if (fundingDataSources.Contains(FundingDataSource.FM81))
            {
                periodisedValuesLookup[FundingDataSource.FM81] = BuildFm81Dictionary(reportServiceDependentData.Get<FM81Global>());
            }

            if (fundingDataSources.Contains(FundingDataSource.FM25))
            {
                periodisedValuesLookup[FundingDataSource.FM25] =
                    BuildFm25Dictionary(reportServiceDependentData.Get<FM25Global>());
            }

            if (fundingDataSources.Contains(FundingDataSource.FM36))
            {
                periodisedValuesLookup[FundingDataSource.FM36] = BuildFm36Dictionary(reportServiceDependentData.Get<FM36Global>());
            }

            if (fundingDataSources.Contains(FundingDataSource.FM99))
            {
                periodisedValuesLookup[FundingDataSource.FM99] = BuildFm99Dictionary(reportServiceDependentData.Get<ALBGlobal>());
            }

            if (fundingDataSources.Contains(FundingDataSource.EAS))
            {
                periodisedValuesLookup[FundingDataSource.EAS] = BuildEASDictionary(reportServiceDependentData.Get<ReferenceDataRoot>());
            }

            return periodisedValuesLookup;
        }

        public Dictionary<string, Dictionary<string, decimal?[][]>> BuildFm25Dictionary(FM25Global fm25Global)
        {
            return fm25Global
                       .Learners?
                       .GroupBy(ld => ld.FundLine, StringComparer.OrdinalIgnoreCase)
                       .ToDictionary(k => k.Key,
                           v => v.SelectMany(ld => ld.LearnerPeriodisedValues)
                               .GroupBy(ldpv => ldpv.AttributeName, StringComparer.OrdinalIgnoreCase)
                               .ToDictionary(k => k.Key, value =>
                                       value.Select(pvGroup => new decimal?[]
                                       {
                                           pvGroup.Period1,
                                           pvGroup.Period2,
                                           pvGroup.Period3,
                                           pvGroup.Period4,
                                           pvGroup.Period5,
                                           pvGroup.Period6,
                                           pvGroup.Period7,
                                           pvGroup.Period8,
                                           pvGroup.Period9,
                                           pvGroup.Period10,
                                           pvGroup.Period11,
                                           pvGroup.Period12,
                                       }).ToArray(),
                                   StringComparer.OrdinalIgnoreCase),
                           StringComparer.OrdinalIgnoreCase)
                   ?? new Dictionary<string, Dictionary<string, decimal?[][]>>();
        }

        public Dictionary<string, Dictionary<string, decimal?[][]>> BuildFm35Dictionary(FM35Global fm35Global)
        {
            return fm35Global?
                       .Learners?
                       .SelectMany(l => l.LearningDeliveries)
                       .GroupBy(ld => ld.LearningDeliveryValues.FundLine, StringComparer.OrdinalIgnoreCase)
                       .ToDictionary(k => k.Key,
                           v => v.SelectMany(ld => ld.LearningDeliveryPeriodisedValues)
                               .GroupBy(ldpv => ldpv.AttributeName, StringComparer.OrdinalIgnoreCase)
                               .ToDictionary(k => k.Key, value =>
                                       value.Select(pvGroup => new decimal?[]
                                       {
                                           pvGroup.Period1,
                                           pvGroup.Period2,
                                           pvGroup.Period3,
                                           pvGroup.Period4,
                                           pvGroup.Period5,
                                           pvGroup.Period6,
                                           pvGroup.Period7,
                                           pvGroup.Period8,
                                           pvGroup.Period9,
                                           pvGroup.Period10,
                                           pvGroup.Period11,
                                           pvGroup.Period12,
                                       }).ToArray(),
                                   StringComparer.OrdinalIgnoreCase),
                           StringComparer.OrdinalIgnoreCase)
                   ?? new Dictionary<string, Dictionary<string, decimal?[][]>>();
        }

        public Dictionary<string, Dictionary<string, decimal?[][]>> BuildFm36Dictionary(FM36Global fm36Global)
        {
            var learningDeliveriesByPeriod = fm36Global?
                .Learners?
                .SelectMany(
                    l =>
                        l.LearningDeliveries?
                            .SelectMany(ld =>
                            {
                                // Get Fund Line By Period
                                var fundLinePeriodisedValues =
                                    ld.LearningDeliveryPeriodisedTextValues?.FirstOrDefault(t =>
                                        t.AttributeName.CaseInsensitiveEquals(AttributeConstants.Fm36FundLineType));

                                if (fundLinePeriodisedValues != null)
                                {
                                    // Flatten
                                    return ld.LearningDeliveryPeriodisedValues.SelectMany(pv => new[]
                                    {
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 0,
                                            FundLine = fundLinePeriodisedValues.Period1, pv.AttributeName,
                                            Value = pv.Period1
                                        },
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 1,
                                            FundLine = fundLinePeriodisedValues.Period2, pv.AttributeName,
                                            Value = pv.Period2
                                        },
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 2,
                                            FundLine = fundLinePeriodisedValues.Period3, pv.AttributeName,
                                            Value = pv.Period3
                                        },
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 3,
                                            FundLine = fundLinePeriodisedValues.Period4, pv.AttributeName,
                                            Value = pv.Period4
                                        },
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 4,
                                            FundLine = fundLinePeriodisedValues.Period5, pv.AttributeName,
                                            Value = pv.Period5
                                        },
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 5,
                                            FundLine = fundLinePeriodisedValues.Period6, pv.AttributeName,
                                            Value = pv.Period6
                                        },
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 6,
                                            FundLine = fundLinePeriodisedValues.Period7, pv.AttributeName,
                                            Value = pv.Period7
                                        },
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 7,
                                            FundLine = fundLinePeriodisedValues.Period8, pv.AttributeName,
                                            Value = pv.Period8
                                        },
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 8,
                                            FundLine = fundLinePeriodisedValues.Period9, pv.AttributeName,
                                            Value = pv.Period9
                                        },
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 9,
                                            FundLine = fundLinePeriodisedValues.Period10, pv.AttributeName,
                                            Value = pv.Period10
                                        },
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 10,
                                            FundLine = fundLinePeriodisedValues.Period11, pv.AttributeName,
                                            Value = pv.Period11
                                        },
                                        new
                                        {
                                            l.LearnRefNumber, ld.AimSeqNumber, PeriodIndex = 11,
                                            FundLine = fundLinePeriodisedValues.Period12, pv.AttributeName,
                                            Value = pv.Period12
                                        },
                                    });
                                }

                                return null;
                            }));

            return learningDeliveriesByPeriod?
                       .Where(p => p != null)
                       .GroupBy(p => p.FundLine, StringComparer.OrdinalIgnoreCase) // Fund Lines
                       .ToDictionary(k => k.Key,
                           v => v.GroupBy(a => a.AttributeName, StringComparer.OrdinalIgnoreCase) // Attributes
                               .ToDictionary(k => k.Key,
                                   flattenedValuesSet => flattenedValuesSet
                                       .GroupBy(fv => new {fv.LearnRefNumber, fv.AimSeqNumber}) // Learning Deliveries
                                       .Select(ld =>
                                       {
                                           var array = new decimal?[12];

                                           foreach (var periodisedValue in ld)
                                           {
                                               array[periodisedValue.PeriodIndex] = periodisedValue.Value; // Periods
                                           }

                                           return array;
                                       }).ToArray(),
                                   StringComparer.OrdinalIgnoreCase),
                           StringComparer.OrdinalIgnoreCase) ??
                   new Dictionary<string, Dictionary<string, decimal?[][]>>();
        }

        public Dictionary<string, Dictionary<string, decimal?[][]>> BuildFm81Dictionary(FM81Global fm81Global)
        {
            return fm81Global
                       .Learners?
                       .SelectMany(l => l.LearningDeliveries)
                       .GroupBy(ld => ld.LearningDeliveryValues.FundLine, StringComparer.OrdinalIgnoreCase)
                       .ToDictionary(k => k.Key,
                           v => v.SelectMany(ld => ld.LearningDeliveryPeriodisedValues)
                               .GroupBy(ldpv => ldpv.AttributeName, StringComparer.OrdinalIgnoreCase)
                               .ToDictionary(k => k.Key, value =>
                                       value.Select(pvGroup => new decimal?[]
                                       {
                                           pvGroup.Period1,
                                           pvGroup.Period2,
                                           pvGroup.Period3,
                                           pvGroup.Period4,
                                           pvGroup.Period5,
                                           pvGroup.Period6,
                                           pvGroup.Period7,
                                           pvGroup.Period8,
                                           pvGroup.Period9,
                                           pvGroup.Period10,
                                           pvGroup.Period11,
                                           pvGroup.Period12,
                                       }).ToArray(),
                                   StringComparer.OrdinalIgnoreCase),
                           StringComparer.OrdinalIgnoreCase)
                   ?? new Dictionary<string, Dictionary<string, decimal?[][]>>();
        }

        public Dictionary<string, Dictionary<string, decimal?[][]>> BuildFm99Dictionary(ALBGlobal fm99Global)
        {
            return fm99Global
                       .Learners?
                       .SelectMany(l => l.LearningDeliveries)
                       .GroupBy(ld => ld.LearningDeliveryValues.FundLine, StringComparer.OrdinalIgnoreCase)
                       .ToDictionary(k => k.Key,
                           v => v.SelectMany(ld => ld.LearningDeliveryPeriodisedValues)
                               .GroupBy(ldpv => ldpv.AttributeName, StringComparer.OrdinalIgnoreCase)
                               .ToDictionary(k => k.Key, value =>
                                       value.Select(pvGroup => new decimal?[]
                                       {
                                           pvGroup.Period1,
                                           pvGroup.Period2,
                                           pvGroup.Period3,
                                           pvGroup.Period4,
                                           pvGroup.Period5,
                                           pvGroup.Period6,
                                           pvGroup.Period7,
                                           pvGroup.Period8,
                                           pvGroup.Period9,
                                           pvGroup.Period10,
                                           pvGroup.Period11,
                                           pvGroup.Period12,
                                       }).ToArray(),
                                   StringComparer.OrdinalIgnoreCase),
                           StringComparer.OrdinalIgnoreCase)
                   ?? new Dictionary<string, Dictionary<string, decimal?[][]>>();
        }

        public Dictionary<string, Dictionary<string, decimal?[][]>> BuildEASDictionary(
            ReferenceDataRoot referenceDataRoot)
        {
            return referenceDataRoot?
                       .EasFundingLines?
                       .GroupBy(fl => fl.FundLine, StringComparer.OrdinalIgnoreCase)
                       .ToDictionary(k => k.Key,
                           v => v.SelectMany(ld => ld.EasSubmissionValues)
                               .GroupBy(easv => easv.AdjustmentTypeName, StringComparer.OrdinalIgnoreCase)
                               .ToDictionary(k => k.Key, value =>
                                       value.Select(pvGroup => new decimal?[]
                                       {
                                           pvGroup.Period1.PaymentValue,
                                           pvGroup.Period2.PaymentValue,
                                           pvGroup.Period3.PaymentValue,
                                           pvGroup.Period4.PaymentValue,
                                           pvGroup.Period5.PaymentValue,
                                           pvGroup.Period6.PaymentValue,
                                           pvGroup.Period7.PaymentValue,
                                           pvGroup.Period8.PaymentValue,
                                           pvGroup.Period9.PaymentValue,
                                           pvGroup.Period10.PaymentValue,
                                           pvGroup.Period11.PaymentValue,
                                           pvGroup.Period12.PaymentValue,
                                       }).ToArray(),
                                   StringComparer.OrdinalIgnoreCase),
                           StringComparer.OrdinalIgnoreCase)
                   ?? new Dictionary<string, Dictionary<string, decimal?[][]>>();
        }
    }
}