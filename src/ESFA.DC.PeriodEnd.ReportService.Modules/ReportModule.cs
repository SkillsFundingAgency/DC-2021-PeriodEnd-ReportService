using System.Collections.Generic;
using System.Collections.Immutable;
using Autofac;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;

namespace ESFA.DC.PeriodEnd.ReportService.Modules
{
    public class ReportModule : Module
    {
        private readonly IReportServiceConfiguration _reportServiceConfiguration;
        private readonly IDataPersistConfiguration _dataPersistConfiguration;

        public ReportModule(IReportServiceConfiguration reportServiceConfiguration, IDataPersistConfiguration dataPersistConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
            _dataPersistConfiguration = dataPersistConfiguration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new FundingSummaryModule(_reportServiceConfiguration, _dataPersistConfiguration));
            builder.RegisterModule(new AppsCoInvestmentModule(_reportServiceConfiguration, _dataPersistConfiguration));
            builder.RegisterModule(new AppsAdditionalPaymentsModule(_reportServiceConfiguration, _dataPersistConfiguration));
            builder.RegisterModule(new AppsMonthlyModule(_reportServiceConfiguration, _dataPersistConfiguration));
            builder.RegisterModule(new ActCountReportModule(_reportServiceConfiguration, _dataPersistConfiguration));
            builder.RegisterModule(new CollectionStatsModule(_reportServiceConfiguration, _dataPersistConfiguration));
            builder.RegisterModule(new ProviderSubmissionModule(_reportServiceConfiguration, _dataPersistConfiguration));
            builder.RegisterModule(new DataQualityModule(_reportServiceConfiguration, _dataPersistConfiguration));
            builder.RegisterModule(new UYPSummaryViewModule(_reportServiceConfiguration, _dataPersistConfiguration));

            builder.RegisterAdapter<IEnumerable<IReport>, IImmutableDictionary<string, IReport>>(c =>
                c.ToImmutableDictionary(x => x.ReportTaskName, x => x));
        }
    }
}
