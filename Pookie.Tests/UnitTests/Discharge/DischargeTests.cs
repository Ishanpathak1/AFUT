using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AFUT.Tests.Config;
using AFUT.Tests.Driver;
using AFUT.Tests.Helpers;
using AFUT.Tests.Pages;
using AFUT.Tests.UnitTests.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace AFUT.Tests.UnitTests.Discharge
{
    [TestCaseOrderer("AFUT.Tests.UnitTests.Attributes.PriorityOrderer", "AFUT.Tests")]
    public class DischargeTests : IClassFixture<AppConfig>
    {
        private readonly AppConfig _config;
        private readonly IPookieDriverFactory _driverFactory;
        private readonly ITestOutputHelper _output;
        private string TargetPc1Id => _config.TestPc1Id;

        public static IEnumerable<object[]> GetTestPc1Ids()
        {
            var config = new AppConfig();
            return config.TestPc1Ids.Select(id => new object[] { id });
        }

        public DischargeTests(AppConfig config, ITestOutputHelper output)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _output = output ?? throw new ArgumentNullException(nameof(output));

            _driverFactory = _config.ServiceProvider.GetService<IPookieDriverFactory>()
                              ?? throw new InvalidOperationException("Driver factory was not registered in the service provider.");

            CaseHomePage.ConfigureDefaultTabs(_config.CaseHomeTabs);
        }

        [Theory]
        [MemberData(nameof(GetTestPc1Ids))]
        [TestPriority(1)]
        public void NavigateToDischargeForm(string pc1Id)
        {
            using var driver = _driverFactory.CreateDriver();

            // Use common helper for the navigation flow
            var (homePage, formsPane) = CommonTestHelper.NavigateToFormsTab(driver, _config, pc1Id);

            Assert.NotNull(homePage);
            Assert.True(homePage.IsLoaded, "Home page did not load after selecting DataEntry role.");
            _output.WriteLine("[PASS] Successfully navigated to Forms tab");

            // Navigate to Discharge form
            NavigateToDischargeForm(driver, formsPane, pc1Id);
            _output.WriteLine("[PASS] Successfully navigated to Discharge form page");

            var pc1Display = CommonTestHelper.FindPc1Display(driver, pc1Id);
            Assert.False(string.IsNullOrWhiteSpace(pc1Display), "Unable to locate PC1 ID on Discharge page.");
            Assert.Contains(pc1Id, pc1Display, StringComparison.OrdinalIgnoreCase);
            _output.WriteLine($"[PASS] Verified PC1 ID display: {pc1Display}");
        }

        [Theory]
        [MemberData(nameof(GetTestPc1Ids))]
        [TestPriority(2)]
        public void SubmitDischargeFormWithValidation(string pc1Id)
        {
            using var driver = _driverFactory.CreateDriver();

            // Use common helper for the navigation flow
            var (homePage, formsPane) = CommonTestHelper.NavigateToFormsTab(driver, _config, pc1Id);

            Assert.NotNull(homePage);
            Assert.True(homePage.IsLoaded, "Home page did not load after selecting DataEntry role.");
            _output.WriteLine("[PASS] Successfully navigated to Forms tab");

            // Navigate to Discharge form
            NavigateToDischargeForm(driver, formsPane, pc1Id);
            _output.WriteLine("[PASS] Successfully navigated to Discharge form page");

            // Enter today's date in the Discharge Date field
            var dateInput = driver.FindElements(By.CssSelector(
                "div.input-group.date input.form-control, " +
                "input.form-control[class*='2dy']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException("Discharge Date input was not found.");

            var todayDate = DateTime.Now.ToString("MM/dd/yy");
            WebElementHelper.SetInputValue(driver, dateInput, todayDate, "Discharge Date", triggerBlur: true);
            driver.WaitForUpdatePanel(10);
            driver.WaitForReady(10);
            Thread.Sleep(500);

            // Submit to proceed to reason selection
            var submitButton = driver.FindElements(By.CssSelector("a.btn.btn-primary"))
                .FirstOrDefault(el => el.Displayed && 
                    !string.IsNullOrWhiteSpace(el.Text) && 
                    el.Text.Contains("Submit", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Submit button was not found.");

            CommonTestHelper.ClickElement(driver, submitButton);
            driver.WaitForUpdatePanel(30);
            driver.WaitForReady(30);
            Thread.Sleep(1000);

            // Select "Other" as the discharge reason
            var reasonDropdown = driver.FindElements(By.CssSelector("select.form-control[id*='ddlDischargeReason']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException("Discharge Reason dropdown was not found.");

            var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(reasonDropdown);
            selectElement.SelectByValue("99");
            _output.WriteLine("[INFO] Selected 'Other' as discharge reason");
            
            // Trigger change event to ensure Specify field appears
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", reasonDropdown);
            
            driver.WaitForUpdatePanel(15);
            driver.WaitForReady(15);
            Thread.Sleep(1500);

            // Verify Specify text box appears
            var specifyTextBox = driver.WaitforElementToBeInDOM(By.CssSelector(
                "input.form-control[id*='Specify'], " +
                "div[id*='DischargeReasonSpecify'] input.form-control"), 10)
                ?? throw new InvalidOperationException("Specify text box was not found after selecting 'Other'");

            Assert.True(specifyTextBox.Displayed, "Specify text box is not displayed after selecting 'Other'");
            _output.WriteLine("[PASS] Specify text box appeared after selecting 'Other'");

            // Submit without filling the Specify field - should trigger validation
            var submitReasonButton = driver.FindElements(By.CssSelector("a.btn.btn-primary"))
                .FirstOrDefault(el => el.Displayed && 
                    !string.IsNullOrWhiteSpace(el.Text) && 
                    el.Text.Contains("Submit", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Submit button was not found.");

            CommonTestHelper.ClickElement(driver, submitReasonButton);
            driver.WaitForUpdatePanel(10);
            driver.WaitForReady(10);
            Thread.Sleep(1000);

            // Verify validation message appears
            var validationMessage = driver.FindElements(By.CssSelector(
                "span[style*='color:Red'], " +
                "span[style*='color: red'], " +
                ".text-danger, " +
                "span[id*='rfvSpecify']"))
                .FirstOrDefault(el => el.Displayed && !string.IsNullOrWhiteSpace(el.Text));

            Assert.NotNull(validationMessage);
            _output.WriteLine("[PASS] Specify field validation message displayed correctly");

            // Select a random valid reason (not "Other")
            reasonDropdown = driver.FindElements(By.CssSelector("select.form-control[id*='ddlDischargeReason']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException("Discharge Reason dropdown was not found.");

            selectElement = new OpenQA.Selenium.Support.UI.SelectElement(reasonDropdown);
            
            var validOptions = selectElement.Options
                .Where(opt => !string.IsNullOrWhiteSpace(opt.GetAttribute("value")) && 
                             opt.GetAttribute("value") != "99" &&
                             !opt.Text.Contains("Select", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!validOptions.Any())
            {
                throw new InvalidOperationException("No valid discharge reason options found.");
            }

            var random = new Random();
            var randomOption = validOptions[random.Next(validOptions.Count)];
            selectElement.SelectByValue(randomOption.GetAttribute("value"));
            _output.WriteLine($"[INFO] Selected random discharge reason: {randomOption.Text.Trim()}");
            
            driver.WaitForUpdatePanel(10);
            driver.WaitForReady(10);
            Thread.Sleep(500);

            // Submit with valid data
            submitReasonButton = driver.FindElements(By.CssSelector("a.btn.btn-primary"))
                .FirstOrDefault(el => el.Displayed && 
                    !string.IsNullOrWhiteSpace(el.Text) && 
                    el.Text.Contains("Submit", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Submit button was not found.");

            CommonTestHelper.ClickElement(driver, submitReasonButton);
            driver.WaitForUpdatePanel(30);
            driver.WaitForReady(30);
            Thread.Sleep(2000);

            // Verify success toast message
            var toastMessage = WebElementHelper.GetToastMessage(driver, 1500);
            Assert.False(string.IsNullOrWhiteSpace(toastMessage), "Success toast message was not displayed.");

            Assert.Contains("saved", toastMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(pc1Id, toastMessage, StringComparison.OrdinalIgnoreCase);
            _output.WriteLine("[PASS] Discharge form submitted successfully");
        }

        [Theory]
        [MemberData(nameof(GetTestPc1Ids))]
        [TestPriority(3)]
        public void ValidateConditionalFieldsForSpecificDischargeReasons(string pc1Id)
        {
            using var driver = _driverFactory.CreateDriver();

            // Use common helper for the navigation flow
            var (homePage, formsPane) = CommonTestHelper.NavigateToFormsTab(driver, _config, pc1Id);

            Assert.NotNull(homePage);
            Assert.True(homePage.IsLoaded, "Home page did not load after selecting DataEntry role.");
            _output.WriteLine("[PASS] Successfully navigated to Forms tab");

            // Navigate to Discharge form
            NavigateToDischargeForm(driver, formsPane, pc1Id);
            _output.WriteLine("[PASS] Successfully navigated to Discharge form page");

            // Enter today's date in the Discharge Date field
            var dateInput = driver.FindElements(By.CssSelector(
                "div.input-group.date input.form-control, " +
                "input.form-control[class*='2dy']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException("Discharge Date input was not found.");

            var todayDate = DateTime.Now.ToString("MM/dd/yy");
            WebElementHelper.SetInputValue(driver, dateInput, todayDate, "Discharge Date", triggerBlur: true);
            driver.WaitForUpdatePanel(10);
            driver.WaitForReady(10);
            Thread.Sleep(500);

            // Submit to proceed to reason selection
            var submitButton = driver.FindElements(By.CssSelector("a.btn.btn-primary"))
                .FirstOrDefault(el => el.Displayed && 
                    !string.IsNullOrWhiteSpace(el.Text) && 
                    el.Text.Contains("Submit", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Submit button was not found.");

            CommonTestHelper.ClickElement(driver, submitButton);
            driver.WaitForUpdatePanel(30);
            driver.WaitForReady(30);
            Thread.Sleep(1000);

            // Test Option 18 - Target Child Death (DOD field should appear)
            _output.WriteLine("[INFO] Testing Option 18 - Target Child Death");
            TestDischargeReasonWithDodField(driver, "18", "divTargetChildDOD", "txtTargetChildDOD", "Target Child DOD", "Missing Target Child DOD");

            // Test Option 21 - PC1 Death (DOD field should appear)
            _output.WriteLine("[INFO] Testing Option 21 - PC1 Death");
            TestDischargeReasonWithDodField(driver, "21", "divPC1DOD", "txtPC1DOD", "PC1 DOD", "Missing PC1 DOD");

            // Test Option 25 - Transferred to another program (List program field should appear)
            _output.WriteLine("[INFO] Testing Option 25 - Transferred to another program");
            TestDischargeReasonWithTransferField(driver, "25");

            // Test Option 37 - Transfer to another HFNY program (should show approval message)
            _output.WriteLine("[INFO] Testing Option 37 - Transfer to another HFNY program");
            TestDischargeReasonWithApprovalMessage(driver, pc1Id, "37");

            _output.WriteLine("[PASS] All conditional field validations completed successfully");
        }

        [Theory]
        [MemberData(nameof(GetTestPc1Ids))]
        [TestPriority(4)]
        public void ReinstateCaseFromDischargeForm(string pc1Id)
        {
            using var driver = _driverFactory.CreateDriver();

            // Use common helper for the navigation flow
            var (homePage, formsPane) = CommonTestHelper.NavigateToFormsTab(driver, _config, pc1Id);

            Assert.NotNull(homePage);
            Assert.True(homePage.IsLoaded, "Home page did not load after selecting DataEntry role.");
            _output.WriteLine("[PASS] Successfully navigated to Forms tab");

            // Navigate to Discharge form
            NavigateToDischargeForm(driver, formsPane, pc1Id);
            _output.WriteLine("[PASS] Successfully navigated to Discharge form page");

            // Find and click Reinstate button
            var reinstateButton = driver.FindElements(By.CssSelector(
                "a.btn.btn-warning, " +
                "a[id*='btnReinstate']"))
                .FirstOrDefault(el => el.Displayed && 
                    !string.IsNullOrWhiteSpace(el.Text) && 
                    el.Text.Contains("Reinstate", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Reinstate button was not found on the Discharge form page.");

            _output.WriteLine("[INFO] Found Reinstate button, clicking...");
            CommonTestHelper.ClickElement(driver, reinstateButton);
            driver.WaitForUpdatePanel(30);
            driver.WaitForReady(30);
            Thread.Sleep(2000);

            // Verify success toast message
            var toastMessage = WebElementHelper.GetToastMessage(driver, 1500);
            Assert.False(string.IsNullOrWhiteSpace(toastMessage), "Reinstate success toast message was not displayed.");

            Assert.Contains("Case Reinstated", toastMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(pc1Id, toastMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("reinstated", toastMessage, StringComparison.OrdinalIgnoreCase);
            _output.WriteLine("[PASS] Case reinstated successfully");
        }

        #region Helper Methods

        /// <summary>
        /// Navigates to the Discharge form page from the forms pane
        /// </summary>
        private void NavigateToDischargeForm(IPookieWebDriver driver, IWebElement formsPane, string pc1Id)
        {
            // Find Discharge link using CSS classes and attributes (NOT ASP.NET IDs)
            var dischargeLink = formsPane.FindElements(By.CssSelector(
                "a.list-group-item.moreInfo[href*='Discharge.aspx'], " +
                "a.list-group-item.moreInfo[href*='PreDischarge.aspx'], " +
                "a.moreInfo[data-formtype='dc'], " +
                "a.list-group-item[title='Discharge']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException("Discharge link was not found inside the Forms tab.");

            _output.WriteLine($"Found Discharge link: {dischargeLink.Text?.Trim()}");
            CommonTestHelper.ClickElement(driver, dischargeLink);
            driver.WaitForUpdatePanel(30);
            driver.WaitForReady(30);
            Thread.Sleep(1000);

            // Verify we're on a Discharge page (can be PreDischarge.aspx for new or discharge.aspx for existing)
            var currentUrl = driver.Url;
            var isDischargeOrPreDischarge = currentUrl.Contains("discharge.aspx", StringComparison.OrdinalIgnoreCase) || 
                                            currentUrl.Contains("PreDischarge.aspx", StringComparison.OrdinalIgnoreCase);
            Assert.True(isDischargeOrPreDischarge, $"Expected Discharge page but got: {currentUrl}");
            Assert.Contains(pc1Id, currentUrl, StringComparison.OrdinalIgnoreCase);
            _output.WriteLine($"[PASS] Discharge form page opened successfully: {currentUrl}");

            // Wait for page to be fully loaded
            driver.WaitForReady(10);
            Thread.Sleep(500);

            // Verify form container is present
            var formContainer = driver.WaitforElementToBeInDOM(By.CssSelector(
                ".panel-body, " +
                ".form-horizontal, " +
                "form, " +
                ".container-fluid"), 10);

            Assert.NotNull(formContainer);
            _output.WriteLine("[PASS] Discharge form container is present on the page");
        }

        /// <summary>
        /// Tests discharge reasons that show DOD (Date of Death) field
        /// </summary>
        private void TestDischargeReasonWithDodField(IPookieWebDriver driver, string reasonValue, 
            string divId, string inputIdPart, string fieldLabel, string expectedValidationMessage)
        {
            // Select the discharge reason
            var reasonDropdown = driver.FindElements(By.CssSelector("select.form-control[id*='ddlDischargeReason']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException("Discharge Reason dropdown was not found.");

            var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(reasonDropdown);
            selectElement.SelectByValue(reasonValue);
            _output.WriteLine($"[INFO] Selected discharge reason option {reasonValue}");

            // Trigger change event
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", reasonDropdown);

            driver.WaitForUpdatePanel(15);
            driver.WaitForReady(15);
            Thread.Sleep(1500);

            // Verify DOD container is visible
            var dodContainer = driver.WaitforElementToBeInDOM(By.CssSelector($"div[id='{divId}'], div[id*='{divId}']"), 10)
                ?? throw new InvalidOperationException($"{fieldLabel} container was not found after selecting option {reasonValue}");

            Assert.True(dodContainer.Displayed, $"{fieldLabel} container is not displayed after selecting option {reasonValue}");
            _output.WriteLine($"[PASS] {fieldLabel} container appeared");

            // Verify DOD input field is visible
            var dodInput = driver.FindElements(By.CssSelector(
                $"input.form-control[id*='{inputIdPart}'], " +
                $"input.form-control[class*='2dy'][id*='{inputIdPart}']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException($"{fieldLabel} input was not found.");

            Assert.True(dodInput.Displayed, $"{fieldLabel} input is not displayed");
            _output.WriteLine($"[PASS] {fieldLabel} input field is visible and accessible");

            // Submit without filling DOD field - should trigger validation
            var submitButton = driver.FindElements(By.CssSelector("a.btn.btn-primary"))
                .FirstOrDefault(el => el.Displayed && 
                    !string.IsNullOrWhiteSpace(el.Text) && 
                    el.Text.Contains("Submit", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Submit button was not found.");

            CommonTestHelper.ClickElement(driver, submitButton);
            driver.WaitForUpdatePanel(10);
            driver.WaitForReady(10);
            Thread.Sleep(1000);

            // Verify validation message appears
            var validationMessage = driver.FindElements(By.XPath(
                $"//*[contains(text(), '{expectedValidationMessage}')]"))
                .FirstOrDefault(el => el.Displayed);

            if (validationMessage == null)
            {
                // Try to find in toast message
                var toastMessage = WebElementHelper.GetToastMessage(driver, 1500);
                if (!string.IsNullOrWhiteSpace(toastMessage))
                {
                    Assert.Contains(expectedValidationMessage, toastMessage, StringComparison.OrdinalIgnoreCase);
                    _output.WriteLine($"[PASS] Validation message found in toast: {toastMessage}");
                }
                else
                {
                    throw new InvalidOperationException($"Validation message '{expectedValidationMessage}' was not found after submitting without DOD");
                }
            }
            else
            {
                Assert.Contains(expectedValidationMessage, validationMessage.Text, StringComparison.OrdinalIgnoreCase);
                _output.WriteLine($"[PASS] Validation message displayed: {validationMessage.Text}");
            }

            // Re-find the DOD input element (it may have become stale after validation)
            dodInput = driver.FindElements(By.CssSelector(
                $"input.form-control[id*='{inputIdPart}'], " +
                $"input.form-control[class*='2dy'][id*='{inputIdPart}']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException($"{fieldLabel} input was not found after validation.");

            // Now enter a date in the DOD field
            var dodDate = DateTime.Now.AddDays(-30).ToString("MM/dd/yy");
            WebElementHelper.SetInputValue(driver, dodInput, dodDate, fieldLabel, triggerBlur: true);
            driver.WaitForUpdatePanel(10);
            driver.WaitForReady(10);
            Thread.Sleep(500);
            _output.WriteLine($"[INFO] Entered DOD date: {dodDate}");
        }

        /// <summary>
        /// Tests discharge reason 25 - Transferred to another program (List program field)
        /// </summary>
        private void TestDischargeReasonWithTransferField(IPookieWebDriver driver, string reasonValue)
        {
            // Select the discharge reason
            var reasonDropdown = driver.FindElements(By.CssSelector("select.form-control[id*='ddlDischargeReason']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException("Discharge Reason dropdown was not found.");

            var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(reasonDropdown);
            selectElement.SelectByValue(reasonValue);
            _output.WriteLine($"[INFO] Selected discharge reason option {reasonValue}");

            // Trigger change event
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", reasonDropdown);

            driver.WaitForUpdatePanel(15);
            driver.WaitForReady(15);
            Thread.Sleep(1500);

            // Verify "List program" container is visible
            var transferContainer = driver.WaitforElementToBeInDOM(By.CssSelector(
                "div[id='divTransferredtoProgram'], " +
                "div[id*='TransferredtoProgram']"), 10)
                ?? throw new InvalidOperationException("List program container was not found after selecting option 25");

            Assert.True(transferContainer.Displayed, "List program container is not displayed after selecting option 25");
            _output.WriteLine("[PASS] List program container appeared");

            // Verify "List program" input field is visible
            var transferInput = driver.FindElements(By.CssSelector(
                "input.form-control[id*='txtTransferredtoProgram']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException("List program input was not found.");

            Assert.True(transferInput.Displayed, "List program input is not displayed");
            _output.WriteLine("[PASS] List program input field is visible and accessible");

            // Submit without filling program field - should trigger validation
            var submitButton = driver.FindElements(By.CssSelector("a.btn.btn-primary"))
                .FirstOrDefault(el => el.Displayed && 
                    !string.IsNullOrWhiteSpace(el.Text) && 
                    el.Text.Contains("Submit", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Submit button was not found.");

            CommonTestHelper.ClickElement(driver, submitButton);
            driver.WaitForUpdatePanel(10);
            driver.WaitForReady(10);
            Thread.Sleep(1000);

            // Verify validation message appears
            var validationMessage = driver.FindElements(By.XPath(
                "//*[contains(text(), 'Missing Transfer to Program')]"))
                .FirstOrDefault(el => el.Displayed);

            if (validationMessage == null)
            {
                // Try to find in toast message
                var toastMessage = WebElementHelper.GetToastMessage(driver, 1500);
                if (!string.IsNullOrWhiteSpace(toastMessage))
                {
                    Assert.Contains("Missing Transfer to Program", toastMessage, StringComparison.OrdinalIgnoreCase);
                    _output.WriteLine($"[PASS] Validation message found in toast: {toastMessage}");
                }
                else
                {
                    throw new InvalidOperationException("Validation message 'Missing Transfer to Program' was not found after submitting without program name");
                }
            }
            else
            {
                Assert.Contains("Missing Transfer to Program", validationMessage.Text, StringComparison.OrdinalIgnoreCase);
                _output.WriteLine($"[PASS] Validation message displayed: {validationMessage.Text}");
            }

            // Re-find the transfer input element (it may have become stale after validation)
            transferInput = driver.FindElements(By.CssSelector(
                "input.form-control[id*='txtTransferredtoProgram']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException("List program input was not found after validation.");

            // Now enter a program name
            WebElementHelper.SetInputValue(driver, transferInput, "Test Transfer Program", "List program", triggerBlur: true);
            driver.WaitForUpdatePanel(10);
            driver.WaitForReady(10);
            Thread.Sleep(500);
            _output.WriteLine("[INFO] Entered program name: Test Transfer Program");
        }

        /// <summary>
        /// Tests discharge reason 37 - Transfer to another HFNY program (shows approval message on submit)
        /// </summary>
        private void TestDischargeReasonWithApprovalMessage(IPookieWebDriver driver, string pc1Id, string reasonValue)
        {
            // Select the discharge reason
            var reasonDropdown = driver.FindElements(By.CssSelector("select.form-control[id*='ddlDischargeReason']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException("Discharge Reason dropdown was not found.");

            var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(reasonDropdown);
            selectElement.SelectByValue(reasonValue);
            _output.WriteLine($"[INFO] Selected discharge reason option {reasonValue}");

            // Trigger change event
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", reasonDropdown);

            driver.WaitForUpdatePanel(15);
            driver.WaitForReady(15);
            Thread.Sleep(1500);

            // Step 1: Submit without selecting program - should trigger validation
            var submitButton = driver.FindElements(By.CssSelector("a.btn.btn-primary"))
                .FirstOrDefault(el => el.Displayed && 
                    !string.IsNullOrWhiteSpace(el.Text) && 
                    el.Text.Contains("Submit", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Submit button was not found.");

            CommonTestHelper.ClickElement(driver, submitButton);
            driver.WaitForUpdatePanel(10);
            driver.WaitForReady(10);
            Thread.Sleep(1000);

            // Verify validation message "Missing Transfer to Program"
            var validationMessage = driver.FindElements(By.XPath(
                "//*[contains(text(), 'Missing Transfer to Program')]"))
                .FirstOrDefault(el => el.Displayed);

            if (validationMessage == null)
            {
                // Try to find in toast message
                var toastMessage = WebElementHelper.GetToastMessage(driver, 1500);
                if (!string.IsNullOrWhiteSpace(toastMessage))
                {
                    Assert.Contains("Missing Transfer to Program", toastMessage, StringComparison.OrdinalIgnoreCase);
                    _output.WriteLine($"[PASS] Validation message found in toast: {toastMessage}");
                }
                else
                {
                    throw new InvalidOperationException("Validation message 'Missing Transfer to Program' was not found after submitting without program selection");
                }
            }
            else
            {
                Assert.Contains("Missing Transfer to Program", validationMessage.Text, StringComparison.OrdinalIgnoreCase);
                _output.WriteLine($"[PASS] Validation message displayed: {validationMessage.Text}");
            }

            // Wait a moment for page to stabilize after validation
            driver.WaitForUpdatePanel(5);
            driver.WaitForReady(5);
            Thread.Sleep(500);

            // Step 2: Select a program from the dropdown (re-find after validation)
            var programDropdown = driver.FindElements(By.CssSelector(
                "select.form-control[id*='ddlTransferredtoProgramFK']"))
                .FirstOrDefault(el => el.Displayed)
                ?? throw new InvalidOperationException("Transfer to Program dropdown was not found after validation.");

            var programSelectElement = new OpenQA.Selenium.Support.UI.SelectElement(programDropdown);
            
            // Get all valid program options (exclude the --Select-- option)
            var validPrograms = programSelectElement.Options
                .Where(opt => !string.IsNullOrWhiteSpace(opt.GetAttribute("value")) && 
                             !opt.Text.Contains("Select", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!validPrograms.Any())
            {
                throw new InvalidOperationException("No valid program options found in dropdown.");
            }

            // Select a random program
            var random = new Random();
            var randomProgram = validPrograms[random.Next(validPrograms.Count)];
            programSelectElement.SelectByValue(randomProgram.GetAttribute("value"));
            _output.WriteLine($"[INFO] Selected transfer program: {randomProgram.Text.Trim()}");

            driver.WaitForUpdatePanel(10);
            driver.WaitForReady(10);
            Thread.Sleep(1000);

            // Step 3: Check the acknowledgment checkbox (re-find after dropdown selection)
            var acknowledgmentCheckbox = driver.FindElements(By.CssSelector(
                "input[type='checkbox'][id*='chkAcknowledgeRemoval']"))
                .FirstOrDefault()
                ?? throw new InvalidOperationException("Acknowledgment checkbox was not found after program selection.");

            // Check if already checked, if not then check it
            if (!acknowledgmentCheckbox.Selected)
            {
                CommonTestHelper.ClickElement(driver, acknowledgmentCheckbox);
                _output.WriteLine("[INFO] Checked acknowledgment checkbox");
            }
            else
            {
                _output.WriteLine("[INFO] Acknowledgment checkbox already checked");
            }

            driver.WaitForUpdatePanel(5);
            driver.WaitForReady(5);
            Thread.Sleep(500);

            // Step 4: Submit the form again
            submitButton = driver.FindElements(By.CssSelector("a.btn.btn-primary"))
                .FirstOrDefault(el => el.Displayed && 
                    !string.IsNullOrWhiteSpace(el.Text) && 
                    el.Text.Contains("Submit", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Submit button was not found.");

            CommonTestHelper.ClickElement(driver, submitButton);
            driver.WaitForUpdatePanel(30);
            driver.WaitForReady(30);
            Thread.Sleep(2000);

            // Verify the supervisory approval message appears
            var approvalMessage = driver.FindElements(By.XPath(
                "//*[contains(text(), 'supervisory approval') or " +
                "contains(text(), 'require supervisory approval') or " +
                "contains(text(), 'forms for this case which require supervisory approval')]"))
                .FirstOrDefault(el => el.Displayed);

            if (approvalMessage == null)
            {
                // Try to find in toast message
                var toastMessage = WebElementHelper.GetToastMessage(driver, 1500);
                if (!string.IsNullOrWhiteSpace(toastMessage))
                {
                    Assert.Contains("supervisory approval", toastMessage, StringComparison.OrdinalIgnoreCase);
                    _output.WriteLine($"[PASS] Supervisory approval message found in toast: {toastMessage}");
                }
                else
                {
                    throw new InvalidOperationException("Supervisory approval message was not found after submitting option 37");
                }
            }
            else
            {
                var messageText = approvalMessage.Text?.Trim();
                Assert.Contains("supervisory approval", messageText, StringComparison.OrdinalIgnoreCase);
                _output.WriteLine($"[PASS] Supervisory approval message displayed: {messageText}");
            }
        }

        #endregion
    }
}

