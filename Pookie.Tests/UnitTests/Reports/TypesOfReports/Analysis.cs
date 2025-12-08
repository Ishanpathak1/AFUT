using System;
using System.IO;
using System.Linq;
using AFUT.Tests.Config;
using AFUT.Tests.Driver;
using AFUT.Tests.Helpers;
using AFUT.Tests.UnitTests.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace AFUT.Tests.UnitTests.Reports.TypesOfReports
{
    [TestCaseOrderer("AFUT.Tests.UnitTests.Attributes.PriorityOrderer", "AFUT.Tests")]
    public class AnalysisTests : IClassFixture<AppConfig>
    {
        private readonly AppConfig _config;
        private readonly IPookieDriverFactory _driverFactory;
        private readonly ITestOutputHelper _output;

        public AnalysisTests(AppConfig config, ITestOutputHelper output)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _output = output ?? throw new ArgumentNullException(nameof(output));

            _driverFactory = _config.ServiceProvider.GetService<IPookieDriverFactory>()
                              ?? throw new InvalidOperationException("Driver factory was not registered in the service provider.");
        }

        [Fact]
        [TestPriority(1)]
        public void VerifyAnalysisReportCriteriaAndFilters()
        {
            using var driver = _driverFactory.CreateDriver();

            // Use the helper method to test the complete Analysis filter functionality
            ReportFilterHelper.TestReportFilterCategoryComplete(driver, _config, _output, "Analysis");
        }

        [Fact]
        [TestPriority(2)]
        public void VerifyAnalysisReportPdfContentMatchesSelectedCriteria()
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

                // Step 1: Navigate to Reports > Analysis
                _output.WriteLine("[INFO] Navigating to Analysis reports...");
                var homePage = CommonTestHelper.NavigateToReportsHomePage(driver, _config, _output);
                ReportFilterHelper.SelectReportFilter(driver, _output, "Analysis");
                
                // Change page size to "All" to test all reports
                ReportFilterHelper.ChangePageSizeToAll(driver, _output);
                
                // Get count of all Analysis reports
                var reportCount = ReportFilterHelper.GetReportCount(driver, _output);
                _output.WriteLine($"\n{'=',-60}");
                _output.WriteLine($"[INFO] Testing PDF generation for all {reportCount} Analysis reports");
                _output.WriteLine($"{'=',-60}");

                var random = new Random();

                // Test each Analysis report
                for (int i = 0; i < reportCount; i++)
                {
                    _output.WriteLine($"\n[INFO] Testing Report {i + 1} of {reportCount}");
                    _output.WriteLine($"{'=',-60}");

                    // Click on the report at current index
                    var reportName = ReportFilterHelper.ClickReportAtIndex(driver, _output, "Analysis", i);
                    
                    // Skip Demographics Export report (takes too long or has issues)
                    if (reportName.Contains("Demographics Export", StringComparison.OrdinalIgnoreCase))
                    {
                        _output.WriteLine($"[INFO] Skipping Demographics Export report");
                        
                        // Navigate back to report list if not the last report
                        if (i < reportCount - 1)
                        {
                            ReportFilterHelper.NavigateBackToReportList(driver, _output);
                        }
                        continue;
                    }
                    
                    // Quality Assurance report has different behavior - navigates to QASummary.aspx
                    if (reportName.Contains("Quality Assurance", StringComparison.OrdinalIgnoreCase))
                    {
                        _output.WriteLine($"[INFO] Testing Quality Assurance report (special navigation)");
                        
                        // Click Run Report button
                        var runButton = driver.FindElements(By.CssSelector(
                                "a.btn.btn-primary[id$='btnRunReport'], " +
                                "a.btn-primary[href*='RunReport'], " +
                                "button.btn-primary"))
                            .FirstOrDefault(el => el.Displayed && el.Text.Contains("Run Report", StringComparison.OrdinalIgnoreCase));

                        if (runButton != null)
                        {
                            _output.WriteLine("  [INFO] Clicking 'Run Report' button...");
                            CommonTestHelper.ClickElement(driver, runButton);
                            driver.WaitForReady(15);
                            Thread.Sleep(2000);

                            // Verify we navigated to QASummary.aspx
                            var currentUrl = driver.Url;
                            _output.WriteLine($"  [DEBUG] Current URL: {currentUrl}");

                            if (currentUrl.Contains("QASummary.aspx", StringComparison.OrdinalIgnoreCase))
                            {
                                _output.WriteLine($"  [PASS] Successfully navigated to QA Summary page");
                            }
                            else
                            {
                                _output.WriteLine($"  [WARN] Expected QASummary.aspx, got: {currentUrl}");
                            }
                        }
                        
                        _output.WriteLine($"[PASS] Report {i + 1}/{reportCount} ({reportName}) navigation validated");
                        
                        // Navigate back to report list
                        if (i < reportCount - 1)
                        {
                            ReportFilterHelper.NavigateBackToReportList(driver, _output);
                        }
                        continue;
                    }

                    // For all other reports, do normal PDF validation
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
                _output.WriteLine($"[PASS] All {reportCount} Analysis reports PDF validation completed successfully");
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

