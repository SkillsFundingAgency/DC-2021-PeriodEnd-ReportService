using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class ProviderEasnInfo
    {
        public int? Ukprn { get; set; }

        //IList<EasFundingLine>
        //return referenceDataRoot?
        //           .EasFundingLines?
        //           .GroupBy(fl => fl.FundLine, StringComparer.OrdinalIgnoreCase)
        //           .ToDictionary(k => k.Key,
        //               v => v.SelectMany(ld => ld.EasSubmissionValues)
        //                   .GroupBy(easv => easv.AdjustmentTypeName, StringComparer.OrdinalIgnoreCase)
        //                   .ToDictionary(k => k.Key, value =>
        //                           value.Select(pvGroup => new decimal?[]
        //                           {
        //                               pvGroup.Period1.PaymentValue,
        //                               pvGroup.Period2.PaymentValue,
        //                               pvGroup.Period3.PaymentValue,
        //                               pvGroup.Period4.PaymentValue,
        //                               pvGroup.Period5.PaymentValue,
        //                               pvGroup.Period6.PaymentValue,
        //                               pvGroup.Period7.PaymentValue,
        //                               pvGroup.Period8.PaymentValue,
        //                               pvGroup.Period9.PaymentValue,
        //                               pvGroup.Period10.PaymentValue,
        //                               pvGroup.Period11.PaymentValue,
        //                               pvGroup.Period12.PaymentValue,
        //                           }).ToArray(),
        //                       StringComparer.OrdinalIgnoreCase),
        //               StringComparer.OrdinalIgnoreCase)
        //       ?? new Dictionary<string, Dictionary<string, decimal?[][]>>();
    }


    public class EasSubmissionInfo
    {
        public Guid? SubmissionId { get; set; }

        public string UKPRN { get; set; }

        public byte? CollectionPeriod { get; set; }
    }

    public class EasSubmissionValueInfo
    {
        public Guid? SubmissionId { get; set; }

        public byte? CollectionPeriod { get; set; }

        public int? PaymentId { get; set; }

        public decimal? PaymentValue { get; set; }
    }

    public class EasPaymentTypeInfo
    {
        public int? PaymentId { get; set; }

        public string PaymentName { get; set; }

        public bool Fm36 { get; set; }

        public string PaymentTypeDescription { get; set; }

        public int? FundingLineId { get; set; }

        public int? AdjustmentTypeId { get; set; }
    }

    public class EasAdjustmentTypeInfo
    {
        public int? Id { get; set; }

        public string Name { get; set; }
    }

    public class EasFundingLineInfo
    {
        public int? Id { get; set; }

        public string Name { get; set; }
    }
}
