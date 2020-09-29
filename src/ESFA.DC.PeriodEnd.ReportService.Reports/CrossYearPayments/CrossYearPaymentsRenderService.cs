using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.Cells;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.CrossYearPayments
{
    public class CrossYearPaymentsRenderService : ICrossYearRenderService
    {
        private const int R12ColumnNumber = 7;
        private const int R13ColumnNumber = 14;
        private const int R14ColumnNumber = 19;
        private const int R01ColumnNumber = 10;
        private const int R02ColumnNumber = 15;
        private const int R03ColumnNumber = 20;
        private const int R14PreviousYearColumnNumber = 4;
        private const int PaymentsUpToR11 = 5;

        private const int R12 = 12;
        private const int R13 = 13;
        private const int R14 = 14;
        private const int R01 = 01;
        private const int R02 = 02;
        private const int R03 = 03;

        private const int ColumnNumber6 = 6;
        private const int ColumnNumber7 = 7;

        private static readonly DateTime R12ContractApprovalDateTime = new DateTime(2020, 08, 07);
        private static readonly DateTime R01ContractApprovalDateTime = new DateTime(2020, 09, 07);
        private static readonly DateTime R02ContractApprovalDateTime = new DateTime(2020, 10, 07);
        private static readonly DateTime R03ContractApprovalDateTime = new DateTime(2020, 11, 06);

        private readonly ICollection<(int, int[])> _fsrR14PreviousYearPeriods = new List<(int, int[])>
        {
            (1718, new[] {6, 7, 8}),
            (1718, new[] {9, 10, 11, 12}),
            (1819, new[] {1, 2, 3, 4, 5, 6, 7, 8}),
            (1819, new[] {9, 10, 11, 12}),
        };

        private readonly ICollection<(int, int[])> _fsrProcuredBasePeriods = new List<(int, int[])>
        {
            (1718, new[] {6, 7, 8}),
            (1718, new[] {9, 10, 11, 12}),
            (1819, new[] {1, 2, 3, 4, 5, 6, 7, 8}),
            (1819, new[] {9, 10, 11, 12}),
            (1920, new[] {1, 2, 3, 4, 5, 6, 7, 8}),
            (1920, new[] {9, 10, 11, 12})
        };

        private readonly ICollection<(int, int[])> _fsrEmployerBasePeriods = new List<(int, int[])>
        {
            (1920, new [] { 1, 2, 3, 4, 5, 6, 7, 8 }),
            (1920, new [] { 9, 10, 11, 12 }),
        };

        private readonly IDictionary<int, IDictionary<int,int>> _collectionPeriodConfiguration = new Dictionary<int, IDictionary<int, int>>
        {
            { 12, new Dictionary<int, int> { {1718, 14}, {1819, 14}, {1920, 12}} },
            { 13, new Dictionary<int, int> { {1718, 14}, {1819, 14}, {1920, 13}} },
            { 14, new Dictionary<int, int> { {1718, 14}, {1819, 14}, {1920, 14}} },
            { 1, new Dictionary<int, int> { {1718, 14}, {1819, 14}, {1920, 12}, {2021, 1}} },
            { 2, new Dictionary<int, int> { {1718, 14}, {1819, 14}, {1920, 13}, {2021, 2}} },
            { 3, new Dictionary<int, int> { {1718, 14}, {1819, 14}, {1920, 14}, {2021, 3}} },
        };

        private readonly int[] _procuredPaymentsPeriod = new[]
        {
            201801, 201802, 201803, 201804, 201805, 201806, 201807, 201808, 201809, 201810, 201811, 201812, 201901,
            201902, 201903, 201904, 201905, 201906, 201907, 201908, 201909, 201910, 201911, 201912, 202001, 202002,
            202003, 202004, 202005, 202006
        };

        private readonly int[] _employersPaymentsPeriod = new[]
        {
            201908, 201909, 201910, 201911, 201912, 202001, 202002,
            202003, 202004, 202005, 202006
        };

        private readonly (int, int[]) _20211 = ( 2021, new[] { 1 } );
        private readonly (int, int[]) _202112 = ( 2021, new[] { 1, 2 } );
        private readonly (int, int[]) _2021123 = ( 2021, new[] { 1, 2, 3} );

        private readonly IDictionary<int, ICollection<int>> _displayPeriodConfiguration = new Dictionary<int, ICollection<int>>
        {
            { 1, new List<int> { R01 } },
            { 2, new List<int> { R01, R13, R02 } },
            { 3, new List<int> { R01, R13, R02, R14, R03 } },
        };

        public Worksheet Render(IReportServiceContext reportServiceContext, CrossYearPaymentsModel model, Worksheet worksheet, Workbook workbook)
        {
            RenderHeader(worksheet, model.HeaderInfo);

            var modelDictionary = model.Deliveries?.ToDictionary(x => x.DeliveryName, x => x) ?? new Dictionary<string, Delivery>();

            Render1618NonLevyContractedApprenticeshipsProcuredDelivery(worksheet, modelDictionary.GetValueOrDefault(Interface.CrossYearPayments.Constants.NonLevy1618ContractedApprenticeshipsProcuredDelivery), reportServiceContext.ReturnPeriod);

            RenderAdultNonLevyContractedApprenticeshipsProcuredDelivery(worksheet, modelDictionary.GetValueOrDefault(Interface.CrossYearPayments.Constants.AdultNonLevyContractedApprenticeshipsProcuredDelivery), reportServiceContext.ReturnPeriod);

            RenderEmployersOnApprenticeshipServiceLevy(worksheet, modelDictionary.GetValueOrDefault(Interface.CrossYearPayments.Constants.EmployersOnApprenticeshipServiceLevy), reportServiceContext.ReturnPeriod);

            RenderEmployersOnApprenticeshipServiceNonLevy(worksheet, modelDictionary.GetValueOrDefault(Interface.CrossYearPayments.Constants.EmployersOnApprenticeshipServiceNonLevy), reportServiceContext.ReturnPeriod);

            RenderFooter(worksheet, model.FooterInfo);

            workbook.CalculateFormula();
            return worksheet;
        }

        private Worksheet RenderHeader(Worksheet worksheet, HeaderInfo headerInfo)
        {
            worksheet.Cells[0, 1].Value = headerInfo.ProviderName;
            worksheet.Cells[1, 1].Value = headerInfo.UKPRN;

            return worksheet;
        }

        private Worksheet RenderFooter(Worksheet worksheet, FooterInfo footerInfo)
        {
            worksheet.Cells[42, 0].Value = footerInfo.ReportGeneratedAt;
            return worksheet;
        }

        private ICollection<T> FilterForDisplayPeriod<T>(ICollection<T> values, int returnPeriod, int returnPeriodToDisplay)
        {
            var periodConfiguration = _displayPeriodConfiguration.GetValueOrDefault(returnPeriod);

            if (periodConfiguration?.Contains(returnPeriodToDisplay) ?? false)
            {
                return values;
            }

            return Array.Empty<T>();
        }

        private Worksheet Render1618NonLevyContractedApprenticeshipsProcuredDelivery(Worksheet worksheet, Delivery delivery, int returnPeriod)
            => RenderProcuredDelivery(worksheet, delivery, returnPeriod, 7, 15);

        private Worksheet RenderAdultNonLevyContractedApprenticeshipsProcuredDelivery(Worksheet worksheet, Delivery delivery, int returnPeriod)
            => RenderProcuredDelivery(worksheet, delivery, returnPeriod, 18, 26);

        private Worksheet RenderProcuredDelivery(Worksheet worksheet, Delivery delivery, int returnPeriod, int startRow, int endRow)
        {
            var subTotalRow = startRow + 7;

            RenderContractNumber(worksheet, startRow, endRow, delivery?.ContractNumber);

            RenderProcuredColumn(worksheet, delivery?.FSRValues, _collectionPeriodConfiguration.GetValueOrDefault(R14), startRow, R14PreviousYearColumnNumber, _fsrR14PreviousYearPeriods);

            RenderPaymentsUpToR11(worksheet, delivery?.FcsPayments, _procuredPaymentsPeriod, subTotalRow, PaymentsUpToR11);

            RenderR12Procured(worksheet, delivery?.FSRValues, delivery?.ContractValues, _collectionPeriodConfiguration.GetValueOrDefault(R12), R12ContractApprovalDateTime, startRow);

            RenderProcuredColumn(worksheet, FilterForDisplayPeriod(delivery?.FSRValues, returnPeriod, R14), _collectionPeriodConfiguration.GetValueOrDefault(R14), startRow, R14ColumnNumber);

            RenderProcuredColumn(worksheet, FilterForDisplayPeriod(delivery?.FSRValues, returnPeriod, R01), FilterForDisplayPeriod(delivery?.ContractValues, returnPeriod, R01), _collectionPeriodConfiguration.GetValueOrDefault(R01), R01ContractApprovalDateTime, startRow, R01ColumnNumber, _20211);

            RenderProcuredColumn(worksheet, FilterForDisplayPeriod(delivery?.FSRValues, returnPeriod, R13), _collectionPeriodConfiguration.GetValueOrDefault(R13), startRow, R13ColumnNumber);

            RenderProcuredColumn(worksheet, FilterForDisplayPeriod(delivery?.FSRValues, returnPeriod, R02), FilterForDisplayPeriod(delivery?.ContractValues, returnPeriod, R02), _collectionPeriodConfiguration.GetValueOrDefault(R02), R02ContractApprovalDateTime, startRow, R02ColumnNumber, _202112);

            RenderProcuredColumn(worksheet, FilterForDisplayPeriod(delivery?.FSRValues, returnPeriod, R03), FilterForDisplayPeriod(delivery?.ContractValues, returnPeriod, R03), _collectionPeriodConfiguration.GetValueOrDefault(R03), R03ContractApprovalDateTime, startRow, R03ColumnNumber, _2021123);

            ClearReconciliationColumn(worksheet, returnPeriod, R02, subTotalRow, R02ColumnNumber + 2);
            ClearReconciliationColumn(worksheet, returnPeriod, R03, subTotalRow, R03ColumnNumber + 2);

            return worksheet;
        }

        private Worksheet RenderEmployersOnApprenticeshipServiceLevy(Worksheet worksheet, Delivery delivery, int returnPeriod)
            => RenderEmployers(worksheet, delivery, returnPeriod, 29, 33);

        private Worksheet RenderEmployersOnApprenticeshipServiceNonLevy(Worksheet worksheet, Delivery delivery, int returnPeriod)
            => RenderEmployers(worksheet, delivery, returnPeriod, 36, 40);

        private Worksheet RenderEmployers(Worksheet worksheet, Delivery delivery, int returnPeriod, int startRow, int endRow)
        {
            var subTotalRow = startRow + 3;

            RenderContractNumber(worksheet, startRow, endRow, delivery?.ContractNumber);

            RenderPaymentsUpToR11(worksheet, delivery?.FcsPayments, _employersPaymentsPeriod, subTotalRow, PaymentsUpToR11);

            RenderEmployersColumn(worksheet, delivery?.FSRValues, _collectionPeriodConfiguration.GetValueOrDefault(R12), startRow, R12ColumnNumber);
            RenderEmployersColumn(worksheet, FilterForDisplayPeriod(delivery?.FSRValues, returnPeriod, R13), _collectionPeriodConfiguration.GetValueOrDefault(R13), startRow, R13ColumnNumber);
            RenderEmployersColumn(worksheet, FilterForDisplayPeriod(delivery?.FSRValues, returnPeriod, R14), _collectionPeriodConfiguration.GetValueOrDefault(R14), startRow, R14ColumnNumber);
            RenderEmployersColumn(worksheet, FilterForDisplayPeriod(delivery?.FSRValues, returnPeriod, R01), _collectionPeriodConfiguration.GetValueOrDefault(R01), startRow, R01ColumnNumber + 1, _20211);
            RenderEmployersColumn(worksheet, FilterForDisplayPeriod(delivery?.FSRValues, returnPeriod, R02), _collectionPeriodConfiguration.GetValueOrDefault(R02), startRow, R02ColumnNumber + 1, _202112);
            RenderEmployersColumn(worksheet, FilterForDisplayPeriod(delivery?.FSRValues, returnPeriod, R03), _collectionPeriodConfiguration.GetValueOrDefault(R03), startRow, R03ColumnNumber + 1, _2021123);

            ClearReconciliationColumn(worksheet, returnPeriod, R02, subTotalRow, R02ColumnNumber + 2);
            ClearReconciliationColumn(worksheet, returnPeriod, R03, subTotalRow, R03ColumnNumber + 2);

            return worksheet;
        }

        private Worksheet RenderContractNumber(Worksheet worksheet, int start, int end, string contractNumber)
        {
            for(var i = start; i <= end; i++)
            {
                worksheet.Cells[i, 0].Value = contractNumber;
            }

            return worksheet;
        }

        private Worksheet RenderPaymentsUpToR11(Worksheet worksheet, ICollection<FcsPayment> values, int[] periods, int rowNum, int columnNum)
        {
            worksheet.Cells[rowNum, columnNum].PutValue(values?.Where(x => periods.Contains(x.Period)).Sum(x => x.Value) ?? 0m);

            return worksheet;
        }

        private Worksheet ClearReconciliationColumn(Worksheet worksheet, int returnPeriod, int returnPeriodToDisplay,  int rowNum, int columnNum)
        {
            var periodConfiguration = _displayPeriodConfiguration.GetValueOrDefault(returnPeriod);

            if (!periodConfiguration?.Contains(returnPeriodToDisplay) ?? true)
            {
                worksheet.Cells[rowNum, columnNum].Value = 0m;
            }

            return worksheet;
        }

        private Worksheet RenderR12Procured(Worksheet worksheet, ICollection<FSRValue> fsrValues, ICollection<ContractValue> contractValues, IDictionary<int, int> collectionPeriodConfiguration, DateTime contractApprovalDateTime, int row)
        {
            RenderContractValuesColumn(worksheet, contractValues, contractApprovalDateTime, row, ColumnNumber6);
            RenderFSRValues(worksheet, fsrValues, _fsrProcuredBasePeriods, collectionPeriodConfiguration, row, ColumnNumber7);

            return worksheet;
        }

        private Worksheet RenderProcuredColumn(Worksheet worksheet, ICollection<FSRValue> values, IDictionary<int, int> collectionPeriodConfiguration, int row, int columnNumber)
        {
            return RenderFSRValues(worksheet, values, _fsrProcuredBasePeriods, collectionPeriodConfiguration, row, columnNumber);
        }

        private Worksheet RenderProcuredColumn(Worksheet worksheet, ICollection<FSRValue> values, IDictionary<int, int> collectionPeriodConfiguration, int row, int columnNumber, ICollection<(int, int[])> periods)
        {
            return RenderFSRValues(worksheet, values, periods, collectionPeriodConfiguration, row, columnNumber);
        }

        private Worksheet RenderProcuredColumn(Worksheet worksheet, ICollection<FSRValue> fsrValues, ICollection<ContractValue> contractValues, IDictionary<int, int> collectionPeriodConfiguration, DateTime contractApprovalDateTime, int row, int columnNumber, (int, int[]) additionalPeriods)
        {
            var periods = new List<(int collectionYear, int[] deliveryPeriods)>(_fsrProcuredBasePeriods) {additionalPeriods};

            var columnNum = columnNumber;
            RenderContractValuesColumn(worksheet, contractValues, contractApprovalDateTime, row, columnNum++);
            RenderFSRValues(worksheet, fsrValues, periods, collectionPeriodConfiguration, row, columnNum);
            return worksheet;
        }

        private Worksheet RenderEmployersColumn(Worksheet worksheet, ICollection<FSRValue> fsrValues, IDictionary<int, int> collectionPeriodConfiguration, int startRowNum, int columnNumber, (int, int[])additionalPeriods)
        {
            var periods = new List<(int collectionYear, int[] deliveryPeriods)>(_fsrEmployerBasePeriods) {additionalPeriods};

            return RenderFSRValues(worksheet, fsrValues, periods, collectionPeriodConfiguration, startRowNum, columnNumber);
        }

        private Worksheet RenderEmployersColumn(Worksheet worksheet, ICollection<FSRValue> fsrValues, IDictionary<int, int> collectionPeriodConfiguration, int startRowNum, int columnNumber)
        {
            return RenderFSRValues(worksheet, fsrValues, _fsrEmployerBasePeriods, collectionPeriodConfiguration, startRowNum, columnNumber);
        }

        private Worksheet RenderContractValuesColumn(Worksheet worksheet, ICollection<ContractValue> contractValues, DateTime contractApprovalDateTime, int startRowNum, int columnNum)
        {
            RenderValueCell(worksheet, contractValues, contractApprovalDateTime, new int[] { 201801, 201802, 201803 }, startRowNum, columnNum);
            RenderValueCell(worksheet, contractValues, contractApprovalDateTime, new int[] { 201804, 201805, 201806, 201807, 201808, 201809, 201810, 201811, 201812, 201901, 201902, 201903 }, startRowNum + 1, columnNum);
            RenderValueCell(worksheet, contractValues, contractApprovalDateTime, new int[] { 201904, 201905, 201906, 201907, 201908, 201909, 201910, 201911, 201912, 202001, 202002, 202003 }, startRowNum + 3, columnNum);
            RenderValueCell(worksheet, contractValues, contractApprovalDateTime, new int[] { 202004, 202005, 202006, 202007, 202008, 202009, 202010, 202011, 202012, 202101, 202102, 202103 }, startRowNum + 5, columnNum);

            return worksheet;
        }

        private Worksheet RenderFSRValues(Worksheet worksheet, ICollection<FSRValue> fsrValues, ICollection<(int collectionYear, int[] deliveryPeriods)> configurations, IDictionary<int, int> collectionPeriodConfiguration, int startRowNum, int columnNum)
        {
            foreach (var item in configurations)
            {
                RenderValueCell(worksheet, fsrValues, item.deliveryPeriods, item.collectionYear, collectionPeriodConfiguration.GetValueOrDefault(item.collectionYear), startRowNum++, columnNum);
            }

            return worksheet;
        }

        private Worksheet RenderValueCell(Worksheet worksheet, ICollection<FSRValue> values, int[] periods, int academicYear, int collectionPeriod, int rowNum, int columnNum)
        {
            worksheet.Cells[rowNum, columnNum].PutValue(values?.Where(x => x.AcademicYear == academicYear && x.CollectionPeriod == collectionPeriod && periods.Contains(x.DeliveryPeriod)).Sum(x => x.Value) ?? 0m);

            return worksheet;
        }

        private Worksheet RenderValueCell(Worksheet worksheet, ICollection<ContractValue> values, DateTime contractApprovalDateTime, int[] periods, int rowNum, int columnNum)
        {
            var value = values?.Where(x => periods.Contains(x.DeliveryPeriod) && x.ApprovalTimestamp <= contractApprovalDateTime).GroupBy(x => x.ApprovalTimestamp).OrderByDescending(x => x.Key).FirstOrDefault()?.Sum(x => x.Value) ?? 0m;

            worksheet.Cells[rowNum, columnNum].PutValue(value);

            return worksheet;
        }
    }
}
