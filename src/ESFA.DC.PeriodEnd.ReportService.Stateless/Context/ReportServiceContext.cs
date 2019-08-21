using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;

namespace ESFA.DC.PeriodEnd.ReportService.Stateless.Context
{
    public sealed class ReportServiceContext : IReportServiceContext
    {
        private readonly IJobContextMessage _jobContextMessage;

        public ReportServiceContext(IJobContextMessage jobContextMessage)
        {
            _jobContextMessage = jobContextMessage;
        }

        public string Filename => _jobContextMessage.KeyValuePairs[JobContextMessageKey.Filename].ToString();

        public int Ukprn => int.Parse(_jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn].ToString());

        public string Container => _jobContextMessage.KeyValuePairs[JobContextMessageKey.Container].ToString();

        public IEnumerable<string> Tasks => _jobContextMessage.Topics[_jobContextMessage.TopicPointer].Tasks.SelectMany(x => x.Tasks);

        public int CollectionYear => int.Parse(_jobContextMessage.KeyValuePairs[JobContextMessageKey.CollectionYear].ToString());

        public int ReturnPeriod => int.Parse(_jobContextMessage.KeyValuePairs[JobContextMessageKey.ReturnPeriod].ToString());

        public long JobId => _jobContextMessage.JobId;

        public DateTime SubmissionDateTimeUtc => _jobContextMessage.SubmissionDateTimeUtc;

        public string CollectionName => _jobContextMessage.KeyValuePairs[MessageKeys.CollectionName].ToString();

        public string CollectionReturnCodeDC => _jobContextMessage.KeyValuePairs[MessageKeys.CollectionReturnCodeDC].ToString();

        public string CollectionReturnCodeESF => _jobContextMessage.KeyValuePairs[MessageKeys.CollectionReturnCodeESF].ToString();

        public string CollectionReturnCodeApp => _jobContextMessage.KeyValuePairs[MessageKeys.CollectionReturnCodeApp].ToString();
    }
}
