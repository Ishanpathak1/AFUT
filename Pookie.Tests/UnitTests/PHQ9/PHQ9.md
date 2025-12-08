# PHQ9 Tests Documentation

## Overview

The `PHQ9Test.cs` file contains automated end-to-end tests for the PHQ9 (Patient Health Questionnaire-9) form. These tests validate form navigation, validation rules for required fields (participant and date), date constraint validation, and successful form submission with grid verification.

## What is PHQ9?

**PHQ-9** (Patient Health Questionnaire-9) is a depression screening tool consisting of 9 questions that assess symptoms of depression over the past 2 weeks. Each question is scored 0-3:

- **0**: Not at all
- **1**: Several days
- **2**: More than half the days
- **3**: Nearly every day

**Total Score**: Sum of all 9 questions (range: 0-27)

**Score Interpretation**:
- 0-4: Minimal depression
- 5-9: Mild depression
- 10-14: Moderate depression
- 15-19: Moderately severe depression
- 20-27: Severe depression

**Note**: The current test suite focuses on form navigation, validation, and submission rather than score calculation.

## Test Structure

### Technology Stack
- **Framework**: xUnit
- **Test Type**: Theory-based tests with parameterized PC1 IDs
- **Test Ordering**: Uses `[TestPriority]` attribute (1-2)
- **Architecture**: Uses `CommonTestHelper` and `WebElementHelper`

### Test File Overview
- **File**: `PHQ9Test.cs`
- **Test Count**: 2 main tests
- **Priority Range**: 1-2 (sequential execution)
- **Dependencies**: Requires PC1 IDs from configuration

## Test Execution Order

1. **Priority 1**: Form navigation and creation (basic smoke test)
2. **Priority 2**: Comprehensive validation and submission test

---

## Detailed Test Breakdown

### Test 1: CheckingTheAddNewOfPHQ9Form (Priority 1)

**Purpose**: Basic smoke test to verify navigation to PHQ9 form and ability to open a new form.

**Test Flow**:

#### Step 1: Navigate to Forms Tab
- Login as DataEntry user
- Navigate to case using provided PC1 ID
- Click Forms tab
- Wait for Forms pane to load

#### Step 2: Navigate to PHQ9 List Page
- Click PHQ9 link in Forms pane
- Wait for PHQ9s list page to load
- Verify URL contains "PHQ9s.aspx"

#### Step 3: Verify PC1 ID Display
- Find PC1 ID display on page
- Assert PC1 ID matches expected value

#### Step 4: Open New PHQ9 Form
- Click "New PHQ9" button
- Wait for form page to load
- Verify URL contains:
  - "PHQ9.aspx"
  - "phq9pk=0" (new form)
  - PC1 ID parameter
- Verify form container is present

**Key Assertions**:
- Forms tab loads successfully
- PHQ9 list page is accessible
- PC1 ID is displayed correctly
- New PHQ9 form can be opened
- Form container elements are present

**Purpose**: This test ensures the basic navigation flow works before running more complex validation tests.

---

### Test 2: CheckingPHQ9FormValidationForParticipantField (Priority 2)

**Purpose**: Comprehensive test of form validation rules and successful submission.

**Test Flow**:

#### Part 1: Participant Field Validation

##### Step 1-3: Navigate and Open Form
- Navigate to Forms tab
- Navigate to PHQ9 list page
- Open new PHQ9 form

##### Step 4: Submit Empty Form
- Click Submit button without filling any fields
- Wait for validation to appear

##### Step 5: Verify Participant Validation
- Find validation message (red text, inline display)
- Assert message contains: **"Participant is required"**

**Participant Validation Selectors**:
```css
span[style*='color: red'][style*='display: inline']
span[style*='color:Red'][style*='display:inline']
.text-danger
span.text-danger
span[id*='rfvParticipant']
```

---

#### Part 2: Date Field Validation

##### Step 6: Select Random Participant
- Select a random participant from dropdown (excluding "Other")
- Wait for update panel

##### Step 7: Submit Without Date
- Click Submit button
- Wait for validation

##### Step 8: Verify Date Required Validation
- Find validation message for date
- Assert message contains: **"PHQ date administered"**
- Assert date is required

---

#### Part 3: Case Start Date Validation

##### Step 9: Enter Invalid Date (Before Case Start)
- Enter date: **"10/01/01"** (intentionally before case start date)
- Trigger blur event
- Wait for update

##### Step 10: Submit With Invalid Date
- Click Submit button
- Wait for validation

##### Step 11: Verify Case Start Date Validation
- Find validation message
- Assert message contains: **"on or after the case start date"**

**Date Validation Logic**:
- PHQ Date must be **≥** Case Start Date
- This prevents backdating screenings to before case was opened

---

#### Part 4: Successful Submission

##### Step 12: Enter Valid Date
- Enter date: **"10/25/25"** (valid date after case start)
- Trigger blur event
- Wait for update

##### Step 13: Submit Valid Form
- Click Submit button
- Wait for submission to complete (2 seconds)

##### Step 14: Verify Success Toast
- Capture toast message
- Assert toast contains:
  - "Form Saved"
  - PC1 ID
- Assert toast is not empty

##### Step 15: Verify Grid Display
- Wait for grid to refresh (1 second)
- Find row with date "10/25/2025"
- Assert row exists
- Assert row contains:
  - "10/25/2025"
  - "PHQ9"

**Success Flow**:
```
Valid data → Submit → Toast notification → Grid refresh → Record appears
```

---

## Validation Rules

### 1. Participant Field

**Rule**: Participant selection is **always required**.

**Validation Message**:
```
"Participant is required"
```

**Field Details**:
- **Type**: Dropdown (select)
- **Options**: 
  - `--Select--` (empty, default)
  - `01` - Primary Caregiver 1 (PC1)
  - `02` - Primary Caregiver 2 (PC2)
  - `03` - Other participant
  - `04` - Other (requires specify field - excluded in tests)

**Test Behavior**: Tests randomly select from valid options (excluding "Other" to avoid additional input complexity).

---

### 2. PHQ Date Administered

**Rule 1**: Date field is **required**.

**Validation Message**:
```
"PHQ date administered" (required)
```

**Rule 2**: Date must be **on or after the case start date**.

**Validation Message**:
```
"[Date] must be on or after the case start date"
```

**Date Format**: MM/DD/YY (e.g., "10/25/25")

**Field Details**:
- **Type**: Text input with date picker
- **CSS Classes**: `form-control`, `replaceBlank`, `2dy` (2-digit year)
- **Validation**: Client-side and server-side

**Test Scenarios**:
- Empty date → Required validation
- "10/01/01" (before case start) → Case start date validation
- "10/25/25" (valid date) → Passes validation

---

### 3. Additional Fields (Not Tested)

The PHQ9 form contains 9 depression screening questions and additional fields that are **not covered** by the current test suite:

- 9 depression questions (scored 0-3 each)
- Total score calculation
- Difficulty level (if problems present)
- Suicide ideation question (Question 9)
- Worker selection
- Notes/comments

**Note**: These fields may have their own validation rules and scoring logic tested elsewhere (possibly in Baseline Form tests).

---

## Helper Methods

### Navigation Helpers

#### NavigateToPHQ9()
Navigates from Forms tab to PHQ9 list page.

**Parameters**:
- `driver`: IPookieWebDriver instance
- `formsPane`: IWebElement of Forms tab pane

**Logic**:
1. Find PHQ9 link in Forms pane
2. Click link using `CommonTestHelper.ClickElement()`
3. Wait for page load (30 seconds)
4. Verify URL contains "PHQ9s.aspx"

**Link Selectors**:
```css
a.list-group-item.moreInfo[href*='PHQ9s.aspx']
a.moreInfo[data-formtype='pq']
a.list-group-item[title='PHQ9']
```

**Output Logging**:
- Link text before click
- Current URL after navigation

---

### Form Creation

#### CreateNewPHQ9Entry()
Opens a new PHQ9 form.

**Parameters**:
- `driver`: IPookieWebDriver instance
- `pc1Id`: PC1 ID to verify in URL
- `expectSuccess`: Boolean (default true) - whether to verify form loaded

**Logic**:
1. Find "New PHQ9" button
2. Click button
3. Wait for form page to load (1.5 seconds)
4. If `expectSuccess = true`:
   - Verify URL contains "PHQ9.aspx", "phq9pk=0", and PC1 ID
   - Verify form container is present
   - Log count of form elements found

**Button Selectors**:
```css
a.btn.btn-default.pull-right[href*='PHQ9.aspx'][href*='phq9pk=0']
a.btn[href*='PHQ9.aspx'][href*='phq9pk=0']
```

**URL Parameters**:
- `PHQ9.aspx`: PHQ9 form page
- `phq9pk=0`: Primary key = 0 (new record)
- `pc1id={pc1Id}`: Case identifier

**Form Container Selectors**:
```css
.panel-body
.form-horizontal
form
.container-fluid
```

**Output Logging**:
- Button text before click
- Current URL after navigation
- Form container presence
- Count of form elements (inputs, selects, textareas, checkboxes, radios)

---

### Dropdown Selection

#### SelectRandomParticipant()
Selects a random participant from the dropdown, excluding "Other".

**Parameters**:
- `driver`: IPookieWebDriver instance

**Logic**:
1. Find participant dropdown (select element with option value "01")
2. Create SelectElement wrapper
3. Get all options **except**:
   - Empty/blank options
   - Option value "04" (Other - requires additional input)
4. Select random option from valid list
5. Log selected option text and value

**Dropdown Selector**:
```css
select.form-control (filtered by presence of option[value='01'])
```

**Exclusion Logic**:
- Excludes empty options: `string.IsNullOrWhiteSpace(value)`
- Excludes "Other": `value != "04"`

**Why Exclude "Other"?**
- Selecting "Other" reveals an additional "Other Specify" text input
- This input would require additional data entry
- Tests focus on core validation, not "Other" specify logic

**Output Logging**:
```
[INFO] Selected random participant: {optionText} (value: {optionValue})
```

---

### Grid Verification

#### FindPHQ9Row()
Finds a specific PHQ9 record in the grid by date.

**Parameters**:
- `driver`: IPookieWebDriver instance
- `dateText`: Date string to search for (e.g., "10/25/2025")

**Returns**: 
- `IWebElement?` - Matching row or null if not found

**Logic**:
1. Wait for grid to be present (20 seconds timeout)
2. Find all data rows (rows with `<td>` elements)
3. Iterate through rows
4. Match row text against `dateText` (case-insensitive)
5. Return first matching row or null

**Grid Selectors**:
```css
table.table.table-condensed
table[id*='grPHQ9']
div.panel-body table
```

**Row Filtering**:
- Must be displayed: `tr.Displayed`
- Must have data cells: `tr.FindElements(By.CssSelector("td")).Any()`

**Output Logging**:
- Count of data rows found
- Match result (found/not found)
- Warning if grid not found or no matching row

**Use Case**: Verify that submitted PHQ9 form appears in the grid with correct date.

---

## Important Concepts

### 1. Participant Selection

**Purpose**: Identifies which household member completed the PHQ9 screening.

**Options**:
- **PC1** (Primary Caregiver 1) - Most common
- **PC2** (Primary Caregiver 2) - If applicable
- **Other participant** - Other household members
- **Other (specify)** - Requires text input (excluded from tests)

**Business Rule**: Every PHQ9 must be linked to a specific participant for proper tracking and reporting.

---

### 2. Date Validation Logic

**Two-Part Validation**:

**Part 1: Required Field**
```
IF date field is empty
THEN show "PHQ date administered is required"
```

**Part 2: Case Start Date Constraint**
```
IF date < case start date
THEN show "Date must be on or after the case start date"
```

**Why This Matters**:
- Prevents backdating screenings to before the case existed
- Ensures temporal data integrity
- Aligns with program compliance requirements

**Test Validation**:
- "10/01/01" → Fails (too old, before case start)
- "10/25/25" → Passes (recent date, after case start)

---

### 3. Validation Message Selectors

PHQ9 uses inline validation with red text styling.

**Common Patterns**:
```css
/* Inline style approach */
span[style*='color: red'][style*='display: inline']
span[style*='color:Red'][style*='display:inline']

/* Bootstrap approach */
.text-danger
span.text-danger

/* ASP.NET validator approach */
span[id*='rfvParticipant']  /* RequiredFieldValidator for Participant */
```

**Why Multiple Selectors?**
- ASP.NET Web Forms uses different validation rendering modes
- Different validators use different styling approaches
- Tests must handle all possible DOM structures

**Filtering**:
- Must be displayed: `el.Displayed`
- Must have text: `!string.IsNullOrWhiteSpace(el.Text)`
- For date validation: `el.Text.Contains("date", StringComparison.OrdinalIgnoreCase)`
- For case start validation: `el.Text.Contains("case start date", StringComparison.OrdinalIgnoreCase)`

---

### 4. Form Submission Flow

**Standard Flow**:
```
1. Fill required fields
   ↓
2. Click Submit button
   ↓
3. Wait for AJAX update panel (30 seconds)
   ↓
4. Wait for page ready (30 seconds)
   ↓
5. Wait additional stabilization (1-2 seconds)
   ↓
6. Verify outcome:
   - Validation errors → Check error messages
   - Success → Check toast notification → Check grid
```

**Submit Button Selectors**:
```css
a.btn.btn-primary[title*='Save']
a.btn.btn-primary
```

**Why Link (`<a>`) Instead of Button?**
- ASP.NET Web Forms often renders LinkButtons as `<a>` tags
- JavaScript handles the click event and triggers postback

---

### 5. Toast Notification Pattern

**Success Toast Components**:
- **Message Type**: "Form Saved"
- **Form Name**: "PHQ9" or full form title
- **Context**: PC1 ID
- **Action**: "successfully saved"

**Example Toast**:
```
"Form Saved: PHQ9 for PC1 12345678 successfully saved"
```

**Toast Helper**:
```csharp
var toastMessage = WebElementHelper.GetToastMessage(driver, 1000);
```

**Timeout**: 1000ms to allow toast to appear and be captured

**Assertions**:
1. Toast is not empty
2. Contains "Form Saved"
3. Contains PC1 ID

---

### 6. Grid Display Verification

**Purpose**: Confirms that submitted form persists and appears in the list of PHQ9 records.

**Grid Structure**:
- Table with class `table` and `table-condensed`
- Rows contain date, participant, score, and action buttons
- Most recent records typically appear at top

**Verification Steps**:
1. Wait for grid refresh (1 second)
2. Find grid table element
3. Search for row containing submitted date
4. Assert row exists
5. Assert row contains expected data ("PHQ9", date)

**Why Wait for Refresh?**
- AJAX submission may take time to refresh grid
- Grid data comes from database
- Ensures test doesn't assert before data is loaded

---

### 7. Wait Strategy

PHQ9 tests use multiple wait mechanisms:

**1. Update Panel Wait**:
```csharp
driver.WaitForUpdatePanel(30);
```
- Waits for ASP.NET AJAX UpdatePanel to finish updating
- 30 second timeout (generous for slow environments)

**2. Ready State Wait**:
```csharp
driver.WaitForReady(30);
```
- Waits for browser document.readyState = "complete"
- Ensures page is fully loaded

**3. Thread.Sleep (Stabilization)**:
```csharp
Thread.Sleep(500);   // Brief pause
Thread.Sleep(1000);  // Standard pause
Thread.Sleep(1500);  // Form load pause
Thread.Sleep(2000);  // Submission pause
```
- Allows UI animations and transitions to complete
- Ensures elements are interactable
- Prevents stale element exceptions

**4. WebDriverWait (Element-Specific)**:
```csharp
driver.WaitforElementToBeInDOM(By.CssSelector(...), 20);
```
- Waits for specific element to appear in DOM
- 20 second timeout

**Best Practice**: Always use all three wait types after significant actions:
```csharp
CommonTestHelper.ClickElement(driver, submitButton);
driver.WaitForUpdatePanel(30);
driver.WaitForReady(30);
Thread.Sleep(1000);
```
---

## What to Keep in Mind

### When Modifying Tests

1. **Test Order Matters**: Priority 1 is a smoke test, Priority 2 is comprehensive. Run in order.

2. **Date Format**: Use 2-digit year format ("10/25/25") for consistency with other date fields in the application.

3. **Participant "Other" Exclusion**: Tests exclude "Other" option (value "04") to avoid "Other Specify" field complexity. If testing "Other", add additional logic to fill specify field.

4. **Validation Selector Fragility**: Validation messages use inline styles and ASP.NET validator IDs. If validation rendering changes, update selectors.

5. **Grid Refresh Timing**: After submission, wait at least 1 second for grid to refresh. Increase if running in slow environments.

6. **PC1 ID Configuration**: Tests use `_config.TestPc1Ids`. Ensure valid PC1 IDs exist in `appsettings.json`.

7. **Case Start Date Dependency**: Invalid date test ("10/01/01") assumes case start date is after 2001. If testing with very old cases, adjust date.

---

### When Adding New Tests

1. **Assign Test Priority**: Add `[TestPriority(N)]` with number > 2

2. **Use Parameterization**: Add `[Theory]` and `[MemberData(nameof(GetTestPc1Ids))]`

3. **Start with Navigation**: Use `CommonTestHelper.NavigateToFormsTab()`

4. **Navigate to PHQ9**: Call `NavigateToPHQ9(driver, formsPane)`

5. **Log All Steps**: Use `_output.WriteLine()` with appropriate prefix (`[PASS]`, `[INFO]`, `[WARN]`)

6. **Reuse Helpers**: Check existing helper methods before writing new code

7. **Test New Validation Rules**: If adding validation tests, follow the pattern:
   - Submit with invalid data
   - Capture validation message
   - Assert message contains expected text
   - Fix data and submit again

8. **Test Score Calculation**: If adding PHQ9 scoring tests, refer to Baseline Form PHQ9 tests as they may already cover this

---

### Common Pitfalls

1. **Validation Message Not Found**: 
   - **Issue**: Validation selector doesn't match rendered DOM
   - **Solution**: Inspect actual DOM, add more selector variants

2. **Stale Element After Submission**:
   - **Issue**: Element reference becomes stale after AJAX update
   - **Solution**: Re-find element after update panel refresh

3. **Grid Row Not Found**:
   - **Issue**: Grid hasn't refreshed yet or date format mismatch
   - **Solution**: Increase wait time, verify exact date format in grid

4. **Participant Dropdown Not Found**:
   - **Issue**: Form not fully loaded or dropdown selector too specific
   - **Solution**: Wait longer, use multiple selectors

5. **Toast Message Missed**:
   - **Issue**: Toast appears and disappears quickly
   - **Solution**: Call `GetToastMessage` immediately after submission

6. **Date Validation Passes When It Shouldn't**:
   - **Issue**: Test date "10/01/01" is actually after case start date
   - **Solution**: Use even older date or verify case start date for test PC1 IDs

7. **Form Container Not Found**:
   - **Issue**: Page navigation failed or timeout too short
   - **Solution**: Verify URL navigation succeeded, increase wait timeout

---

## Test Data Requirements

### Prerequisites
- Valid PC1 IDs in `appsettings.json` under `TestPc1Ids`
- Test user with DataEntry role credentials
- Cases must have:
  - Start dates before "10/25/25" (for valid date test)
  - Start dates after "10/01/01" (for invalid date test)
- At least one participant option available in dropdown

### Recommended Test Data
- **PC1 ID**: Any valid case ID
- **Case Start Date**: Between 10/02/01 and 10/24/25 (to satisfy both validation scenarios)

### Test Creates
- One PHQ9 form with:
  - Random participant (excluding "Other")
  - Date: "10/25/25"
  - No score data (minimal form submission)

### Test Does NOT Delete
- PHQ9 records are **not deleted** by these tests
- Records accumulate in the grid
- Consider periodic manual cleanup or add delete test

---

## Running the Tests

### Run All PHQ9 Tests
```bash
dotnet test --filter "FullyQualifiedName~PHQ9Tests"
```

### Run Specific Test
```bash
# Smoke test only
dotnet test --filter "FullyQualifiedName~PHQ9Tests.CheckingTheAddNewOfPHQ9Form"

# Validation test only
dotnet test --filter "FullyQualifiedName~PHQ9Tests.CheckingPHQ9FormValidationForParticipantField"
```

### Run in Order (Automatic)
The `[TestPriority]` attribute ensures proper execution order.

---

## Troubleshooting

### Test Fails: "PHQ9 link was not found"
- **Issue**: Can't find PHQ9 link in Forms tab
- **Solution**: 
  - Verify Forms tab loaded successfully
  - Check if PHQ9 is available for this case type
  - Verify link selectors match actual DOM

### Test Fails: "New PHQ9 button was not found"
- **Issue**: Button not visible on PHQ9 list page
- **Solution**: 
  - Verify navigation to PHQ9s.aspx succeeded
  - Check user permissions (DataEntry role should have access)
  - Verify button selectors

### Test Fails: "Participant dropdown was not found"
- **Issue**: Dropdown element not located
- **Solution**: 
  - Verify form page loaded completely
  - Increase wait time before searching for dropdown
  - Check if form structure has changed

### Test Fails: "No valid participant options found"
- **Issue**: Dropdown exists but has no selectable options
- **Solution**: 
  - Verify case has participants (PC1, PC2, etc.)
  - Check database to ensure participant records exist
  - Verify test data configuration

### Test Fails: Validation message not found
- **Issue**: Validation triggered but message selector doesn't match
- **Solution**: 
  - Inspect actual validation message DOM structure
  - Add additional selector variants
  - Check if ASP.NET validation mode changed

### Test Fails: "PHQ date administered must be on or after case start date" appears incorrectly
- **Issue**: Date "10/25/25" is actually before case start date
- **Solution**: 
  - Check case start date in database for test PC1 ID
  - Use a more recent date in the test
  - Or use a test case with earlier start date

### Test Fails: Toast message not found
- **Issue**: Submission succeeded but toast not captured
- **Solution**: 
  - Increase wait time before calling `GetToastMessage`
  - Verify submission actually succeeded (check for validation errors)
  - Check if toast notification system changed

### Test Fails: Grid row not found after submission
- **Issue**: Record not appearing in grid
- **Solution**: 
  - Increase wait time after submission (2-3 seconds)
  - Verify submission succeeded (check toast)
  - Check date format in grid (may display as "10/25/2025" not "10/25/25")
  - Verify grid selector matches actual grid element

### Test Fails: Timeout waiting for elements
- **Issue**: Elements not appearing within timeout period
- **Solution**: 
  - Increase timeout values (currently 10-30 seconds)
  - Check network latency and server performance
  - Verify test environment is running properly
  - Check browser console for JavaScript errors

---

## Field Reference

### Form Fields

| Field | Type | Selector | Required | Notes |
|-------|------|----------|----------|-------|
| **Participant** | Dropdown | `select.form-control` (with option value="01") | Yes | Identifies who completed screening |
| **PHQ Date Administered** | Input | `input.form-control.replaceBlank`, `input[class*='2dy']` | Yes | Must be ≥ case start date |
| **PHQ9 Questions (9)** | Radio buttons | Not tested | Conditional | Each scored 0-3 |
| **Total Score** | Display | Not tested | Calculated | Sum of 9 questions (0-27) |
| **Difficulty Level** | Radio buttons | Not tested | Conditional | If any problems present |
| **Worker** | Dropdown | Not tested | Likely required | Who administered screening |
| **Notes** | Textarea | Not tested | Optional | Additional comments |

**Note**: Current tests only validate Participant and Date fields. Other fields may have their own validation logic not covered here.

---

### Grid

| Element | Selector | Notes |
|---------|----------|-------|
| **Table** | `table.table.table-condensed`, `table[id*='grPHQ9']` | Main PHQ9 grid |
| **Data Rows** | `tbody tr` (with `td` elements) | All PHQ9 records |
| **Edit Button** | Not documented | Opens edit form (not tested) |
| **Delete Button** | Not documented | Deletes record (not tested) |

**Grid Columns** (typical structure, not validated):
- Date Administered
- Participant
- Total Score
- Severity Level
- Actions (Edit/Delete)

---

### Validation Messages

| Validation | Selector Pattern | Message Text |
|------------|------------------|--------------|
| **Participant Required** | `span[id*='rfvParticipant']`, `.text-danger` | "Participant is required" |
| **Date Required** | `.text-danger` (contains "date") | "PHQ date administered [is required]" |
| **Date vs Case Start** | `.text-danger` (contains "case start date") | "[Date] must be on or after the case start date" |

---
