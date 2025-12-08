# Discharge Tests Documentation

## Overview

The **Discharge** test suite contains automated end-to-end tests for the Discharge form functionality. These tests validate case discharge workflows, including date entry, discharge reason selection with conditional "Other specify" validation, form submission, and case reinstatement.

## What is Discharge?

**Discharge** is the process of formally closing a case when a participant is no longer receiving home visiting services. The discharge form captures the date and reason for discharge, allowing programs to track case closure information and outcomes.

**Purpose**:
- Document case closure date and reason
- Track discharge reasons for reporting and analysis
- Support case reinstatement if participant returns to services
- Maintain accurate case status and history

**Workflow**:
```
Navigate to Forms Tab
    ↓
Click "Discharge" Link
    ↓
Enter Discharge Date
    ↓
Submit to Proceed to Reason Selection
    ↓
Select Discharge Reason
    ├─ If "Other" selected → Enter Specify Text (required)
    └─ If specific reason → No additional input needed
    ↓
Submit Form
    ↓
Success Toast: "Discharge saved"
    ↓
Case Status Changes to "Discharged"
    ↓
(Optional) Click "Reinstate" Button
    ↓
Success Toast: "Case Reinstated"
    ↓
Case Status Returns to "Active"
```

---

## Test File Overview

| File | Tests | Primary Focus |
|------|-------|---------------|
| **DischargeTests.cs** | 4 | Navigation, discharge reason validation, conditional fields, reinstatement |

**Total Tests**: 4 (all parameterized with PC1 IDs)

**Test Priorities**: Tests are ordered 1-4 using `[TestPriority]` attribute for sequential execution.

---

## Test Execution Order

Tests run in priority order:

1. **Priority 1**: Navigate to Discharge form
2. **Priority 2**: Submit Discharge form with validation (includes "Other specify" validation)
3. **Priority 3**: Validate conditional fields for specific discharge reasons (18, 21, 25, 37)
4. **Priority 4**: Reinstate case from Discharge form

---

## Detailed Test Breakdown

### Test 1: NavigateToDischargeForm (Priority 1)

**Purpose**: Smoke test to verify navigation to Discharge form page.

**Test Flow**:

#### Part 1: Navigate to Forms Tab
1. Sign in as DataEntry user
2. Search for case by PC1 ID
3. Open Forms tab

#### Part 2: Navigate to Discharge Form
4. Find Discharge link in Forms pane:
   - `a.moreInfo[href*='Discharge.aspx']`
   - `a.moreInfo[data-formtype='dc']`
5. Click Discharge link
6. Wait for page load

#### Part 3: Verify Page Loaded
7. Assert URL contains `discharge.aspx` OR `PreDischarge.aspx`
8. Assert URL contains PC1 ID
9. Verify form container is present
10. Find and verify PC1 ID display on page

**Discharge Pages**:
- **PreDischarge.aspx**: New discharge (case not yet discharged)
- **discharge.aspx**: Existing discharge (case already discharged, can reinstate)

**Assertions**:
- Discharge form page loaded
- PC1 ID appears in URL and on page
- Form container is present

---

### Test 2: SubmitDischargeFormWithValidation (Priority 2)

**Purpose**: Comprehensive test of discharge form submission including date entry, discharge reason selection, "Other specify" validation, and successful save.

**Test Flow**:

#### Part 1: Navigate to Discharge Form
1-6. Same as Test 1 (navigate to Discharge form)

#### Part 2: Enter Discharge Date
7. Find discharge date input: `input.form-control[class*='2dy']`
8. Enter today's date: `MM/dd/yy` format
9. Trigger blur event
10. Wait for page update

#### Part 3: Submit to Proceed to Reason Selection
11. Find Submit button: `a.btn.btn-primary` with text "Submit"
12. Click Submit
13. Wait for page update

**Note**: First submission with date proceeds to reason selection screen

#### Part 4: Select "Other" as Discharge Reason
14. Find discharge reason dropdown: `select[id*='ddlDischargeReason']`
15. Select "Other" (value "99")
16. Trigger change event via JavaScript
17. Wait for page update (1500ms)

#### Part 5: Verify "Specify" Field Appears
18. Wait for Specify text box: `input[id*='Specify']`
19. Assert Specify text box is **displayed**

**Conditional Logic**: When "Other" is selected, a "Specify" text input appears (required).

#### Part 6: Submit Without Filling Specify → Validation
20. Find Submit button
21. Click Submit
22. Wait for validation

#### Part 7: Verify Validation Message
23. Find validation message: `span[style*='color:Red']` or `span[id*='rfvSpecify']`
24. Assert validation message is **displayed**

**Expected Validation**: "Specify" field is required when "Other" is selected

#### Part 8: Select Valid (Non-"Other") Reason
25. Find discharge reason dropdown again
26. Get all valid options (exclude "Other" and placeholder)
27. Select random valid reason
28. Wait for page update

#### Part 9: Submit with Valid Data
29. Find Submit button
30. Click Submit
31. Wait for submission (2000ms)

#### Part 10: Verify Success Toast
32. Get toast message (wait up to 1500ms)
33. Assert toast is not empty
34. Assert toast contains "saved"
35. Assert toast contains PC1 ID

**Success Toast Example**: "Discharge saved for PC1 AB12345678"

---

### Test 3: ValidateConditionalFieldsForSpecificDischargeReasons (Priority 3)

**Purpose**: Comprehensive test of discharge reasons that trigger conditional fields and validation messages.

**Test Flow**:

#### Part 1: Navigate to Discharge Form
1-6. Same as Test 1 (navigate to Discharge form)

#### Part 2: Enter Discharge Date
7-10. Same as Test 2 (enter discharge date)

#### Part 3: Submit to Proceed to Reason Selection
11-13. Same as Test 2 (submit to reach reason selection)

#### Part 4: Test Option 18 - Target Child Death
14. Select discharge reason "18" (Target Child Death)
15. Trigger change event via JavaScript
16. Wait for page update (1500ms)

#### Part 5: Verify Target Child DOD Field Appears
17. Find DOD container: `div[id='divTargetChildDOD']`
18. Assert DOD container is **displayed**
19. Find DOD input: `input.form-control[id*='txtTargetChildDOD']`
20. Assert DOD input is **displayed**

#### Part 5a: Test Validation - Submit Without DOD
21. Find Submit button
22. Click Submit without entering DOD
23. Wait for validation
24. Verify validation message: "Missing Target Child DOD"
25. Assert validation message is displayed

#### Part 5b: Fill DOD Field
26. Enter a date (30 days ago) in MM/dd/yy format
27. Trigger blur event

**Conditional Logic**: When option 18 is selected, Target Child DOD field appears (required).
**Validation**: Submitting without DOD shows "Missing Target Child DOD" error.

#### Part 6: Test Option 21 - PC1 Death
28. Select discharge reason "21" (PC1 Death)
29. Trigger change event via JavaScript
30. Wait for page update (1500ms)

#### Part 7: Verify PC1 DOD Field Appears
31. Find DOD container: `div[id='divPC1DOD']`
32. Assert DOD container is **displayed**
33. Find DOD input: `input.form-control[id*='txtPC1DOD']`
34. Assert DOD input is **displayed**

#### Part 7a: Test Validation - Submit Without DOD
35. Find Submit button
36. Click Submit without entering DOD
37. Wait for validation
38. Verify validation message: "Missing PC1 DOD"
39. Assert validation message is displayed

#### Part 7b: Fill DOD Field
40. Enter a date (30 days ago) in MM/dd/yy format
41. Trigger blur event

**Conditional Logic**: When option 21 is selected, PC1 DOD field appears (required).
**Validation**: Submitting without DOD shows "Missing PC1 DOD" error.

#### Part 8: Test Option 25 - Transferred to Another Program
42. Select discharge reason "25" (Transferred to another program)
43. Trigger change event via JavaScript
44. Wait for page update (1500ms)

#### Part 9: Verify List Program Field Appears
45. Find transfer container: `div[id='divTransferredtoProgram']`
46. Assert transfer container is **displayed**
47. Find program input: `input.form-control[id*='txtTransferredtoProgram']`
48. Assert program input is **displayed**

#### Part 9a: Test Validation - Submit Without Program Name
49. Find Submit button
50. Click Submit without entering program name
51. Wait for validation
52. Verify validation message: "Missing Transfer to Program"
53. Assert validation message is displayed

#### Part 9b: Fill Program Field
54. Enter program name: "Test Transfer Program"
55. Trigger blur event

**Conditional Logic**: When option 25 is selected, "List program" text field appears (required).
**Validation**: Submitting without program name shows "Missing Transfer to Program" error.

#### Part 10: Test Option 37 - Transfer to Another HFNY Program
56. Select discharge reason "37" (Transfer to another HFNY program)
57. Trigger change event via JavaScript
58. Wait for page update (1500ms)

#### Part 10a: Test Validation - Submit Without Program Selection
59. Find Submit button
60. Click Submit without selecting program
61. Wait for validation
62. Verify validation message: "Missing Transfer to Program"
63. Assert validation message is displayed

**First Validation**: Must select a program from dropdown before proceeding.

#### Part 10b: Select Transfer Program
64. Find program dropdown: `select[id*='ddlTransferredtoProgramFK']`
65. Get all valid program options (exclude "--Select--")
66. Select random valid program from dropdown
67. Wait for page update

**Program Dropdown**: Contains multiple HFNY programs (Program 2, Program 3, etc.)

#### Part 10c: Check Acknowledgment Checkbox
68. Find acknowledgment checkbox: `input[type='checkbox'][id*='chkAcknowledgeRemoval']`
69. Check if already checked
70. If not checked, click to check it
71. Wait for page update

**Acknowledgment**: User must acknowledge that discharge-related forms (FollowUp, PSI) will be removed when transfer is accepted.

#### Part 10d: Submit and Verify Supervisory Approval Message
72. Find Submit button
73. Click Submit
74. Wait for submission (2000ms)
75. Search for approval message containing "supervisory approval"
76. If not found on page, check toast message
77. Assert message contains "supervisory approval"
78. Assert message indicates forms require approval before transfer

**Expected Message**: "There are forms for this case which require supervisory approval before transfer to another HFNY program."

**Conditional Logic**: When option 37 is selected with program and acknowledgment, validation message appears preventing submission if forms require supervisory approval.

---

### Test 4: ReinstateCaseFromDischargeForm (Priority 4)

**Purpose**: Test case reinstatement functionality to restore a discharged case to active status.

**Test Flow**:

#### Part 1: Navigate to Discharge Form
1-6. Same as Test 1 (navigate to Discharge form)

**Note**: If case is already discharged (from Test 2), the Discharge page will show reinstatement option.

#### Part 2: Find and Click Reinstate Button
7. Find Reinstate button: `a.btn.btn-warning[id*='btnReinstate']`
8. Assert button is displayed
9. Click Reinstate button
10. Wait for page update (2000ms)

#### Part 3: Verify Reinstate Success Toast
11. Get toast message (wait up to 1500ms)
12. Assert toast is not empty
13. Assert toast contains "Case Reinstated"
14. Assert toast contains "reinstated"
15. Assert toast contains PC1 ID

**Success Toast Example**: "Case Reinstated - PC1 AB12345678 has been reinstated successfully"

**Result**: Case status changes from "Discharged" back to "Active"

---

## Form Fields

### Discharge Form

| Field | Selector | Required | Conditional | Notes |
|-------|----------|----------|-------------|-------|
| **Discharge Date** | `input.form-control[class*='2dy']` | Yes | - | Date in MM/dd/yy format |
| **Discharge Reason** | `select[id*='ddlDischargeReason']` | Yes | - | Dropdown with multiple reasons + "Other" |
| **Discharge Reason Specify** | `input[id*='Specify']` | Conditional | If Reason = "Other" (99) | Visible only when "Other" selected |
| **Target Child DOD** | `input.form-control[id*='txtTargetChildDOD']` | Conditional | If Reason = 18 | Date of Death for Target Child |
| **PC1 DOD** | `input.form-control[id*='txtPC1DOD']` | Conditional | If Reason = 21 | Date of Death for PC1 |
| **List Program (text)** | `input.form-control[id*='txtTransferredtoProgram']` | Conditional | If Reason = 25 | Program name for transfer |
| **List Program (dropdown)** | `select.form-control[id*='ddlTransferredtoProgramFK']` | Conditional | If Reason = 37 | Select HFNY program for transfer |
| **Acknowledgment Checkbox** | `input[type='checkbox'][id*='chkAcknowledgeRemoval']` | Conditional | If Reason = 37 | Acknowledge form removal on transfer |

### Discharge Reasons

**Common Reasons** (vary by implementation):
- Completed program
- Moved out of service area
- Refused services
- Lost contact
- Ineligible for services
- Child removed from home
- **18**: Target Child Death - Requires DOD (Date of Death) field
- **21**: PC1 Death - Requires DOD (Date of Death) field
- **25**: Transferred to another program - Requires "List program" text field
- **37**: Transfer to another HFNY program - Shows supervisory approval message on submit
- **99**: Other - Requires specify text

---

## Buttons

| Button | Selector | Purpose | Availability |
|--------|----------|---------|--------------|
| **Submit** | `a.btn.btn-primary` | Submit discharge form | Always (on discharge form) |
| **Reinstate** | `a.btn.btn-warning[id*='btnReinstate']` | Reinstate discharged case | Only if case is already discharged |

---

## Helper Methods Summary

### Navigation

#### NavigateToDischargeForm()
Navigate from Forms pane to Discharge form page.

**Parameters**:
- `driver`: Web driver
- `formsPane`: Forms tab pane element
- `pc1Id`: PC1 ID for verification

**Flow**:
1. Find Discharge link in Forms pane
2. Click link
3. Wait for page load
4. Verify URL contains `discharge.aspx` or `PreDischarge.aspx`
5. Verify URL contains PC1 ID
6. Verify form container is present

**Selectors Tried**:
```css
a.list-group-item.moreInfo[href*='Discharge.aspx']
a.list-group-item.moreInfo[href*='PreDischarge.aspx']
a.moreInfo[data-formtype='dc']
a.list-group-item[title='Discharge']
```

**Assertions**:
- Page URL is correct
- Form container exists

---

## Important Concepts

### 1. Two-Step Discharge Process

**Step 1: Enter Discharge Date**
- Enter date
- Submit
- Proceeds to Step 2

**Step 2: Select Discharge Reason**
- Select reason from dropdown
- If "Other": Fill specify field
- Submit
- Case is discharged

**Why Two Steps?**: Separation ensures proper date entry before proceeding to reason selection.

---

### 2. Conditional "Other Specify" Field

**Rule**: When "Other" discharge reason is selected, a "Specify" text input appears and becomes **required**.

**Logic**:
```
IF Discharge Reason = "Other" (value "99")
THEN
    "Specify" text input → Visible
    "Specify" text input → Required
ELSE
    "Specify" text input → Hidden
```

**Validation**: Submitting without filling "Specify" when "Other" is selected triggers validation error.

**Test Coverage**: Test 2 validates this conditional logic thoroughly.

---

### 3. Conditional Date of Death (DOD) Fields

**Rule**: When specific death-related discharge reasons are selected, a DOD (Date of Death) field appears and becomes **required**.

#### Option 18: Target Child Death

**Logic**:
```
IF Discharge Reason = 18 (Target Child Death)
THEN
    Container "divTargetChildDOD" → Visible
    Input "txtTargetChildDOD" → Required
    Label: "DOD:" appears
```

**Field Details**:
- Date input with calendar picker (`.input-group.date`)
- Uses 2-digit year format (`class="form-control 2dy"`)
- Date input selector: `input[id*='txtTargetChildDOD']`

#### Option 21: PC1 Death

**Logic**:
```
IF Discharge Reason = 21 (PC1 Death)
THEN
    Container "divPC1DOD" → Visible
    Input "txtPC1DOD" → Required
    Label: "DOD:" appears
```

**Field Details**:
- Date input with calendar picker (`.input-group.date`)
- Uses 2-digit year format (`class="form-control 2dy"`)
- Date input selector: `input[id*='txtPC1DOD']`

**Test Coverage**: Test 3 validates both DOD fields appear, are accessible, and show appropriate validation messages when submitted empty.

---

### 4. Conditional "List Program" Field

**Rule**: When discharge reason 25 (Transferred to another program) is selected, a "List program" text field appears and becomes **required**.

**Logic**:
```
IF Discharge Reason = 25 (Transferred to another program)
THEN
    Container "divTransferredtoProgram" → Visible
    Input "txtTransferredtoProgram" → Required
    Label: "List program" appears
```

**Field Details**:
- Standard text input (`class="form-control"`)
- Used to specify the program name participant is transferring to
- Input selector: `input[id*='txtTransferredtoProgram']`

**Test Coverage**: Test 3 validates this field appears, shows validation when empty, and accepts input.

---

### 5. Comprehensive Validation Testing

**Test 3 Validation Approach**: For each discharge reason with conditional fields, the test follows a pattern:

1. **Field Appearance**: Verify conditional field appears when reason is selected
2. **Empty Submission**: Submit without filling required field
3. **Validation Message**: Verify appropriate error message appears
4. **Fill Field**: Enter valid data in the field
5. **Continue**: Proceed to next test

**Benefits of This Approach**:
- Ensures validation is working correctly
- Confirms error messages are displayed to users
- Tests complete user workflow (with mistakes)
- Validates field requirements are enforced

**Important Implementation Detail**: After validation appears, elements may become stale (their DOM references are no longer valid). The test handles this by **re-finding elements** after validation before attempting to fill them with valid data. This pattern prevents `StaleElementReferenceException` errors.

**Validation Testing by Option**:
- **Option 18**: Validates "Missing Target Child DOD" error
- **Option 21**: Validates "Missing PC1 DOD" error
- **Option 25**: Validates "Missing Transfer to Program" error
- **Option 37**: Validates "Missing Transfer to Program" error, then supervisory approval message

---

### 6. Supervisory Approval Message for Option 37

**Rule**: When discharge reason 37 (Transfer to another HFNY program) is selected and submitted, a validation message appears if forms require supervisory approval.

**Logic**:
```
IF Discharge Reason = 37 (Transfer to another HFNY program)
AND Forms require supervisory approval
THEN
    Show message: "There are forms for this case which require 
                   supervisory approval before transfer to 
                   another HFNY program."
    Prevent submission
```

**Message Location**: Can appear either:
- On page as text/alert
- In toast notification

**Purpose**: Ensures forms requiring supervisor review are approved before case transfer.

**Additional Requirements for Option 37**:
- Must select a program from dropdown (`ddlTransferredtoProgramFK`)
- Must check acknowledgment checkbox (`chkAcknowledgeRemoval`)
- Validation occurs in two stages:
  1. First validates program is selected ("Missing Transfer to Program")
  2. Then validates supervisory approval requirement

**Test Coverage**: Test 3 validates:
1. "Missing Transfer to Program" validation when program not selected
2. Program dropdown selection
3. Acknowledgment checkbox checking
4. Supervisory approval message after complete submission

---

### 7. Discharge Pages

**PreDischarge.aspx**:
- Shown when case is **not yet discharged**
- Allows entering discharge date and reason
- Submit button available

**discharge.aspx** (or similar):
- Shown when case is **already discharged**
- Displays existing discharge information
- **Reinstate** button available (instead of Submit)

**Test Handling**: Tests accept either page URL for flexibility.

---

### 8. Case Reinstatement

**Purpose**: Restore a discharged case to active status if participant returns to services.

**Button**: `a.btn.btn-warning` (warning color indicates important action)

**Flow**:
```
Click "Reinstate" button
    ↓
Case status changes: Discharged → Active
    ↓
Success toast displays: "Case Reinstated"
    ↓
Case history records reinstatement
```

**Use Cases**:
- Participant returns to services
- Discharge was entered in error
- Participant re-enrolls

**Test Coverage**: Test 4 validates reinstatement functionality.

---

### 9. Date Format

**Format**: `MM/dd/yy` (short year format)

**Example**: `12/07/25` for December 7, 2025

**Input Class**: `[class*='2dy']` - Indicates 2-digit year date picker

**Test Uses**: `DateTime.Now.ToString("MM/dd/yy")`

---

### 10. jQuery Toast Notifications

**Success Toasts**:

1. **Discharge Saved**:
```
"Discharge saved for PC1 AB12345678"
```
- Contains: "saved"
- Contains: PC1 ID

2. **Case Reinstated**:
```
"Case Reinstated - PC1 AB12345678 has been reinstated successfully"
```
- Contains: "Case Reinstated"
- Contains: "reinstated"
- Contains: PC1 ID

**Wait Strategy**: Poll for up to 1500ms for toast to appear.

---

### 11. Validation Selectors

**Validation Message Selectors** (multiple tried for robustness):
```css
span[style*='color:Red']
span[style*='color: red']
.text-danger
span[id*='rfvSpecify']
```

**Why Multiple?**: ASP.NET validation can render in different formats depending on configuration.

**Validation Messages by Discharge Reason**:
- **Option 18**: "Missing Target Child DOD" - shown when DOD field is not filled
- **Option 21**: "Missing PC1 DOD" - shown when DOD field is not filled
- **Option 25**: "Missing Transfer to Program" - shown when program name is not entered
- **Option 37**: "Missing Transfer to Program" - shown when program dropdown is not selected
- **Option 37 (after program selected)**: "There are forms for this case which require supervisory approval before transfer to another HFNY program."
- **Option 99**: Specify field required validation - shown when "Other" specify field is empty

**XPath Search**: Test uses XPath to search for validation messages containing specific text, as they may appear in various locations (page content, toast notifications, validation spans).

---

## What to Keep in Mind

### When Modifying Tests

1. **Test Order May Matter**: Test 3 (Reinstate) expects case to be discharged (by Test 2 or manual setup).

2. **Two-Step Process**: Discharge requires two submissions - date first, then reason.

3. **Page URLs Vary**: Accept both `PreDischarge.aspx` and `discharge.aspx` for flexibility.

4. **JavaScript Change Event**: "Other" specify field appearance requires triggering change event.

5. **Long Wait After "Other" Selection**: 1500ms wait needed for specify field to appear.

6. **Re-find Elements After Validation**: After submitting and triggering validation, elements become stale. Always re-find elements before interacting with them post-validation.

---

### When Adding New Tests

1. **Assign Priority**: Add `[TestPriority(N)]` with number > 4.

2. **Use Parameterization**: Add `[Theory]` and `[MemberData(nameof(GetTestPc1Ids))]`.

3. **Start with Navigation**: Use `NavigateToDischargeForm()` helper.

4. **Log Actions**: Use `_output.WriteLine()` for all significant actions.

5. **Wait After Submit**: Use 2000ms sleep after final submit for processing.

6. **Verify with Toast**: Success should show jQuery toast - use `WebElementHelper.GetToastMessage()`.

---

### Common Pitfalls

1. **Specify Field Not Appearing**: Didn't trigger change event after selecting "Other".
   - **Solution**: Use JavaScript to dispatch change event on dropdown.

2. **Validation Not Appearing**: Submitted too quickly before page updated.
   - **Solution**: Increase wait time after selecting "Other" to 1500ms.

3. **Submit Button Not Found**: Looking for button on wrong step of process.
   - **Solution**: Re-find Submit button at each step - it may change.

4. **Reinstate Button Not Found**: Case not actually discharged.
   - **Solution**: Run Test 2 first to discharge case, or manually discharge case.

5. **URL Assertion Fails**: Checking for specific page when either is valid.
   - **Solution**: Accept both `PreDischarge.aspx` and `discharge.aspx`.

6. **Toast Not Found**: Toast appeared and disappeared quickly.
   - **Solution**: Reduce wait time from 2000ms to 1500ms, or call `GetToastMessage()` sooner.

7. **Dropdown Options Empty**: Excluding all options including valid ones.
   - **Solution**: Only exclude "Other" (value "99") and placeholder, keep all others.

8. **Conditional Fields Not Appearing**: DOD or List Program fields don't show after selecting reason.
   - **Solution**: Verify change event triggered, increase wait time to 1500ms, check div IDs match.

9. **Supervisory Approval Message Not Found**: Message expected but not appearing for option 37.
   - **Solution**: Check both page content and toast notification, verify case has forms requiring approval.

10. **Stale Element Reference After Validation**: Elements become stale after submitting and triggering validation.
   - **Solution**: Re-find elements after validation appears before interacting with them again. The test now handles this by re-finding input fields after validation.

---

## Test Data Requirements

### Prerequisites
- Test user with DataEntry role
- Valid PC1 IDs in `appsettings.json` under `TestPc1Ids`
- Cases must be:
  - **Active** (not already discharged) for Tests 1-3
  - **Discharged** for Test 4 (can be discharged by Test 2 or Test 3)

### Test Creates
- Discharge records (Test 2, Test 3)

### Test Modifies
- Case status: Active → Discharged (Test 2, Test 3)
- Case status: Discharged → Active (Test 4)

### Test Deletes
- None (but reinstatement reverses discharge)

**Net Impact**: Case is discharged and then reinstated (returns to original active status).

**Note**: Test 3 validates conditional fields but does not complete submission (only validates field appearance).

---

## Running the Tests

### Run All Discharge Tests
```bash
dotnet test --filter "FullyQualifiedName~DischargeTests"
```

### Run Specific Test
```bash
# Navigation (Priority 1)
dotnet test --filter "FullyQualifiedName~DischargeTests.NavigateToDischargeForm"

# Validation and submission (Priority 2)
dotnet test --filter "FullyQualifiedName~DischargeTests.SubmitDischargeFormWithValidation"

# Conditional fields validation (Priority 3)
dotnet test --filter "FullyQualifiedName~DischargeTests.ValidateConditionalFieldsForSpecificDischargeReasons"

# Reinstatement (Priority 4)
dotnet test --filter "FullyQualifiedName~DischargeTests.ReinstateCaseFromDischargeForm"
```

---

## Troubleshooting

### Test Fails: "Discharge link was not found"
- **Issue**: Link not in Forms pane or selector changed
- **Solution**: Verify Forms tab loaded, check link selector

### Test Fails: "Discharge Date input was not found"
- **Issue**: Page didn't load or selector changed
- **Solution**: Verify page loaded, check input selector `[class*='2dy']`

### Test Fails: "Discharge Reason dropdown was not found"
- **Issue**: Didn't submit date first, or page didn't update
- **Solution**: Ensure date submitted before looking for reason dropdown

### Test Fails: "Specify text box was not found"
- **Issue**: Change event not triggered or insufficient wait time
- **Solution**: Verify JavaScript change event fired, increase wait to 1500ms

### Test Fails: Validation message doesn't appear
- **Issue**: Validation logic changed or selector incorrect
- **Solution**: Check validation message selectors, verify validation still works manually

### Test Fails: "Reinstate button was not found"
- **Issue**: Case not discharged, or button selector changed
- **Solution**: Run Test 2 first to discharge case, verify button selector

### Test Fails: Toast not found after submission
- **Issue**: Toast appeared too quickly and disappeared
- **Solution**: Call `GetToastMessage()` immediately after submission, reduce wait time

### Test Fails: Random reason selection fails
- **Issue**: No valid options found after filtering
- **Solution**: Verify discharge reason dropdown has options other than "Other" and placeholder

### Test Fails: "Target Child DOD container was not found"
- **Issue**: Option 18 selected but DOD field didn't appear
- **Solution**: Verify change event triggered, check container div ID is "divTargetChildDOD", increase wait to 1500ms

### Test Fails: "PC1 DOD container was not found"
- **Issue**: Option 21 selected but DOD field didn't appear
- **Solution**: Verify change event triggered, check container div ID is "divPC1DOD", increase wait to 1500ms

### Test Fails: "List program container was not found"
- **Issue**: Option 25 selected but program field didn't appear
- **Solution**: Verify change event triggered, check container div ID is "divTransferredtoProgram", increase wait to 1500ms

### Test Fails: "Supervisory approval message was not found"
- **Issue**: Option 37 submitted but approval message didn't appear
- **Solution**: Check if case actually has forms requiring approval, verify message appears in toast or on page, check XPath selector

---

## Selectors Reference

### Navigation

| Element | Selector |
|---------|----------|
| **Discharge Link** | `a.moreInfo[href*='Discharge.aspx']`, `a.moreInfo[data-formtype='dc']` |

### Form Fields

| Element | Selector |
|---------|----------|
| **Discharge Date Input** | `input.form-control[class*='2dy']` |
| **Discharge Reason Dropdown** | `select.form-control[id*='ddlDischargeReason']` |
| **Discharge Reason Specify** | `input.form-control[id*='Specify']` |
| **Target Child DOD Input** | `input.form-control[id*='txtTargetChildDOD']` |
| **Target Child DOD Container** | `div[id='divTargetChildDOD']` |
| **PC1 DOD Input** | `input.form-control[id*='txtPC1DOD']` |
| **PC1 DOD Container** | `div[id='divPC1DOD']` |
| **List Program Input (text)** | `input.form-control[id*='txtTransferredtoProgram']` |
| **List Program Container** | `div[id='divTransferredtoProgram']` |
| **Transfer Program Dropdown** | `select.form-control[id*='ddlTransferredtoProgramFK']` |
| **Acknowledgment Checkbox** | `input[type='checkbox'][id*='chkAcknowledgeRemoval']` |
| **Transfer Acknowledgment Container** | `div[id='divTransferAcknowledgment']` |

### Buttons

| Element | Selector |
|---------|----------|
| **Submit Button** | `a.btn.btn-primary` (with text "Submit") |
| **Reinstate Button** | `a.btn.btn-warning[id*='btnReinstate']` |

### Validation

| Element | Selector |
|---------|----------|
| **Validation Message** | `span[style*='color:Red']`, `span[id*='rfvSpecify']` |

### Form Container

| Element | Selector |
|---------|----------|
| **Form Container** | `.panel-body`, `.form-horizontal`, `form` |
