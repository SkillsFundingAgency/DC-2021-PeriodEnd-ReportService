using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aspose.Cells;
using CsvHelper;
using CsvHelper.Configuration;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.Generation;
using ESFA.DC.PeriodEnd.ReportService.Model.Styling;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports
{
    public abstract class AbstractInternalReport
    {
        private readonly IValueProvider _valueProvider;
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly Dictionary<Worksheet, int> _currentRow = new Dictionary<Worksheet, int>();

        protected AbstractInternalReport(
            IValueProvider valueProvider,
            IDateTimeProvider dateTimeProvider)
        {
            _valueProvider = valueProvider;
            _dateTimeProvider = dateTimeProvider;
        }

        public abstract string ReportFileName { get; set; }

        public string GetFilename(IReportServiceContext reportServiceContext)
        {
            DateTime dateTime = _dateTimeProvider.ConvertUtcToUk(reportServiceContext.SubmissionDateTimeUtc);
            return $"{reportServiceContext.ReturnPeriod.ToString().PadLeft(2, '0')}_{ReportFileName} {dateTime:yyyyMMdd-HHmmss}";
        }

        protected Workbook GetWorkbookFromTemplate(string templateFileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(templateFileName));

            using (Stream manifestResourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                return new Workbook(manifestResourceStream);
            }
        }

        protected void WriteExcelRecords<TMapper, TModel>(
            Worksheet worksheet,
            TMapper classMap,
            TModel record,
            CellStyle recordStyle,
            bool pivot = false)
            where TMapper : ClassMap
            where TModel : class
        {
            ModelProperty[] names = classMap.MemberMaps.OrderBy(x => x.Data.Index).Select(x => new ModelProperty(x.Data.Names.Names.ToArray(), (PropertyInfo)x.Data.Member)).ToArray();
            WriteExcelRecords(worksheet, classMap, names, record, recordStyle, pivot);
        }

        protected void WriteExcelRecords<TMapper, TModel>(
            Worksheet worksheet,
            TMapper classMap,
            ModelProperty[] modelProperties,
            TModel record,
            CellStyle recordStyle,
            bool pivot = false)
            where TMapper : ClassMap
            where TModel : class
        {
            int currentRow = GetCurrentRow(worksheet);

            int column = 0;
            if (pivot)
            {
                // If we have pivoted then we need to move one column in, as the header is in column 1.
                column = 1;
            }

            List<object> values = new List<object>();
            foreach (ModelProperty modelProperty in modelProperties)
            {
                _valueProvider.GetFormattedValue(values, modelProperty.MethodInfo.GetValue(record), classMap, modelProperty);
            }

            worksheet.Cells.ImportObjectArray(values.ToArray(), currentRow, column, pivot);
            if (recordStyle != null)
            {
                worksheet.Cells.CreateRange(currentRow, column, pivot ? values.Count : 1, pivot ? 1 : values.Count).ApplyStyle(recordStyle.Style, recordStyle.StyleFlag);
            }

            if (pivot)
            {
                currentRow += values.Count;
            }
            else
            {
                currentRow++;
            }

            SetCurrentRow(worksheet, currentRow);
        }

        protected int GetCurrentRow(Worksheet worksheet)
        {
            if (!_currentRow.ContainsKey(worksheet))
            {
                _currentRow.Add(worksheet, 0);
            }

            return _currentRow[worksheet];
        }

        protected void SetCurrentRow(Worksheet worksheet, int currentRow)
        {
            _currentRow[worksheet] = currentRow;
        }

        protected void WriteCsvRecords<TMapper, TModel>(CsvWriter csvWriter, IEnumerable<TModel> records)
            where TMapper : ClassMap
            where TModel : class
        {
            csvWriter.Configuration.RegisterClassMap<TMapper>();

            csvWriter.WriteHeader<TModel>();
            csvWriter.NextRecord();

            csvWriter.WriteRecords(records);

            csvWriter.Configuration.UnregisterClassMap();
        }
    }
}