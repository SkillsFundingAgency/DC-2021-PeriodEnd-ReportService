using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.Serialization.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Service;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Tests.Reports
{
    public sealed class TestCollectionStatsReport
    {
        [Fact]
        public async Task TestDataExtractReportGeneration()
        {
            int ukPrn = 10036143;
            string json = string.Empty;
            int collectionYear = 1920;
            int returnPeriod = 1;
            DateTime dateTime = DateTime.UtcNow;

            string filename = $"R{returnPeriod:D2}_CollectionStats";

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(ukPrn);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.CollectionYear).Returns(collectionYear);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(returnPeriod);
            
            IValueProvider valueProvider = new ValueProvider();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IJsonSerializationService> jsonMock = new Mock<IJsonSerializationService>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IJobQueueManagerProviderService> jobQueueManagerProviderServiceMock = new Mock<IJobQueueManagerProviderService>();

            storage.Setup(x => x.SaveAsync($"{filename}.json", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((key, value, ct) => json = value)
                .Returns(Task.CompletedTask);

            var collectionStatsInfo = BuildCollectionStatsModel();
            string collectionStats = "[{\"CollectionName\":\"EAS1920\",\"CountOfComplete\":6,\"CountOfFail\":1,\"Total\":7,\"Percent\":85.71},{\"CollectionName\":\"ESFR2 - 1920\",\"CountOfComplete\":2,\"CountOfFail\":1,\"Total\":3,\"Percent\":66.67},{\"CollectionName\":\"ILR1920\",\"CountOfComplete\":7,\"CountOfFail\":14,\"Total\":21,\"Percent\":33.33},{\"CollectionName\":\"Total\",\"CountOfComplete\":15,\"CountOfFail\":16,\"Total\":31,\"Percent\":48.39}]";

            jsonMock.Setup(x => x.Serialize<IEnumerable<CollectionStatsModel>>(collectionStatsInfo))
                .Returns(collectionStats);

            jobQueueManagerProviderServiceMock.Setup(x => x
            .GetCollectionStatsModels(It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(collectionStatsInfo);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);

            var report = new CollectionStatsReport(
                valueProvider: valueProvider,
                dateTimeProvider: dateTimeProviderMock.Object,
                jsonSerializationService: jsonMock.Object,
                streamableKeyValuePersistenceService: storage.Object,
                jobQueueManagerProviderService: jobQueueManagerProviderServiceMock.Object);

            await report.GenerateReport(reportServiceContextMock.Object, CancellationToken.None);

            json.Should().NotBeNullOrEmpty();
#if DEBUG
            File.WriteAllText($"C://Temp//{filename}.json", json);
#endif
            json.Should().Be(collectionStats);
        }

        private IEnumerable<CollectionStatsModel> BuildCollectionStatsModel()
        {
            return new List<CollectionStatsModel>()
            {
                new CollectionStatsModel()
                {
                    CollectionName = "EAS1920",
                    CountOfComplete = 6,
                    CountOfFail = 1
                },
                new CollectionStatsModel()
                {
                    CollectionName = "ESFR2-1920",
                    CountOfComplete = 2,
                    CountOfFail = 1
                },
                new CollectionStatsModel()
                {
                    CollectionName = "ILR1920",
                    CountOfComplete = 7,
                    CountOfFail = 14
                },
                new CollectionStatsModel()
                {
                    CollectionName = "Total",
                    CountOfComplete = 15,
                    CountOfFail = 16
                }
            };
        }
    }
}
