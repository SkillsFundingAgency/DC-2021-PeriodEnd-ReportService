using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.CrossYearPayments
{
    public class CrossYearPaymentsModelBuilder : ICrossYearModelBuilder
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        private IDictionary<string, ICollection<string>> ReportSectionGroupDictionary => new Dictionary<string, ICollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { Interface.CrossYearPayments.Constants.NonLevy1618ContractedApprenticeshipsProcuredDelivery, new List<string> { Interface.CrossYearPayments.Constants.Apprenticeship1618NonLevyProcuredFundLine } },
            { Interface.CrossYearPayments.Constants.AdultNonLevyContractedApprenticeshipsProcuredDelivery, new List<string> { Interface.CrossYearPayments.Constants.Apprenticeship19PlusNonLevyContractFundLine } },
            { Interface.CrossYearPayments.Constants.EmployersOnApprenticeshipServiceLevy, new List<string> { Interface.CrossYearPayments.Constants.Apprenticeship1618EmployerOnAppServiceLevyFundLine, Interface.CrossYearPayments.Constants.Apprenticeship19PlusEmployerOnAppServiceLevyFundLine } },
            { Interface.CrossYearPayments.Constants.EmployersOnApprenticeshipServiceNonLevy, new List<string> { Interface.CrossYearPayments.Constants.Apprenticeship1618EmployerOnAppServiceNonLevyFundLine, Interface.CrossYearPayments.Constants.Apprenticeship19PlusEmployerOnAppServiceNonLevyFundLine } }
        };
        private IDictionary<string, string> FspLookup => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { Interface.CrossYearPayments.Constants.NonLevy1618ContractedApprenticeshipsProcuredDelivery, Interface.CrossYearPayments.Constants.NLAP16182018 },
            { Interface.CrossYearPayments.Constants.AdultNonLevyContractedApprenticeshipsProcuredDelivery, Interface.CrossYearPayments.Constants.ANLAP2018 },
            { Interface.CrossYearPayments.Constants.EmployersOnApprenticeshipServiceLevy, Interface.CrossYearPayments.Constants.LEVY1799 },
            { Interface.CrossYearPayments.Constants.EmployersOnApprenticeshipServiceNonLevy, Interface.CrossYearPayments.Constants.NONLEVY2019 }
        };

        private IDictionary<string, ICollection<int>> ReportSectionPaymentTypeLookup => new Dictionary<string, ICollection<int>>(StringComparer.OrdinalIgnoreCase)
        {
            { Interface.CrossYearPayments.Constants.NonLevy1618ContractedApprenticeshipsProcuredDelivery, new List<int> { 100, 105, 106, 107, 130, 162 } },
            { Interface.CrossYearPayments.Constants.AdultNonLevyContractedApprenticeshipsProcuredDelivery, new List<int> { 108, 113, 114, 115, 132, 163 } },
            { Interface.CrossYearPayments.Constants.EmployersOnApprenticeshipServiceLevy, new List<int> { 69, 70, 71, 75, 76, 77, 81, 83, 126, 128, 158, 160 } },
            { Interface.CrossYearPayments.Constants.EmployersOnApprenticeshipServiceNonLevy, new List<int> { 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 164, 165} }
        };

        public CrossYearPaymentsModelBuilder(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public CrossYearPaymentsModel Build(CrossYearDataModel dataModel, IReportServiceContext reportServiceContext)
        {
            var headerInfo = BuildHeader(dataModel, reportServiceContext);
            var footerInfo = BuildFooter();
            var deliveries = BuildDeliveries(dataModel);

            return new CrossYearPaymentsModel
            {
                HeaderInfo = headerInfo,
                FooterInfo = footerInfo,
                Deliveries = deliveries
            };
        }

        private ICollection<Delivery> BuildDeliveries(CrossYearDataModel dataModel)
        {
            var deliveries = new List<Delivery>();

            var contractDictionary = dataModel.FcsContracts
                .GroupBy(x => x.FundingStreamPeriodCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Select(y => y.ContractAllocationNumber).ToList(), StringComparer.OrdinalIgnoreCase);

            foreach (var section in ReportSectionGroupDictionary)
            {
                var fsp = FspLookup.GetValueOrDefault(section.Key);
                var paymentTypes = ReportSectionPaymentTypeLookup.GetValueOrDefault(section.Key);

                var contracts = contractDictionary?.GetValueOrDefault(fsp);
                var contractNumbers = string.Join(";", contractDictionary?.GetValueOrDefault(fsp) ?? Enumerable.Empty<string>());

                var payments = dataModel.Payments.Where(p => section.Value.Contains(p.FundingLineType));
                var adjustmentPayments = dataModel.AdjustmentPayments.Where(p => paymentTypes.Contains(p.PaymentType));

                var values = adjustmentPayments.Select(p =>
                {
                    int.TryParse(p.CollectionPeriodName.Substring(p.CollectionPeriodName.Length - 2), out var period);
                    return new FSRValue
                    {
                        AcademicYear = p.AcademicYear,
                        DeliveryPeriod = p.CollectionPeriod,
                        CollectionPeriod = period,
                        Value = p.Amount
                    };
                });

                values = values.Concat(payments.Select(p => new FSRValue
                {
                    AcademicYear = p.AcademicYear,
                    DeliveryPeriod = p.DeliveryPeriod,
                    CollectionPeriod = p.CollectionPeriod,
                    Value = p.Amount
                }));

                var contractValues = dataModel.FcsAllocations
                    .Where(x => fsp.Equals(x.FspCode, StringComparison.OrdinalIgnoreCase))
                    .Select(x => new ContractValue
                    {
                        DeliveryPeriod = x.Period,
                        Value = x.PlannedValue,
                        ApprovalTimestamp = x.ApprovalTimestamp
                    });

                deliveries.Add(new Delivery
                {
                    DeliveryName = section.Key,
                    ContractNumber = contractNumbers,
                    FSRValues = values.ToList(),
                    ContractValues = contractValues.ToList(),
                    FcsPayments = dataModel.FcsPayments.Where(x => x.FspCode.Equals(fsp, StringComparison.OrdinalIgnoreCase) && contracts.Contains(x.ContractAllocationNumber)).ToList()
                });
            }

            return deliveries;
        }

        private HeaderInfo BuildHeader(CrossYearDataModel dataModel, IReportServiceContext reportServiceContext)
        {
            return new HeaderInfo
            {
                UKPRN = reportServiceContext.Ukprn,
                ProviderName = dataModel.OrgName
            };
        }

        private FooterInfo BuildFooter()
        {
            var dateTimeNowUtc = _dateTimeProvider.GetNowUtc();
            var dateTimeNowUk = _dateTimeProvider.ConvertUtcToUk(dateTimeNowUtc);

            return new FooterInfo
            {
                ReportGeneratedAt = $"Report generated at {dateTimeNowUk.TimeOfDayOnDateStringFormat()}"
            };
        }
    }
}
