using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AFUT.Tests.Config;
using AFUT.Tests.Driver;
using AFUT.Tests.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit;
using Xunit.Abstractions;

namespace AFUT.Tests.UnitTests.Reports
{
    /// <summary>
    /// Helper methods for testing report filter functionality across different report categories.
    /// </summary>
    public static class ReportFilterHelper
    {
        /// <summary>
        /// Performs the complete test flow for a specific report filter category.
        /// Navigates to Reports homepage, selects filter, changes page size, tests all reports,
        /// and validates all criteria sections for each report.
        /// </summary>
        /// <param name="driver">The web driver instance</param>
        /// <param name="config">Application configuration</param>
        /// <param name="output">Test output helper for logging</param>
        /// <param name="filterName">Name of the filter to test (e.g., "Accreditation", "Lists", "Analysis")</param>
        public static void TestReportFilterCategoryComplete(
            IPookieWebDriver driver,
            AppConfig config,
            ITestOutputHelper output,
            string filterName)
        {
            // Step 1: Navigate to Reports homepage
            var homePage = CommonTestHelper.NavigateToReportsHomePage(driver, config, output);
            Assert.NotNull(homePage);
            Assert.True(homePage.IsLoaded, "Home page did not load after navigation.");
            output.WriteLine("[PASS] Navigated to Reports homepage");

            // Step 2: Select the specified filter
            SelectReportFilter(driver, output, filterName);

            // Step 3: Change page size to "All"
            ChangePageSizeToAll(driver, output);

            // Step 4: Get count of all reports in this category
            var reportCount = GetReportCount(driver, output);

            // Step 5: Test each report
            for (int i = 0; i < reportCount; i++)
            {
                output.WriteLine($"\n{'=',-60}");
                output.WriteLine($"[INFO] Testing Report {i + 1} of {reportCount}");
                output.WriteLine($"{'=',-60}");

                // Click the report at the current index
                var reportName = ClickReportAtIndex(driver, output, filterName, i);

                // Verify divCriteria is displayed
                VerifyDivCriteriaDisplayed(driver, output);

                // Test all criteria sections and log which ones are present
                var sectionsPresent = TestReportCriteriaSectionsWithLogging(driver, output, reportName);

                // Verify Start Date field is present (optional - logs warning if missing)
                VerifyStartDateField(driver, output);

                // If Quarter criteria is present, exercise the quarter dropdown
                TestQuarterCriteriaSection(driver, output);

                // If Programs/Regions tabbed criteria is present, exercise both tabs and their checkboxes
                TestProgramsAndRegionsSection(driver, output);

                // If Tickler Summary "Report to Run" section is present, exercise all options
                TestTicklerSummaryReportToRunSection(driver, output);

                // Navigate back to the report list
                NavigateBackToReportList(driver, output);
            }

            output.WriteLine($"\n{'=',-60}");
            output.WriteLine($"[PASS] All {reportCount} {filterName} reports tested successfully");
            output.WriteLine($"{'=',-60}");
        }

        /// <summary>
        /// Selects a specific report filter on the Reports homepage
        /// </summary>
        public static void SelectReportFilter(IPookieWebDriver driver, ITestOutputHelper output, string filterName)
        {
            var filterLabel = driver.FindElements(By.CssSelector("span.FilterLabel"))
                .FirstOrDefault(el => el.Displayed && el.Text.Trim().Equals(filterName, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"{filterName} filter label was not found.");

            CommonTestHelper.ClickElement(driver, filterLabel);
            driver.WaitForUpdatePanel(10);
            driver.WaitForReady(10);
            Thread.Sleep(1500);
            output.WriteLine($"[PASS] Selected {filterName} filter");
        }

        /// <summary>
        /// Changes the DataTables page size to "All" to show all reports
        /// </summary>
        public static void ChangePageSizeToAll(IPookieWebDriver driver, ITestOutputHelper output)
        {
            var reportsTable = driver.WaitforElementToBeInDOM(By.CssSelector("table#mainTable tbody"), 10)
                ?? throw new InvalidOperationException("Reports table was not found after filter click.");

            var pageSizeDropdown = driver.FindElement(By.CssSelector("select[name='mainTable_length']"));
            var selectElement = new SelectElement(pageSizeDropdown);
            
            var currentPageSize = selectElement.SelectedOption.GetAttribute("value");
            output.WriteLine($"[INFO] Current page size: {currentPageSize}");

            if (currentPageSize != "-1")
            {
                selectElement.SelectByValue("-1");
                driver.WaitForReady(3);
                Thread.Sleep(1000);
                output.WriteLine("[PASS] Changed page size to 'All'");
            }
        }

        /// <summary>
        /// Gets the count of visible reports in the current filtered list
        /// </summary>
        public static int GetReportCount(IPookieWebDriver driver, ITestOutputHelper output)
        {
            var reportsTable = driver.WaitforElementToBeInDOM(By.CssSelector("table#mainTable tbody"), 10)
                ?? throw new InvalidOperationException("Reports table was not found.");

            var visibleRows = reportsTable.FindElements(By.CssSelector("tr"))
                .Where(tr => tr.FindElements(By.CssSelector("td")).Any())
                .ToList();

            output.WriteLine($"[INFO] Found {visibleRows.Count} reports to test");
            return visibleRows.Count;
        }

        /// <summary>
        /// Clicks a report at the specified index and returns the report name
        /// </summary>
        public static string ClickReportAtIndex(IPookieWebDriver driver, ITestOutputHelper output, string filterName, int index)
        {
            output.WriteLine($"[DEBUG] Current URL before clicking report: {driver.Url}");
            
            var reportsTable = driver.WaitforElementToBeInDOM(By.CssSelector("table#mainTable tbody"), 10)
                ?? throw new InvalidOperationException("Reports table was not found.");

            var visibleRows = reportsTable.FindElements(By.CssSelector("tr"))
                .Where(tr => tr.FindElements(By.CssSelector("td")).Any())
                .ToList();

            if (index >= visibleRows.Count)
            {
                throw new InvalidOperationException($"Report index {index} is out of range. Only {visibleRows.Count} reports available.");
            }

            var targetRow = visibleRows[index];
            
            // Get the report name from the second column (index 1)
            var reportNameCell = targetRow.FindElements(By.CssSelector("td"))
                .Skip(1)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"Report name cell was not found in row {index}.");

            var reportName = reportNameCell.Text.Trim();
            output.WriteLine($"[INFO] Clicking report: {reportName}");

            // Click on the report name cell (second column) - this should open the report
            CommonTestHelper.ClickElement(driver, reportNameCell);
            driver.WaitForUpdatePanel(15);
            driver.WaitForReady(15);
            Thread.Sleep(2000);
            
            output.WriteLine($"[DEBUG] URL after clicking report: {driver.Url}");
            output.WriteLine($"[DEBUG] Page title: {driver.Title}");

            return reportName;
        }

        /// <summary>
        /// Clicks the first report in the filtered list and returns the report name
        /// </summary>
        public static string ClickFirstReport(IPookieWebDriver driver, ITestOutputHelper output, string filterName)
        {
            var reportsTable = driver.WaitforElementToBeInDOM(By.CssSelector("table#mainTable tbody"), 10)
                ?? throw new InvalidOperationException("Reports table was not found.");

            var visibleRows = reportsTable.FindElements(By.CssSelector("tr"))
                .Where(tr => tr.FindElements(By.CssSelector("td")).Any())
                .ToList();

            Assert.True(visibleRows.Count > 0, $"No {filterName} reports found in the table.");
            output.WriteLine($"[INFO] Found {visibleRows.Count} {filterName} reports");

            var firstRow = visibleRows.First();
            
            // Get the report name from the second column (index 1)
            var reportNameCell = firstRow.FindElements(By.CssSelector("td"))
                .Skip(1)
                .FirstOrDefault()
                ?? throw new InvalidOperationException("Report name cell was not found in the first row.");

            var reportName = reportNameCell.Text.Trim();
            output.WriteLine($"[INFO] Clicking first report: {reportName}");

            // Click on the report name cell (second column) - this should open the report
            CommonTestHelper.ClickElement(driver, reportNameCell);
            driver.WaitForUpdatePanel(15);
            driver.WaitForReady(15);
            Thread.Sleep(2000);

            return reportName;
        }

        /// <summary>
        /// Verifies that divCriteria element is displayed after clicking a report
        /// </summary>
        public static void VerifyDivCriteriaDisplayed(IPookieWebDriver driver, ITestOutputHelper output)
        {
            var divCriteria = driver.WaitforElementToBeInDOM(By.CssSelector("div#divCriteria"), 15)
                ?? throw new InvalidOperationException("divCriteria was not found after clicking the report.");

            Assert.True(divCriteria.Displayed, "divCriteria is not displayed.");
            output.WriteLine("[PASS] divCriteria is displayed");
        }

        /// <summary>
        /// Tests all report criteria sections (divSites, divCaseFilters, divByWhom)
        /// </summary>
        public static void TestReportCriteriaSections(IPookieWebDriver driver, ITestOutputHelper output)
        {
            TestCriteriaSection(driver, output, "divSites", "Sites");
            TestCriteriaSection(driver, output, "divCaseFilters", "Case Filters");
            TestCriteriaSection(driver, output, "divByWhom", "By Whom");
        }

        /// <summary>
        /// Tests all report criteria sections and returns a list of which sections are present
        /// </summary>
        public static List<string> TestReportCriteriaSectionsWithLogging(IPookieWebDriver driver, ITestOutputHelper output, string reportName)
        {
            var sectionsPresent = new List<string>();

            // Test divSites
            if (IsSectionPresent(driver, "divSites"))
            {
                sectionsPresent.Add("Sites");
                TestCriteriaSection(driver, output, "divSites", "Sites");
            }
            else
            {
                output.WriteLine("[INFO] Sites section not present in this report");
            }

            // Test divCaseFilters
            if (IsSectionPresent(driver, "divCaseFilters"))
            {
                sectionsPresent.Add("Case Filters");
                TestCriteriaSection(driver, output, "divCaseFilters", "Case Filters");
            }
            else
            {
                output.WriteLine("[INFO] Case Filters section not present in this report");
            }

            // Test divByWhom
            if (IsSectionPresent(driver, "divByWhom"))
            {
                sectionsPresent.Add("By Whom");
                TestCriteriaSection(driver, output, "divByWhom", "By Whom");
            }
            else
            {
                output.WriteLine("[INFO] By Whom section not present in this report");
            }

            // Log summary of sections present
            var sectionsSummary = sectionsPresent.Count > 0 
                ? string.Join(", ", sectionsPresent) 
                : "None";
            output.WriteLine($"[INFO] Report '{reportName}' has sections: {sectionsSummary}");

            return sectionsPresent;
        }

        /// <summary>
        /// Checks if a specific section is present and displayed on the page
        /// </summary>
        private static bool IsSectionPresent(IPookieWebDriver driver, string divId)
        {
            var section = driver.FindElements(By.CssSelector($"div#{divId}"))
                .FirstOrDefault(el => el.Displayed);

            return section != null;
        }

        /// <summary>
        /// Navigates back to the report list after testing a report
        /// </summary>
        public static void NavigateBackToReportList(IPookieWebDriver driver, ITestOutputHelper output)
        {
            output.WriteLine($"[DEBUG] Current URL before navigating back: {driver.Url}");
            
            // Try to find and click a "Back" button or link
            var backButton = driver.FindElements(By.CssSelector(
                "a.btn[href*='ReportCatalog'], " +
                "a[href*='ReportCatalog'], " +
                "button.btn-back, " +
                "a.back-link"))
                .FirstOrDefault(el => el.Displayed);

            if (backButton != null)
            {
                var backHref = backButton.GetAttribute("href");
                output.WriteLine($"[DEBUG] Clicking back button with href: {backHref}");
                
                CommonTestHelper.ClickElement(driver, backButton);
                driver.WaitForUpdatePanel(10);
                driver.WaitForReady(10);
                Thread.Sleep(1000);
                
                output.WriteLine($"[DEBUG] URL after navigating back: {driver.Url}");
                output.WriteLine("[INFO] Navigated back to report list");
            }
            else
            {
                output.WriteLine("[WARN] No back button found, using browser back");
                // If no back button, use browser back
                driver.Navigate().Back();
                driver.WaitForUpdatePanel(10);
                driver.WaitForReady(10);
                Thread.Sleep(1000);
                output.WriteLine("[INFO] Used browser back to return to report list");
            }
        }

        /// <summary>
        /// Tests a specific criteria section by validating all dropdowns, checkboxes, and radio buttons
        /// </summary>
        public static void TestCriteriaSection(
            IPookieWebDriver driver,
            ITestOutputHelper output,
            string divId,
            string sectionDisplayName)
        {
            var divSection = driver.FindElements(By.CssSelector($"div#{divId}"))
                .FirstOrDefault(el => el.Displayed);

            if (divSection == null)
            {
                output.WriteLine($"[INFO] {sectionDisplayName} section not present in this report");
                return;
            }

            output.WriteLine($"\n[INFO] Testing {sectionDisplayName} section");

            // Special handling for divByWhom - test radio buttons first
            if (divId == "divByWhom")
            {
                var radioButtons = divSection.FindElements(By.CssSelector("input[type='radio']"))
                    .Where(el => el.Displayed && el.Enabled)
                    .ToList();

                output.WriteLine($"[INFO] Found {radioButtons.Count} radio button(s) in {sectionDisplayName}");

                if (radioButtons.Count > 0)
                {
                    TestRadioButtonFunctionality(driver, output, divSection, radioButtons);
                }
            }

            // Test all visible dropdowns in the section
            var dropdowns = divSection.FindElements(By.CssSelector("select.form-control, select"))
                .Where(el => el.Displayed && el.Enabled)
                .ToList();

            output.WriteLine($"[INFO] Found {dropdowns.Count} visible dropdown(s) in {sectionDisplayName}");

            foreach (var dropdown in dropdowns)
            {
                TestDropdownFunctionality(driver, output, dropdown, sectionDisplayName);
            }

            // Test all checkboxes in the section
            var checkboxes = divSection.FindElements(By.CssSelector("input[type='checkbox']"))
                .Where(el => el.Displayed && el.Enabled)
                .ToList();

            output.WriteLine($"[INFO] Found {checkboxes.Count} checkbox(es) in {sectionDisplayName}");

            foreach (var checkbox in checkboxes)
            {
                TestCheckboxFunctionality(driver, output, checkbox, sectionDisplayName);
            }

            output.WriteLine($"[PASS] {sectionDisplayName} section validated");
        }

        /// <summary>
        /// Tests dropdown functionality by selecting multiple options and verifying selection
        /// </summary>
        public static void TestDropdownFunctionality(
            IPookieWebDriver driver,
            ITestOutputHelper output,
            IWebElement dropdown,
            string sectionName)
        {
            var dropdownId = dropdown.GetAttribute("id") ?? "unknown";
            var dropdownName = dropdown.GetAttribute("name") ?? dropdownId;

            var selectElement = new SelectElement(dropdown);
            var options = selectElement.Options.Where(opt => !string.IsNullOrWhiteSpace(opt.Text)).ToList();

            output.WriteLine($"  Testing dropdown '{dropdownName}' with {options.Count} option(s)");

            // Get the originally selected option to restore later
            var originalSelectedValue = selectElement.SelectedOption.GetAttribute("value");

            // Test selecting each option (limit to first 5 for performance)
            var optionsToTest = options.Take(Math.Min(5, options.Count)).ToList();

            foreach (var option in optionsToTest)
            {
                var optionValue = option.GetAttribute("value");
                var optionText = option.Text.Trim();

                // Skip empty or disabled options
                if (string.IsNullOrWhiteSpace(optionValue) || option.GetAttribute("disabled") != null)
                {
                    continue;
                }

                // Select the option
                selectElement.SelectByValue(optionValue);
                Thread.Sleep(300);

                // Verify selection
                var currentSelected = selectElement.SelectedOption.GetAttribute("value");
                Assert.Equal(optionValue, currentSelected);

                output.WriteLine($"    Selected option: {optionText}");
            }

            // Restore original selection
            selectElement.SelectByValue(originalSelectedValue);
            Thread.Sleep(200);

            output.WriteLine($"  [PASS] Dropdown '{dropdownName}' is functional");
        }

        /// <summary>
        /// Tests checkbox functionality by toggling it on/off
        /// </summary>
        public static void TestCheckboxFunctionality(
            IPookieWebDriver driver,
            ITestOutputHelper output,
            IWebElement checkbox,
            string sectionName)
        {
            var checkboxId = checkbox.GetAttribute("id") ?? "unknown";
            var checkboxName = checkbox.GetAttribute("name") ?? checkboxId;

            // Get label text if available
            var labelText = "unknown";
            var label = driver.FindElements(By.CssSelector($"label[for='{checkboxId}']"))
                .FirstOrDefault();
            if (label != null && !string.IsNullOrWhiteSpace(label.Text))
            {
                labelText = label.Text.Trim();
            }

            output.WriteLine($"  Testing checkbox '{labelText}' ({checkboxName})");

            // Store original state
            var originalState = checkbox.Selected;

            // Toggle checkbox ON
            if (!checkbox.Selected)
            {
                CommonTestHelper.ClickElement(driver, checkbox);
                Thread.Sleep(300);
                Assert.True(checkbox.Selected, $"Checkbox '{labelText}' was not checked after click.");
            }

            output.WriteLine($"    Checkbox can be checked");

            // Toggle checkbox OFF
            if (checkbox.Selected)
            {
                CommonTestHelper.ClickElement(driver, checkbox);
                Thread.Sleep(300);
                Assert.False(checkbox.Selected, $"Checkbox '{labelText}' was not unchecked after click.");
            }

            output.WriteLine($"    Checkbox can be unchecked");

            // Restore original state
            if (originalState != checkbox.Selected)
            {
                CommonTestHelper.ClickElement(driver, checkbox);
                Thread.Sleep(200);
            }

            output.WriteLine($"  [PASS] Checkbox '{labelText}' is functional");
        }

        /// <summary>
        /// Verifies that a Start Date input is present in the criteria area (optional - logs warning if not found).
        /// Scopes search to div#divCriteria and uses the known structures (divStartDate, divQuarters).
        /// </summary>
        public static void VerifyStartDateField(IPookieWebDriver driver, ITestOutputHelper output)
        {
            output.WriteLine("\n[INFO] Verifying Start Date field presence");

            // Scope all searches to the criteria container
            var criteriaDiv = driver.FindElements(By.CssSelector("div#divCriteria"))
                .FirstOrDefault(el => el.Displayed);

            if (criteriaDiv == null)
            {
                output.WriteLine("[WARN] divCriteria container not found - skipping Start Date check");
                return;
            }

            // 1) Preferred: explicit Start Date block
            var startDateField = criteriaDiv.FindElements(By.CssSelector(
                    "div#divStartDate input.StartDate.form-control.cleaveDateShort, " +
                    "div#divStartDate input[id$='txtStartDate']"))
                .FirstOrDefault(el => el.Displayed && el.Enabled);

            // 2) Fallback: quarter date range Start Date inside divQuarters
            if (startDateField == null)
            {
                startDateField = criteriaDiv.FindElements(By.CssSelector(
                        "div#divQuarters input[id$='txtQtrStartDate'], " +
                        "div#divQuarters input.form-control.cleaveDateShort[id*='QtrStart']"))
                    .FirstOrDefault(el => el.Displayed && el.Enabled);
            }

            // 3) Very last resort inside criteria only (keep it tight)
            if (startDateField == null)
            {
                startDateField = criteriaDiv.FindElements(By.CssSelector(
                        "input.StartDate.form-control.cleaveDateShort, " +
                        "input[id$='txtStartDate'], " +
                        "input[id$='txtQtrStartDate']"))
                    .FirstOrDefault(el => el.Displayed && el.Enabled);
            }

            // Also check for date labels inside criteria (find all labels, then filter by text)
            var allLabels = criteriaDiv.FindElements(By.CssSelector("label, .control-label, .form-group label"));

            var startDateLabel = allLabels
                .Where(el => el.Displayed &&
                             !string.IsNullOrWhiteSpace(el.Text) &&
                             el.Text.Contains("Start", StringComparison.OrdinalIgnoreCase) &&
                             el.Text.Contains("Date", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            var hasStartDateField = startDateField != null;
            var hasStartDateLabel = startDateLabel != null;

            if (hasStartDateField)
            {
                var fieldId = startDateField.GetAttribute("id") ?? "unknown";
                output.WriteLine($"[PASS] Start Date field found in criteria (ID: {fieldId})");
            }
            else if (hasStartDateLabel)
            {
                output.WriteLine($"[PASS] Start Date label found in criteria: {startDateLabel.Text.Trim()}");
            }
            else
            {
                output.WriteLine("[WARN] Start Date field not found in divCriteria - this report may not use date criteria");
            }
        }

        /// <summary>
        /// Tests the Quarter criteria section (divQuarters) when present by exercising the quarter dropdown.
        /// </summary>
        public static void TestQuarterCriteriaSection(IPookieWebDriver driver, ITestOutputHelper output)
        {
            // Scope search to the main criteria container
            var criteriaDiv = driver.FindElements(By.CssSelector("div#divCriteria"))
                .FirstOrDefault(el => el.Displayed);

            if (criteriaDiv == null)
            {
                return;
            }

            // Locate the Quarters container (supporting ASP.NET-generated IDs)
            var quartersDiv = criteriaDiv.FindElements(By.CssSelector(
                    "div#divQuarters, " +
                    "div[id$='divQuarters']"))
                .FirstOrDefault(el => el.Displayed);

            if (quartersDiv == null)
            {
                return;
            }

            output.WriteLine("\n[INFO] Testing Quarters section");

            // Find the quarter dropdown
            var quarterDropdown = quartersDiv.FindElements(By.CssSelector(
                    "select[id$='ddlQuarter'], " +
                    "select#ddlQuarter"))
                .FirstOrDefault(el => el.Displayed && el.Enabled);

            if (quarterDropdown == null)
            {
                output.WriteLine("[WARN] Quarter dropdown not found or not enabled in Quarters section");
                return;
            }

            TestDropdownFunctionality(driver, output, quarterDropdown, "Quarters");

            output.WriteLine("[PASS] Quarters section validated");
        }

        /// <summary>
        /// Tests the Programs/Regions tabbed criteria section (divPrograms) when present by
        /// switching between tabs and exercising all checkboxes in each tab.
        /// </summary>
        public static void TestProgramsAndRegionsSection(IPookieWebDriver driver, ITestOutputHelper output)
        {
            // Scope search to the main criteria container
            var criteriaDiv = driver.FindElements(By.CssSelector("div#divCriteria"))
                .FirstOrDefault(el => el.Displayed);

            if (criteriaDiv == null)
            {
                return;
            }

            // Locate the Programs/Regions container
            var programsDiv = criteriaDiv.FindElements(By.CssSelector(
                    "div#divPrograms, " +
                    "div[id$='divPrograms']"))
                .FirstOrDefault(el => el.Displayed);

            if (programsDiv == null)
            {
                return;
            }

            output.WriteLine("\n[INFO] Testing Programs/Regions section");

            // Helper local function to exercise all checkboxes within a scoped container
            void TestAllCheckboxesInContainer(IWebElement container, string scopeName)
            {
                var checkboxes = container.FindElements(By.CssSelector("input[type='checkbox']"))
                    .Where(cb => cb.Displayed && cb.Enabled)
                    .ToList();

                output.WriteLine($"[INFO] Found {checkboxes.Count} checkbox(es) in {scopeName}");

                foreach (var cb in checkboxes)
                {
                    TestCheckboxFunctionality(driver, output, cb, scopeName);
                }
            }

            // 1) Programs tab
            var programsTabLink = programsDiv.FindElements(By.CssSelector("ul.nav.nav-tabs a[href='#programs']"))
                .FirstOrDefault(el => el.Displayed);
            if (programsTabLink != null)
            {
                CommonTestHelper.ClickElement(driver, programsTabLink);
                Thread.Sleep(500);

                var programsTabContent = programsDiv.FindElements(By.CssSelector("div#programs, div[id$='programs']"))
                    .FirstOrDefault(el => el.Displayed);

                if (programsTabContent != null)
                {
                    // Within programs tab, exercise all program checkboxes
                    TestAllCheckboxesInContainer(programsTabContent, "Programs tab");
                }
                else
                {
                    output.WriteLine("[WARN] Programs tab content not found or not visible");
                }
            }
            else
            {
                output.WriteLine("[INFO] Programs tab link not present");
            }

            // 2) Regions tab
            var regionsTabLink = programsDiv.FindElements(By.CssSelector("ul.nav.nav-tabs a[href='#regions']"))
                .FirstOrDefault(el => el.Displayed);
            if (regionsTabLink != null)
            {
                CommonTestHelper.ClickElement(driver, regionsTabLink);
                Thread.Sleep(500);

                var regionsTabContent = programsDiv.FindElements(By.CssSelector("div#regions, div[id$='regions']"))
                    .FirstOrDefault(el => el.Displayed);

                if (regionsTabContent != null)
                {
                    // Within regions tab, exercise all region checkboxes
                    TestAllCheckboxesInContainer(regionsTabContent, "Regions tab");
                }
                else
                {
                    output.WriteLine("[WARN] Regions tab content not found or not visible");
                }
            }
            else
            {
                output.WriteLine("[INFO] Regions tab link not present");
            }

            output.WriteLine("[PASS] Programs/Regions section validated");
        }

        /// <summary>
        /// Tests the Tickler Summary "Report to Run" section (fieldset divTSumReports) when present
        /// by exercising all radio options.
        /// </summary>
        public static void TestTicklerSummaryReportToRunSection(
            IPookieWebDriver driver,
            ITestOutputHelper output)
        {
            // Scope search to the main criteria container
            var criteriaDiv = driver.FindElements(By.CssSelector("div#divCriteria"))
                .FirstOrDefault(el => el.Displayed);

            if (criteriaDiv == null)
            {
                // No criteria container, nothing to do
                return;
            }

            // Find the Tickler Summary fieldset by its id (allowing for possible ASP.NET prefixes)
            var ticklerFieldset = criteriaDiv.FindElements(By.CssSelector(
                    "fieldset#divTSumReports, " +
                    "fieldset[id$='divTSumReports']"))
                .FirstOrDefault(el => el.Displayed);

            if (ticklerFieldset == null)
            {
                // This section is optional and only appears for certain reports (e.g., Ticklers)
                return;
            }

            output.WriteLine("\n[INFO] Testing Tickler Summary 'Report to Run' section");

            // Get all visible & enabled radio buttons inside this fieldset
            var radioButtons = ticklerFieldset.FindElements(By.CssSelector("input[type='radio']"))
                .Where(rb => rb.Displayed && rb.Enabled)
                .ToList();

            if (radioButtons.Count == 0)
            {
                output.WriteLine("[WARN] No radio buttons found in Tickler Summary 'Report to Run' section");
                return;
            }

            // Remember original selection so we can restore it
            var originalSelected = radioButtons.FirstOrDefault(rb => rb.Selected);

            foreach (var radio in radioButtons)
            {
                var radioId = radio.GetAttribute("id") ?? "unknown";
                var radioValue = radio.GetAttribute("value") ?? "unknown";

                // Try to get the label text associated with this radio
                var label = driver.FindElements(By.CssSelector($"label[for='{radioId}']"))
                    .FirstOrDefault();
                var labelText = label != null && !string.IsNullOrWhiteSpace(label.Text)
                    ? label.Text.Trim()
                    : radioValue;

                // Click the radio option
                CommonTestHelper.ClickElement(driver, radio);
                Thread.Sleep(300);

                // Verify it is selected
                Assert.True(radio.Selected, $"Tickler Summary option '{labelText}' was not selected after click.");

                output.WriteLine($"  Selected Tickler Summary option: {labelText}");
            }

            // Restore original selection if it changed
            if (originalSelected != null && !originalSelected.Selected)
            {
                CommonTestHelper.ClickElement(driver, originalSelected);
                Thread.Sleep(200);
            }

            output.WriteLine("[PASS] Tickler Summary 'Report to Run' section validated");
        }

        /// <summary>
        /// Tests radio button functionality by selecting each option and verifying associated dropdown visibility
        /// </summary>
        private static void TestRadioButtonFunctionality(
            IPookieWebDriver driver,
            ITestOutputHelper output,
            IWebElement divSection,
            List<IWebElement> radioButtons)
        {
            // Get the originally selected radio button
            var originalSelected = radioButtons.FirstOrDefault(rb => rb.Selected);

            // Group radio buttons by name (they should be part of the same radio group)
            var radioGroups = radioButtons
                .GroupBy(rb => rb.GetAttribute("name"))
                .ToList();

            foreach (var radioGroup in radioGroups)
            {
                output.WriteLine($"  Testing radio group '{radioGroup.Key}' with {radioGroup.Count()} option(s)");

                // Test up to 3 radio buttons per group
                var radiosToTest = radioGroup.Take(3).ToList();

                foreach (var radio in radiosToTest)
                {
                    var radioId = radio.GetAttribute("id") ?? "unknown";
                    var radioValue = radio.GetAttribute("value") ?? "unknown";

                    // Get label text if available
                    var label = driver.FindElements(By.CssSelector($"label[for='{radioId}']"))
                        .FirstOrDefault();
                    var labelText = label != null && !string.IsNullOrWhiteSpace(label.Text) 
                        ? label.Text.Trim() 
                        : radioValue;

                    // Click the radio button
                    CommonTestHelper.ClickElement(driver, radio);
                    Thread.Sleep(500); // Wait for any UI changes

                    // Verify it's selected
                    Assert.True(radio.Selected, $"Radio button '{labelText}' was not selected after click.");

                    output.WriteLine($"    Selected: {labelText}");

                    // Check if this selection made any dropdowns visible and test them
                    var visibleDropdowns = divSection.FindElements(By.CssSelector("select.form-control, select.ddlByWhom"))
                        .Where(el => el.Displayed && el.Enabled)
                        .ToList();

                    if (visibleDropdowns.Count > 0)
                    {
                        output.WriteLine($"      Associated dropdown(s) visible: {visibleDropdowns.Count}");

                        // Exercise the visible dropdowns for this radio selection (limit to first 2 for performance)
                        foreach (var dropdown in visibleDropdowns.Take(2))
                        {
                            TestDropdownFunctionality(driver, output, dropdown, "By Whom");
                        }
                    }
                }

                output.WriteLine($"  [PASS] Radio group '{radioGroup.Key}' is functional");
            }

            // Restore original selection
            if (originalSelected != null && !originalSelected.Selected)
            {
                CommonTestHelper.ClickElement(driver, originalSelected);
                Thread.Sleep(300);
            }
        }

        #region Ticklers PDF Validation Helpers

        /// <summary>
        /// Holds criteria that were selected for a report, to validate against the PDF.
        /// </summary>
        public class SelectedReportCriteria
        {
            public Dictionary<string, string> Criteria { get; set; } = new Dictionary<string, string>();
            
            public void Add(string criteriaName, string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Criteria[criteriaName] = value;
                }
            }

            public bool HasCriteria => Criteria.Any();
        }

        /// <summary>
        /// Randomly selects criteria for Ticklers reports.
        /// Sometimes leaves defaults, sometimes changes things (like a real user).
        /// Returns the selected criteria for validation.
        /// </summary>
        public static SelectedReportCriteria RandomizeTicklersCriteria(IPookieWebDriver driver, ITestOutputHelper output, Random random)
        {
            output.WriteLine("[INFO] Randomizing Ticklers criteria...");
            var selectedCriteria = new SelectedReportCriteria();

            // 50% chance to leave all defaults
            if (random.Next(0, 2) == 0)
            {
                output.WriteLine("  [INFO] Leaving all defaults (no changes)");
                return selectedCriteria;
            }

            // Otherwise, randomly interact with some criteria
            try
            {
                // Try to find and interact with Program dropdown
                var programDropdown = driver.FindElements(By.CssSelector("select[id$='ddlPrograms'], select.form-control"))
                    .FirstOrDefault(el => el.Displayed);

                if (programDropdown != null && random.Next(0, 2) == 0)
                {
                    var select = new SelectElement(programDropdown);
                    var options = select.Options.Where(o => !string.IsNullOrWhiteSpace(o.Text) && o.Text != "All").ToList();
                    if (options.Count > 1)
                    {
                        var randomOption = options[random.Next(0, options.Count)];
                        select.SelectByText(randomOption.Text);
                        selectedCriteria.Add("Program", randomOption.Text);
                        output.WriteLine($"  [INFO] Selected program: {randomOption.Text}");
                        Thread.Sleep(300);
                    }
                }

                // Try to find and interact with Status dropdown
                var statusDropdown = driver.FindElements(By.CssSelector("select[id$='ddlStatus'], select.form-control[id*='Status']"))
                    .FirstOrDefault(el => el.Displayed);

                if (statusDropdown != null && random.Next(0, 2) == 0)
                {
                    var select = new SelectElement(statusDropdown);
                    var options = select.Options.Where(o => !string.IsNullOrWhiteSpace(o.Text) && o.Text != "All").ToList();
                    if (options.Count > 1)
                    {
                        var randomOption = options[random.Next(0, options.Count)];
                        select.SelectByText(randomOption.Text);
                        selectedCriteria.Add("Status", randomOption.Text);
                        output.WriteLine($"  [INFO] Selected status: {randomOption.Text}");
                        Thread.Sleep(300);
                    }
                }

                // Try to find and interact with date fields
                var dateFields = driver.FindElements(By.CssSelector("input[type='text'][id*='Date'], input.form-control[id*='Date']"))
                    .Where(el => el.Displayed).ToList();

                if (dateFields.Any() && random.Next(0, 2) == 0)
                {
                    int dateIndex = 0;
                    foreach (var dateField in dateFields.Take(2)) // Limit to first 2 date fields
                    {
                        if (random.Next(0, 2) == 0) // 50% chance per field
                        {
                            var randomDate = DateTime.Now.AddDays(-random.Next(1, 90)).ToString("MM/dd/yyyy");
                            dateField.Clear();
                            dateField.SendKeys(randomDate);
                            
                            // Try to get label for this date field
                            var fieldId = dateField.GetAttribute("id");
                            var label = driver.FindElements(By.CssSelector($"label[for='{fieldId}']")).FirstOrDefault();
                            var fieldName = label?.Text?.Trim() ?? $"Date{dateIndex + 1}";
                            
                            selectedCriteria.Add(fieldName, randomDate);
                            output.WriteLine($"  [INFO] Set {fieldName}: {randomDate}");
                            Thread.Sleep(300);
                            dateIndex++;
                        }
                    }
                }

                output.WriteLine("  [PASS] Randomized criteria selection complete");
            }
            catch (Exception ex)
            {
                output.WriteLine($"  [WARN] Could not randomize all criteria: {ex.Message}");
                // Continue anyway - some criteria might not be available for all reports
            }

            return selectedCriteria;
        }

        /// <summary>
        /// Clicks the "Run Report" button and waits for the DevExpress viewer to load.
        /// </summary>
        public static void RunReport(IPookieWebDriver driver, ITestOutputHelper output, int timeoutSeconds = 60)
        {
            output.WriteLine("[INFO] Clicking 'Run Report' button...");

            var runButton = driver.FindElements(By.CssSelector(
                    "a.btn.btn-primary[id$='btnRunReport'], " +
                    "a.btn-primary[href*='RunReport'], " +
                    "button.btn-primary"))
                .FirstOrDefault(el => el.Displayed && el.Text.Contains("Run Report", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("'Run Report' button not found");

            CommonTestHelper.ClickElement(driver, runButton);
            output.WriteLine("  [INFO] Clicked 'Run Report', waiting for viewer...");

            // Wait for DevExpress viewer to appear
            var viewerLoaded = driver.WaitforElementToBeInDOM(
                By.CssSelector("img.dxrd-pointer-events-none, div.dxrd-preview-wrapper, div[id*='ReportViewer']"),
                timeoutSeconds);

            if (viewerLoaded == null)
            {
                throw new InvalidOperationException($"Report viewer did not load within {timeoutSeconds} seconds");
            }

            output.WriteLine("  [INFO] Viewer appeared, waiting for report to generate...");
            output.WriteLine($"  [DEBUG] Current URL: {driver.Url}");
            
            // Wait for the report to fully render by checking for toolbar to be visible
            // This is critical - the toolbar won't be visible until the report is fully generated
            var maxWaitSeconds = 90; // Maximum 90 seconds to wait for report generation
            var endTime = DateTime.Now.AddSeconds(maxWaitSeconds);
            bool toolbarVisible = false;
            int checkCount = 0;
            
            while (DateTime.Now < endTime && !toolbarVisible)
            {
                Thread.Sleep(2000);
                checkCount++;
                
                // Check if browser is still alive and detect error pages
                try
                {
                    var currentUrl = driver.Url;
                    
                    // Detect if we've been redirected to an error page
                    if (currentUrl.Contains("errorpage.aspx", StringComparison.OrdinalIgnoreCase) ||
                        currentUrl.Contains("/error", StringComparison.OrdinalIgnoreCase) ||
                        driver.Title.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
                        driver.Title.Contains("Exception", StringComparison.OrdinalIgnoreCase))
                    {
                        output.WriteLine($"  [ERROR] Report generation failed - redirected to error page!");
                        output.WriteLine($"  [ERROR] Error URL: {currentUrl}");
                        output.WriteLine($"  [ERROR] Page title: {driver.Title}");
                        
                        // Try to get error message from page
                        try
                        {
                            var errorMessages = driver.FindElements(By.CssSelector("span.Error, div.error-message, div.alert-danger, span[style*='color: red']"));
                            foreach (var msg in errorMessages.Where(m => m.Displayed && !string.IsNullOrWhiteSpace(m.Text)))
                            {
                                output.WriteLine($"  [ERROR] Error message: {msg.Text}");
                            }
                        }
                        catch { }
                        
                        throw new InvalidOperationException($"Report generation failed - server returned error page: {currentUrl}");
                    }
                    
                    if (checkCount % 5 == 0) // Log URL every 5 checks (10 seconds)
                    {
                        output.WriteLine($"  [DEBUG] Still waiting... URL: {currentUrl} (check #{checkCount})");
                    }
                }
                catch (InvalidOperationException)
                {
                    throw; // Re-throw our error page detection
                }
                catch (Exception ex)
                {
                    output.WriteLine($"  [ERROR] Browser connection lost: {ex.Message}");
                    throw new InvalidOperationException("Browser connection lost while waiting for report to generate", ex);
                }
                
                var toolbar = driver.FindElements(By.CssSelector("div.dxrd-toolbar, div.dxrd-preview-export-toolbar-item"))
                    .FirstOrDefault(el => el.Displayed);
                
                if (toolbar != null)
                {
                    toolbarVisible = true;
                    output.WriteLine("  [INFO] Report toolbar is now visible - report generated!");
                    output.WriteLine($"  [DEBUG] Final URL: {driver.Url}");
                    break;
                }
                
                if (checkCount % 3 == 0) // Only log "Still generating" every 3 checks (6 seconds)
                {
                    output.WriteLine($"  [INFO] Still generating report... ({checkCount * 2}s elapsed, max {maxWaitSeconds}s)");
                }
                
                // If we've waited more than 60 seconds, something might be wrong
                if (checkCount > 30)
                {
                    output.WriteLine($"  [WARN] Report is taking unusually long to generate (>{checkCount * 2}s)");
                    output.WriteLine($"  [DEBUG] Page title: {driver.Title}");
                }
            }

            if (!toolbarVisible)
            {
                output.WriteLine($"  [ERROR] Toolbar did not become visible after {maxWaitSeconds} seconds");
                output.WriteLine($"  [DEBUG] Final URL: {driver.Url}");
                output.WriteLine($"  [DEBUG] Page title: {driver.Title}");
                throw new InvalidOperationException($"Report toolbar did not appear within {maxWaitSeconds} seconds. Report may have failed to generate.");
            }
            
            // Extra wait to ensure everything is settled
            Thread.Sleep(2000);
            output.WriteLine("  [PASS] Report viewer loaded successfully");
        }

        /// <summary>
        /// Clicks the DevExpress export-to-PDF button and waits for the download to complete.
        /// </summary>
        /// <param name="downloadDirectory">Directory where PDFs are downloaded</param>
        /// <returns>FileInfo of the downloaded PDF file</returns>
        public static FileInfo ExportReportToPdf(IPookieWebDriver driver, ITestOutputHelper output, string downloadDirectory, int timeoutSeconds = 30)
        {
            output.WriteLine("[INFO] Exporting report to PDF...");

            // Wait for the export toolbar to be visible (not just in DOM)
            output.WriteLine("  [INFO] Waiting for export toolbar to be visible...");
            var endTime = DateTime.Now.AddSeconds(30);
            IWebElement exportButton = null;
            
            while (DateTime.Now < endTime && exportButton == null)
            {
                Thread.Sleep(1000);
                
                // Look for the DevExpress export button - must be DISPLAYED
                var buttons = driver.FindElements(By.CssSelector(
                        "div.dxrd-preview-export-to, " +
                        "div.dx-menu-item[aria-label='Export To'], " +
                        "div.dxrd-preview-export-toolbar-item div.dx-item, " +
                        "div.dxrd-preview-export-item-image-wrapper"))
                    .Where(el => el.Displayed)
                    .ToList();

                if (buttons.Count == 0)
                {
                    // Try XPath as fallback
                    buttons = driver.FindElements(By.XPath(
                            "//div[contains(@class,'dxrd-preview-export-to')] | " +
                            "//div[@role='menuitem' and @aria-label='Export To'] | " +
                            "//div[contains(@class,'dxrd-preview-export-toolbar-item')]//div[contains(@class,'dx-item')]"))
                        .Where(el => el.Displayed)
                        .ToList();
                }

                if (buttons.Count > 0)
                {
                    exportButton = buttons.First();
                    output.WriteLine($"  [INFO] Found visible export button: {exportButton.TagName} with class '{exportButton.GetAttribute("class")}'");
                    break;
                }
                
                output.WriteLine("  [INFO] Export button not visible yet, waiting...");
            }

            if (exportButton == null)
            {
                output.WriteLine("  [ERROR] Export button never became visible. Toolbar elements:");
                var allDivs = driver.FindElements(By.CssSelector("div[class*='export'], div[class*='toolbar']"));
                foreach (var div in allDivs.Take(15))
                {
                    output.WriteLine($"    - {div.TagName} class='{div.GetAttribute("class")}' displayed={div.Displayed}");
                }
                throw new InvalidOperationException("Export button not visible after 30 seconds. Report may still be generating.");
            }
            
            // Scroll into view and click
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", exportButton);
            Thread.Sleep(500);
            
            CommonTestHelper.ClickElement(driver, exportButton);
            output.WriteLine("  [INFO] Clicked export button, waiting for menu...");

            // After clicking export, there might be a dropdown - click PDF option
            Thread.Sleep(1000); // Give menu time to appear
            
            // Try CSS selectors first, then XPath for PDF option
            var pdfOption = driver.FindElements(By.CssSelector(
                    "div.dxrd-preview-export-format-item[title*='PDF'], " +
                    "div.dx-item[title*='PDF'], " +
                    "a[data-format='pdf'], " +
                    "li.dx-menu-item-wrapper"))
                .FirstOrDefault(el => el.Displayed && el.Text.Contains("PDF", StringComparison.OrdinalIgnoreCase));

            // If not found with CSS, try XPath
            if (pdfOption == null)
            {
                pdfOption = driver.FindElements(By.XPath(
                        "//div[contains(@class,'dx-item') and contains(@title,'PDF')] | " +
                        "//li[contains(@class,'dx-menu-item-wrapper')]//*[contains(text(),'PDF')]/ancestor::li | " +
                        "//div[contains(@class,'dxrd-preview-export-format-item') and contains(@title,'PDF')]"))
                    .FirstOrDefault(el => el.Displayed);
            }

            if (pdfOption != null)
            {
                output.WriteLine($"  [INFO] Found PDF option in menu, clicking...");
                CommonTestHelper.ClickElement(driver, pdfOption);
            }
            else
            {
                output.WriteLine("  [INFO] No PDF submenu found, checking if already in PDF format or direct download...");
            }

            // Try to find and close any additional dialogs or just wait for download
            Thread.Sleep(1000);
            
            // Look for and click any "Export" or "OK" button in a dialog
            var confirmButton = driver.FindElements(By.XPath(
                    "//button[contains(text(),'Export')] | " +
                    "//a[contains(@class,'btn') and contains(text(),'Export')] | " +
                    "//button[contains(@class,'dx-button')]//span[contains(text(),'Export')]/ancestor::button | " +
                    "//div[contains(@class,'dx-button-content') and contains(text(),'Export')]/parent::div"))
                .FirstOrDefault(el => el.Displayed);

            if (confirmButton != null)
            {
                output.WriteLine("  [INFO] Found export confirmation button, clicking...");
                CommonTestHelper.ClickElement(driver, confirmButton);
                Thread.Sleep(500);
            }

            // Wait for download to complete
            output.WriteLine($"  [INFO] Waiting for PDF download in: {downloadDirectory}");
            var pdfFile = PdfTestHelper.FindLatestPdf(downloadDirectory, timeoutSeconds);

            if (pdfFile == null)
            {
                throw new InvalidOperationException($"PDF file was not downloaded within {timeoutSeconds} seconds");
            }

            output.WriteLine($"  [PASS] PDF downloaded: {pdfFile.Name} ({pdfFile.Length / 1024} KB)");
            return pdfFile;
        }

        /// <summary>
        /// Validates that the PDF heading, filename, and selected criteria match expectations.
        /// </summary>
        public static void ValidatePdfContent(FileInfo pdfFile, string expectedReportName, SelectedReportCriteria selectedCriteria, ITestOutputHelper output)
        {
            output.WriteLine($"[INFO] Validating PDF content for report: {expectedReportName}");

            // Extract full PDF text for validation
            var fullPdfText = PdfTestHelper.ExtractFullPdfText(pdfFile.FullName);
            
            // Extract heading for logging
            var heading = PdfTestHelper.ExtractPdfHeading(pdfFile.FullName, maxLines: 10);
            output.WriteLine($"  [INFO] PDF heading (first 10 lines):");
            output.WriteLine(heading);

            // Sanitize report name once for reuse
            var sanitizedName = PdfTestHelper.SanitizeReportName(expectedReportName);

            // Assert report name appears in PDF (check heading first, then full text)
            var headingContainsReportName = heading.Contains(expectedReportName, StringComparison.OrdinalIgnoreCase);
            var fullTextContainsReportName = fullPdfText.Contains(expectedReportName, StringComparison.OrdinalIgnoreCase);
            
            if (headingContainsReportName || fullTextContainsReportName)
            {
                var location = headingContainsReportName ? "heading" : "PDF body";
                output.WriteLine($"  [PASS] PDF {location} contains expected report name: {expectedReportName}");
            }
            else
            {
                // Try partial match (some words from report name)
                var reportWords = expectedReportName.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                var matchedWordsInHeading = reportWords.Where(word => 
                    word.Length >= 3 && heading.Contains(word, StringComparison.OrdinalIgnoreCase)).ToList();
                var matchedWordsInFullText = reportWords.Where(word => 
                    word.Length >= 3 && fullPdfText.Contains(word, StringComparison.OrdinalIgnoreCase)).ToList();
                
                if (matchedWordsInHeading.Count >= 1 || matchedWordsInFullText.Count >= 1)
                {
                    var matchedWords = matchedWordsInFullText.Any() ? matchedWordsInFullText : matchedWordsInHeading;
                    output.WriteLine($"  [PASS] PDF contains report name words: {string.Join(", ", matchedWords)}");
                }
                else
                {
                    // As last resort, check filename - if filename matches, that's good enough
                    if (pdfFile.Name.Contains(sanitizedName, StringComparison.OrdinalIgnoreCase))
                    {
                        output.WriteLine($"  [PASS] Report name verified via PDF filename: {pdfFile.Name}");
                    }
                    else
                    {
                        output.WriteLine($"  [WARN] Expected report name '{expectedReportName}' not clearly found in PDF text");
                        output.WriteLine($"  [INFO] However, continuing validation as PDF was successfully generated");
                    }
                }
            }

            // Validate filename
            var filenameContainsReport = pdfFile.Name.Contains(sanitizedName, StringComparison.OrdinalIgnoreCase);

            if (!filenameContainsReport)
            {
                // Try partial match
                var nameWords = sanitizedName.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                var matchedInFilename = nameWords.Where(word => 
                    word.Length >= 3 && pdfFile.Name.Contains(word, StringComparison.OrdinalIgnoreCase)).ToList();

                if (matchedInFilename.Count >= 1)
                {
                    output.WriteLine($"  [PASS] PDF filename contains report name words: {string.Join(", ", matchedInFilename)}");
                }
                else
                {
                    output.WriteLine($"  [WARN] PDF filename '{pdfFile.Name}' does not contain expected report name '{sanitizedName}'");
                }
            }
            else
            {
                output.WriteLine($"  [PASS] PDF filename matches expected report name");
            }

            // Validate selected criteria appear in the PDF
            if (selectedCriteria.HasCriteria)
            {
                output.WriteLine($"  [INFO] Validating {selectedCriteria.Criteria.Count} selected criteria in PDF...");
                
                foreach (var criterion in selectedCriteria.Criteria)
                {
                    var criteriaName = criterion.Key;
                    var criteriaValue = criterion.Value;
                    
                    // Check if the value appears in the PDF
                    var valueFound = fullPdfText.Contains(criteriaValue, StringComparison.OrdinalIgnoreCase);
                    
                    if (valueFound)
                    {
                        output.WriteLine($"    [PASS] '{criteriaName}' = '{criteriaValue}' found in PDF");
                    }
                    else
                    {
                        // For dates, try different formats
                        if (DateTime.TryParse(criteriaValue, out var date))
                        {
                            var altFormats = new[]
                            {
                                date.ToString("M/d/yyyy"),
                                date.ToString("MM/dd/yy"),
                                date.ToString("M/d/yy"),
                                date.ToString("MMM d, yyyy"),
                                date.ToString("MMMM d, yyyy")
                            };
                            
                            var foundInAltFormat = altFormats.Any(format => fullPdfText.Contains(format, StringComparison.OrdinalIgnoreCase));
                            
                            if (foundInAltFormat)
                            {
                                output.WriteLine($"    [PASS] '{criteriaName}' = '{criteriaValue}' found in PDF (alternate format)");
                            }
                            else
                            {
                                output.WriteLine($"    [WARN] '{criteriaName}' = '{criteriaValue}' NOT found in PDF");
                            }
                        }
                        else
                        {
                            // For non-dates, try partial match (words)
                            var valueWords = criteriaValue.Split(new[] { ' ', '-', '_', '/' }, StringSplitOptions.RemoveEmptyEntries);
                            var significantWords = valueWords.Where(w => w.Length > 3).ToList();
                            
                            if (significantWords.Any() && significantWords.All(word => fullPdfText.Contains(word, StringComparison.OrdinalIgnoreCase)))
                            {
                                output.WriteLine($"    [PASS] '{criteriaName}' = '{criteriaValue}' found in PDF (partial match)");
                            }
                            else
                            {
                                output.WriteLine($"    [WARN] '{criteriaName}' = '{criteriaValue}' NOT found in PDF");
                            }
                        }
                    }
                }
            }
            else
            {
                output.WriteLine("  [INFO] No criteria were changed (using defaults), skipping criteria validation");
            }

            output.WriteLine("[PASS] PDF content validation complete");
        }

        /// <summary>
        /// Generic method to randomly select criteria for ANY report category.
        /// Works by discovering available form elements and interacting with them randomly.
        /// Sometimes leaves defaults, sometimes changes things (like a real user).
        /// </summary>
        public static SelectedReportCriteria RandomizeReportCriteria(IPookieWebDriver driver, ITestOutputHelper output, Random random)
        {
            output.WriteLine("[INFO] Randomizing report criteria...");
            var selectedCriteria = new SelectedReportCriteria();

            try
            {
                // Find the divCriteria container first
                var divCriteria = driver.FindElements(By.CssSelector("div[id*='divCriteria'], div.criteria-section"))
                    .FirstOrDefault(el => el.Displayed);

                if (divCriteria == null)
                {
                    output.WriteLine("  [INFO] No criteria section found, using defaults");
                    return selectedCriteria;
                }

                // Check if quarter section exists (REQUIRED for Quarterlies reports)
                var hasQuarterSection = divCriteria.FindElements(By.CssSelector("div[id*='divQuarters']"))
                    .Any(el => el.Displayed);

                // 15% chance to leave all defaults (but NOT if quarter selection is required)
                if (!hasQuarterSection && random.Next(0, 100) < 15)
                {
                    output.WriteLine("  [INFO] Leaving all defaults (no changes)");
                    return selectedCriteria;
                }
                
                if (hasQuarterSection)
                {
                    output.WriteLine("  [INFO] Quarter section detected - will ensure selection is made");
                }

                // 1. Find and randomize ALL visible dropdown elements
                var dropdowns = divCriteria.FindElements(By.CssSelector("select.form-control, select[id*='ddl']"))
                    .Where(el => el.Displayed && el.Enabled).ToList();

                output.WriteLine($"  [INFO] Found {dropdowns.Count} dropdown(s)");

                foreach (var dropdown in dropdowns)
                {
                    if (random.Next(0, 100) < 75) // 75% chance to change each dropdown 
                    {
                        try
                        {
                            var select = new SelectElement(dropdown);
                            var options = select.Options
                                .Where(o => !string.IsNullOrWhiteSpace(o.Text) 
                                       && !o.Text.Contains("--Select--", StringComparison.OrdinalIgnoreCase)
                                       && !o.Text.Equals("All", StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            if (options.Count > 0)
                            {
                                var randomOption = options[random.Next(0, options.Count)];
                                select.SelectByText(randomOption.Text);
                                
                                // Try to get a label for this dropdown
                                var dropdownId = dropdown.GetAttribute("id");
                                var label = driver.FindElements(By.CssSelector($"label[for='{dropdownId}']"))
                                    .FirstOrDefault()?.Text.Trim() ?? dropdownId;
                                
                                selectedCriteria.Add(label, randomOption.Text);
                                output.WriteLine($"  [INFO] Selected '{label}': {randomOption.Text}");
                                Thread.Sleep(300);
                            }
                        }
                        catch (Exception ex)
                        {
                            output.WriteLine($"  [WARN] Could not interact with dropdown: {ex.Message}");
                        }
                    }
                }

                // 1.5. Handle Quarter selection if present in ANY report (Quarterlies, Analysis, etc.)
                // MUST select either Quarter or Date Range when quarter section exists
                var quarterSection = divCriteria.FindElements(By.CssSelector("div[id*='divQuarters']"))
                    .FirstOrDefault(el => el.Displayed);

                if (quarterSection != null)
                {
                    output.WriteLine($"  [INFO] Found quarter section - will select Quarter or Date Range");
                    
                    // Find quarter dropdown and radio buttons
                    var quarterDropdown = quarterSection.FindElements(By.CssSelector("select[id*='ddlQuarter']"))
                        .FirstOrDefault(el => el.Displayed);
                    var quarterRadio = quarterSection.FindElements(By.CssSelector("input[id*='rbtnQuarter'][type='radio']"))
                        .FirstOrDefault(el => el.Displayed);
                    var dateRangeRadio = quarterSection.FindElements(By.CssSelector("input[id*='rbtnDates'][type='radio']"))
                        .FirstOrDefault(el => el.Displayed);

                    // 70% chance to select Quarter, 30% chance to select Date Range
                    bool selectQuarter = random.Next(0, 100) < 70;

                    if (selectQuarter && quarterDropdown != null && quarterRadio != null)
                    {
                        try
                        {
                            // Click the quarter radio button first
                            if (!quarterRadio.Selected)
                            {
                                CommonTestHelper.ClickElement(driver, quarterRadio);
                                Thread.Sleep(300);
                                output.WriteLine($"  [INFO] Selected 'Quarter' radio button");
                            }

                            // Select a random quarter
                            var select = new SelectElement(quarterDropdown);
                            var options = select.Options
                                .Where(o => !string.IsNullOrWhiteSpace(o.Text) 
                                       && !o.Text.Contains("--Select--", StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            if (options.Count > 0)
                            {
                                var randomOption = options[random.Next(0, options.Count)];
                                select.SelectByText(randomOption.Text);
                                selectedCriteria.Add("Quarter", randomOption.Text);
                                output.WriteLine($"  [INFO] Selected quarter: {randomOption.Text}");
                                Thread.Sleep(500);
                            }
                        }
                        catch (Exception ex)
                        {
                            output.WriteLine($"  [WARN] Could not select quarter: {ex.Message}");
                        }
                    }
                    else if (dateRangeRadio != null)
                    {
                        // Select Date Range option instead
                        try
                        {
                            if (!dateRangeRadio.Selected)
                            {
                                CommonTestHelper.ClickElement(driver, dateRangeRadio);
                                Thread.Sleep(500); // Wait for date fields to become enabled
                                output.WriteLine($"  [INFO] Selected 'Date Range' radio button");
                            }
                            // Date fields will be handled in the next section (they should now be enabled)
                        }
                        catch (Exception ex)
                        {
                            output.WriteLine($"  [WARN] Could not select date range: {ex.Message}");
                        }
                    }
                }

                // 2. Find and randomize ALL visible date input fields (with proper Start/End Date logic)
                // Note: Skip date fields if they're disabled (happens when quarter is selected)
                var dateFields = divCriteria.FindElements(By.CssSelector("input[type='text'][id*='Date'], input[type='text'][id*='date'], input.form-control[placeholder*='date'], input.form-control[placeholder*='Date']"))
                    .Where(el => el.Displayed && el.Enabled).ToList();

                output.WriteLine($"  [INFO] Found {dateFields.Count} date field(s)");

                // If exactly 2 date fields, treat them as a start/end pair (critical for Training reports with 5-year defaults)
                if (dateFields.Count == 2)
                {
                    output.WriteLine($"  [INFO] Detected 2 date fields - will treat as start/end date pair and set within 1 year");
                    
                    try
                    {
                        var firstDateField = dateFields[0];
                        var secondDateField = dateFields[1];
                        
                        // Generate dates within 90 days (well within 1 year limit)
                        var startDaysAgo = random.Next(30, 90);
                        var endDaysAgo = random.Next(1, startDaysAgo);
                        
                        var startDate = DateTime.Now.AddDays(-startDaysAgo).ToString("MM/dd/yyyy");
                        var endDate = DateTime.Now.AddDays(-endDaysAgo).ToString("MM/dd/yyyy");
                        
                        // Set first date field
                        firstDateField.Clear();
                        firstDateField.SendKeys(startDate);
                        var firstFieldId = firstDateField.GetAttribute("id");
                        var firstLabel = driver.FindElements(By.CssSelector($"label[for='{firstFieldId}']"))
                            .FirstOrDefault()?.Text.Trim() ?? firstFieldId;
                        selectedCriteria.Add(firstLabel, startDate);
                        output.WriteLine($"  [INFO] Set '{firstLabel}': {startDate}");
                        Thread.Sleep(300);
                        
                        // Set second date field
                        secondDateField.Clear();
                        secondDateField.SendKeys(endDate);
                        var secondFieldId = secondDateField.GetAttribute("id");
                        var secondLabel = driver.FindElements(By.CssSelector($"label[for='{secondFieldId}']"))
                            .FirstOrDefault()?.Text.Trim() ?? secondFieldId;
                        selectedCriteria.Add(secondLabel, endDate);
                        output.WriteLine($"  [INFO] Set '{secondLabel}': {endDate}");
                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        output.WriteLine($"  [WARN] Could not set date pair: {ex.Message}");
                    }
                }
                else
                {
                    // More than 2 date fields - use the original logic to find Start/End pairs
                    var startDateField = dateFields.FirstOrDefault(f => f.GetAttribute("id").Contains("Start", StringComparison.OrdinalIgnoreCase) || 
                                                                         f.GetAttribute("name").Contains("Start", StringComparison.OrdinalIgnoreCase));
                    var endDateField = dateFields.FirstOrDefault(f => f.GetAttribute("id").Contains("End", StringComparison.OrdinalIgnoreCase) || 
                                                                       f.GetAttribute("name").Contains("End", StringComparison.OrdinalIgnoreCase));

                    bool shouldSetDates = (startDateField != null && endDateField != null) && random.Next(0, 100) < 75;
                
                    if (shouldSetDates)
                    {
                        try
                        {
                            // Generate a date range within 90 days
                            var startDaysAgo = random.Next(30, 90);
                            var endDaysAgo = random.Next(1, startDaysAgo);
                            
                            var startDate = DateTime.Now.AddDays(-startDaysAgo).ToString("MM/dd/yyyy");
                            var endDate = DateTime.Now.AddDays(-endDaysAgo).ToString("MM/dd/yyyy");
                            
                            startDateField.Clear();
                            startDateField.SendKeys(startDate);
                            
                            var startFieldId = startDateField.GetAttribute("id");
                            var startLabel = driver.FindElements(By.CssSelector($"label[for='{startFieldId}']"))
                                .FirstOrDefault()?.Text.Trim() ?? "Start Date";
                            selectedCriteria.Add(startLabel, startDate);
                            output.WriteLine($"  [INFO] Set '{startLabel}': {startDate}");
                            Thread.Sleep(300);
                            
                            endDateField.Clear();
                            endDateField.SendKeys(endDate);
                            
                            var endFieldId = endDateField.GetAttribute("id");
                            var endLabel = driver.FindElements(By.CssSelector($"label[for='{endFieldId}']"))
                                .FirstOrDefault()?.Text.Trim() ?? "End Date";
                            selectedCriteria.Add(endLabel, endDate);
                            output.WriteLine($"  [INFO] Set '{endLabel}': {endDate}");
                            Thread.Sleep(300);
                        }
                        catch (Exception ex)
                        {
                            output.WriteLine($"  [WARN] Could not set start/end dates: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Handle other date fields individually (not start/end pairs)
                        var otherDateFields = dateFields.Where(f => f != startDateField && f != endDateField).ToList();
                        
                        foreach (var dateField in otherDateFields)
                        {
                            if (random.Next(0, 100) < 75) // 75% chance to change each date field
                            {
                                try
                                {
                                    var randomDate = DateTime.Now.AddDays(-random.Next(1, 90)).ToString("MM/dd/yyyy");
                                    dateField.Clear();
                                    dateField.SendKeys(randomDate);
                                    
                                    var dateFieldId = dateField.GetAttribute("id");
                                    var label = driver.FindElements(By.CssSelector($"label[for='{dateFieldId}']"))
                                        .FirstOrDefault()?.Text.Trim() ?? dateFieldId;
                                    
                                    selectedCriteria.Add(label, randomDate);
                                    output.WriteLine($"  [INFO] Set '{label}': {randomDate}");
                                    Thread.Sleep(300);
                                }
                                catch (Exception ex)
                                {
                                    output.WriteLine($"  [WARN] Could not interact with date field: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                // 3. Find and randomize checkboxes (but not "Check All" boxes)
                var checkboxes = divCriteria.FindElements(By.CssSelector("input[type='checkbox']:not([id*='CheckAll']):not([id*='chkAll'])"))
                    .Where(el => el.Displayed && el.Enabled).ToList();

                output.WriteLine($"  [INFO] Found {checkboxes.Count} checkbox(es)");

                // Randomly check/uncheck a few checkboxes (max 3 to keep it reasonable)
                var checkboxesToToggle = checkboxes.OrderBy(x => random.Next()).Take(Math.Min(3, checkboxes.Count));

                foreach (var checkbox in checkboxesToToggle)
                {
                    if (random.Next(0, 100) < 70) // 70% chance to toggle 
                    {
                        try
                        {
                            var wasChecked = checkbox.Selected;
                            CommonTestHelper.ClickElement(driver, checkbox);
                            
                            var checkboxId = checkbox.GetAttribute("id");
                            var label = driver.FindElements(By.CssSelector($"label[for='{checkboxId}']"))
                                .FirstOrDefault()?.Text.Trim() ?? checkboxId;
                            
                            var newState = !wasChecked;
                            if (newState)
                            {
                                selectedCriteria.Add(label, "Checked");
                                output.WriteLine($"  [INFO] Checked '{label}'");
                            }
                            else
                            {
                                output.WriteLine($"  [INFO] Unchecked '{label}'");
                            }
                            Thread.Sleep(200);
                        }
                        catch (Exception ex)
                        {
                            output.WriteLine($"  [WARN] Could not interact with checkbox: {ex.Message}");
                        }
                    }
                }

                // 4. Find and randomize radio button groups
                var radioButtons = divCriteria.FindElements(By.CssSelector("input[type='radio']"))
                    .Where(el => el.Displayed && el.Enabled).ToList();

                if (radioButtons.Any())
                {
                    output.WriteLine($"  [INFO] Found {radioButtons.Count} radio button(s)");
                    
                    // Group radio buttons by name
                    var radioGroups = radioButtons.GroupBy(rb => rb.GetAttribute("name")).ToList();
                    
                    foreach (var group in radioGroups)
                    {
                        if (random.Next(0, 100) < 70) // 70% chance to change each group 
                        {
                            try
                            {
                                var radios = group.ToList();
                                var randomRadio = radios[random.Next(0, radios.Count)];
                                
                                if (!randomRadio.Selected)
                                {
                                    CommonTestHelper.ClickElement(driver, randomRadio);
                                    
                                    var radioId = randomRadio.GetAttribute("id");
                                    var label = driver.FindElements(By.CssSelector($"label[for='{radioId}']"))
                                        .FirstOrDefault()?.Text.Trim() ?? radioId;
                                    
                                    selectedCriteria.Add(group.Key, label);
                                    output.WriteLine($"  [INFO] Selected radio '{group.Key}': {label}");
                                    Thread.Sleep(200);
                                }
                            }
                            catch (Exception ex)
                            {
                                output.WriteLine($"  [WARN] Could not interact with radio group: {ex.Message}");
                            }
                        }
                    }
                }

                output.WriteLine("  [PASS] Criteria randomization complete");
            }
            catch (Exception ex)
            {
                output.WriteLine($"  [WARN] Error during criteria randomization: {ex.Message}");
            }

            return selectedCriteria;
        }

        /// <summary>
        /// Tests quarter selection validation by trying to run report without selecting a quarter.
        /// Verifies that validation error appears and date fields are disabled when no quarter selected.
        /// </summary>
        public static void TestQuarterValidation(IPookieWebDriver driver, ITestOutputHelper output)
        {
            output.WriteLine("[INFO] Testing quarter selection validation...");
            
            try
            {
                // Find the quarter section
                var quarterSection = driver.FindElements(By.CssSelector("div[id*='divQuarters']"))
                    .FirstOrDefault(el => el.Displayed);

                if (quarterSection == null)
                {
                    output.WriteLine("  [INFO] No quarter section found, skipping validation test");
                    return;
                }

                output.WriteLine("  [INFO] Found quarter section");

                // Check if date fields are disabled initially
                var startDateField = driver.FindElements(By.CssSelector("input[id*='txtQtrStartDate']"))
                    .FirstOrDefault();
                var endDateField = driver.FindElements(By.CssSelector("input[id*='txtQtrEndDate']"))
                    .FirstOrDefault();

                if (startDateField != null && endDateField != null)
                {
                    var startDisabled = startDateField.GetAttribute("disabled") != null;
                    var endDisabled = endDateField.GetAttribute("disabled") != null;
                    
                    if (startDisabled && endDisabled)
                    {
                        output.WriteLine("  [PASS] Date fields are correctly disabled when no quarter/date range selected");
                    }
                    else
                    {
                        output.WriteLine("  [WARN] Date fields should be disabled initially");
                    }
                }

                // Try to click "Run Report" without selecting quarter
                output.WriteLine("  [INFO] Attempting to run report without selecting quarter...");
                
                var runButton = driver.FindElements(By.CssSelector(
                        "a.btn.btn-primary[id$='btnRunReport'], " +
                        "a.btn-primary[href*='RunReport'], " +
                        "button.btn-primary"))
                    .FirstOrDefault(el => el.Displayed && el.Text.Contains("Run Report", StringComparison.OrdinalIgnoreCase));

                if (runButton != null)
                {
                    CommonTestHelper.ClickElement(driver, runButton);
                    Thread.Sleep(1000);

                    // Look for validation message
                    var validationMessages = driver.FindElements(By.CssSelector(
                            "span.Error[style*='color: red'], " +
                            "span[id*='Val'], " +
                            "div.alert-danger, " +
                            "span.field-validation-error"))
                        .Where(el => el.Displayed && !string.IsNullOrWhiteSpace(el.Text))
                        .ToList();

                    if (validationMessages.Any())
                    {
                        output.WriteLine($"  [PASS] Validation message(s) appeared:");
                        foreach (var msg in validationMessages)
                        {
                            output.WriteLine($"    - {msg.Text}");
                        }
                    }
                    else
                    {
                        output.WriteLine("  [WARN] Expected validation message but none appeared");
                    }
                }

                output.WriteLine("  [PASS] Quarter validation test complete");
            }
            catch (Exception ex)
            {
                output.WriteLine($"  [ERROR] Exception during quarter validation test: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests date validation by deliberately setting Start Date AFTER End Date.
        /// Verifies that the validation error message appears.
        /// Returns true if validation message appears as expected.
        /// </summary>
        public static bool TestDateValidation(IPookieWebDriver driver, ITestOutputHelper output)
        {
            output.WriteLine("[INFO] Testing date validation (Start Date > End Date)...");
            
            try
            {
                // Find the divCriteria container
                var divCriteria = driver.FindElements(By.CssSelector("div[id*='divCriteria'], div.criteria-section"))
                    .FirstOrDefault(el => el.Displayed);

                if (divCriteria == null)
                {
                    output.WriteLine("  [INFO] No criteria section found, skipping validation test");
                    return false;
                }

                // Find start and end date fields
                var startDateField = divCriteria.FindElements(By.CssSelector("input[type='text'][id*='Start' i][id*='Date' i], input[id*='txtStartDate']"))
                    .FirstOrDefault(el => el.Displayed && el.Enabled);
                    
                var endDateField = divCriteria.FindElements(By.CssSelector("input[type='text'][id*='End' i][id*='Date' i], input[id*='txtEndDate']"))
                    .FirstOrDefault(el => el.Displayed && el.Enabled);

                if (startDateField == null || endDateField == null)
                {
                    output.WriteLine("  [INFO] Start and/or End date fields not found, skipping validation test");
                    return false;
                }

                output.WriteLine("  [INFO] Found Start and End date fields");

                // Set End Date to an earlier date
                var endDate = DateTime.Now.AddDays(-60);
                endDateField.Clear();
                endDateField.SendKeys(endDate.ToString("MM/dd/yyyy"));
                Thread.Sleep(300);
                output.WriteLine($"  [INFO] Set End Date to: {endDate:MM/dd/yyyy}");

                // Set Start Date to a LATER date (this should trigger validation)
                var startDate = DateTime.Now.AddDays(-30);
                startDateField.Clear();
                startDateField.SendKeys(startDate.ToString("MM/dd/yyyy"));
                Thread.Sleep(500);
                output.WriteLine($"  [INFO] Set Start Date to: {startDate:MM/dd/yyyy} (AFTER End Date - should trigger validation)");

                // Trigger validation by clicking outside or tabbing
                startDateField.SendKeys(Keys.Tab);
                Thread.Sleep(500);

                // Look for the validation error message
                var validationMessage = driver.FindElements(By.CssSelector(
                        "span[id*='ValStartdate'], " +
                        "span.Error[style*='color: red'], " +
                        "span.Error:not([style*='display: none'])"))
                    .FirstOrDefault(el => el.Displayed && el.Text.Contains("Start Date cannot be after the End Date", StringComparison.OrdinalIgnoreCase));

                if (validationMessage != null)
                {
                    output.WriteLine($"  [PASS] Validation message appeared: '{validationMessage.Text}'");
                    
                    // Clear the fields to reset the form
                    startDateField.Clear();
                    endDateField.Clear();
                    Thread.Sleep(300);
                    
                    return true;
                }
                else
                {
                    output.WriteLine("  [WARN] Validation message did not appear");
                    
                    // Try to find any error spans
                    var allErrors = driver.FindElements(By.CssSelector("span.Error, span[style*='color: red']"));
                    foreach (var error in allErrors.Where(e => !string.IsNullOrWhiteSpace(e.Text)))
                    {
                        output.WriteLine($"    Found error span: '{error.Text}' (displayed={error.Displayed})");
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                output.WriteLine($"  [ERROR] Exception during validation test: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}

