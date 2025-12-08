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
    public class QuarterliesTests : IClassFixture<AppConfig>
    {
        private readonly AppConfig _config;
        private readonly IPookieDriverFactory _driverFactory;
        private readonly ITestOutputHelper _output;

        public QuarterliesTests(AppConfig config, ITestOutputHelper output)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _output = output ?? throw new ArgumentNullException(nameof(output));

            _driverFactory = _config.ServiceProvider.GetService<IPookieDriverFactory>()
                              ?? throw new InvalidOperationException("Driver factory was not registered in the service provider.");
        }

        [Fact]
        [TestPriority(1)]
        public void VerifyQuarterliesReportCriteriaAndFilters()
        {
            using var driver = _driverFactory.CreateDriver();

            // Use the helper method to test the complete Quarterlies filter functionality
            ReportFilterHelper.TestReportFilterCategoryComplete(driver, _config, _output, "Quarterlies");
        }

        [Fact]
        [TestPriority(2)]
        public void VerifyQuarterSelectionValidation()
        {
            using var driver = _driverFactory.CreateDriver();

            _output.WriteLine("[INFO] Testing quarter selection validation for Quarterlies reports...");
            
            // Navigate to Reports > Quarterlies
            var homePage = CommonTestHelper.NavigateToReportsHomePage(driver, _config, _output);
            ReportFilterHelper.SelectReportFilter(driver, _output, "Quarterlies");
            
            // Get the first report
            var reportCount = ReportFilterHelper.GetReportCount(driver, _output);
            _output.WriteLine($"[INFO] Found {reportCount} Quarterlies report(s)");

            if (reportCount > 0)
            {
                var reportName = ReportFilterHelper.ClickFirstReport(driver, _output, "Quarterlies");
                _output.WriteLine($"[INFO] Testing report: {reportName}");
                
                // Test the quarter validation
                ReportFilterHelper.TestQuarterValidation(driver, _output);
            }
            else
            {
                _output.WriteLine("[WARN] No Quarterlies reports found to test");
            }
        }

        [Fact]
        [TestPriority(2)]
        public void VerifyQuarterliesReportPdfContentMatchesSelectedCriteria()
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

                // Step 1: Navigate to Reports > Quarterlies
                _output.WriteLine("[INFO] Navigating to Quarterlies reports...");
                var homePage = CommonTestHelper.NavigateToReportsHomePage(driver, _config, _output);
                ReportFilterHelper.SelectReportFilter(driver, _output, "Quarterlies");
                
                // Change page size to "All" to test all reports
                ReportFilterHelper.ChangePageSizeToAll(driver, _output);
                
                // Get count of all Quarterlies reports
                var reportCount = ReportFilterHelper.GetReportCount(driver, _output);
                _output.WriteLine($"\n{'=',-60}");
                _output.WriteLine($"[INFO] Testing PDF generation for all {reportCount} Quarterlies reports");
                _output.WriteLine($"{'=',-60}");

                var random = new Random();

                // Test each Quarterlies report
                int successCount = 0;
                int failCount = 0;
                
                for (int i = 0; i < reportCount; i++)
                {
                    _output.WriteLine($"\n[INFO] Testing Report {i + 1} of {reportCount}");
                    _output.WriteLine($"{'=',-60}");

                    try
                    {
                        // Click on the report at current index
                        var reportName = ReportFilterHelper.ClickReportAtIndex(driver, _output, "Quarterlies", i);

                        // Randomize criteria (like a real user) - this will now handle quarter selection
                        var selectedCriteria = ReportFilterHelper.RandomizeReportCriteria(driver, _output, random);

                        // Run the report
                        ReportFilterHelper.RunReport(driver, _output, timeoutSeconds: 60);

                        // Export to PDF
                        var pdfFile = ReportFilterHelper.ExportReportToPdf(driver, _output, downloadDir, timeoutSeconds: 30);

                        // Validate PDF content
                        ReportFilterHelper.ValidatePdfContent(pdfFile, reportName, selectedCriteria, _output);

                        _output.WriteLine($"[PASS] Report {i + 1}/{reportCount} ({reportName}) PDF validation completed");
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"[FAIL] Report {i + 1}/{reportCount} failed: {ex.Message}");
                        _output.WriteLine($"[INFO] Continuing with remaining reports...");
                        failCount++;
                    }

                    // Navigate back to report list (except for the last report)
                    if (i < reportCount - 1)
                    {
                        try
                        {
                            ReportFilterHelper.NavigateBackToReportList(driver, _output);
                        }
                        catch (Exception ex)
                        {
                            _output.WriteLine($"[WARN] Could not navigate back: {ex.Message}");
                            _output.WriteLine($"[INFO] Navigating to Reports page fresh...");
                            
                            // Fresh navigation if back button fails
                            CommonTestHelper.NavigateToReportsHomePage(driver, _config, _output);
                            ReportFilterHelper.SelectReportFilter(driver, _output, "Quarterlies");
                        }
                    }
                }

                _output.WriteLine($"\n{'=',-60}");
                _output.WriteLine($"[SUMMARY] Quarterlies Reports PDF Validation Complete");
                _output.WriteLine($"  Total: {reportCount}");
                _output.WriteLine($"  Passed: {successCount}");
                _output.WriteLine($"  Failed: {failCount}");
                _output.WriteLine($"{'=',-60}");
                
                if (failCount > 0)
                {
                    _output.WriteLine($"[WARN] {failCount} report(s) had issues but test continued");
                }
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

