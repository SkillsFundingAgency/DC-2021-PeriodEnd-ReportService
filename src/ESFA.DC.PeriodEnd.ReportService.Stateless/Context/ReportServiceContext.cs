using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.PeriodEnd.ReportService.Stateless.Context
{
    public sealed class ReportServiceContext : IReportServiceContext
    {
        private readonly IJobContextMessage _jobContextMessage;
        private readonly ISerializationService _serializationService;

        public ReportServiceContext(IJobContextMessage jobContextMessage, ISerializationService serializationService)
        {
            _jobContextMessage = jobContextMessage;
            _serializationService = serializationService;
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

        public IEnumerable<ReturnPeriod> ILRPeriods => (IEnumerable<ReturnPeriod>)_jobContextMessage.KeyValuePairs[MessageKeys.ILRPeriods];

        public IEnumerable<ReturnPeriod> ILRPeriodsAdjustedTimes => GetReturnPeriodsWithAdjustedEndTimes((IEnumerable<ReturnPeriod>)_jobContextMessage.KeyValuePairs[MessageKeys.ILRPeriods]);

        public IEnumerable<ReturnPeriod> GetReturnPeriodsWithAdjustedEndTimes(IEnumerable<ReturnPeriod> returnPeriods)
        {
            foreach (ReturnPeriod period in returnPeriods.OrderBy(p => p.PeriodNumber))
            {
                if (period.PeriodNumber == 14)
                {
                    period.EndDateTimeUtc = period.EndDateTimeUtc.AddDays(14);
                }
                else if (returnPeriods.Any(p => p.PeriodNumber == period.PeriodNumber + 1))
                {
                    period.EndDateTimeUtc = returnPeriods.Single(p => p.PeriodNumber == period.PeriodNumber + 1).StartDateTimeUtc.AddSeconds(-1);
                }
            }

            return returnPeriods;
        }
    }
}
