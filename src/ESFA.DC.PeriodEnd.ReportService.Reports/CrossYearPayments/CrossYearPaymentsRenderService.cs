using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.Cells;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.CrossYearPayments
{
    public class CrossYearPaymentsRenderService : ICrossYearRenderService
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        private const int R12ColumnNumber = 7;
        private const int R13ColumnNumber = 14;
        private const int R14ColumnNumber = 19;
        private const int R01ColumnNumber = 10;
        private const int R02ColumnNumber = 15;
        private const int R03ColumnNumber = 20;

        private const string R12 = "R12";
        private const string R13 = "R13";
        private const string R14 = "R14";
        private const string R01 = "R01";
        private const string R02 = "R02";
        private const string R03 = "R03";

        private const int ColumnNumber6 = 6;
        private const int ColumnNumber7 = 7;

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

        private readonly (int, int[]) _20211 = ( 2021, new[] { 1 } );
        private readonly (int, int[]) _202112 = ( 2021, new[] { 1, 2 } );
        private readonly (int, int[]) _2021123 = ( 2021, new[] { 1, 2, 3} );

        public CrossYearPaymentsRenderService(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public Worksheet Render(CrossYearPaymentsModel model, Worksheet worksheet, Workbook workbook)
        {
            var modelDictionary = model.Deliveries.ToDictionary(x => x.DeliveryName, x => x);

            Render1618NonLevyContractedApprenticeshipsProcuredDelivery(worksheet, modelDictionary.GetValueOrDefault(Interface.CrossYearPayments.Constants.NonLevy1618ContractedApprenticeshipsProcuredDelivery));

            RenderAdultNonLevyContractedApprenticeshipsProcuredDelivery(worksheet, modelDictionary.GetValueOrDefault(Interface.CrossYearPayments.Constants.AdultNonLevyContractedApprenticeshipsProcuredDelivery));

            RenderEmployersOnApprenticeshipServiceLevy(worksheet, modelDictionary.GetValueOrDefault(Interface.CrossYearPayments.Constants.EmployersOnApprenticeshipServiceLevy));

            RenderEmployersOnApprenticeshipServiceNonLevy(worksheet, modelDictionary.GetValueOrDefault(Interface.CrossYearPayments.Constants.EmployersOnApprenticeshipServiceNonLevy));

            return worksheet;
        }

        private Worksheet Render1618NonLevyContractedApprenticeshipsProcuredDelivery(Worksheet worksheet, Delivery delivery)
            => RenderProcuredDelivery(worksheet, delivery, 7, 15);

        private Worksheet RenderAdultNonLevyContractedApprenticeshipsProcuredDelivery(Worksheet worksheet, Delivery delivery)
            => RenderProcuredDelivery(worksheet, delivery, 18, 26);

        private Worksheet RenderProcuredDelivery(Worksheet worksheet, Delivery delivery, int startRow, int endRow)
        {
            RenderContractNumber(worksheet, startRow, endRow, delivery?.ContractNumber);

            var deliveryDictionary = delivery?.PeriodDeliveries.ToDictionary(x => x.ReturnPeriod, x => x);

            RenderR12Procured(worksheet, deliveryDictionary?.GetValueOrDefault(R12), startRow);
            RenderProcuredColumn(worksheet, deliveryDictionary?.GetValueOrDefault(R13), startRow, R13ColumnNumber);
            RenderProcuredColumn(worksheet, deliveryDictionary?.GetValueOrDefault(R14), startRow, R14ColumnNumber);
            RenderR01Procured(worksheet, deliveryDictionary?.GetValueOrDefault(R01), startRow);
            RenderR02Procured(worksheet, deliveryDictionary?.GetValueOrDefault(R02), startRow);
            RenderR03Procured(worksheet, deliveryDictionary?.GetValueOrDefault(R03), startRow);

            return worksheet;
        }

        private Worksheet RenderEmployersOnApprenticeshipServiceLevy(Worksheet worksheet, Delivery delivery)
            => RenderEmployers(worksheet, delivery, 29, 33);

        private Worksheet RenderEmployersOnApprenticeshipServiceNonLevy(Worksheet worksheet, Delivery delivery)
            => RenderEmployers(worksheet, delivery, 36, 40);

        private Worksheet RenderEmployers(Worksheet worksheet, Delivery delivery, int startRow, int endRow)
        {
            RenderContractNumber(worksheet, startRow, endRow, delivery?.ContractNumber);

            var deliveryDictionary = delivery?.PeriodDeliveries.ToDictionary(x => x.ReturnPeriod, x => x);

            RenderEmployersColumn(worksheet, deliveryDictionary?.GetValueOrDefault(R12).FSRValues, startRow, R12ColumnNumber);
            RenderEmployersColumn(worksheet, deliveryDictionary?.GetValueOrDefault(R13).FSRValues, startRow, R13ColumnNumber);
            RenderEmployersColumn(worksheet, deliveryDictionary?.GetValueOrDefault(R14).FSRValues, startRow, R14ColumnNumber);
            RenderEmployersColumn(worksheet, deliveryDictionary?.GetValueOrDefault(R01).FSRValues, startRow, R01ColumnNumber + 1, _20211);
            RenderEmployersColumn(worksheet, deliveryDictionary?.GetValueOrDefault(R02).FSRValues, startRow, R02ColumnNumber + 1, _202112);
            RenderEmployersColumn(worksheet, deliveryDictionary?.GetValueOrDefault(R03).FSRValues, startRow, R03ColumnNumber + 1, _2021123);

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

        private Worksheet RenderR12Procured(Worksheet worksheet, PeriodDelivery periodDelivery, int row)
        {
            RenderContractValuesColumn(worksheet, periodDelivery?.ContractValues, row, ColumnNumber6);
            RenderFSRValues(worksheet, periodDelivery?.FSRValues, _fsrProcuredBasePeriods, row, ColumnNumber7);

            return worksheet;
        }

        private Worksheet RenderProcuredColumn(Worksheet worksheet, PeriodDelivery periodDelivery, int row, int columnNumber)
        {
            return RenderFSRValues(worksheet, periodDelivery?.FSRValues, _fsrProcuredBasePeriods, row, columnNumber);
        }

        private Worksheet RenderR01Procured(Worksheet worksheet, PeriodDelivery periodDelivery, int row)
        {
            var periods = _fsrProcuredBasePeriods;
            periods.Add(_20211);

            var columnNum = R01ColumnNumber;
            RenderContractValuesColumn(worksheet, periodDelivery?.ContractValues, row, columnNum++);
            RenderFSRValues(worksheet, periodDelivery?.FSRValues, periods, row, columnNum);
            return worksheet;
        }

        private Worksheet RenderR02Procured(Worksheet worksheet, PeriodDelivery periodDelivery, int row)
        {
            var periods = _fsrProcuredBasePeriods;
            periods.Add(_202112);

            var columnNum = R02ColumnNumber;
            RenderContractValuesColumn(worksheet, periodDelivery?.ContractValues, row, columnNum++);
            RenderFSRValues(worksheet, periodDelivery?.FSRValues, periods, row, columnNum);
            return worksheet;
        }

        private Worksheet RenderR03Procured(Worksheet worksheet, PeriodDelivery periodDelivery, int row)
        {
            var periods = _fsrProcuredBasePeriods;
            periods.Add(_2021123);

            var columnNum = R03ColumnNumber;
            RenderContractValuesColumn(worksheet, periodDelivery?.ContractValues, row, columnNum++);
            RenderFSRValues(worksheet, periodDelivery?.FSRValues, periods, row, columnNum);
            return worksheet;
        }

        private Worksheet RenderEmployersColumn(Worksheet worksheet, ICollection<FSRValue> fsrValues, int startRowNum, int columnNumber, (int, int[])additionalPeriods)
        {
            var periods = _fsrEmployerBasePeriods;
            periods.Add(additionalPeriods);

            return RenderFSRValues(worksheet, fsrValues, periods, startRowNum, columnNumber);
        }

        private Worksheet RenderEmployersColumn(Worksheet worksheet, ICollection<FSRValue> fsrValues, int startRowNum, int columnNumber)
        {
            return RenderFSRValues(worksheet, fsrValues, _fsrEmployerBasePeriods, startRowNum, columnNumber);
        }

        //        private Worksheet RenderContractAndFsrValues(Worksheet worksheet, PeriodDelivery periodDelivery, int startColumn, int startRow)
        //        {
        //            var summaryRow = startRow + 7;
        //
        //            RenderContractValuesColumn(worksheet, periodDelivery?.ContractValues, startRow, startColumn);
        //
        //            RenderFSRValuesColumn(worksheet, periodDelivery?.FSRValues, startRow, startColumn + 1);
        //
        //            //worksheet.Cells[summaryRow, startColumn + 1].Value = periodDelivery?.FSRSubtotal ?? 0m;
        //            //worksheet.Cells[summaryRow, startColumn + 2].Value = periodDelivery?.FSRReconciliationSubtotal ?? 0m;
        //            //worksheet.Cells[startRow + 7, startColumn + 3].Value = periodDelivery?.CappingSubtotal ?? 0m;
        //
        //            return worksheet;
        //        }

        private Worksheet RenderContractValuesColumn(Worksheet worksheet, ICollection<ContractValue> contractValues, int startRowNum, int columnNum)
        {
            RenderValueCell(worksheet, contractValues, new int[] { 201801, 201802, 201803 }, startRowNum, columnNum);
            RenderValueCell(worksheet, contractValues, new int[] { 201804, 201805, 201806, 201807, 201808, 201809, 201810, 201811, 201812, 201901, 201902, 201903 }, startRowNum + 1, columnNum);
            RenderValueCell(worksheet, contractValues, new int[] { 201904, 201905, 201906, 201907, 201908, 201909, 201910, 201911, 201912, 202001, 202002, 202003 }, startRowNum + 3, columnNum);
            RenderValueCell(worksheet, contractValues, new int[] { 202004, 202005, 202006, 202007, 202008, 202009, 202010, 202011, 202012, 202101, 202102, 202103 }, startRowNum + 5, columnNum);

            return worksheet;
        }

        private Worksheet RenderFSRValues(Worksheet worksheet, ICollection<FSRValue> fsrValues, ICollection<(int collectionYear, int[] deliveryPeriods)> configurations, int startRowNum, int columnNum)
        {
            foreach (var item in configurations)
            {
                RenderValueCell(worksheet, fsrValues, item.deliveryPeriods, item.collectionYear, startRowNum++, columnNum);
            }

            return worksheet;
        }

        private Worksheet RenderValueCell(Worksheet worksheet, ICollection<FSRValue> values, int[] periods, int academicYear, int rowNum, int columnNum)
        {
            worksheet.Cells[rowNum, columnNum].Value = values?.Where(x => x.AcademicYear == academicYear && periods.Contains(x.Period)).Sum(x => x.Value) ?? 0m;

            return worksheet;
        }

        private Worksheet RenderValueCell(Worksheet worksheet, ICollection<ContractValue> values, int[] periods, int rowNum, int columnNum)
        {
            worksheet.Cells[rowNum, columnNum].Value = values?.Where(x => periods.Contains(x.Period)).Sum(x => x.Value) ?? 0m;

            return worksheet;
        }
    }
}
