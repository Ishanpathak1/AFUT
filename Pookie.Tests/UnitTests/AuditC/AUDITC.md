# Audit-C Tests Documentation

## Overview

The `AuditCTests.cs` file contains automated end-to-end tests for the Audit-C (Alcohol Use Disorders Identification Test - Consumption) form functionality. These tests validate form navigation, data entry, validation rules, scoring logic, and CRUD operations (Create, Read, Update, Delete).

## Test Structure

### Technology Stack
- **Framework**: xUnit
- **Selenium WebDriver**: Browser automation
- **Test Ordering**: Uses `[TestPriority]` attribute to ensure tests run in sequence
- **Parameterization**: Uses `[Theory]` with `[MemberData]` to run each test against multiple PC1 IDs from configuration

### Test Priority Order
Tests are executed in priority order (1-4) to ensure proper setup and dependency management:

1. **Priority 1**: `CheckingTheAddNewOfAuditCForm` - Initial navigation and form creation
2. **Priority 2**: `CheckingAuditCFormValidationAndSubmission` - Comprehensive validation and business logic testing
3. **Priority 3**: `CheckEditButton` - Edit existing records
4. **Priority 4**: `CheckDeleteButton` - Delete functionality with confirmation flows

## Test Methods

### 1. CheckingTheAddNewOfAuditCForm (Priority 1)

**Purpose**: Validates basic navigation to Audit-C and form creation.

**What it tests**:
- Navigation from Home → Role Selection → Search → Forms Tab → Audit-C
- PC1 ID display verification
- "New Audit-C" button functionality
- Form load verification

**Key Assertions**:
- Home page loads successfully
- PC1 ID appears correctly on Audit-C page
- New Audit-C form displays after clicking button

---

### 2. CheckingAuditCFormValidationAndSubmission (Priority 2)

**Purpose**: Comprehensive test of form validation, scoring logic, and submission.

**What it tests**:

#### A. Initial Validation
- Submitting empty form shows "Question #4 is required!" error
- Date input accepts format "10/12/16"

#### B. Scoring Logic (Progressive Dropdown Selection)
The test validates that the **Total Audit-C Score** calculation works correctly:

1. **After selecting Q4 (How Often)**: Score shows "N/A" or empty
2. **After selecting Q5 (Daily Drinks)**: Score still shows "N/A"
3. **After selecting Q6 (More Than Six)**: Score shows calculated value (e.g., "4")

**Important**: The score only calculates after ALL THREE dropdowns are selected.

#### C. Result Calculation (Positive/Negative)
Tests that the Audit-C result is determined by Q5 (Daily Drinks) selection:
- **Option 1 ("1 or 2")**: Result = "Negative"
- **Options 2-5 ("3 or 4", "5 or 6", "7 to 9", "10 or more")**: Result = "Positive"

#### D. Score Validity
Tests that score validity is properly calculated:
- **Valid**: When all three dropdowns have real values selected
- **Invalid**: When any dropdown is set to "--Select--"

The test validates all 5 options in Q6 (More Than Six) show "valid" when selected.

#### E. Conditional Validation
**Business Rule**: When Q4 (How Often) is NOT set to "1. Never (0)", both Q5 and Q6 become required.

Tests validate:
- Setting Q4 to a non-"Never" value, then submitting with Q5 = "--Select--" shows: "Question #5 is required when question #4 is not set to '1. Never (0)'"
- Setting Q4 to a non-"Never" value, then submitting with Q6 = "--Select--" shows: "Question #6 is required when question #4 is not set to '1. Never (0)'"

#### F. Date and Worker Validation
- Date must be after case start date (validates "10/12/16" fails, then "10/26/25" succeeds)
- Worker selection is required (tests "Question #2 is required" error)
- Selecting "105, Worker" clears worker validation

#### G. Successful Submission
- After all validations pass, form submits successfully
- Success toast message appears containing "Form Saved" and PC1 ID
- Record appears in grid with correct date ("10/26/25" or "10/26/2025") and score ("4")

**Key Assertions**:
- Validation messages appear at correct times
- Score calculation follows business rules
- Positive/Negative result follows Q5 selection
- Grid updates after successful submission

---

### 3. CheckEditButton (Priority 3)

**Purpose**: Tests editing existing Audit-C records.

**What it tests**:
- Finding an existing editable row (prefers rows with "Test" in text)
- Extracting `AuditCPK` from edit link URL
- Clicking edit button navigates to edit form
- Changing Q5 (Daily Drinks) to "Option 4: 7 to 9 (3)"
- Submitting the edited form
- Verifying the grid updates with new values:
  - Total Score = "7"
  - Positive? = "True"

**Key Assertions**:
- Edit button is clickable
- Form loads with existing data
- Changes save successfully
- Grid reflects updated values (score and positive flag)

---

### 4. CheckDeleteButton (Priority 4)

**Purpose**: Tests delete functionality with both cancel and confirm flows.

**What it tests**:

#### A. Cancel Delete Flow
1. Clicks delete button on an existing row
2. Delete confirmation modal appears
3. Clicks "Cancel" button
4. Modal closes
5. Row still exists in grid

#### B. Confirm Delete Flow
1. Clicks delete button on the same row
2. Delete confirmation modal appears
3. Clicks "Confirm/Delete" button
4. Success toast appears with "Form Deleted" and "Audit-C"
5. Row is removed from grid

**Key Assertions**:
- Delete modal appears and closes properly
- Cancel preserves the record
- Confirm removes the record
- Success toast displays after deletion

---

## Helper Methods

### Navigation Helpers
- `NavigateToAuditC()` - Navigates to Audit-C from Forms tab
- `CreateNewAuditCEntry()` - Clicks "New Audit-C" button and waits for form load

### Dropdown Selection Helpers
- `SelectAuditCHowOften()` - Selects Q4 (How Often) dropdown
- `SelectAuditCDailyDrinks()` - Selects Q5 (Daily Drinks) dropdown
- `SelectAuditCMoreThanSix()` - Selects Q6 (More Than Six) dropdown

All dropdown helpers use `WebElementHelper.SelectDropdownOption()` with CSS selectors.

### Data Retrieval Helpers
- `GetAuditCTotalScore()` - Retrieves total score display
- `GetAuditCResult()` - Retrieves Positive/Negative result
- `GetAuditCScoreValidation()` - Retrieves "valid" or "invalid" text

### Form Submission Helpers
- `ClickSubmitButton()` - Clicks submit button with waits
- `SubmitAndCaptureValidation()` - Clicks submit and returns validation summary text
- `GetValidationSummaryText()` - Extracts validation error messages

### Grid Helpers
- `FindAuditCRow()` - Finds row by date and detail text
- `GetExistingEditableAuditCRow()` - Finds an editable row (prefers "Test" rows)
- `FindAuditCRowByPk()` - Finds row by AuditCPK query parameter
- `WaitForAuditCRowByPk()` - Waits for row to appear by PK
- `WaitForAuditCRowRemoval()` - Waits for row to be removed after delete

### Modal Helpers
- `OpenDeleteModal()` - Clicks delete button and waits for modal
- `FindModalElement()` - Finds element within modal
- `IsModalDisplayed()` - Checks if modal is visible
- `WaitForModalToClose()` - Waits for modal to close

### Delete Flow Helpers
- `CancelDeleteFlow()` - Executes cancel delete workflow
- `ConfirmDeleteFlow()` - Executes confirm delete workflow

### Toast Message Helpers
- `WaitForToastMessage()` - Waits for success toast after save
- `WaitForDeleteToastMessage()` - Waits for success toast after delete

### Utility Helpers
- `ExtractQueryParameter()` - Extracts query string parameter from URL

---

## Important Concepts

### 1. Audit-C Scoring Business Logic

The Audit-C has 6 questions, but the tests focus on Q4, Q5, and Q6:

| Question | Dropdown | Options | Score Impact |
|----------|----------|---------|--------------|
| Q4 | How Often | 1-5 (Never to 4+ times/week) | 0-4 points |
| Q5 | Daily Drinks | 1-5 (1-2 to 10+) | 0-4 points |
| Q6 | More Than Six | 1-5 (Never to Daily) | 0-4 points |

**Total Score**: Sum of Q4 + Q5 + Q6 (range: 0-12)
- Score displays as "N/A" until ALL THREE dropdowns are selected
- Score displays as "invalid" if any dropdown is "--Select--" after being selected

### 2. Positive/Negative Result Logic

The result is determined by Q5 (Daily Drinks):
- **Negative**: When Q5 = "1. 1 or 2 (0)"
- **Positive**: When Q5 = any other option (2-5)

This is independent of the total score calculation.

### 3. Conditional Validation

**Rule**: When Q4 (How Often) is NOT "1. Never (0)", then Q5 and Q6 are REQUIRED.

This means:
- If Q4 = "Never", user can leave Q5 and Q6 blank (optional)
- If Q4 = any other value, Q5 and Q6 must be filled in

The tests validate this by:
1. Setting Q4 to "2. Monthly or less" (not Never)
2. Setting Q6 to "--Select--" and verifying Q5 error
3. Setting Q5 to "--Select--" and verifying Q6 error

### 4. Date Validation

The Audit-C date (Q1) must be after the case start date. The tests:
- First try "10/12/16" which fails validation
- Then use "10/26/25" which passes validation

### 5. Worker Selection (Q2)

Worker selection is required. The tests use:
- Worker: "105, Worker"
- Value: "105"

This is selected via `WebElementHelper.SelectWorker()`.

---

## Coding Standards Applied

### 1. CSS Classes Over IDs 
All selectors use CSS classes and semantic attributes instead of ASP.NET generated IDs:

```csharp
// Good - Uses CSS classes and partial IDs
"a#ctl00_ContentPlaceHolder1_ucForms_lnkAuditC.moreInfo, " +
"a[data-formtype='ac'].moreInfo, " +
"a.list-group-item[href*='AuditCs.aspx']"
```

### 2. Use Existing Helper Methods 
Tests leverage:
- `CommonTestHelper.NavigateToFormsTab()` - For login → role → search → forms flow
- `CommonTestHelper.FindPc1Display()` - For PC1 ID verification
- `CommonTestHelper.ClickElement()` - For clicking with JavaScript fallback
- `WebElementHelper.SelectWorker()` - For worker dropdown
- `WebElementHelper.SelectDropdownOption()` - For all dropdown selections
- `WebElementHelper.FindElementInModalOrPage()` - For finding elements
- `WebElementHelper.SetInputValue()` - For setting input fields

### 3. No Unnecessary Try-Catch 
Tests let exceptions bubble up for clear test failures. Try-catch is only used in helper methods for fallback logic (not shown in test methods themselves).

### 4. Clear Error Messages 
All assertions use descriptive messages:

```csharp
Assert.False(string.IsNullOrWhiteSpace(pc1Display), 
    "Unable to locate PC1 ID on Audit-C page.");
```

---

## What to Keep in Mind

### When Modifying Tests

1. **Test Order Matters**: Tests run in priority order. Priority 2 creates a record that Priority 3 and 4 use for edit/delete.

2. **Waits Are Critical**: Always wait for update panels and page ready after interactions:
   ```csharp
   driver.WaitForUpdatePanel(10);
   driver.WaitForReady(10);
   Thread.Sleep(500); // Allow UI to settle
   ```

3. **Stale Element References**: After postbacks, elements may become stale. Re-find elements as needed:
   ```csharp
   // Re-find after postback
   dateInput = WebElementHelper.FindElementInModalOrPage(...);
   ```

4. **Grid Refresh Timing**: After save/delete, the grid may take time to refresh. Increase sleep if needed:
   ```csharp
   Thread.Sleep(1000); // Increased for grid refresh
   ```

5. **Date Format Flexibility**: The grid may display dates as "10/26/25" or "10/26/2025". Tests should handle both:
   ```csharp
   var auditCRow = FindAuditCRow(driver, "10/26/2025", "4");
   if (auditCRow == null)
   {
       auditCRow = FindAuditCRow(driver, "10/26/25", "4");
   }
   ```

### When Adding New Tests

1. **Follow Test Priority Pattern**: Assign appropriate `[TestPriority]` number
2. **Use Parameterization**: Add `[Theory]` and `[MemberData(nameof(GetTestPc1Ids))]`
3. **Start with Navigation**: Use `CommonTestHelper.NavigateToFormsTab()`
4. **Use Existing Helpers**: Check `WebElementHelper` and `CommonTestHelper` first
5. **Log Important Steps**: Use `_output.WriteLine()` with `[INFO]`, `[PASS]`, `[WARN]`, `[DEBUG]` prefixes

### Common Pitfalls

1. **Forgetting to Wait**: Always wait after dropdown changes or button clicks
2. **Validation Order**: Test conditional validation by setting the dependent field LAST
3. **Score Timing**: Remember score is "N/A" until ALL THREE dropdowns are selected
4. **Modal State**: Verify modal is truly open/closed, not just checking existence
5. **PC1 ID Variations**: Different environments may have different PC1 IDs - use configuration

---

## Test Data Requirements

### Prerequisites
- Valid PC1 IDs configured in `appsettings.json` under `TestPc1Ids`
- Test user with appropriate permissions (DataEntry role)
- Case must have a start date before "10/26/25" for date validation tests

### Test Creates
- Multiple Audit-C records during `CheckingAuditCFormValidationAndSubmission`
- At least one final record with date "10/26/25" and score "4"

### Test Modifies
- One existing Audit-C record (changes score to "7" and Positive to "True")

### Test Deletes
- One existing Audit-C record (the one modified in edit test)

---

## Running the Tests

### Run All Audit-C Tests
```bash
dotnet test --filter "FullyQualifiedName~AuditCTests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~AuditCTests.CheckingTheAddNewOfAuditCForm"
```

### Run in Order (Recommended)
The `[TestPriority]` attribute ensures proper execution order automatically.

---

## Troubleshooting

### Test Fails at Navigation
- **Issue**: Cannot find Audit-C link in Forms tab
- **Solution**: Verify Forms tab loads, check CSS selectors for Audit-C link

### Test Fails at Dropdown Selection
- **Issue**: Dropdown not found or option not available
- **Solution**: Check dropdown ID selectors, verify options exist in dropdown

### Test Fails at Score Validation
- **Issue**: Score shows unexpected value or "N/A"
- **Solution**: Ensure all three dropdowns are selected, wait for update panel

### Test Fails at Grid Verification
- **Issue**: Row not found after save
- **Solution**: Increase sleep after save, check date format variations

### Test Fails at Delete
- **Issue**: Modal doesn't appear or row not removed
- **Solution**: Verify delete button selector, check modal visibility logic

