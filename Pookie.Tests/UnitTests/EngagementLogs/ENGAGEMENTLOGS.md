# Engagement Logs Tests Documentation

## Overview

The `EngagementLogsTests.cs` file contains automated end-to-end tests for the Engagement Log (Pre-assessment) form functionality. These tests validate form navigation, conditional field visibility based on case status, validation rules, form submission, and delete operations for engagement log records.

## Test Structure

### Technology Stack
- **Framework**: xUnit
- **Test Type**: Theory-based tests with parameterized PC1 IDs
- **Test Ordering**: Uses `[TestPriority]` attribute (1-9)
- **Architecture**: Direct Selenium with helper methods

### Test File Overview
- **File**: `EngagementLogsTests.cs`
- **Test Count**: 9 tests
- **Priority Range**: 1-9 (sequential execution)
- **Dependencies**: Requires PC1 IDs from configuration

## Test Execution Order

Tests run in priority order using `[TestPriority]` attribute:

1. **Priority 1**: Navigation and basic page load
2. **Priority 2**: Case Status 1 validation
3. **Priority 3**: Case Status 2 conditional fields
4. **Priority 4**: Case Status 2 + Not Assigned → Termination fields
5. **Priority 5**: Case Status 3 termination fields
6. **Priority 6**: Case Status 3 submission and grid verification
7. **Priority 7**: Delete with cancel and confirm
8. **Priority 8**: Duplicate month validation
9. **Priority 9**: Case Status 2 + Assigned submission and grid verification

---

## Engagement Log Business Logic

### Case Status Options

The engagement log form has **three main case status options**:

| Status Value | Status Name | Meaning |
|--------------|-------------|---------|
| **01** | Active (< 1 month) | Case is less than 1 month old - **Not allowed** for engagement logs |
| **02** | Parent Enrolls | Parent enrolls in the program (assessment completed) |
| **03** | Engagement Efforts Terminated | Engagement attempts ended before enrollment |

### Conditional Field Logic

The form displays different fields based on case status selection:

#### Case Status 01 (Active < 1 month)
- **Validation**: "You can not enter a Engagement Log record with a case status of 1"
- **Purpose**: Prevents logging engagement before sufficient time has passed

#### Case Status 02 (Parent Enrolls)
- **Shows**: Case Assignment Section
  - Radio buttons: Assigned? (Yes/No)
  - Worker dropdown
  - Assignment date input

**If "Yes" selected (Case Assigned)**:
- Worker dropdown and assignment date **required**
- No termination fields shown

**If "No" selected (Case NOT Assigned)**:
- Shows **Termination Section**:
  - Termination Date input
  - Termination Reason dropdown

#### Case Status 03 (Engagement Efforts Terminated)
- **Shows**: Termination Section
  - Termination Date input
  - Termination Reason dropdown
- **Hides**: Case Assignment Section (not applicable)

---

## Detailed Test Breakdown

### Test 1: CheckingTheEngagementLogButton (Priority 1)

**Purpose**: Validates navigation to Engagement Log page and PC1 ID display.

**Test Flow**:
1. Sign in as DataEntry role
2. Navigate to Search Cases
3. Search for PC1 ID from config
4. Click Forms tab
5. Click Engagement Log link
6. Verify page loads
7. Verify PC1 ID is displayed on page

**What it tests**:
- Navigation flow: Home → Search → Forms → Engagement Log
- PC1 ID verification on Engagement Log page
- Page content is visible

**Key Assertions**:
- Home page loads after role selection
- PC1 ID is found on Engagement Log page
- PC1 ID matches the target ID

**Use Case**: Smoke test to ensure basic navigation works.

---

### Test 2: CheckingValdiationOneMonthIsOver (Priority 2)

**Purpose**: Validates that Case Status 01 (Active < 1 month) triggers validation error.

**Test Flow**:
1. Navigate to Engagement Log
2. Click "New Form" button
3. Enter activity month: "11/2025"
4. Click "Add New" to advance
5. Select **Case Status = 01**
6. Click Submit
7. Verify validation message appears

**Expected Validation**:
```
"You can not enter a Engagement Log record with a case status of 1"
```

**Key Assertions**:
- Case Status dropdown appears
- Validation message contains the expected text

**Business Rule**: Cannot create engagement log if case is less than 1 month old (Status 01).

---

### Test 3: CheckingAdditionalQuestionsAppearWhenCaseStatusTwoSelected (Priority 3)

**Purpose**: Validates that selecting Case Status 02 (Parent Enrolls) displays case assignment fields.

**Test Flow**:
1. Navigate to Engagement Log
2. Open New Form
3. Enter activity month: "11/2025"
4. Advance to case status step
5. Select **Case Status = 02**
6. Wait for fields to appear
7. Verify Case Assignment Section is displayed

**Fields that should appear**:
- Yes/No radio buttons: "Case Assigned?"
- Worker dropdown (`ddlFSW`)
- Assignment date input (`txtFSWDate`)

**Key Assertions**:
- Case assignment section (`trAssessmentCompleted`) is displayed
- Radio buttons (Yes/No) are visible
- Worker dropdown is visible
- Assignment date input is visible

**Use Case**: Validates conditional field visibility for Status 02.

---

### Test 4: CheckingTerminationFieldsAppearWhenCaseStatusTwoAndCaseNotAssigned (Priority 4)

**Purpose**: Validates that selecting "No" for case assignment shows termination fields.

**Test Flow**:
1. Navigate to Engagement Log
2. Open New Form
3. Select **Case Status = 02**
4. Select **"No"** radio button (Case NOT Assigned)
5. Wait for termination fields to appear
6. Verify termination fields are displayed

**Fields that should appear**:
- Termination date input (`txtTerminationDate`)
- Termination reason dropdown (`ddlTerminationReason`)

**Key Assertions**:
- Termination section (`trEffortsTerminated`) is displayed
- Termination date input is visible
- Termination reason dropdown is visible

**Business Logic**: If case not assigned after assessment, must record termination date and reason.

---

### Test 5: CheckingTerminationFieldsAppearWhenCaseStatusThreeSelected (Priority 5)

**Purpose**: Validates that Case Status 03 (Engagement Efforts Terminated) displays termination fields and hides assignment fields.

**Test Flow**:
1. Navigate to Engagement Log
2. Open New Form
3. Select **Case Status = 03**
4. Wait for fields to update
5. Verify termination fields are displayed
6. Verify case assignment section is NOT displayed

**Key Assertions**:
- Termination section (`trEffortsTerminated`) is displayed
- Termination date input is visible
- Termination reason dropdown is visible
- Case assignment section (`trAssessmentCompleted`) is **NOT visible** (or doesn't exist)

**Business Logic**: Status 03 means engagement terminated before enrollment, so assignment fields are not relevant.

---

### Test 6: CheckingCaseStatusThreeSubmissionAppearsInGrid (Priority 6)

**Purpose**: Validates that submitting a Case Status 03 form creates a record in the grid.

**Test Flow**:
1. Navigate to Engagement Log
2. Open New Form
3. Select **Case Status = 03**
4. Enter **Termination Date**: "11/18/25"
5. Select **Termination Reason**: "36 Participant Refused"
6. Click Submit
7. Wait for submission (2 seconds)
8. Verify row appears in preassessment grid

**Grid Verification**:
- Searches for row containing:
  - Date: "11/18/25"
  - Status: "Engagement Efforts Terminated"

**Key Assertions**:
- Form submits successfully
- Preassessment grid contains the new record
- Row displays correct date and status

**Use Case**: Happy path test for Case Status 03 submission.

---

### Test 7: CheckingDeletingPreassessmentRecordRequiresConfirmation (Priority 7)

**Purpose**: Validates delete operation with cancel and confirm flows.

**Test Flow**:

#### Part 1: Cancel Delete
1. Navigate to Engagement Log
2. Find first row with Delete button in preassessment grid
3. Capture row identifier (first column text)
4. Click **Delete** button
5. Wait for confirmation modal to appear
6. Click **"No, return"** (Cancel button)
7. Verify modal closes
8. Verify row is still present in grid

#### Part 2: Confirm Delete
1. Re-find the same row (avoid stale element)
2. Click **Delete** button again
3. Wait for confirmation modal
4. Click **Confirm Delete** button
5. Wait for deletion (2 seconds)
6. Verify row is removed from grid

**Key Assertions**:
- Delete modal appears when Delete clicked
- Cancel button closes modal without deleting
- Row still exists after cancel
- Confirm button deletes the record
- Row no longer exists after confirm

**Modal Elements**:
- Modal class: `.dc-confirmation-modal`
- Cancel button: `button.btn.btn-default` with text "No, return"
- Confirm button: `a.btn.btn-primary[id*='btnDelete'][id$='lbConfirmDelete']`

**Use Case**: Validates safe delete with confirmation.

---

### Test 8: CheckingDuplicateMonthValidationDisplayedWhenFormExists (Priority 8)

**Purpose**: Validates that attempting to create a form for an existing activity month shows validation error.

**Test Flow**:
1. Navigate to Engagement Log
2. Click "New Form" button
3. Attempt to trigger duplicate month validation (up to 3 attempts)
   - Enter activity month: "11/2025" (month that already has a record)
   - Click "Add New" button
   - Check for validation message
4. Verify validation message appears

**Expected Validation**:
```
"There is already a form entered for this activity month."
```

**Retry Logic**:
- Test attempts up to 3 times to trigger validation
- Handles case where "Missing Activity Month" appears instead
- Throws error if duplicate validation doesn't appear

**Key Assertions**:
- Validation summary (`.alert.alert-danger`) is displayed
- Message contains "There is already a form entered for this activity month."

**Business Rule**: Cannot create multiple engagement log forms for the same activity month.

---

### Test 9: CheckingAssignedCaseStatusTwoSubmitsToGrid (Priority 9)

**Purpose**: Validates that submitting Case Status 02 with assigned worker creates a record in the grid.

**Test Flow**:
1. Navigate to Engagement Log
2. Open New Form
3. Select **Case Status = 02** (Parent Enrolls)
4. Select **"Yes"** radio button (Case Assigned)
5. Select **Worker**: "Test, Derek" (or value "3489")
6. Enter **Assignment Date**: "11/18/25"
7. Click Submit
8. Wait for submission (2 seconds)
9. Verify row appears in preassessment grid

**Grid Verification**:
- Searches for row containing:
  - Date: "11/18/25"
  - Status: "Parent Enrolls"

**Key Assertions**:
- Form submits successfully
- Preassessment grid contains the new record
- Row displays correct date and "Parent Enrolls" status

**Worker Selection Logic**:
```csharp
try {
    workerSelect.SelectByText("Test, Derek");
} catch (NoSuchElementException) {
    workerSelect.SelectByValue("3489");
}
```
Handles case where worker name might not match exactly by falling back to value.

**Use Case**: Happy path test for Case Status 02 with assigned worker.

---

## Helper Methods

### Navigation Helpers

#### SignInAsDataEntry()
Signs in and selects DataEntry role, returns HomePage.

**Flow**:
1. Navigate to app URL
2. Sign in with credentials from config
3. Select "Program 1" → "DataEntry" role
4. Assert sign-in and role selection succeeded
5. Return HomePage object

---

#### NavigateToEngagementLog()
Navigates from current page to Engagement Log page.

**Flow**:
1. Call `NavigateToFormsTab()` to get to Forms tab
2. Find Engagement Log link: `a.moreInfo[data-formtype='pa']`
3. Click link
4. Wait for page load

---

#### NavigateToFormsTab()
Navigates to Forms tab and returns forms pane element.

**Flow**:
1. Find navigation bar
2. Click "Search Cases" button
3. Verify Search Cases page loaded
4. Enter PC1 ID in search box
5. Click Search button
6. Wait for results
7. Click Forms tab
8. Wait for Forms tab content to activate
9. Return forms pane element

---

### Form Interaction Helpers

#### OpenNewFormAndGetCaseStatusDropdown()
Opens new form dialog and returns Case Status dropdown.

**Flow**:
1. Click "New Form" button
2. Wait for modal to appear
3. Enter activity month: "11/2025"
4. Click "Add New" button
5. Wait for case status step to appear
6. Call `EnsureCaseStatusDropdown()` with retry logic
7. Return case status dropdown element

**Retry Logic**: Up to 3 attempts to find dropdown, re-entering activity month if needed.

---

#### EnsureCaseStatusDropdown()
Ensures case status dropdown appears, with retry logic.

**Flow**:
1. Attempt to find case status dropdown
2. If not found:
   - Re-enter activity month
   - Re-click "Add New" button
   - Wait and retry
3. Up to 3 attempts
4. Throw error if dropdown never appears

**Purpose**: Handles timing issues where form may not advance immediately to case status step.

---

### Validation Helpers

#### TriggerDuplicateMonthValidation()
Attempts to trigger duplicate month validation message.

**Flow**:
1. For each attempt (up to 3):
   - Enter activity month: "11/2025"
   - Click "Add New" button
   - Wait for validation
   - Check if duplicate month message appears
   - If found, return validation text
   - If "Missing Activity Month" appears, retry
2. Throw error if validation doesn't appear after 3 attempts

**Purpose**: Handles cases where validation may not appear on first try due to timing.

---

### Grid Helpers

#### FindPreassessmentRow()
Finds a row in preassessment grid by date and status text.

**Parameters**:
- `formDateText`: Date to search for (e.g., "11/18/25")
- `statusText`: Status to search for (e.g., "Parent Enrolls", "Engagement Efforts Terminated")

**Returns**: `IWebElement` for matching row, or `null` if not found.

**Logic**:
```csharp
// Find grid
var grid = driver.WaitforElementToBeInDOM(
    By.CssSelector("table[id$='grPreassessments']"), 20);

// Search rows for date AND status match
foreach (var row in rows) {
    var dateMatch = row.Text.Contains(formDateText);
    var statusMatch = row.Text.Contains(statusText);
    if (dateMatch && statusMatch) {
        return row;
    }
}
```

---

#### FindEngagementLogRow()
Finds a row by worker name and month text (unused in current tests).

**Parameters**:
- `workerName`: Worker name to search for
- `monthText`: Month text to search for

**Returns**: `IWebElement` for matching row, or `null` if not found.

---

### Modal Helpers

#### FindElementInModalOrPage()
Finds an element, preferring modal context if modal is open.

**Search Strategy**:
1. Check if modal is visible (`.modal.show`, `.modal.in`, etc.)
2. If modal exists, search within modal first
3. Fallback: search on entire page
4. Wait up to specified timeout (default 10 seconds)

**Purpose**: Handles dynamic content that can appear in modals or on page.

---

#### WaitForDeleteConfirmationModal()
Waits for delete confirmation modal to appear within a delete control.

**Parameters**:
- `deleteControl`: Parent element containing the modal
- `timeoutMilliseconds`: Wait timeout (default 5000ms)

**Returns**: Modal element (`.dc-confirmation-modal`)

**Modal Detection**:
- Checks if element is displayed OR
- Has class "in" OR has class "show"

---

### Input Helpers

#### SetInputValue()
Sets input field value with multiple fallback strategies.

**Strategies**:
1. **Try standard SendKeys**:
   ```csharp
   input.Clear();
   input.SendKeys(value);
   ```

2. **If not interactable, use JavaScript**:
   ```csharp
   js.ExecuteScript("arguments[0].value = arguments[1]; ...", input, value);
   ```

3. **Verify value was set correctly**:
   - Check `input.GetAttribute("value")`
   - Retry with JavaScript if mismatch

4. **If still failing, remove readonly attribute**:
   ```csharp
   js.ExecuteScript("arguments[0].removeAttribute('readonly');", input);
   ```

5. **Trigger blur event if requested**:
   ```csharp
   input.SendKeys(Keys.Tab); // Try tab first
   js.ExecuteScript("arguments[0].dispatchEvent(new Event('blur', ...));", input);
   ```

**Purpose**: Handles various input field scenarios (readonly, masked, date pickers, etc.).

---

#### ClickElement()
Clicks an element with JavaScript fallback.

**Strategy**:
1. Try standard `element.Click()`
2. If fails:
   - Scroll element into view (center)
   - Wait 200ms
   - Click via JavaScript

**Purpose**: Handles cases where element is not clickable due to overlays, positioning, etc.

---

## Important Concepts

### 1. Activity Month

**Format**: MM/yyyy (e.g., "11/2025" for November 2025)

**Purpose**: Identifies which month the engagement log record applies to.

**Business Rule**: Only one engagement log record allowed per activity month (enforced by duplicate validation).

---

### 2. Case Status Conditional Logic

The form uses **show/hide logic** based on case status:

```
Case Status 01 → Validation error (not allowed)
Case Status 02 → Show assignment section
   ├─ If "Yes" → Worker + Date required, No termination
   └─ If "No" → Show termination section
Case Status 03 → Show termination section, Hide assignment
```

**Implementation**: Server-side update panel logic triggers field visibility changes.

---

### 3. Preassessment Grid

The preassessment grid displays all engagement log records for the case.

**Columns** (typical):
- Activity Month
- Date
- Case Status / Outcome
- Worker (if assigned)
- Termination Date / Reason (if applicable)
- Actions (Edit / Delete)

**Grid ID**: `table[id$='grPreassessments']` or full ID `ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1_grPreassessments`

---

### 4. Delete Confirmation Modal

Delete uses a **confirmation modal** pattern:

```
Click Delete Button
    ↓
Modal appears with message
    ↓
User chooses:
  → "No, return" (Cancel) - Modal closes, no deletion
  → "Yes, delete" (Confirm) - Record deleted
```

**Modal Class**: `.dc-confirmation-modal`

**Modal Location**: Inside `.delete-control` div (nested in row)

---

### 5. Update Panel Waits

Engagement Log uses ASP.NET AJAX Update Panels extensively.

**After most actions**:
```csharp
driver.WaitForUpdatePanel(30);
driver.WaitForReady(30);
Thread.Sleep(500-1500); // Additional stabilization
```

**Purpose**: Ensures page has finished AJAX updates before continuing.

---

### 6. Retry Logic

Several helpers use **retry logic** to handle timing issues:

- `EnsureCaseStatusDropdown()`: Up to 3 attempts to find dropdown
- `TriggerDuplicateMonthValidation()`: Up to 3 attempts to trigger validation
- `FindElementInModalOrPage()`: Polls for element with timeout

**Pattern**:
```csharp
for (var attempt = 1; attempt <= maxAttempts; attempt++) {
    // Try to find/do something
    if (successful) { return result; }
    // Retry logic
    Thread.Sleep(200);
}
throw new Exception("Failed after max attempts");
```

---

### 7. Step Logging

All tests use a `steps` list for detailed logging:

```csharp
var steps = new List<(string Action, string Result)>();
steps.Add(("Action name", "Result description"));

// At end of test:
foreach (var step in steps) {
    _output.WriteLine($"{step.Action}: {step.Result}");
}
```

**Purpose**: Provides detailed test execution trace for debugging.

---

## Coding Standards Applied

### 1. Parameterized Tests 
Uses `[Theory]` with `[MemberData]` to run against multiple PC1 IDs:

```csharp
[Theory]
[MemberData(nameof(GetTestPc1Ids))]
public void TestName(string pc1Id) { ... }
```

### 2. Test Priority 
Uses `[TestPriority]` to ensure sequential execution:

```csharp
[TestPriority(1)]  // Runs first
[TestPriority(2)]  // Runs second
```

### 3. Detailed Logging 
Uses `ITestOutputHelper` for comprehensive logging:

```csharp
_output.WriteLine($"[INFO] Step description: {details}");
_output.WriteLine($"[WARN] Warning message");
```

### 4. Robust Element Finding 
Uses multiple fallback strategies for finding elements:

```csharp
var element = driver.FindElements(By.CssSelector(
    "selector1, selector2, selector3"))
    .FirstOrDefault(el => el.Displayed);
```

### 5. JavaScript Fallbacks 
Uses JavaScript when standard Selenium fails:

```csharp
try {
    element.Click();
} catch {
    js.ExecuteScript("arguments[0].click();", element);
}
```

---

## What to Keep in Mind

### When Modifying Tests

1. **Test Order Matters**: Tests run in priority order. Later tests may depend on data created by earlier tests (e.g., Priority 8 expects a record to exist for duplicate validation).

2. **Activity Month Consistency**: Tests use "11/2025" consistently. Changing this may affect duplicate validation test.

3. **Worker Selection**: Test 9 uses specific worker "Test, Derek" (value 3489). Ensure this worker exists in test environment.

4. **Wait Times Are Critical**: Update panel waits are essential. Reducing wait times may cause intermittent failures.

5. **Stale Elements**: After delete/submit, always re-find grid elements:
   ```csharp
   var gridAfterDelete = driver.WaitforElementToBeInDOM(...);
   var rows = gridAfterDelete.FindElements(...);
   ```

6. **Modal Context**: When interacting with modals, use `FindElementInModalOrPage()` instead of direct `driver.FindElement()`.

---

### When Adding New Tests

1. **Assign Test Priority**: Add `[TestPriority(N)]` with appropriate number
2. **Use Parameterization**: Add `[Theory]` and `[MemberData(nameof(GetTestPc1Ids))]`
3. **Initialize Steps List**: `var steps = new List<(string Action, string Result)>();`
4. **Use SignInAsDataEntry**: Start with `var homePage = SignInAsDataEntry(driver);`
5. **Navigate Consistently**: Use `NavigateToEngagementLog(driver, pc1Id, steps)`
6. **Log All Steps**: Add meaningful steps throughout test
7. **Output Steps**: `foreach (var step in steps) { _output.WriteLine(...); }`

---

### Common Pitfalls

1. **Forgetting Update Panel Waits**: Always wait after clicking buttons or changing dropdowns
2. **Not Handling Retry Logic**: Case status dropdown may not appear immediately, use retry logic
3. **Assuming Modal Context**: Element may be in modal or on page, use flexible finding
4. **Stale Element After Delete**: Always re-find grid after delete confirmation
5. **Incorrect Activity Month Format**: Use "MM/yyyy" format, not "MM/dd/yyyy"
6. **Missing Steps Logging**: Steps provide crucial debugging info, don't skip
7. **Not Checking Conditional Fields**: Verify fields are actually visible before interacting

---

## Test Data Requirements

### Prerequisites
- Valid PC1 IDs in `appsettings.json` under `TestPc1Ids`
- Test user with DataEntry role
- Cases must exist for the PC1 IDs

### Test Creates
- Multiple engagement log records (Case Status 02 and 03)
- Records with activity month "11/2025"
- Records with various termination reasons

### Test Modifies
- None (tests only create and delete)

### Test Deletes
- One preassessment record (Priority 7 test)

**Note**: Test 6 and Test 9 create records that persist. Clean up may be needed if running tests repeatedly.

---

## Running the Tests

### Run All Engagement Log Tests
```bash
dotnet test --filter "FullyQualifiedName~EngagementLogsTests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~EngagementLogsTests.CheckingCaseStatusThreeSubmissionAppearsInGrid"
```

### Run Specific Priority
```bash
# Run only Priority 1 test
dotnet test --filter "FullyQualifiedName~EngagementLogsTests.CheckingTheEngagementLogButton"
```

### Run in Order (Automatic)
The `[TestPriority]` attribute ensures proper execution order automatically.

---

## Troubleshooting

### Test Fails: "Engagement Log link was not found"
- **Issue**: Link not found in Forms tab
- **Solution**: Verify Forms tab loaded, check CSS selectors for engagement log link

### Test Fails: "Case Status dropdown did not appear"
- **Issue**: Dropdown not appearing after advancing from activity month
- **Solution**: Increase wait times, check retry logic in `EnsureCaseStatusDropdown()`

### Test Fails: "Preassessment grid did not contain the record"
- **Issue**: Submitted record not appearing in grid
- **Solution**: Increase wait time after submit, verify submission actually succeeded

### Test Fails: "Row should be removed after delete confirmation"
- **Issue**: Row still present after delete
- **Solution**: Verify delete actually completed, check for error messages, increase wait time

### Test Fails: "Duplicate month validation did not appear"
- **Issue**: Validation not triggered or wrong month used
- **Solution**: Ensure a record already exists for "11/2025", check retry logic

### Test Fails: "Worker 'Test, Derek' not found"
- **Issue**: Specific worker doesn't exist in test environment
- **Solution**: Update test to use a different worker or ensure worker exists

### Test Fails: Stale element reference
- **Issue**: Trying to access element after page update
- **Solution**: Re-find element after AJAX updates or page refreshes

---

## Field Reference

### Activity Month Input
- **ID Pattern**: `input[id$='txtActivityMonth']`
- **Format**: MM/yyyy (e.g., "11/2025")
- **Class**: `form-control mon-year`

### Case Status Dropdown
- **ID Pattern**: `select[id$='ddlCaseStatus']`
- **Values**:
  - "01" - Active < 1 month (Not Allowed)
  - "02" - Parent Enrolls
  - "03" - Engagement Efforts Terminated

### Case Assignment Section
- **Row ID**: `trAssessmentCompleted`
- **Radio Buttons**:
  - Yes: `input[id$='rbtnAssigned']`
  - No: `input[id$='rbtnNotAssigned']`
- **Worker Dropdown**: `select[id$='ddlFSW']`
- **Assignment Date**: `input[id$='txtFSWDate']`

### Termination Section
- **Row ID**: `trEffortsTerminated`
- **Termination Date**: `input[id$='txtTerminationDate']`
- **Termination Reason**: `select[id$='ddlTerminationReason']`
  - Example value: "36" (Participant Refused)

### Grid
- **Table ID**: `table[id$='grPreassessments']`
- **Full ID**: `ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1_grPreassessments`

### Buttons
- **New Form**: `a[id$='btnAdd'].btn.btn-default.pull-right`
- **Add New (in modal)**: `button[id$='btnSubmit'].btn.btn-primary`
- **Final Submit**: `a[id$='btnSubmit'].btn.btn-primary`
- **Delete**: `a[id*='btnDelete'][id$='lbDelete'].btn.btn-danger`
- **Confirm Delete**: `a[id$='lbConfirmDelete'].btn.btn-primary`
- **Cancel Delete**: `button.btn.btn-default` (text: "No, return")

---

