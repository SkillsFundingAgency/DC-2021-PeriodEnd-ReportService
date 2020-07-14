using Autofac;
using ESFA.DC.BulkCopy;
using ESFA.DC.BulkCopy.Configuration;
using ESFA.DC.BulkCopy.Interfaces;
using ESFA.DC.CsvService;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.ExcelService;
using ESFA.DC.ExcelService.Interface;
using ESFA.DC.FileService;
using ESFA.DC.FileService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Services;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CsvFileService>().As<ICsvFileService>();
            builder.RegisterType<ExcelFileService>().As<IExcelFileService>();
            builder.RegisterType<FileNameService>().As<IFileNameService>();
            builder.RegisterType<BulkInsert>().As<IBulkInsert>();
            builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>();

            builder.RegisterType<ReportZipService>().As<IReportZipService>();
            builder.RegisterType<ZipArchiveService>().As<IZipArchiveService>();

            builder.Register(c => new BulkInsertConfiguration
            {
                BatchSize = 5000,
                BulkCopyTimeoutSeconds = 600
            }).As<IBulkInsertConfiguration>();
        }
    }
}
