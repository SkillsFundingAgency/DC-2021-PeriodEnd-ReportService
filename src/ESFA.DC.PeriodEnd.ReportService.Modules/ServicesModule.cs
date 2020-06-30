using Autofac;
using ESFA.DC.ExcelService;
using ESFA.DC.ExcelService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Persist;
using ESFA.DC.PeriodEnd.ReportService.Reports.Services;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExcelFileService>().As<IExcelFileService>();
            builder.RegisterType<FileNameService>().As<IFileNameService>();
            builder.RegisterType<BulkInsert>().As<IBulkInsert>();
        }
    }
}
