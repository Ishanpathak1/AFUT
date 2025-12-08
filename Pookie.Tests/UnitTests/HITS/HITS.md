# HITS Tests Documentation

## Overview

The `HITSTests.cs` file contains automated end-to-end tests for the HITS (Hurt, Insult, Threaten, Scream) domestic violence screening form. These tests validate form navigation, validation rules, scoring logic, CRUD operations, and proper display of results in the grid.

## What is HITS?

**HITS** is a domestic violence screening tool that asks 4 questions:

1. **Question 4**: How often does your partner physically **HURT** you?
2. **Question 5**: How often does your partner **INSULT** or talk down to you?
3. **Question 6**: How often does your partner **THREATEN** you with harm?
4. **Question 7**: How often does your partner **SCREAM** or curse at you?

Each question has 5 response options (scored 1-5):
- 1. Not at all (1 point)
- 2. Rarely (2 points)
- 3. Sometimes (3 points)
- 4. Fairly Often (4 points)
- 5. Frequently (5 points)

**Total Score**: Sum of Q4-Q7 (range: 4-20)
- **Score < 10**: Negative (low risk)
- **Score ≥ 10**: Positive (indicates possible domestic violence)

## Test Structure

### Technology Stack
- **Framework**: xUnit
- **Test Type**: Theory-based tests with parameterized PC1 IDs
- **Test Ordering**: Uses `[TestPriority]` attribute (1-3)
- **Architecture**: Uses `CommonTestHelper` and `WebElementHelper`

### Test File Overview
- **File**: `HITSTests.cs`
- **Test Count**: 3 main tests
- **Priority Range**: 1-3 (sequential execution)
- **Dependencies**: Requires PC1 IDs from configuration

## Test Execution Order

1. **Priority 1**: Form validation and initial submission (Negative result)
2. **Priority 2**: Edit form and update to Positive result
3. **Priority 3**: Delete with cancel and confirm flows

---

## Detailed Test Breakdown

### Test 1: CheckingHITSFormValidationAndSubmission (Priority 1)

**Purpose**: Comprehensive test of form validation, conditional logic, scoring, and successful submission.

**Test Flow**:

#### Step 1: Submit Empty Form
- Click Submit without filling anything
- Verify **3 validation errors** appear:
  - "Question #1 is required" (Date)
  - "Cannot retrieve workers" (Worker)
  - "Question 8 is required if any of questions 4-7 are not answered"

#### Step 2: Fill Screen Date (Invalid)
- Enter date: **"10/12/16"**
- Date is before case start date (will fail validation later)

#### Step 3: Select Worker
- Select worker from config: `HitsWorkerName` / `HitsWorkerId`

#### Step 4: Answer Question 8 BEFORE Questions 4-7
- Select Q8 (Not Done Reason): **"1. PC1 does not want to disclose"**
- **Purpose**: Tests that Q8 can bypass Q4-Q7 requirements

#### Step 5: Submit with Q8 Answered
- Verify only date validation remains:
  - "Question #1 must be after the case start date" ✓
  - "Question 8 is required..." is GONE ✓

#### Step 6: Clear Question 8
- Reset Q8 to "--Select--"
- **Purpose**: Now Q4-Q7 become required again

#### Step 7: Answer Question 4 Only
- Select Q4 (Hurt): **"1. Not at all"** (1 point)

#### Step 8: Submit with Only Q4 Answered
- Verify validations:
  - "Question #1 must be after the case start date" ✓
  - "Question 8 is required if any of questions 4-7 are not answered" ✓ (because Q5, Q6, Q7 still empty)

#### Step 9: Change Date to Valid
- Change date to: **"10/26/25"**

#### Step 10: Submit with Valid Date
- Verify date validation is GONE ✓
- Q8 validation still present (Q5-Q7 still empty)

#### Step 11: Answer Question 5
- Select Q5 (Insult): **"1. Not at all"** (1 point)
- Verify HITS Score = **"N/A"** (not all questions answered yet)

#### Step 12: Answer Question 6
- Select Q6 (Threaten): **"1. Not at all"** (1 point)
- Verify HITS Score = **"N/A"** (still waiting for Q7)

#### Step 13: Answer Question 7
- Select Q7 (Scream): **"1. Not at all"** (1 point)

#### Step 14: Verify Score Calculation
After ALL 4 questions answered:
- **Total Score**: 4 (1+1+1+1)
- **Result**: Negative (score < 10)
- **Score Validity**: Valid

#### Step 15: Final Submission
- Click Submit
- Wait 2 seconds

#### Step 16: Verify Success and Grid Display
- Success toast contains:
  - "Form Saved"
  - "HITS"
  - "successfully saved"
- Grid row verification:
  - **Column 3** (Date): "10/26/2025"
  - **Column 4** (Total Score): "4"
  - **Column 5** (Positive?): "False"
  - **Column 6** (Invalid?): "False"

**Key Validations Tested**:
1. Required field validation (Date, Worker)
2. Date must be after case start date
3. Q8 bypass logic (if Q8 answered, Q4-Q7 not required)
4. Q4-Q7 progressive scoring (N/A until all answered)
5. Negative result for score = 4

---

### Test 2: CheckingHITSFormEditAndUpdateToPositive (Priority 2)

**Purpose**: Validates editing an existing HITS form and changing result from Negative to Positive.

**Test Flow**:

#### Step 1: Click Edit Button
- Find first HITS form in table (`table#tblHITSs`)
- Click Edit button (`.edit-HITS`)
- Wait for edit form to load

#### Step 2-5: Change All Questions to "Frequently" (5 points each)
- Q4 (Hurt): **"5. Frequently"** (5 points)
- Q5 (Insult): **"5. Frequently"** (5 points)
- Q6 (Threaten): **"5. Frequently"** (5 points)
- Q7 (Scream): **"5. Frequently"** (5 points)

#### Step 6: Verify Updated Score Calculation
- **Total Score**: 20 (5+5+5+5)
- **Result**: Positive (score ≥ 10)

#### Step 7: Submit Updated Form
- Click Submit
- Wait 2 seconds

#### Step 8: Verify Updated Grid Display
- Success toast contains "Form Saved" + "HITS"
- Grid row verification:
  - **Total Score**: "20"
  - **Positive?**: "True" (changed from False)

**Key Concepts Tested**:
- Editing existing HITS forms
- Score recalculation after changes
- Result changes from Negative → Positive when score reaches 10+
- Grid updates reflect changes

---

### Test 3: CheckingHITSFormDeleteWithCancelAndConfirm (Priority 3)

**Purpose**: Validates delete operation with both cancel and confirm flows.

**Test Flow**:

#### Part 1: Cancel Delete Flow
1. Get initial row count (only rows with delete buttons)
2. Click **Delete** button on first HITS form
3. Wait for modal to appear (1 second)
4. Click **"No"** to cancel deletion
5. Wait for modal to close
6. Verify row count is **unchanged** (form still exists)

#### Part 2: Confirm Delete Flow
1. Click **Delete** button again
2. Wait for modal to appear
3. Click **"Yes"** to confirm deletion
4. Wait for deletion to complete (2 seconds)
5. Verify success toast:
   - "Form Deleted"
   - "HITS"
   - "successfully deleted"
6. Verify row count **decreased by 1** (form removed)

**Key Assertions**:
- Initial count > 0 (at least one form exists)
- Cancel preserves the form
- Confirm removes the form
- Success toast appears after deletion
- Final count = initial count - 1

**Delete Button Selector**: `button.delete-HITS, button[id*='btnDeleteHITS']`

**Modal Buttons**:
- **No/Cancel**: `button.btn.btn-default[data-dismiss='modal']`
- **Yes/Confirm**: `a[id*='lbDeleteHITS'].btn-danger.modal-delete`

---

## HITS Scoring Logic

### Question Structure

| Question | Field Name | What it Asks | Dropdown ID |
|----------|------------|--------------|-------------|
| Q1 | HITS Date | Date administered | `txtHITSDate` |
| Q2 | Worker | Worker who administered | Worker dropdown |
| Q4 | Hurt | Physical hurt | `ddlHITSHurt` |
| Q5 | Insult | Insult/talk down | `ddlHITSInsult` |
| Q6 | Threaten | Threaten with harm | `ddlHITSThreaten` |
| Q7 | Scream | Scream/curse | `ddlHITSScream` |
| Q8 | Not Done Reason | Why not completed | `ddlHITSNotDoneReason` |

### Scoring System

**Each question (Q4-Q7) scored 1-5**:
```
1. Not at all      → 0 point
2. Rarely          → 1 points
3. Sometimes       → 2 points
4. Fairly Often    → 3 points
5. Frequently      → 4 points
```

**Total Score**: Sum of Q4 + Q5 + Q6 + Q7 (range: 4-20)

**Score Display**:
- Shows **"N/A"** until ALL 4 questions are answered
- Shows calculated score after all 4 answered

**Result Interpretation**:
- **Score 4-9**: Result = "Negative" (low risk)
- **Score 10-20**: Result = "Positive" (indicates possible domestic violence)

### Score Validity

**Valid**: When all 4 questions (Q4-Q7) have actual values selected (not "--Select--")
**Invalid**: When any question is unanswered or set to "--Select--"

---

## Conditional Logic

### Question 8 Bypass Rule

**Business Rule**: 
```
IF Q8 (Not Done Reason) is answered
THEN Q4-Q7 are optional (screening not completed)
ELSE Q4-Q7 are all required
```

**Q8 Options**:
- "01" - PC1 does not want to disclose
- Other options for why screening wasn't completed

**Use Case**: Allows recording that screening was offered but not completed.

---

## Helper Methods

### Navigation Helpers

#### NavigateToHITS()
Navigates from Forms tab to HITS page.

**Selectors**:
```css
a#ctl00_ContentPlaceHolder1_ucForms_lnkHITS
a[data-formtype='hi'].moreInfo
a.list-group-item[href*='HITSs.aspx']
```

---

### Form Creation/Editing

#### CreateNewHITSEntry()
Clicks "New HITS" button to open blank form.

**Button Selector**:
```css
a#ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1_lnkNewHITS.btn.btn-default.pull-right
a[id$='lnkNewHITS'].btn
a.btn[href*='HITS.aspx']
```

---

#### EditExistingHITSEntry()
Finds and clicks Edit button on first HITS form in table.

**Logic**:
1. Wait for table: `table#tblHITSs`
2. Find Edit button within table: `.edit-HITS` or `a[id*='lnkEditHITS']`
3. Button must have text "Edit"
4. Click using `CommonTestHelper.ClickElement()`

---

### Dropdown Selection

#### SelectHITSQuestion()
Selects an option in a HITS question dropdown.

**Parameters**:
- `dropdownId`: The partial ID (e.g., "ddlHITSHurt")
- `optionText`: Display text (e.g., "1. Not at all")
- `optionValue`: Value to select (e.g., "01")

**Selector Pattern**:
```css
select#ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1_{dropdownId}
select[id$='{dropdownId}']
```

**Usage Example**:
```csharp
SelectHITSQuestion(driver, "ddlHITSHurt", "5. Frequently", "05");
```

---

### Score Retrieval

#### GetHITSScore()
Returns the total HITS score as a string.

**Selector**: `span[id$='lblHITSScore']`
**Returns**: Score value (e.g., "4", "20") or "N/A" if not all questions answered

---

#### GetHITSResult()
Returns the HITS result (Positive/Negative).

**Selector**: `span[id$='lblHITSResult']`
**Returns**: "Positive" or "Negative"

---

#### GetHITSScoreValidity()
Returns the score validity status.

**Selector**: `span[id$='lblHITSScoreValidity']`
**Returns**: "Valid" or empty string

---

### Form Submission

#### SubmitHITSForm()
Finds and clicks the Submit button.

**Selector**:
```css
a#ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1_SubmitHITS_LoginView1_btnSubmit
a[id*='btnSubmit'].btn.btn-primary
a.btn.btn-primary[title*='Save']
```

---

#### SubmitAndCaptureValidation()
Submits form and returns validation summary text.

**Logic**:
1. Call `SubmitHITSForm()`
2. Wait for update panel
3. Find validation summary: `.validation-summary-errors`, `.alert.alert-danger`
4. Return validation text or empty string

---

### Delete Operations

#### ClickDeleteButton()
Finds and clicks Delete button on first HITS form in table.

**Logic**:
1. Find table: `table#tblHITSs`
2. Find Delete button: `.delete-HITS` or `button[id*='btnDeleteHITS']`
3. Button must have text "Delete"
4. Click using `CommonTestHelper.ClickElement()`

---

#### ClickModalNoButton()
Clicks "No" button in delete confirmation modal.

**Selector**:
```css
button.btn.btn-default[data-dismiss='modal']
div.modal-footer button[data-dismiss='modal']
```
Filter: Must contain text "No"

---

#### ClickModalYesButton()
Clicks "Yes" button in delete confirmation modal.

**Selector**:
```css
a#ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1_lbDeleteHITS
a[id*='lbDeleteHITS'].btn-danger.modal-delete
div.modal-footer a.btn-danger
```
Filter: Must contain text "Yes"

---

### Utility Helpers

#### FindElementInModalOrPage()
Finds an element using WebDriverWait.

**Strategy**:
1. Wait up to specified timeout
2. Poll for elements matching selector
3. Return first displayed element
4. Throw error if not found within timeout

---

#### SetInputValue()
Sets input field value and optionally triggers blur.

**Logic**:
1. Clear existing value
2. Send keys
3. If `triggerBlur = true`:
   - Execute JavaScript blur event
   - Wait 300ms
4. Log the action

---

## Important Concepts

### 1. Progressive Score Display

The HITS score displays as **"N/A"** until ALL 4 questions (Q4-Q7) are answered:

```
After Q4 only: Score = N/A
After Q4 + Q5: Score = N/A
After Q4 + Q5 + Q6: Score = N/A
After Q4 + Q5 + Q6 + Q7: Score = 4 (or calculated sum)
```

**Purpose**: Ensures score is only shown when all data is available.

---

### 2. Question 8 Bypass Logic

**Conditional Requirement**:
```
IF any of Q4-Q7 are unanswered
THEN Q8 (Not Done Reason) is REQUIRED
ELSE Q8 is optional
```

**Bypass**:
```
IF Q8 is answered (reason provided)
THEN Q4-Q7 are optional (screening not completed)
```

**Use Cases**:
- PC1 refused to answer
- PC1 doesn't want to disclose
- Other reasons screening couldn't be completed

**Validation Message**:
```
"Question 8 is required if any of questions 4-7 are not answered"
```

---

### 3. Date Validation

**Rule**: HITS Date (Q1) must be **after the case start date**.

**Test Scenarios**:
- "10/12/16" → Fails validation (too old)
- "10/26/25" → Passes validation

**Validation Message**:
```
"Question #1 must be after the case start date"
```

---

### 4. Worker Validation

**Rule**: Worker selection is **always required**.

**Validation Message**:
```
"Cannot retrieve workers"
```
(Appears when no worker is selected)

**Worker Selection**:
Uses `WebElementHelper.SelectWorker(driver, workerName, workerId)` from config.

---

### 5. Score Calculation Examples

| Q4 | Q5 | Q6 | Q7 | Total | Result |
|----|----|----|-------|-------|--------|
| Not at all (1) | Not at all (1) | Not at all (1) | Not at all (1) | **4** | **Negative** |
| Rarely (2) | Rarely (2) | Rarely (2) | Rarely (2) | **8** | **Negative** |
| Sometimes (3) | Sometimes (3) | Sometimes (3) | Not at all (1) | **10** | **Positive** |
| Fairly Often (4) | Fairly Often (4) | Fairly Often (4) | Fairly Often (4) | **16** | **Positive** |
| Frequently (5) | Frequently (5) | Frequently (5) | Frequently (5) | **20** | **Positive** |

**Threshold**: Score ≥ 10 → Positive

---

### 6. Grid Column Structure

The HITS grid (`table#tblHITSs`) typically has these columns:

| Column | Index | Content | Example |
|--------|-------|---------|---------|
| Actions | 1 | Edit/Delete buttons | - |
| Worker | 2 | Worker name | "105, Worker" |
| **Date** | **3** | Screen date | "10/26/2025" |
| **Total Score** | **4** | Sum of Q4-Q7 | "4" or "20" |
| **Positive?** | **5** | True/False | "False" or "True" |
| **Invalid?** | **6** | True/False | "False" |

Tests use `td:nth-child(N)` selectors to verify specific columns.

---

### 7. Delete Modal Pattern

Delete confirmation uses Bootstrap modal:

```
Click Delete
    ↓
Modal appears
    ├─ Title: Confirmation message
    ├─ Body: "Are you sure?"
    ├─ Footer:
    │   ├─ No button (Cancel) - closes modal, no deletion
    │   └─ Yes button (Confirm) - deletes record
    ↓
If Yes clicked:
    ↓
Success toast: "Form Deleted" + "HITS" + "successfully deleted"
    ↓
Grid updates: Row count decreases by 1
```

---


## What to Keep in Mind

### When Modifying Tests

1. **Test Order Matters**: Priority 1 creates a record that Priority 2 edits and Priority 3 deletes.

2. **Score Display Timing**: Score shows "N/A" until ALL 4 questions answered. Don't expect a score before Q7 is answered.

3. **Q8 Bypass Logic**: If Q8 is answered, Q4-Q7 validations don't trigger. Clear Q8 to test Q4-Q7 validation.

4. **Date Format**: Use 2-digit year format ("10/26/25") for consistency.

5. **Grid Column Indices**: Tests use `td:nth-child(N)`. If grid columns change, update indices.

6. **Worker Configuration**: Tests use `_config.TestWorkerName` and `_config.TestWorkerId`. Ensure these are configured.

7. **Waits Are Critical**: Always wait after dropdown changes:
   ```csharp
   driver.WaitForUpdatePanel(10);
   driver.WaitForReady(10);
   Thread.Sleep(500);
   ```

---

### When Adding New Tests

1. **Assign Test Priority**: Add `[TestPriority(N)]` with number > 3
2. **Use Parameterization**: Add `[Theory]` and `[MemberData(nameof(GetTestPc1Ids))]`
3. **Start with Navigation**: Use `CommonTestHelper.NavigateToFormsTab()`
4. **Navigate to HITS**: Call `NavigateToHITS(driver, formsPane)`
5. **Log Steps**: Use `_output.WriteLine()` with `[INFO]` prefix
6. **Use Existing Helpers**: Check `SelectHITSQuestion()`, `GetHITSScore()`, etc.

---

### Common Pitfalls

1. **Expecting Score Before All Questions Answered**: Score is "N/A" until Q4, Q5, Q6, AND Q7 are all answered.

2. **Q8 Validation Confusion**: Remember that Q8 can bypass Q4-Q7 requirements. Clear Q8 if testing Q4-Q7 validation.

3. **Stale Element After Delete**: After deletion, re-find grid elements:
   ```csharp
   var tableRowsAfterDelete = driver.FindElements(By.CssSelector("table#tblHITSs tbody tr"));
   ```

4. **Grid Column Assumptions**: Don't hardcode column indices if grid structure might change.

5. **Date Validation Order**: Date validation must be fixed BEFORE other validations will clear.

6. **Worker Config Missing**: Tests fail if `TestWorkerName` or `TestWorkerId` not in config.

---

## Test Data Requirements

### Prerequisites
- Valid PC1 IDs in `appsettings.json` under `TestPc1Ids`
- Test worker configured: `TestWorkerName` and `TestWorkerId`
- Test user with DataEntry role
- Cases must have start dates before "10/26/25"

### Test Creates
- One HITS form (Priority 1) with:
  - Date: "10/26/25"
  - All questions answered with "Not at all" (1 point each)
  - Total Score: 4
  - Result: Negative

### Test Modifies
- Priority 2 edits the form to:
  - All questions "Frequently" (5 points each)
  - Total Score: 20
  - Result: Positive

### Test Deletes
- Priority 3 deletes the form created by Priority 1

**Net Result**: After all tests run, no HITS forms remain (created, modified, then deleted).

---

## Running the Tests

### Run All HITS Tests
```bash
dotnet test --filter "FullyQualifiedName~HITSTests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~HITSTests.CheckingHITSFormValidationAndSubmission"
```

### Run in Order (Automatic)
The `[TestPriority]` attribute ensures proper execution order.

---

## Troubleshooting

### Test Fails: "HITS link was not found"
- **Issue**: Can't find HITS link in Forms tab
- **Solution**: Verify Forms tab loaded, check CSS selectors

### Test Fails: "New HITS button was not found"
- **Issue**: Button not visible on HITS page
- **Solution**: Verify navigation completed, check button selectors

### Test Fails: Score is "N/A" when expected
- **Issue**: Not all 4 questions answered
- **Solution**: Ensure Q4, Q5, Q6, AND Q7 all have values selected

### Test Fails: Q8 validation appears when it shouldn't
- **Issue**: All Q4-Q7 are answered but Q8 validation still shows
- **Solution**: Verify Q8 dropdown is cleared (set to "--Select--")

### Test Fails: Date validation not clearing
- **Issue**: Date still invalid or wait time too short
- **Solution**: Verify date is after case start date, increase wait time after date entry

### Test Fails: Grid doesn't show updated values
- **Issue**: Grid not refreshed or timing issue
- **Solution**: Increase wait time after submit, verify submit succeeded

### Test Fails: Delete count mismatch
- **Issue**: Row count includes non-deletable rows
- **Solution**: Test correctly filters for rows with delete buttons only

### Test Fails: Worker not found
- **Issue**: Configured worker doesn't exist in test environment
- **Solution**: Update `TestWorkerName` and `TestWorkerId` in config

---

## Field Reference

### Form Fields

| Field | Type | Selector | Required | Notes |
|-------|------|----------|----------|-------|
| **Q1 - Date** | Input | `input[id$='txtHITSDate']` | Yes | Must be after case start date |
| **Q2 - Worker** | Dropdown | Worker dropdown | Yes | From configuration |
| **Q4 - Hurt** | Dropdown | `select[id$='ddlHITSHurt']` | Conditional | Required unless Q8 answered |
| **Q5 - Insult** | Dropdown | `select[id$='ddlHITSInsult']` | Conditional | Required unless Q8 answered |
| **Q6 - Threaten** | Dropdown | `select[id$='ddlHITSThreaten']` | Conditional | Required unless Q8 answered |
| **Q7 - Scream** | Dropdown | `select[id$='ddlHITSScream']` | Conditional | Required unless Q8 answered |
| **Q8 - Not Done Reason** | Dropdown | `select[id$='ddlHITSNotDoneReason']` | Conditional | Required if Q4-Q7 incomplete |

### Display Fields (Read-Only)

| Field | Selector | Content |
|-------|----------|---------|
| **Total Score** | `span[id$='lblHITSScore']` | "N/A" or "4"-"20" |
| **Result** | `span[id$='lblHITSResult']` | "Positive" or "Negative" |
| **Score Validity** | `span[id$='lblHITSScoreValidity']` | "Valid" or "" |

### Grid

| Element | Selector | Notes |
|---------|----------|-------|
| **Table** | `table#tblHITSs` | Main HITS grid |
| **Data Rows** | `tbody tr` | All HITS records |
| **Edit Button** | `a.edit-HITS` | Opens edit form |
| **Delete Button** | `button.delete-HITS` | Opens delete modal |

### Grid Columns (by index)

- Column 1: Actions (Edit/Delete buttons)
- Column 2: Worker
- **Column 3**: Date (validated in tests)
- **Column 4**: Total Score (validated in tests)
- **Column 5**: Positive? (validated in tests)
- **Column 6**: Invalid? (validated in tests)

---


