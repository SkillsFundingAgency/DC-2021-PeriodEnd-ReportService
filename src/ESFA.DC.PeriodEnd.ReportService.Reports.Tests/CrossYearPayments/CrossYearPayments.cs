using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ExcelService;
using ESFA.DC.ExcelService.Interface;
using ESFA.DC.FileService;
using ESFA.DC.PeriodEnd.ReportService.Reports.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.CrossYearPayments
{
    public class CrossYearPayments
    {
        [Fact]
        public async Task GenerateReportEmptyModel()
        {
            var cancellationToken = CancellationToken.None;
            var academicYear = 2021;
            var returnPeriod = 1;
            var ukprn = 123456;
            var fileName = "FileName.xlsx";
            var container = "";

            var crossYearPaymentsModel = new CrossYearPaymentsModel();
            var reportServiceContext = new Mock<IReportServiceContext>();

            var dataModel = new CrossYearDataModel();

            reportServiceContext.SetupGet(c => c.Ukprn).Returns(ukprn);
            reportServiceContext.SetupGet(c => c.CollectionYear).Returns(academicYear);
            reportServiceContext.SetupGet(c => c.Container).Returns(container);
            reportServiceContext.SetupGet(c => c.ReturnPeriod).Returns(returnPeriod);

            var excelFileServiceMock = new Mock<IExcelFileService>();
            var fileNameServiceMock = new Mock<IFileNameService>();
            var modelBuilderMock = new Mock<ICrossYearModelBuilder>();
            var renderServiceMock = new Mock<ICrossYearRenderService>();
            var dataProviderMock = new Mock<ICrossYearDataProvider>();

            var excelService = GetExcelFileService(excelFileServiceMock, false);
            var renderService = GetRenderService(renderServiceMock, false);

            fileNameServiceMock.Setup(s => s.GetFilename(reportServiceContext.Object, "Beta Cross Year Indicative Payments Report", OutputTypes.Excel, true, true)).Returns(fileName);

            dataProviderMock.Setup(b => b.ProvideAsync(reportServiceContext.Object, cancellationToken)).ReturnsAsync(dataModel);

            modelBuilderMock.Setup(b => b.Build(dataModel, reportServiceContext.Object)).Returns(crossYearPaymentsModel);

            var report = NewReport(excelService, fileNameServiceMock.Object, modelBuilderMock.Object, renderService, dataProviderMock.Object);

            await report.GenerateReport(reportServiceContext.Object, cancellationToken);
        }

        [Fact]
        public async Task GenerateReport()
        {
            var cancellationToken = CancellationToken.None;
            var academicYear = 2021;
            var returnPeriod = 2;
            var ukprn = 123456;
            var fileName = "FileName.xlsx";
            var container = "";

            var crossYearPaymentsModel = new CrossYearPaymentsModel()
            {
                HeaderInfo = new HeaderInfo
                {
                    ProviderName = "Test Provider",
                    UKPRN = 12345678
                },
                FooterInfo = new FooterInfo
                {
                    ReportGeneratedAt = "Report generated at 13:34:52 08/11/20"
                },
                Deliveries = new List<Delivery>
                {
                    new Delivery
                    {
                        ContractNumber = "Contract 1",
                        DeliveryName = "16-18 Non-Levy Contracted Apprenticeships - Procured delivery",
                        ContractValues = new List<ContractValue>
                        {
                            new ContractValue
                            {
                                DeliveryPeriod = 201801,
                                Value = 12000m
                            },
                            new ContractValue
                            {
                                DeliveryPeriod = 201804,
                                Value = 22000m
                            },
                            new ContractValue
                            {
                                DeliveryPeriod = 201904,
                                Value = 32000m
                            }
                        },
                        FSRValues = new List<FSRValue>
                        {
                            new FSRValue
                            {
                                AcademicYear = 1718,
                                DeliveryPeriod = 6,
                                CollectionPeriod = 14,
                                Value = 1m
                            },
                            new FSRValue
                            {
                                AcademicYear = 1819,
                                DeliveryPeriod = 6,
                                CollectionPeriod = 14,
                                Value = 1m
                            },
                            new FSRValue
                            {
                                AcademicYear = 2021,
                                DeliveryPeriod = 1,
                                CollectionPeriod = 3,
                                Value = 1m
                            },
                            new FSRValue
                            {
                                AcademicYear = 2021,
                                DeliveryPeriod = 2,
                                CollectionPeriod = 3,
                                Value = 1m
                            }
                        }
                    },
                    new Delivery
                    {
                        ContractNumber = "Contract2",
                        DeliveryName = "Adult Non-Levy Contracted Apprenticeships - Procured delivery",
                        FSRValues = new List<FSRValue>
                        {
                            new FSRValue
                            {
                                AcademicYear = 1718,
                                DeliveryPeriod = 6,
                                CollectionPeriod = 14,
                                Value = 1m
                            },
                        }
                    }
                }
            };
            var reportServiceContext = new Mock<IReportServiceContext>();

            var dataModel = new CrossYearDataModel();

            reportServiceContext.SetupGet(c => c.Ukprn).Returns(ukprn);
            reportServiceContext.SetupGet(c => c.CollectionYear).Returns(academicYear);
            reportServiceContext.SetupGet(c => c.Container).Returns(container);
            reportServiceContext.SetupGet(c => c.ReturnPeriod).Returns(returnPeriod);

            var excelFileServiceMock = new Mock<IExcelFileService>();
            var fileNameServiceMock = new Mock<IFileNameService>();
            var modelBuilderMock = new Mock<ICrossYearModelBuilder>();
            var renderServiceMock = new Mock<ICrossYearRenderService>();
            var dataProviderMock = new Mock<ICrossYearDataProvider>();

            var excelService = GetExcelFileService(excelFileServiceMock, false);
            var renderService = GetRenderService(renderServiceMock, false);

            fileNameServiceMock.Setup(s => s.GetFilename(reportServiceContext.Object, "Beta Cross Year Indicative Payments Report", OutputTypes.Excel, true, true)).Returns(fileName);

            dataProviderMock.Setup(b => b.ProvideAsync(reportServiceContext.Object, cancellationToken)).ReturnsAsync(dataModel);

            modelBuilderMock.Setup(b => b.Build(dataModel, reportServiceContext.Object)).Returns(crossYearPaymentsModel);

            var report = NewReport(excelService, fileNameServiceMock.Object, modelBuilderMock.Object, renderService, dataProviderMock.Object);

            await report.GenerateReport(reportServiceContext.Object, cancellationToken);
        }

        private Reports.CrossYearPayments.CrossYearPayments NewReport(IExcelFileService excelFileService = null, IFileNameService fileNameService = null, ICrossYearModelBuilder modelBuilder = null, ICrossYearRenderService crossYearRenderService = null, ICrossYearDataProvider crossYearDataProvider = null)
        {
            return new Reports.CrossYearPayments.CrossYearPayments(
                excelFileService ?? Mock.Of<IExcelFileService>(),
                fileNameService ?? Mock.Of<IFileNameService>(),
                modelBuilder ?? Mock.Of<ICrossYearModelBuilder>(),
                crossYearDataProvider ?? Mock.Of<ICrossYearDataProvider>(),
                crossYearRenderService ?? Mock.Of<ICrossYearRenderService>());
        }

        private IExcelFileService GetExcelFileService(Mock<IExcelFileService> excelFileServiceMock, bool useMock = true)
        {
            return useMock ? excelFileServiceMock.Object : new ExcelFileService(new FileSystemFileService());
        }

        private ICrossYearRenderService GetRenderService(Mock<ICrossYearRenderService> crossYearRenderService, bool useMock = true)
        {
            return useMock ? crossYearRenderService.Object : new CrossYearPaymentsRenderService();
        }
    }
}
