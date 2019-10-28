using Aspose.Cells;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Service;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Model.Org;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Tests.Reports
{
    public sealed class TestProviderSubmissionsReport
    {
        [Fact]
        public async Task TestProviderSubmissionsReportGeneration()
        {
            byte[] xlsx = null;
            DateTime dateTime = DateTime.UtcNow;
            string reportFileName = "ILR Provider Submissions Report";
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
            reportServiceContextMock.SetupGet(x => x.ILRPeriodsAdjustedTimes).Returns(BuildReturnPeriodsModel());

            var filename = $"R{returnPeriod.ToString().PadLeft(2, '0')}_{reportFileName} R{returnPeriod.ToString().PadLeft(2, '0')} {dateTime:yyyyMMdd-HHmmss}";

            Mock<ILogger> loggerMock = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IOrgProviderService> orgProviderMock = new Mock<IOrgProviderService>();
            Mock<IProviderSubmissionsModelBuilder> providerSubmissionModelBuilderMock = new Mock<IProviderSubmissionsModelBuilder>();
            Mock<IJobQueueManagerProviderService> jobQueueManagerMock = new Mock<IJobQueueManagerProviderService>();
            Mock<IIlrPeriodEndProviderService> ilrPeriodEndProviderServiceMock = new Mock<IIlrPeriodEndProviderService>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
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

            List<OrgModel> orgInfo = BuildOrgModel(ukPrn);
            IEnumerable<ProviderSubmissionModel> fileDetails = BuildFileDetailsModel();
            List<OrganisationCollectionModel> expectedReturner = new List<OrganisationCollectionModel> { new OrganisationCollectionModel { Ukprn = ukPrn }, new OrganisationCollectionModel { Ukprn = 10036145 }, new OrganisationCollectionModel { Ukprn = 10036147 } };
            List<long> actualReturner = new List<long> { ukPrn, 10036145, 10036147 };
            IEnumerable<ProviderSubmissionModel> providerSubmissionModel = BuildProviderSubmissionModel();

            orgProviderMock.Setup(x => x.GetOrgDetailsForUKPRNsAsync(It.IsAny<List<long>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(orgInfo);
            ilrPeriodEndProviderServiceMock.Setup(x => x.GetFileDetailsLatestSubmittedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(fileDetails);
            ilrPeriodEndProviderServiceMock.Setup(x => x.GetPeriodReturn(It.IsAny<DateTime?>(), It.IsAny<IEnumerable<ReturnPeriod>>()))
                .Returns(12);
            jobQueueManagerMock.Setup(x => x.GetExpectedReturnersUKPRNsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedReturner);
            jobQueueManagerMock.Setup(x => x.GetActualReturnersUKPRNsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(actualReturner);
            providerSubmissionModelBuilderMock.Setup(x => x.BuildModel(It.IsAny<List<ProviderSubmissionModel>>(), It.IsAny<IEnumerable<OrgModel>>(), It.IsAny<List<OrganisationCollectionModel>>(), It.IsAny<IEnumerable<long>>(), It.IsAny<IEnumerable<ReturnPeriod>>(), It.IsAny<int>()))
                .Returns(providerSubmissionModel);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);
            
            var report = new ProviderSubmissionsReport(
                logger: loggerMock.Object,
                dateTimeProvider: dateTimeProviderMock.Object,
                orgProviderService: orgProviderMock.Object,
                ilrPeriodEndProviderService: ilrPeriodEndProviderServiceMock.Object,
                providerSubmissionsModelBuilder: providerSubmissionModelBuilderMock.Object,
                jobQueueManagerProviderService: jobQueueManagerMock.Object,
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
            wb.Worksheets[0].Name.Should().Be("Provider Submissions");
        }

        private IEnumerable<ProviderSubmissionModel> BuildFileDetailsModel()
        {
            return new List<ProviderSubmissionModel>
            {
                new ProviderSubmissionModel
                {
                    Ukprn = 10006341,
                    SubmittedDateTime = new DateTime(2019, 05, 01)
                }
            };
        }

        private IEnumerable<ProviderSubmissionModel> BuildProviderSubmissionModel()
        {
            return new List<ProviderSubmissionModel>
            {
                new ProviderSubmissionModel
                {
                    Ukprn = 10036143,
                    SubmittedDateTime = new DateTime(2019, 05, 01),
                    TotalErrors = 3550,
                    TotalInvalid = 1045,
                    TotalValid = 1560,
                    TotalWarnings = 4890,
                    Name = "WOODSPEEN TRAINING LIMITED",
                    Expected = true,
                    Returned = true,
                    LatestReturn = "R02"
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
