using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Mappers;
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataExtractReport;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Builders;
using ESFA.DC.PeriodEnd.ReportService.Service.Service;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Tests.Reports
{
    public sealed class TestDataExtractReport
    {
        [Fact]
        public async Task TestDataExtractReportGeneration()
        {
            int ukPrn = 10036143;
            string csv = string.Empty;
            int collectionYear = 1920;
            int returnPeriod = 1;
            DateTime dateTime = DateTime.UtcNow;

            string collectionName = "ILR1920";
            string collectionReturnCodeApp = "APPS01";
            string collectionReturnCodeDC = "R01";
            string collectionReturnCodeESF = "ESF01";
            
            string filename = $"R{returnPeriod:D2}_Data Extract Report R{returnPeriod:D2} {dateTime:yyyyMMdd-HHmmss}";

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(ukPrn);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.CollectionYear).Returns(collectionYear);
            reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(returnPeriod);
            reportServiceContextMock.SetupGet(x => x.CollectionReturnCodeApp).Returns(collectionReturnCodeApp);
            reportServiceContextMock.SetupGet(x => x.CollectionReturnCodeDC).Returns(collectionReturnCodeDC);
            reportServiceContextMock.SetupGet(x => x.CollectionReturnCodeESF).Returns(collectionReturnCodeESF);
            reportServiceContextMock.SetupGet(x => x.CollectionName).Returns(collectionName);

            Mock<ILogger> logger = new Mock<ILogger>();
            IValueProvider valueProvider = new ValueProvider();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IFCSProviderService> fcsProviderServiceMock = new Mock<IFCSProviderService>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<ISummarisationProviderService> summarisationProviderServiceMock = new Mock<ISummarisationProviderService>();
            
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((key, value, ct) => csv = value)
                .Returns(Task.CompletedTask);

            var summarisationInfo = BuildSummarisationModel(collectionReturnCodeApp, collectionReturnCodeDC, collectionReturnCodeESF);
            IEnumerable<string> organisationIDs = new List<string> { "ORG000001" };
            var fcsInfo = BuildFCSModel(organisationIDs);

            summarisationProviderServiceMock.Setup(x => x
            .GetSummarisedActualsForDataExtractReport(collectionName, new List<string> { collectionReturnCodeApp, collectionReturnCodeDC, collectionReturnCodeESF }, CancellationToken.None))
                .ReturnsAsync(summarisationInfo);
            fcsProviderServiceMock.Setup(x => x.GetFCSForDataExtractReport(organisationIDs, CancellationToken.None))
                .ReturnsAsync(fcsInfo);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);
            var dataExtractModelBuilder = new DataExtractModelBuilder();

            var report = new DataExtractReport(
                logger: logger.Object,
                streamableKeyValuePersistenceService: storage.Object,
                summarisationProviderService: summarisationProviderServiceMock.Object,
                fcsProviderService: fcsProviderServiceMock.Object,
                dateTimeProvider: dateTimeProviderMock.Object,
                valueProvider: valueProvider,
                modelBuilder: dataExtractModelBuilder);

            await report.GenerateReport(reportServiceContextMock.Object, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            IEnumerable<DataExtractModel> result;
            List<string> headers;

            using (CsvReader csvReader = new CsvReader(new StringReader(csv)))
            {
                csvReader.Configuration.RegisterClassMap<DataExtractMapper>();
                result = csvReader.GetRecords<DataExtractModel>().ToList();
                headers = csvReader.Context.HeaderRecord.ToList();
            }

            headers.Count().Should().Be(13);
            headers.Should().Contain("Id");
            headers.Should().Contain("CollectionReturnCode");
            headers.Should().Contain("ukprn");
            headers.Should().Contain("OrganisationId");
            headers.Should().Contain("PeriodTypeCode");
            headers.Should().Contain("Period");
            headers.Should().Contain("FundingStreamPeriodCode");
            headers.Should().Contain("CollectionType");
            headers.Should().Contain("ContractAllocationNumber");
            headers.Should().Contain("UoPCode");
            headers.Should().Contain("DeliverableCode");
            headers.Should().Contain("ActualVolume");
            headers.Should().Contain("ActualValue");

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(1);
            result.First().UkPrn.Should().Be(ukPrn);
            result.First().Id.Should().Be(1);
            result.First().OrganisationId.Should().Be("ORG000001");
            result.First().Period.Should().Be(1);
            result.First().ActualValue.Should().Be(1.25M);
            result.First().ActualVolume.Should().Be(1);
            result.First().CollectionReturnCode.Should().Be("R01");
            result.First().CollectionType.Should().Be("ILR1920");
            result.First().ContractAllocationNumber.Should().Be("APPS-1317");
            result.First().DeliverableCode.Should().Be(6);
            result.First().FundingStreamPeriodCode.Should().Be("APPS1920");
            result.First().PeriodTypeCode.Should().Be("CM");
            result.First().UoPCode.Should().Be("1");
        }

        private IEnumerable<DataExtractModel> BuildSummarisationModel(string collectionReturnCodeApp, string collectionReturnCodeDC, string collectionReturnCodeESF)
        {
            return new List<DataExtractModel>() {
                new DataExtractModel()
                {
                    Id = 1,
                    OrganisationId = "ORG000001",
                    Period = 1,
                    UkPrn = 10036143,
                    ActualValue = 1.25M,
                    ActualVolume = 1,
                    CollectionReturnCode = "R01",
                    CollectionType = "ILR1920",
                    ContractAllocationNumber = "APPS-1317",
                    DeliverableCode = 6,
                    FundingStreamPeriodCode = "APPS1920",
                    PeriodTypeCode = "CM",
                    UoPCode = "1"
                }
            };
        }

        private List<DataExtractFcsInfo> BuildFCSModel(IEnumerable<string> organisationIDs)
        {
            return new List<DataExtractFcsInfo>() {
                new DataExtractFcsInfo()
                {
                    UkPrn = 10036143,
                    ContractVersionNumber = 1,
                    OrganisationIdentifier = "ORG000001"
                }
            };
        }
    }
}