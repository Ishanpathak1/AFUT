# ScreenForms Tests Documentation

## Overview

The ScreenForms test suite contains automated end-to-end tests for the Screening Form functionality within the Referrals module. These tests validate the screening process that determines whether a referred participant is eligible for home visiting services based on demographic risk criteria.

## What is the Screening Form?

The **Screening Form** (HVScreen.aspx) is used to assess whether a referral meets eligibility criteria for home visiting services. It evaluates demographic risk factors to determine if services should be offered.

**Purpose**:
- Screen referrals before they become active cases
- Assess risk factors (not married, no prenatal care, poverty, under 21, child welfare involvement)
- Calculate screen result (Positive/Negative) based on risk criteria
- Document screening date and expected due date
- Track whether services were offered

**Workflow**:
```
Referral Received
    ↓
Create Screen Form
    ↓
Fill Risk Criteria (Q15-Q19)
    ↓
Calculate Screen Result
    ├─ Positive → Offer Services
    └─ Negative → Do Not Offer Services
    ↓
Submit Screen Form
    ↓
Referral Moves Forward in Process
```

## Test File Overview

| File | Test Count | Primary Focus |
|------|------------|---------------|
| **ScreenFormsTests.cs** | 6 | Screen form validation, risk assessment, screen result logic |
| **ScreenFormsEditButtonTests.cs** | 1 | Edit referral navigation |
| **ScreenFormsDeleteButtonTests.cs** | 1 | Delete referral from waiting screen |

**Total Tests**: 8

---

## Test File 1: ScreenFormsTests.cs

### Purpose
Core tests for screening form functionality, validation, risk assessment, and screen result calculation.

### Tests

#### 1. ReferralsWaiting_ClickCreateScreenForm_NavigatesToHvScreen

**Purpose**: Smoke test to verify "Create Screen Form" link navigates to HVScreen page.

**Test Flow**:
1. Login and navigate to Referrals page
2. Find Referrals Waiting Screen table
3. Find first row with "Create Screen Form" link
4. Click "Create Screen Form" link
5. Wait for navigation
6. Verify URL contains:
   - "HVScreen.aspx"
   - "ReferralFK" (referral foreign key parameter)
7. Log page contents (headings, summary, tables)

**Purpose**: Ensures basic navigation to screening form works.

**Referrals Waiting Screen Table**: Shows referrals that need to be screened before becoming active cases.

---

#### 2. ScreenForm_CheckValidationMessages_UpdateAfterFieldsFilled

**Purpose**: Comprehensive test of progressive validation and successful form submission.

**Test Flow**:

**Part 1: Initial Validation (Empty Form)**
1. Navigate to screen form page
2. Switch to "Demographic Criteria" tab (Risk tab)
3. Click Submit without filling any fields
4. Capture validation messages
5. Verify expected validations:
   - "Date of Screening is required."
   - "Q3 Required" (Q3: Relation to Target Child)
   - "Primary Language spoken in the home is required."
   - "Question 15 is required" (Not Married?)
   - "Question 16 is required" (No Prenatal Care?)
   - "Question 17 is required" (Poverty?)

**Part 2: Fill Date, Verify Validation Updates**
6. Switch to "Screen Information" tab
7. Fill Screening Date: "11/17/25"
8. Switch back to "Demographic Criteria" tab
9. Submit
10. Verify:
    - "Date of Screening is required." REMOVED ✓
    - Other validations still present

**Part 3: Fill Primary Language**
11. Select Primary Language: "01" (English)
12. Submit
13. Verify:
    - "Primary Language spoken in the home is required." REMOVED ✓
    - Q3 and risk questions still required

**Part 4: Fill Q3 (Relation to Target Child)**
14. Switch to "Screen Information" tab
15. Select Q3 (Relation to TC): "01" (Mother)
16. Switch back to "Demographic Criteria" tab
17. Submit
18. Verify:
    - "Q3 Required" REMOVED ✓
    - Risk questions still required

**Part 5: Fill Risk Questions (Q15-Q17)**
19. Select Q15 (Not Married): "0" (No/False)
20. Submit → Verify "Question 15 is required" REMOVED ✓
21. Select Q16 (No Prenatal Care): "1" (Yes/True)
22. Submit → Verify "Question 16 is required" REMOVED ✓
23. Select Q17 (Poverty): "9" (Unknown)
24. Submit → Verify "Question 17 is required" REMOVED ✓

**Part 6: Fill Remaining Fields and Submit**
25. Switch to "Screen Information" tab
26. Fill Expected Due Date: "11/01/25"
27. Switch back to "Demographic Criteria" tab
28. Select Referral Made: "2" (value 2)
29. Click Submit for final submission
30. Wait for success toast (up to 15 seconds)
31. Verify toast contains:
    - "Screen Form Saved"
    - "has been saved successfully"

**Success Toast**: Uses jQuery toast plugin (`.jq-toast-single`)

**Key Validation Pattern**: Each field validation clears as soon as that field is filled, providing progressive feedback.

---

#### 3. ScreenForm_ScreenDateBeforeReferral_ShowsValidationError

**Purpose**: Test date validation - screen date must be on or after referral date.

**Test Flow**:
1. Navigate to screen form page
2. Switch to "Screen Information" tab
3. Fill Screening Date: **"11/09/25"** (known to be before referral date for test data)
4. Switch to "Demographic Criteria" tab
5. Fill Primary Language: "01" (English)
6. Switch back to "Screen Information" tab
7. Fill Q3: "01" (Mother)
8. Switch to "Demographic Criteria" tab
9. Fill all risk questions:
   - Q15 (Not Married): "0" (No)
   - Q16 (No Prenatal Care): "1" (Yes)
   - Q17 (Poverty): "9" (Unknown)
10. Switch to "Screen Information" tab
11. Fill Expected Due Date: "11/01/25"
12. Switch back to "Demographic Criteria" tab
13. Fill Referral Made: "2"
14. Click Submit
15. Verify validation error appears:
    - **"Screen Date cannot be before the Referral Date on the Referral form"**

**Date Validation Rule**: Screen Date >= Referral Date

**Why This Matters**: Can't screen someone before they were referred.

---

#### 4. ScreenForm_DemographicCriteria_ScreenResultRespondsToRiskSelections

**Purpose**: Test screen result calculation logic based on risk question responses.

**Test Flow**:

**Setup**:
1. Navigate to screen form page
2. Switch to "Demographic Criteria" tab
3. Capture initial Q18 and Q19 values (static fields not editable in test):
   - Q18: Under 21?
   - Q19: Child Welfare Participant?

**Scenario 1: All False**
4. Set Q15, Q16, Q17 to **False/False/False**
5. Verify Screen Result = **"Negative"**

**Scenario 2: One True**
6. Set Q15, Q16, Q17 to **True/False/False**
7. Verify Screen Result = **"Positive"**

**Scenario 3: All Unknown**
8. Set Q15, Q16, Q17 to **Unknown/Unknown/Unknown**
9. Verify Screen Result = **"Positive"**

**Scenario 4: Mix with True**
10. Set Q15, Q16, Q17 to **Unknown/Unknown/True**
11. Verify Screen Result = **"Positive"**

**Scenario 5: Mix with False**
12. Set Q15, Q16, Q17 to **Unknown/Unknown/False**
13. Verify Screen Result depends on static Q18, Q19 values

**Screen Result Calculation Logic**:
```csharp
IF any risk question = True
THEN result = "Positive"
ELSE IF all editable questions = Unknown (and no True values)
THEN result = "Positive"
ELSE result = "Negative"
```

**Risk Questions**:
- **Q15**: Not Married?
- **Q16**: No Prenatal Care?
- **Q17**: Poverty?
- **Q18**: Under 21? (static - pre-filled from referral data)
- **Q19**: Child Welfare Participant? (static - pre-filled from referral data)

**Screen Result Field**: `input[id$='txtScreenResult']` (read-only, calculated field)

---

#### 5. ScreenForm_PositiveResult_ShowsServicesOfferedQuestion

**Purpose**: Verify "Services Offered" question appears when result is Positive.

**Test Flow**:
1. Navigate to screen form page
2. Switch to "Demographic Criteria" tab
3. Set Q15, Q16, Q17 to **True/False/False** (forces Positive result)
4. Wait for Screen Result to update to "Positive"
5. Look for "Services Offered" question container
6. Assert container is **visible**

**Services Offered Question**: "If screen result is positive, were services offered?"

**Conditional Display Rule**: Only shows when Screen Result = "Positive"

---

#### 6. ScreenForm_NegativeResult_HidesServicesOfferedQuestion

**Purpose**: Verify "Services Offered" question is hidden when result is Negative.

**Test Flow**:
1. Navigate to screen form page
2. Switch to "Demographic Criteria" tab
3. Set Q15, Q16, Q17 to **False/False/False** (forces Negative result)
4. Wait for Screen Result to update to "Negative"
5. Look for "Services Offered" question container
6. Assert container is **null or not displayed**

**Conditional Display Rule**: Hidden when Screen Result = "Negative"

**Why Hidden?**: If screen is negative, services won't be offered, so question is irrelevant.

---

## Test File 2: ScreenFormsEditButtonTests.cs

### Purpose
Test edit referral navigation from Referrals Waiting Screen table.

### Test

#### ReferralsWaitingScreen_ClickEditLink_NavigatesToReferralDetails

**Purpose**: Verify Edit link navigates to referral detail page.

**Test Flow**:
1. Login and navigate to Referrals page
2. Find Referrals Waiting Screen table
3. Find first visible "Edit" link in table:
   - Selector: `a[id*='lnkEditReferral']` or `a.btn.btn-default`
   - Must have text "Edit" or aria-label "Edit"
4. Capture link href
5. Scroll link into view
6. Click link (use JavaScript if click intercepted)
7. Wait for navigation
8. Verify current URL contains:
   - "Referral.aspx"
   - "ReferralPK" (referral ID parameter)
9. Verify URL matches expected path from href

**Edit Link**: Opens referral detail/edit page, allowing editing of referral information.

**URL Pattern**: `Referral.aspx?ReferralPK={id}`

---

## Test File 3: ScreenFormsDeleteButtonTests.cs

### Purpose
Test deletion of referrals from Referrals Waiting Screen table.

### Test

#### ReferralsWaitingScreen_ClickDelete_ConfirmsRemoval

**Purpose**: Delete referral from waiting screen with confirmation modal.

**Test Flow**:

**Part 1: Setup**
1. Login and navigate to Referrals page
2. Find Referrals Waiting Screen table
3. Get all referral rows (exclude "No data available" row)
4. Verify at least one row exists
5. Capture initial row count
6. Select first row as target
7. Capture row signature (text of all cells for verification)

**Part 2: Click Delete**
8. Find Delete link in target row:
   - Selector: `a.btn.btn-danger` or `.delete-control a.btn.btn-danger`
   - ID contains "btnDeleteReferral" or text contains "Delete"
9. Scroll delete link into view
10. Click delete link (use JavaScript if click intercepted)

**Part 3: Confirm Deletion**
11. Wait for delete confirmation modal (`.dc-confirmation-modal.modal`)
12. Verify modal text contains "Delete Confirmation"
13. Find "Yes"/"delete" button in modal footer:
    - Selector: `.modal-footer a.btn.btn-primary`
    - Text contains "Yes" or "delete"
14. Click confirm button
15. Wait for page ready (1.5 seconds)

**Part 4: Verify Deletion**
16. Wait for modal to close
17. Wait for delete result (up to 20 seconds)
18. Check deletion result:
    - **RowRemoved**: Row removed from table (count decreased by 1)
    - **TableNowEmpty**: Table shows empty state (was last row)
    - **WaitingSectionHidden**: Entire waiting section removed (table disappeared)
19. Log appropriate success message

**Delete Verification Logic**: Uses row signature to identify specific row that was deleted, handles three possible outcomes.

---

## Important Concepts

### 1. Screen Form Tabs

The screen form has 2 main tabs:

**Tab 1: Screen Information (main)**
- Screening Date (Q1)
- Expected Due Date
- Q3: Relation to Target Child
- Other screening details

**Tab 2: Demographic Criteria (risk)**
- Primary Language
- Q15: Not Married?
- Q16: No Prenatal Care?
- Q17: Poverty?
- Q18: Under 21? (static)
- Q19: Child Welfare Participant? (static)
- Screen Result (calculated)
- Services Offered? (conditional)
- Referral Made

**Tab Switching**: Tests frequently switch between tabs using `SwitchToScreenFormTab(driver, "main", "Screen Information")` or `SwitchToScreenFormTab(driver, "risk", "Demographic Criteria")`.

---

### 2. Risk Assessment Questions

**Q15: Not Married?**
- **Dropdown**: `select[id$='ddlRiskNotMarried']`
- **Options**: Yes (1), No (0/2), Unknown (9/-1)
- **Risk Factor**: Single parent household

**Q16: No Prenatal Care?**
- **Dropdown**: `select[id$='ddlRiskNoPrenatalCare']`
- **Options**: Yes (1), No (0/2), Unknown (9/-1)
- **Risk Factor**: Lack of medical care during pregnancy

**Q17: Poverty?**
- **Dropdown**: `select[id$='ddlRiskPoor']`
- **Options**: Yes (1), No (0/2), Unknown (9/-1)
- **Risk Factor**: Living in poverty

**Q18: Under 21?**
- **Dropdown**: `select[id$='ddlRiskUnder21']`
- **Static**: Pre-filled from referral data (participant age)
- **Options**: Yes (1), No (0/2), Unknown (9/-1)
- **Risk Factor**: Young parent

**Q19: Child Welfare Participant?**
- **Dropdown**: `select[id$='ddlRiskCWP']`
- **Static**: Pre-filled from referral data
- **Options**: Yes (1), No (0/2), Unknown (9/-1)
- **Risk Factor**: Involvement with child protective services

---

### 3. Screen Result Calculation

**Screen Result Field**: `input[id$='txtScreenResult']` (read-only, auto-calculated)

**Calculation Logic**:

```
STEP 1: Check for any "True" (Yes) responses
IF Q15 = True OR Q16 = True OR Q17 = True OR Q18 = True OR Q19 = True
THEN Screen Result = "Positive"

STEP 2: Check for all editable questions = Unknown
ELSE IF (Q15 = Unknown AND Q16 = Unknown AND Q17 = Unknown)
     AND (no True values exist)
THEN Screen Result = "Positive"

STEP 3: Default
ELSE Screen Result = "Negative"
```

**Rationale**:
- **Any True** → Positive: At least one risk factor present
- **All Unknown** → Positive: Cannot confirm no risk, err on side of caution
- **Mix of No/False** → Negative: No confirmed risk factors

**Real-Time Update**: Screen Result updates immediately when risk question values change.

---

### 4. Conditional "Services Offered" Question

**Question**: "If screen result is positive, were services offered?"

**Visibility Rule**:
```
IF Screen Result = "Positive"
THEN show "Services Offered?" question
ELSE hide "Services Offered?" question
```

**Container Selector**:
```css
span[id$='lblScreenResultLabel']  /* Label contains question text */
label[for*='ServicesOffered']
/* Then find ancestor div with class 'row' or 'form-group' */
```

**Purpose**: Document whether participant was offered services after positive screen.

---

### 5. Date Validation

**Rule 1: Screening Date Required**
- Screening Date (Q1) cannot be empty
- **Validation**: "Date of Screening is required."

**Rule 2: Screen Date >= Referral Date**
- Screening Date must be on or after the date the referral was received
- **Validation**: "Screen Date cannot be before the Referral Date on the Referral form"

**Date Fields**:
- **Screening Date**: `input[id$='txtScreenDate']`
- **Expected Due Date**: `input[id$='txtEDC']`

**Date Format**: MM/DD/YY (e.g., "11/17/25")

---

### 6. Progressive Validation

**Pattern**: Tests submit form multiple times, filling one field at a time, verifying validation updates progressively.

**Example Flow**:
```
Submit (empty) → 6 validations
Fill Date → Submit → 5 validations (date cleared)
Fill Primary Language → Submit → 4 validations (language cleared)
Fill Q3 → Submit → 3 validations (Q3 cleared)
Fill Q15 → Submit → 2 validations (Q15 cleared)
Fill Q16 → Submit → 1 validation (Q16 cleared)
Fill Q17 → Submit → 0 validations (all cleared)
Fill remaining fields → Submit → Success!
```

**Purpose**: Ensures validation provides real-time feedback as user fills form.

---

### 7. Risk Answer Choices

Tests use an enum to represent risk question answers:

```csharp
enum RiskAnswerChoice
{
    False,   // No, Not applicable
    True,    // Yes, Risk factor present
    Unknown  // Unknown, Don't know, Not assessed
}
```

**Text Preferences** (what tests look for in dropdown text):
- **False**: "No", "False"
- **True**: "Yes", "True"
- **Unknown**: "Unknown", "Don't Know", "Not Sure", "N/A", "Not Assessed"

**Value Preferences** (dropdown option values):
- **False**: "0", "2"
- **True**: "1"
- **Unknown**: "9", "-1"

**Selection Strategy**: Tests try multiple strategies:
1. Select by value (preferred)
2. Select by visible text
3. Set via JavaScript if element not interactable

---

### 8. jQuery Toast Notifications

**Success Toast Pattern**:
```html
<div class="jq-toast-single jq-icon-success">
    <h2 class="jq-toast-heading">Screen Form Saved</h2>
    <span class="jq-toast-text">has been saved successfully</span>
    <span class="close-jq-toast-single">×</span>
</div>
```

**Toast Verification**:
- Wait up to 15 seconds for toast to appear
- Look for `.jq-toast-single` with class `.jq-icon-success`
- Extract heading and body text
- Verify contains expected messages

**Success Toast Messages**:
- Heading: "Screen Form Saved"
- Body: "has been saved successfully"

---

### 9. Referrals Waiting Screen Table

**Purpose**: Shows referrals that need to be screened before becoming active cases.

**Table Selectors**:
```css
table[id*='grReferralsWaitingScreen']
.table.table-condensed.table-responsive.dataTable
.panel table.table
```

**Identification**: Table text contains "Create Screen Form", "Referrals Waiting", or ID contains "WaitingScreen".

**Typical Columns**:
- PC1 Name
- Referral Date
- Expected Due Date
- Actions (Create Screen Form, Edit, Delete)

**Row Filtering**: Exclude rows with text "No data available".

---

## Helper Methods Summary

### Navigation Helpers

#### NavigateToScreenFormPage()
Logs in, navigates to Referrals page, finds waiting screen table, clicks "Create Screen Form" link.

**Compound Helper**: Combines multiple steps for test setup.

---

#### SwitchToScreenFormTab()
Switches between screen form tabs (main/risk).

**Parameters**:
- `tabKey`: "main" or "risk"
- `tabDescription`: Human-readable description for logging

**Retry Logic**: Attempts up to 3 times, handles `ElementClickInterceptedException` and `StaleElementReferenceException`.

---

### Form Field Helpers

#### FillScreeningDate()
Fills screening date field, triggers change event.

**Default**: "11/17/25" (or accepts override)
**Returns**: Date string that was filled

---

#### FillExpectedDueDate()
Fills expected due date field.

**Default**: "11/01/25"
**Returns**: Date string that was filled

---

#### SelectReferralMadeOption()
Selects option from Referral Made dropdown.

**Dropdown**: `select[id$='ddlReferralMade']`

---

### Risk Question Helpers

#### SetDemographicRiskResponses()
Sets Q15, Q16, Q17 risk responses in one call.

**Parameters**: Three `RiskAnswerChoice` enum values

---

#### SetRiskQuestionResponse()
Sets single risk question response.

**Strategy**:
1. Try select by value (from value preferences)
2. Try select by text (from text preferences)
3. Try set via JavaScript
4. Throw error if all fail

---

#### GetRiskAnswerChoiceFromDropdown()
Reads current selection from risk question dropdown.

**Returns**: `RiskAnswerChoice` enum value

---

### Result Helpers

#### GetScreenResultValue()
Reads Screen Result field value.

**Returns**: "Positive", "Negative", or empty string

---

#### WaitForScreenResultValue()
Waits for Screen Result to update to expected value.

**Timeout**: 5 seconds (polls every 200ms)

---

#### AssertScreenResult()
Waits for expected result and logs success.

---

#### DetermineExpectedScreenResult()
Calculates expected screen result based on risk answers.

**Logic**: Implements business rules for Positive/Negative determination.

---

### Validation Helpers

#### GetScreenFormValidationMessages()
Finds all visible validation error messages on page.

**Returns**: `HashSet<string>` of unique validation messages

**Selectors**:
```css
.validation-summary-errors li
.alert
.alert-danger
.text-danger
span[style*='color: red']
[id*='rv']
[id*='rfv']
```

---

### Submit Helpers

#### FindScreenFormSubmitButton()
Finds Submit button with 5 second timeout.

**Selector**: `a[id*='btnSubmit']`, `button[id*='btnSubmit']`, `input[id*='btnSubmit']`

---

#### ClickScreenFormSubmit()
Scrolls to submit button and clicks (handles interception).

---

### Toast Helpers

#### WaitForSuccessToast()
Waits up to 15 seconds for success toast to appear.

**Returns**: List with [heading, body] text

**Toast Detection**: Looks for `.jq-toast-single` with `.jq-icon-success` class.

---

### Services Offered Helpers

#### TryGetServicesOfferedQuestionContainer()
Finds container for "Services Offered?" question.

**Returns**: Container `IWebElement` or null if not found/visible

---

### Table Helpers

#### FindReferralsWaitingScreenTable()
Finds Referrals Waiting Screen table.

---

#### GetReferralRows()
Returns list of data rows from table (excludes empty message).

---

#### CaptureRowSignature()
Captures text signature of row for verification after deletion.

**Returns**: Concatenated cell texts: "cell1 | cell2 | cell3"

---

### Edit/Delete Helpers

#### FindCreateScreenLink()
Finds "Create Screen Form" link in table row.

---

#### FindDeleteLink()
Finds Delete link in table row.

---

#### FindVisibleEditReferralLink()
Finds Edit link in waiting screen table.

---

## What to Keep in Mind

### When Modifying Tests

1. **Tab Switching Required**: Many fields are on different tabs - must switch tabs before interacting.

2. **Real-Time Calculation**: Screen Result updates immediately when risk questions change - no need to submit.

3. **Static vs Editable Fields**: Q18 and Q19 are pre-filled from referral data and not changed by tests.

4. **Progressive Validation**: Each field submission clears only validations for fields that have been filled.

5. **Date Dependency**: Screen Date must be >= Referral Date for test data being used.

6. **Toast Timing**: Success toast appears after submit - wait up to 15 seconds.

7. **Services Offered Conditional**: Only appears for Positive results - don't expect it for Negative.

---

### When Adding New Tests

1. **Start with NavigateToScreenFormPage()**: Most tests need this setup.

2. **Use Tab Helpers**: Always use `SwitchToScreenFormTab()` for tab navigation.

3. **Use Risk Question Helpers**: Use `SetRiskQuestionResponse()` instead of manually selecting.

4. **Log Extensively**: Log every step and intermediate values for debugging.

5. **Test Screen Result Logic**: When adding risk question tests, verify Screen Result updates.

6. **Test Conditional Display**: If adding fields that show/hide, test both states.

7. **Handle Multiple Outcomes**: For delete tests, handle row removal, empty table, or section hidden.

---

### Common Pitfalls

1. **Forgot to Switch Tabs**: Field not found because it's on a different tab.
   - **Solution**: Switch to correct tab before interacting with field.

2. **Screen Result Not Updating**: Expecting immediate update but checking too soon.
   - **Solution**: Use `WaitForScreenResultValue()` with timeout.

3. **Toast Not Found**: Checking too soon after submit.
   - **Solution**: Use `WaitForSuccessToast()` with 15 second timeout.

4. **Services Offered Not Visible**: Looking for field when result is Negative.
   - **Solution**: Only check for field visibility when result is Positive.

5. **Date Validation Fails**: Screen date is actually after referral date for test data.
   - **Solution**: Use date that's definitely before referral date or check test data.

6. **Risk Question Selection Fails**: Dropdown options don't match expected text/values.
   - **Solution**: Check actual dropdown options in browser, update preferences if needed.

7. **Delete Verification Fails**: Row count doesn't match expected.
   - **Solution**: Ensure you're filtering out "No data available" rows correctly.

---

## Test Data Requirements

### Prerequisites
- Test user with DataEntry role
- At least one referral in "Referrals Waiting Screen" status (needs screening)
- Referral Date for test data should be before "11/17/25" (for date validation tests)

### Test Creates
- Screen forms with various risk criteria combinations
- Screen forms with Positive and Negative results

### Test Modifies
- N/A (tests don't edit existing screen forms)

### Test Deletes
- One referral from waiting screen (ScreenFormsDeleteButtonTests)

**Net Impact**: Screen forms created, one referral deleted from waiting screen.

---

## Running the Tests

### Run All ScreenForms Tests
```bash
dotnet test --filter "FullyQualifiedName~ScreenForms"
```

### Run Specific Test File
```bash
# Main screen form tests
dotnet test --filter "FullyQualifiedName~ScreenFormsTests"

# Edit button test
dotnet test --filter "FullyQualifiedName~ScreenFormsEditButtonTests"

# Delete button test
dotnet test --filter "FullyQualifiedName~ScreenFormsDeleteButtonTests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~ScreenFormsTests.ScreenForm_CheckValidationMessages_UpdateAfterFieldsFilled"
```

---

## Troubleshooting

### Test Fails: "Referrals Waiting Screen table not found"
- **Issue**: No referrals in waiting screen status
- **Solution**: Create at least one referral that hasn't been screened yet

### Test Fails: "Create Screen Form link not found"
- **Issue**: Referral already has a screen form
- **Solution**: Use a referral that hasn't been screened, or create new referral

### Test Fails: Tab not found
- **Issue**: Tab selectors don't match or tab didn't load
- **Solution**: Verify tab exists, increase wait time, check tab key ("main" or "risk")

### Test Fails: Screen Result not updating
- **Issue**: JavaScript not triggering calculation
- **Solution**: Ensure change events are triggered after setting values, wait longer

### Test Fails: "Services Offered" question not found when expected
- **Issue**: Screen Result not Positive or container selector changed
- **Solution**: Verify Screen Result = "Positive", check container selector

### Test Fails: Date validation not appearing
- **Issue**: Screen date is actually after referral date
- **Solution**: Use earlier screen date (e.g., "11/09/25"), verify referral date

### Test Fails: Risk question selection fails
- **Issue**: Dropdown values/texts don't match preferences
- **Solution**: Inspect dropdown options, update text/value preferences

### Test Fails: Success toast not found
- **Issue**: Toast appeared and disappeared, or different toast implementation
- **Solution**: Increase wait timeout, check for alternative toast selectors

### Test Fails: Delete confirmation modal not appearing
- **Issue**: Delete link didn't trigger modal or modal selector changed
- **Solution**: Verify delete link clicked successfully, check modal selector

### Test Fails: Stale element after tab switch
- **Issue**: Element reference became stale during tab navigation
- **Solution**: Re-find element after tab switch, use retry logic

---

## Field Reference

### Screen Information Tab (main)

| Field | Type | Selector | Required | Notes |
|-------|------|----------|----------|-------|
| **Screening Date** | Text | `input[id$='txtScreenDate']` | Yes | Must be >= Referral Date |
| **Expected Due Date** | Text | `input[id$='txtEDC']` | Yes | Pregnancy due date |
| **Q3: Relation to TC** | Dropdown | `select[id$='ddlRelation2TC']` | Yes | Mother, Father, Other |

---

### Demographic Criteria Tab (risk)

| Field | Type | Selector | Required | Notes |
|-------|------|----------|----------|-------|
| **Primary Language** | Dropdown | `select[id$='ddlPrimaryLanguage']` | Yes | English, Spanish, etc. |
| **Q15: Not Married?** | Dropdown | `select[id$='ddlRiskNotMarried']` | Yes | Yes/No/Unknown |
| **Q16: No Prenatal Care?** | Dropdown | `select[id$='ddlRiskNoPrenatalCare']` | Yes | Yes/No/Unknown |
| **Q17: Poverty?** | Dropdown | `select[id$='ddlRiskPoor']` | Yes | Yes/No/Unknown |
| **Q18: Under 21?** | Dropdown | `select[id$='ddlRiskUnder21']` | N/A | Static (pre-filled) |
| **Q19: CWP?** | Dropdown | `select[id$='ddlRiskCWP']` | N/A | Static (pre-filled) |
| **Screen Result** | Text (read-only) | `input[id$='txtScreenResult']` | N/A | Calculated |
| **Services Offered?** | Dropdown | (conditional) | Conditional | Only if result = Positive |
| **Referral Made** | Dropdown | `select[id$='ddlReferralMade']` | Yes | Yes/No |

---

### Referrals Waiting Screen Table

| Element | Selector | Notes |
|---------|----------|-------|
| **Table** | `table[id*='grReferralsWaitingScreen']` | Shows referrals waiting for screening |
| **Data Rows** | `tbody tr` | Excludes "No data available" |
| **Create Screen Link** | `a[id*='lnkCreateScreen']` | Opens HVScreen.aspx |
| **Edit Link** | `a[id*='lnkEditReferral']` | Opens Referral.aspx |
| **Delete Link** | `a.btn.btn-danger` | Opens delete confirmation modal |

---

## Business Logic Summary

### Screen Result Determination

**Priority 1: Any True Answer**
```
IF Q15 = True OR Q16 = True OR Q17 = True OR Q18 = True OR Q19 = True
THEN Screen Result = "Positive"
```

**Priority 2: All Editable Unknown**
```
ELSE IF Q15 = Unknown AND Q16 = Unknown AND Q17 = Unknown
     AND (no True values in Q18, Q19)
THEN Screen Result = "Positive"
```

**Priority 3: Default**
```
ELSE Screen Result = "Negative"
```

---

### Eligibility Decision

```
Screen Result = Positive
    ↓
Offer Services
    ├─ Yes → Services Offered = Yes → Referral proceeds
    └─ No → Services Offered = No → Referral closed

Screen Result = Negative
    ↓
Do Not Offer Services → Referral closed
```

---

### Risk Factors Assessed

1. **Not Married**: Single parent household (less support system)
2. **No Prenatal Care**: Health risk to mother and baby
3. **Poverty**: Economic hardship affecting child development
4. **Under 21**: Young parent (less life experience, education)
5. **Child Welfare Participant**: History with protective services

**Philosophy**: Any one risk factor is enough to qualify for home visiting services.

