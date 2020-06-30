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

        public ReportModule(IReportServiceConfiguration reportServiceConfiguration)
        {
            _reportServiceConfiguration = reportServiceConfiguration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new FundingSummaryModule(_reportServiceConfiguration));
            builder.RegisterModule(new AppsCoInvestmentModule(_reportServiceConfiguration));

            builder.RegisterAdapter<IEnumerable<IReport>, IImmutableDictionary<string, IReport>>(c =>
                c.ToImmutableDictionary(x => x.ReportTaskName, x => x));
        }
    }
}
