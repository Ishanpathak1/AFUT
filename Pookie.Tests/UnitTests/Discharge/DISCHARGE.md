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
| **DischargeTests.cs** | 3 | Navigation, discharge reason validation, reinstatement |

**Total Tests**: 3 (all parameterized with PC1 IDs)

**Test Priorities**: Tests are ordered 1-3 using `[TestPriority]` attribute for sequential execution.

---

## Test Execution Order

Tests run in priority order:

1. **Priority 1**: Navigate to Discharge form
2. **Priority 2**: Submit Discharge form with validation (includes "Other specify" validation)
3. **Priority 3**: Reinstate case from Discharge form

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

### Test 3: ReinstateCaseFromDischargeForm (Priority 3)

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
| **Discharge Reason Specify** | `input[id*='Specify']` | Conditional | If Reason = "Other" | Visible only when "Other" selected |

### Discharge Reasons

**Common Reasons** (vary by implementation):
- Completed program
- Moved out of service area
- Refused services
- Lost contact
- Ineligible for services
- Child removed from home
- **Other** (value "99") - Requires specify text

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

### 3. Discharge Pages

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

### 4. Case Reinstatement

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

**Test Coverage**: Test 3 validates reinstatement functionality.

---

### 5. Date Format

**Format**: `MM/dd/yy` (short year format)

**Example**: `12/07/25` for December 7, 2025

**Input Class**: `[class*='2dy']` - Indicates 2-digit year date picker

**Test Uses**: `DateTime.Now.ToString("MM/dd/yy")`

---

### 6. jQuery Toast Notifications

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

### 7. Validation Selectors

**Validation Message Selectors** (multiple tried for robustness):
```css
span[style*='color:Red']
span[style*='color: red']
.text-danger
span[id*='rfvSpecify']
```

**Why Multiple?**: ASP.NET validation can render in different formats depending on configuration.

---

## What to Keep in Mind

### When Modifying Tests

1. **Test Order May Matter**: Test 3 (Reinstate) expects case to be discharged (by Test 2 or manual setup).

2. **Two-Step Process**: Discharge requires two submissions - date first, then reason.

3. **Page URLs Vary**: Accept both `PreDischarge.aspx` and `discharge.aspx` for flexibility.

4. **JavaScript Change Event**: "Other" specify field appearance requires triggering change event.

5. **Long Wait After "Other" Selection**: 1500ms wait needed for specify field to appear.

---

### When Adding New Tests

1. **Assign Priority**: Add `[TestPriority(N)]` with number > 3.

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

---

## Test Data Requirements

### Prerequisites
- Test user with DataEntry role
- Valid PC1 IDs in `appsettings.json` under `TestPc1Ids`
- Cases must be:
  - **Active** (not already discharged) for Tests 1-2
  - **Discharged** for Test 3 (can be discharged by Test 2)

### Test Creates
- Discharge records (Test 2)

### Test Modifies
- Case status: Active → Discharged (Test 2)
- Case status: Discharged → Active (Test 3)

### Test Deletes
- None (but reinstatement reverses discharge)

**Net Impact**: Case is discharged and then reinstated (returns to original active status).

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

# Reinstatement (Priority 3)
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
