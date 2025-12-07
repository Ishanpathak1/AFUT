using System;
using System.IO;
using System.Linq;
using AFUT.Tests.Config;
using AFUT.Tests.Driver;
using AFUT.Tests.Helpers;
using AFUT.Tests.UnitTests.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace AFUT.Tests.UnitTests.Reports.TypesOfReports
{
    [TestCaseOrderer("AFUT.Tests.UnitTests.Attributes.PriorityOrderer", "AFUT.Tests")]
    public class TrainingTests : IClassFixture<AppConfig>
    {
        private readonly AppConfig _config;
        private readonly IPookieDriverFactory _driverFactory;
        private readonly ITestOutputHelper _output;

        public TrainingTests(AppConfig config, ITestOutputHelper output)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _output = output ?? throw new ArgumentNullException(nameof(output));

            _driverFactory = _config.ServiceProvider.GetService<IPookieDriverFactory>()
                              ?? throw new InvalidOperationException("Driver factory was not registered in the service provider.");
        }

        [Fact]
        [TestPriority(1)]
        public void VerifyTrainingReportCriteriaAndFilters()
        {
            using var driver = _driverFactory.CreateDriver();

            // Use the helper method to test the complete Training filter functionality
            ReportFilterHelper.TestReportFilterCategoryComplete(driver, _config, _output, "Training");
        }

        [Fact]
        [TestPriority(2)]
        public void VerifyTrainingReportPdfContentMatchesSelectedCriteria()
        {
            // Set up download directory
            var downloadDir = Path.Combine(Path.GetTempPath(), "PookieTestDownloads", Guid.NewGuid().ToString());
            Directory.CreateDirectory(downloadDir);
            _output.WriteLine($"[INFO] Download directory: {downloadDir}");

            try
            {
                using var driver = _driverFactory.CreateDriver(downloadDir);

                // Clean up old PDFs in download directory
                PdfTestHelper.CleanupOldPdfs(downloadDir, olderThanHours: 0);

                // Step 1: Navigate to Reports > Training
                _output.WriteLine("[INFO] Navigating to Training reports...");
                var homePage = CommonTestHelper.NavigateToReportsHomePage(driver, _config, _output);
                ReportFilterHelper.SelectReportFilter(driver, _output, "Training");
                
                // Change page size to "All" to test all reports
                ReportFilterHelper.ChangePageSizeToAll(driver, _output);
                
                // Get count of all Training reports
                var reportCount = ReportFilterHelper.GetReportCount(driver, _output);
                _output.WriteLine($"\n{'=',-60}");
                _output.WriteLine($"[INFO] Testing PDF generation for all {reportCount} Training reports");
                _output.WriteLine($"{'=',-60}");

                var random = new Random();

                // Test each Training report
                for (int i = 0; i < reportCount; i++)
                {
                    _output.WriteLine($"\n[INFO] Testing Report {i + 1} of {reportCount}");
                    _output.WriteLine($"{'=',-60}");

                    // Click on the report at current index
                    var reportName = ReportFilterHelper.ClickReportAtIndex(driver, _output, "Training", i);

                    // Randomize criteria (like a real user)
                    var selectedCriteria = ReportFilterHelper.RandomizeReportCriteria(driver, _output, random);

                    // Run the report
                    ReportFilterHelper.RunReport(driver, _output, timeoutSeconds: 60);

                    // Export to PDF
                    var pdfFile = ReportFilterHelper.ExportReportToPdf(driver, _output, downloadDir, timeoutSeconds: 30);

                    // Validate PDF content
                    ReportFilterHelper.ValidatePdfContent(pdfFile, reportName, selectedCriteria, _output);

                    _output.WriteLine($"[PASS] Report {i + 1}/{reportCount} ({reportName}) PDF validation completed");

                    // Navigate back to report list (except for the last report)
                    if (i < reportCount - 1)
                    {
                        ReportFilterHelper.NavigateBackToReportList(driver, _output);
                    }
                }

                _output.WriteLine($"\n{'=',-60}");
                _output.WriteLine($"[PASS] All {reportCount} Training reports PDF validation completed successfully");
                _output.WriteLine($"{'=',-60}");
            }
            finally
            {
                // Cleanup: Delete download directory
                try
                {
                    if (Directory.Exists(downloadDir))
                    {
                        Directory.Delete(downloadDir, recursive: true);
                        _output.WriteLine($"[INFO] Cleaned up download directory: {downloadDir}");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"[WARN] Could not clean up download directory: {ex.Message}");
                }
            }
        }
    }
}

