using Autofac;
using ESFA.DC.BulkCopy;
using ESFA.DC.BulkCopy.Configuration;
using ESFA.DC.BulkCopy.Interfaces;
using ESFA.DC.CsvService;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.ExcelService;
using ESFA.DC.ExcelService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Services;

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

            builder.Register(c => new BulkInsertConfiguration
            {
                BatchSize = 5000,
                BulkCopyTimeoutSeconds = 600
            }).As<IBulkInsertConfiguration>();
        }
    }
}
