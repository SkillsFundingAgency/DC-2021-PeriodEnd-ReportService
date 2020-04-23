using Aspose.Cells;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.Org;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Tests.Reports
{
    public sealed class TestDataQualityReport
    {
        [Fact]
        public async Task TestDataQualityReportGeneration()
        {
            byte[] xlsx = null;
            DateTime dateTime = DateTime.UtcNow;
            string reportFileName = "Data Quality Report";
            int ukPrn = 10036143;
            int collectionYear = 1920;
            int returnPeriod = 12;
            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(ukPrn);
            reportServiceContextMock.SetupGet(x => x.CollectionYear).Returns(collectionYear);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(returnPeriod);
            reportServiceContextMock.SetupGet(x => x.ILRPeriods).Returns(BuildReturnPeriodsModel());

            var filename = $"R{returnPeriod.ToString().PadLeft(2, '0')}_{reportFileName} R{returnPeriod.ToString().PadLeft(2, '0')} {dateTime:yyyyMMdd-HHmmss}";

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IOrgProviderService> orgProviderMock = new Mock<IOrgProviderService>();
            Mock<IIlrPeriodEndProviderService> ilrPeriodEndProviderServiceMock = new Mock<IIlrPeriodEndProviderService>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IJobQueueManagerProviderService> jobQueueManagerProviderServiceMock = new Mock<IJobQueueManagerProviderService>();
            IValueProvider valueProvider = new ValueProvider();

            storage.Setup(x => x.SaveAsync($"{filename}.xlsx", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<string, Stream, CancellationToken>(
                    (key, value, ct) =>
                    {
                        value.Seek(0, SeekOrigin.Begin);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            value.CopyTo(ms);
                            xlsx = ms.ToArray();
                        }
                    })
                .Returns(Task.CompletedTask);

            var orgInfo = BuildOrgModel(ukPrn);
            var fileDetails = BuildFileDetailsModel();
            var dataQualityReturningInfo = BuilDataQualityReturningModel();
            var top20RuleViolations = BuildTop20RuleViolationModel();
            var providerWithoutValidLearner = BuildProviderWithoutValidLearnerModel(ukPrn);
            var providerWithInValidLearner = BuildProviderWithInvalidLearnerModel(ukPrn);

            orgProviderMock.Setup(x => x.GetOrgDetailsForUKPRNsAsync(It.IsAny<List<long>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(orgInfo);
            jobQueueManagerProviderServiceMock
                .Setup(x => x.GetCollectionIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(8);
            jobQueueManagerProviderServiceMock.Setup(x => x.GetFilePeriodInfoForCollection(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fileDetails);
            ilrPeriodEndProviderServiceMock.Setup(x => x.GetReturningProvidersAsync(It.IsAny<int>(), It.IsAny<List<ReturnPeriod>>(), It.IsAny<List<FilePeriodInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataQualityReturningInfo);
            ilrPeriodEndProviderServiceMock.Setup(x => x.GetTop20RuleViolationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(top20RuleViolations);
            ilrPeriodEndProviderServiceMock.Setup(x => x.GetProvidersWithoutValidLearners(It.IsAny<List<FilePeriodInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(providerWithoutValidLearner);
            ilrPeriodEndProviderServiceMock.Setup(x => x.GetProvidersWithInvalidLearners(It.IsAny<int>(), It.IsAny<List<ReturnPeriod>>(), It.IsAny<List<FilePeriodInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(providerWithInValidLearner);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);

            var report = new DataQualityReport(
                logger: logger.Object,
                dateTimeProvider: dateTimeProviderMock.Object,
                orgProviderService : orgProviderMock.Object,
                ilrPeriodEndProviderService: ilrPeriodEndProviderServiceMock.Object,
                jobQueueManagerProviderService: jobQueueManagerProviderServiceMock.Object,
                streamableKeyValuePersistenceService: storage.Object,
                valueProvider: valueProvider);

            await report.GenerateReport(reportServiceContextMock.Object, CancellationToken.None);

            xlsx.Should().NotBeNullOrEmpty();
#if DEBUG
            File.WriteAllBytes($"C://Temp//{filename}.xlsx", xlsx);
#endif
            Stream stream = new MemoryStream(xlsx);
            Workbook wb = new Workbook(stream);
            wb.Should().NotBeNull();
            wb.Worksheets.Count().Should().BeGreaterThan(0);
            wb.Worksheets[0].Name.Should().Be("Data Quality");
        }

        private IEnumerable<FilePeriodInfo> BuildFileDetailsModel()
        {
            return new List<FilePeriodInfo>
            {
                new FilePeriodInfo
                {
                    Filename = "10006341/ILR-10006341-1920-20190805-110110-35.XML",
                    SubmittedTime = new DateTime(2019, 05, 01),
                    UKPRN = 10006341
                }
            };
        }

        private List<RuleViolationsInfo> BuildTop20RuleViolationModel()
        {
            return new List<RuleViolationsInfo>
            {
                new RuleViolationsInfo
                {
                    RuleName = "LearnStartDate_16",
                    ErrorMessage = "The Learning start date must not be before the start date of the contract",
                    Learners = 50,
                    NoOfErrors = 17,
                    Providers = 15
                }
            };
        }

        private IEnumerable<Top10ProvidersWithInvalidLearners> BuildProviderWithInvalidLearnerModel(int ukPrn)
        {
            return new List<Top10ProvidersWithInvalidLearners>
            {
                new Top10ProvidersWithInvalidLearners
                {
                    Ukprn = ukPrn,
                    Name = "AMERSHAM & WYCOMBE COLLEGE",
                    LatestFileName = "10006341/ILR-10006341-1920-20190805-110110-35.XML",
                    LatestReturn = "R12",
                    NoOfValidLearners = 2250,
                    NoOfInvalidLearners = 1050,
                    Status = "Active",
                    SubmittedDateTime = new DateTime(2019, 05, 01)
                }
            };
        }

        private IEnumerable<ProviderWithoutValidLearners> BuildProviderWithoutValidLearnerModel(int ukprn)
        {
            return new List<ProviderWithoutValidLearners>
            {
                new ProviderWithoutValidLearners
                {
                    Ukprn = ukprn,
                    Name = "AMERSHAM & WYCOMBE COLLEGE",
                    LatestFileSubmitted = new DateTime(2019, 5, 01)
                }
            };
        }

        private IEnumerable<DataQualityReturningProviders> BuilDataQualityReturningModel()
        {
            return new List<DataQualityReturningProviders>
            {
                new DataQualityReturningProviders
                {
                    Collection = "R12",
                    Description = "Returning Providers per Period",
                    EarliestValidSubmission = new DateTime(2019, 4, 01),
                    LastValidSubmission =  new DateTime(2019, 5, 01),
                    NoOfProviders = 15,
                    NoOfValidFilesSubmitted = 3598
                }
            };
        }

        private List<OrgModel> BuildOrgModel(int ukprn)
        {
            return new List<OrgModel>
            {
                new OrgModel
                {
                    Ukprn = ukprn,
                    Name = "WOODSPEEN TRAINING LIMITED",
                    Status = "Active"
                }
            };
        }

        private List<ReturnPeriod> BuildReturnPeriodsModel()
        {
            return new List<ReturnPeriod>
            {
                new ReturnPeriod
                    {
                        StartDateTimeUtc = new DateTime(2019, 05, 01, 13, 30, 00),
                        EndDateTimeUtc = new DateTime(2019, 05, 02, 15, 30, 45),
                        PeriodNumber = 12
                    }
            };
        }
    }
}