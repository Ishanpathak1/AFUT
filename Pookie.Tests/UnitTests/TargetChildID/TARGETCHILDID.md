# Target Child ID Tests Documentation

## Overview

The **TargetChildID** test suite contains automated end-to-end tests for the Target Child Information and Birth Outcomes (TCID) form. This multi-tab form collects comprehensive information about the target child (newborn) including birth outcomes, health insurance, medical provider information, PHQ-9 depression screening, and MIECHV-specific data.

## What is Target Child ID (TCID)?

**Target Child ID** is a form completed upon the birth of the target child in a home visiting case. The target child is the newborn that the case was opened for.

**Purpose**:
- Document birth outcomes and vital statistics
- Track health insurance and medical provider information
- Conduct PHQ-9 depression screening for caregivers
- Collect MIECHV-required data elements
- Record additional optional items (parity, delivery type, breastfeeding)

**Workflow**:
```
Navigate to Forms Tab
    ↓
Click "Target Child Information and Birth Outcomes" link
    ↓
View TCID Grid (existing entries)
    ↓
Click "New TCID" OR Edit existing entry
    ↓
Complete Multi-Tab Form:
    ├─ Target Child (birth info, gestational age, birth weight)
    ├─ Health Insurance (Medicaid, insurance types, medical provider/facility)
    ├─ Additional Items (parity, delivery type, breastfeeding)
    ├─ PHQ-9 (depression screening with scoring)
    └─ MIECHV (medical care source, prenatal care)
    ↓
Submit Form
    ↓
Success Toast Displayed
```

---

## Test Files Overview

| File | Tests | Primary Focus |
|------|-------|---------------|
| **TargetChildIDTests.cs** | 6 | Main form tests: navigation, validation, birth weight, health insurance, medical provider/facility modals |
| **TargetChildIDAdditionalItemsTests.cs** | 1 | Additional Items tab: parity, delivery type, breastfeeding, tooltips |
| **TargetChildIDPhq9Tests.cs** | 1 | PHQ-9 tab: date validation, participant selection, scoring, worker assignment |
| **TargetChildIDMiechvTests.cs** | 1 | MIECHV tab: medical care source with "Other specify" toggle |

**Total Tests**: 9 (all parameterized with PC1 IDs)

**Test Priorities**: Tests are ordered 1-9 using `[TestPriority]` attribute for sequential execution.

**Partial Class Pattern**: All test files use `partial class TargetChildIDTests` to share common helper methods and constants.

---

## Form Structure

### 5 Tabs

| Tab | ID | Purpose |
|-----|----|---------|
| **Target Child** | `#TargetChild` | Birth outcomes (name, DOB, term, gestational age, birth weight) |
| **Health Insurance** | `#HealthInsurance` | Medicaid, insurance types, medical provider/facility |
| **Additional Items** | `#OptionalItems` | Optional fields (parity, delivery type, breastfeeding) |
| **PHQ-9** | `#PHQ9` | Depression screening for caregiver with scoring |
| **MIECHV** | `#MIECHV` | MIECHV-required fields (medical care source, prenatal care) |

---

## Test Execution Order

Tests run in priority order (1-9):

1. **Priority 1**: Navigate to Target Child Information form
2. **Priority 2**: New TCID button displays info alert
3. **Priority 3**: Validation and submission (progressive validation)
4. **Priority 4**: Existing TCID prenatal care date validation
5. **Priority 5**: Existing TCID birth weight validation
6. **Priority 6**: Health insurance options respect Medicaid selection + medical provider/facility modals
7. **Priority 7**: Additional Items parity and delivery flow
8. **Priority 8**: PHQ-9 date validation and scoring
9. **Priority 9**: MIECHV medical care source "Other specify" toggle

---

## Detailed Test Breakdown

### Test 1: NavigateToTargetChildInformationForm (Priority 1)

**File**: TargetChildIDTests.cs

**Purpose**: Smoke test to verify navigation to Target Child Information page.

**Test Flow**:
1. Sign in as DataEntry user
2. Navigate to Forms tab for target PC1 ID
3. Find Target Child Information link: `a.moreInfo[data-formtype='TCIBO']`
4. Click link
5. Wait for page load

**Assertions**:
- URL contains `TCIDs.aspx`
- URL contains `pc1id={pc1Id}`

---

### Test 2: NewTcidButtonDisplaysInfoAlert (Priority 2)

**File**: TargetChildIDTests.cs

**Purpose**: Verify "New TCID" button opens form with info alert.

**Test Flow**:
1. Navigate to Target Child page
2. Click "New TCID" button
3. Wait for info alert: `.alert.alert-info`

**Info Alert Expected Text**:
- "Complete this form upon the birth of the target child"
- "target child is the newborn"

**Assertions**:
- Info alert is displayed
- Validation summary container exists on page

---

### Test 3: ValidationAndSubmission (Priority 3)

**File**: TargetChildIDTests.cs

**Purpose**: Comprehensive progressive validation test covering multiple tabs.

**Test Flow**:

#### Part 1: Submit Empty Form
1. Open New TCID form
2. Switch to MIECHV tab
3. Click Submit
4. Capture validation errors

**Expected Validations**:
- "Term of Birth is required."
- "First Name is required!" (appears 2 times - Target Child and MIECHV)
- "Parity is required."

#### Part 2: Fill Birth Term
5. Switch to Target Child tab
6. Select random birth term
7. Switch to MIECHV tab
8. Click Submit

**Expected**: "Term of Birth is required." cleared

#### Part 3: Fill Target Child First Name
9. Switch to Target Child tab
10. Enter first name: "Gwen"
11. Switch to MIECHV tab
12. Click Submit

**Expected**: One "First Name is required!" cleared (1 remaining)

#### Part 4: Fill Target Child Last Name
13. Switch to Target Child tab
14. Enter last name: "Venom"
15. Switch to MIECHV tab
16. Click Submit

**Expected**: All "First Name is required!" cleared

#### Part 5: Fill Parity
17. Switch to Additional Items tab
18. Select random parity
19. Switch to MIECHV tab
20. Click Submit

**Expected**: 
- "Parity is required." cleared
- "Gestational Age is required and must be a valid whole number!" appears

#### Part 6: Test Invalid Gestational Age
21. Switch to Target Child tab
22. Enter gestational age: "-1"
23. Switch to MIECHV tab
24. Click Submit

**Expected**: "Gestation Age must be between 0 and 40 weeks!"

#### Part 7: Fill Valid Gestational Age
25. Switch to Target Child tab
26. Enter gestational age: "38"

**Purpose**: Demonstrates progressive validation where each field clears its own validation message.

---

### Test 4: ExistingTcidPrenatalCareValidation (Priority 4)

**File**: TargetChildIDTests.cs

**Purpose**: Test prenatal care date validation - must be before target child's DOB.

**Test Flow**:

#### Part 1: Enter Invalid Date (Future)
1. Navigate to Target Child page
2. Open existing TCID entry
3. Switch to Target Child tab
4. Set prenatal care date: "12/01/25" (future date)
5. Switch to MIECHV tab
6. Click Submit

**Expected Validation**: "Date Began Receiving Prenatal Care can not start after the birth of the child"

#### Part 2: Enter Valid Date
7. Switch to Target Child tab
8. Set prenatal care date: "11/01/25" (past date)
9. Switch to MIECHV tab
10. Click Submit (no validation expected)

**Expected**:
- Success toast displayed
- Toast contains: "Form Saved", "Target Child Identification", PC1 ID

---

### Test 5: ExistingTcidBirthWeightValidation (Priority 5)

**File**: TargetChildIDTests.cs

**Purpose**: Test birth weight validation - pounds < 17, ounces < 16.

**Test Flow**:

#### Part 1: Test Invalid Weights
1. Navigate to Target Child page
2. Open existing TCID entry

**Test 3 Invalid Combinations**:

| Pounds | Ounces | Expected Validation |
|--------|--------|---------------------|
| -1 | -2 | "Birth weight pounds must be less than 17" AND "Birth weight ounces must be less than 16" |
| 20 | 20 | Same as above |
| 17 | 16 | Same as above |

#### Part 2: Enter Valid Weight
3. Generate random valid weight: lbs 0-16, oz 0-15
4. Enter birth weight
5. Click Submit

**Expected**:
- Success toast displayed
- Toast contains: "Form Saved", "Target Child Identification", PC1 ID

---

### Test 6: HealthInsuranceOptionsRespectMedicaidSelection (Priority 6)

**File**: TargetChildIDTests.cs

**Purpose**: Comprehensive test of Health Insurance tab, including Medicaid logic and medical provider/facility modals.

**Test Flow**:

#### Part 1: Medicaid = Yes → Health Insurance Checkboxes Disabled
1. Navigate to Target Child page
2. Open existing TCID entry
3. Switch to Health Insurance tab
4. Select Medicaid: "Yes"
5. Assert Medicaid case number input is visible
6. Assert all health insurance checkboxes are **disabled**:
   - Family/Child Health
   - Private Insurance
   - Other
   - Uninsured
   - Unknown

#### Part 2: Medicaid = No → Health Insurance Checkboxes Enabled
7. Select Medicaid: "No"
8. Assert all health insurance checkboxes are **enabled**
9. Check "Other" checkbox
10. Assert "Other specify" text input is **visible**
11. Uncheck "Other" checkbox

#### Part 3: Medicaid = Unknown → Health Insurance Checkboxes Enabled
12. Select Medicaid: "Unknown"
13. Assert all health insurance checkboxes are **enabled**
14. Check "Other" checkbox
15. Assert "Other specify" text input is **visible**

#### Part 4: Medical Provider Conditional Logic (Q19)
16. Select Medicaid: "Yes"
17. Enter Medicaid case number: "MCN12345"
18. Select Medical Provider Question (Q19): **"No"**
19. Assert medical provider dropdown is **disabled**
20. Assert medical facility dropdown is **disabled**
21. Assert "Not in List" doctor link has **disabled** class
22. Assert "Not in List" facility link has **disabled** class

#### Part 5: Medical Provider Question = Yes
23. Select Medical Provider Question (Q19): **"Yes"**
24. Assert medical provider dropdown is **enabled**
25. Assert medical facility dropdown is **enabled**
26. Assert "Not in List" doctor link is **enabled**
27. Assert "Not in List" facility link is **enabled**

#### Part 6: Medical Provider Modal - Validation
28. Click "Not in List" doctor link
29. Wait for modal: `.modal.show .modal-content`
30. Leave first name and last name empty
31. Click Submit in modal
32. Assert validation appears: "Provider's Last Name"

#### Part 7: Medical Provider Modal - Add Provider
33. Fill modal fields:
    - First Name: "testone"
    - Last Name: "testtwo"
    - Address: "aaaaaaaa"
    - State: "aa"
    - Zip: "00000"
    - Phone: "000000000"
34. Click Submit
35. Wait for modal to close
36. Switch back to Health Insurance tab

**Expected**:
- Medical provider dropdown now contains "testtwo"
- "testtwo" is auto-selected in dropdown

#### Part 8: Medical Facility Modal - Validation
37. Click "Not in List" facility link
38. Wait for modal
39. Click Submit without filling
40. Assert validation appears: "Facility"

#### Part 9: Medical Facility Modal - Add Facility
41. Fill modal fields:
    - Name: "avengers"
    - Address: "aaaa"
    - City: "aaaaaa"
    - State: "aa"
    - Zip: "00000"
    - Phone: "3434343434"
42. Click Submit
43. Wait for modal to close
44. Switch back to Health Insurance tab

**Expected**:
- Medical facility dropdown now contains "avengers"
- "avengers" is auto-selected in dropdown

#### Part 10: Submit Form
45. Click Submit (no validation expected)

**Expected**:
- Success toast displayed
- Toast contains: "Form Saved", "Target Child Identification", PC1 ID

---

### Test 7: AdditionalItemsParityAndDeliveryFlow (Priority 7)

**File**: TargetChildIDAdditionalItemsTests.cs

**Purpose**: Test Additional Items tab fields and tooltips.

**Test Flow**:

#### Part 1: Parity Validation
1. Navigate to Target Child page
2. Open existing TCID entry
3. Switch to Additional Items tab
4. Clear parity selection (select placeholder)
5. Click Submit

**Expected Validation**: "Parity is required."

#### Part 2: Fill Additional Items Fields
6. Switch to Additional Items tab
7. Select random parity
8. Select random delivery type
9. Select random "Child fed breast milk" option
10. Store selected values for verification

#### Part 3: Test Tooltip Icons
11. Find all tooltip icons (glyphicon-question-sign)
12. For first 2 icons:
    - Click icon
    - Wait for tooltip to appear: `.tooltip-inner`
    - Assert tooltip has text
    - Log tooltip text

**Expected**: At least 2 tooltip icons with helpful text

#### Part 4: Submit and Verify Persistence
13. Click Submit
14. Verify success toast
15. Navigate back to TCID grid
16. Reopen same entry
17. Switch to Additional Items tab
18. Verify parity, delivery type, and breast milk selections persisted

---

### Test 8: Phq9DateValidationAndSave (Priority 8)

**File**: TargetChildIDPhq9Tests.cs

**Purpose**: Comprehensive PHQ-9 tab test covering date validation, participant selection, scoring, and worker assignment.

**Test Flow**:

#### Part 1: Invalid Date Entry
1. Navigate to Target Child page
2. Open existing TCID entry
3. Switch to PHQ-9 tab
4. Enter invalid date: "000000"
5. Press Enter

**Expected**: Input clears (invalid entry rejected)

#### Part 2: Date Before Intake Validation
6. Get Intake Date from label
7. Get Target Child DOB from label
8. Set PHQ date to (Intake Date - 1 day)
9. Click Submit

**Expected Validation**: "[PHQ-9] The PHQ date administered must be on or after the TC's date of birth!"

#### Part 3: Valid Date and Participant Selection
10. Switch to PHQ-9 tab
11. Set PHQ date to (Target Child DOB + 1 day)
12. Select PHQ participant: "Other" (value "04")
13. Assert "Participant specify" input is **visible**
14. Enter specify text: "Non-family support"
15. Select PHQ participant: "Primary Caretaker 1" (value "01")
16. Assert "Participant specify" input is **hidden**

#### Part 4: Worker Selection
17. Select PHQ participant: "Primary Caretaker 2" (value "02")
18. Select PHQ worker: "105"

#### Part 5: Test Incomplete PHQ Scoring
19. Set PHQ scores (some blank): `["01", "02", "01", "", "01", "02", "", "02", "01"]`
20. Clear difficulty and referral
21. Wait for labels to update

**Expected**:
- PHQ Result: "N/A"
- PHQ Score Validity: "Invalid"

#### Part 6: Select Difficulty and Referral
22. Select difficulty: "Somewhat difficult" (value "02")
23. Check referral checkbox

#### Part 7: Test Complete PHQ Scoring (Positive/Valid)
24. Set all PHQ scores to max: `["04", "04", "04", "04", "04", "04", "04", "04", "04"]`
25. Wait for labels to update

**Expected**:
- PHQ Result: "Positive"
- PHQ Score Validity: "Valid"

**PHQ Scoring Logic**:
- **Score Range**: 0-27 (9 questions × 0-3 points each)
- **Result**:
  - < 10: Negative
  - >= 10: Positive
- **Validity**:
  - All 9 questions answered: Valid
  - Any question blank: Invalid, shows "N/A"

#### Part 8: Submit and Verify Persistence
26. Click Submit
27. Verify success toast
28. Navigate back to TCID grid
29. Reopen same entry
30. Switch to PHQ-9 tab
31. Verify participant, worker, and score values persisted

---

### Test 9: MiechvMedicalCareSourceOtherSpecifyToggle (Priority 9)

**File**: TargetChildIDMiechvTests.cs

**Purpose**: Test MIECHV tab "Medical Care Source" field with conditional "Other specify" input.

**Test Flow**:

#### Part 1: Select "Other" → Show Specify Input
1. Navigate to Target Child page
2. Open existing TCID entry
3. Switch to MIECHV tab
4. Select Medical Care Source: "Other" (value "06")
5. Assert "Medical care source specify" input is **visible**
6. Enter specify text: "Community clinic"

#### Part 2: Select Non-"Other" → Hide Specify Input
7. Select Medical Care Source: different option (value "02")
8. Wait for page update
9. Assert "Medical care source specify" input is **hidden**

#### Part 3: Submit and Verify Persistence
10. Click Submit
11. Verify success toast
12. Navigate back to TCID grid
13. Reopen same entry
14. Switch to MIECHV tab
15. Verify medical care source selection persisted

---

## Required Fields

### Target Child Tab

| Field | Selector | Required | Validation | Notes |
|-------|----------|----------|------------|-------|
| **Birth Term** | `select[id$='ddlBirthTerm']` | Yes | - | Full term, Premature, etc. |
| **First Name** | `input[id$='txtTCFirstName']` | Yes | - | Also validated on MIECHV tab |
| **Last Name** | `input[id$='txtTCLastName']` | Yes | - | |
| **Gestational Age** | `input[id$='txtGestationalAge']` | Yes | 0-40 weeks | Must be valid whole number |
| **Birth Weight Lbs** | `input[id$='txtBirthWtLbs']` | No | < 17 | |
| **Birth Weight Oz** | `input[id$='txtBirthWtOz']` | No | < 16 | |
| **Prenatal Care Date** | `input[id*='Prenatal'][type='text']` | No | < Target Child DOB | Must be before birth |

### Health Insurance Tab

| Field | Selector | Required | Conditional | Notes |
|-------|----------|----------|-------------|-------|
| **Medicaid** | `select[id$='ddlTCReceivingMedicaid']` | No | - | Yes/No/Unknown |
| **Medicaid Case Number** | `input[id$='txtTcHIMedicaidCaseNumber']` | No | If Medicaid = Yes | Visible only when Yes |
| **Health Insurance Checkboxes** | Various `input[id$='chkTCHI...']` | No | Disabled if Medicaid = Yes | Family/Child Health, Private, Other, Uninsured, Unknown |
| **Other Specify** | `input[id$='txtTCHIOtherSpecify']` | No | If "Other" checked | |
| **Has Medical Provider (Q19)** | `select[id$='ddlTCHasMedicalProvider']` | No | - | Controls Q20 |
| **Medical Provider** | `select[id$='ddlTCMedicalProviderFK']` | No | If Q19 = Yes | Disabled if Q19 = No |
| **Medical Facility** | `select[id$='ddlTCMedicalFacilityFK']` | No | If Q19 = Yes | Disabled if Q19 = No |

### Additional Items Tab

| Field | Selector | Required | Notes |
|-------|----------|----------|-------|
| **Parity** | `select[id$='ddlParity']` | Yes | Number of pregnancies |
| **Delivery Type** | `select[id$='ddlDeliveryType']` | No | Vaginal, C-section, etc. |
| **Child Fed Breast Milk** | `select[id$='ddlChildFedBreastMilk']` | No | Yes/No/Unknown |

### PHQ-9 Tab

| Field | Selector | Required | Conditional | Notes |
|-------|----------|----------|-------------|-------|
| **PHQ Date** | `input[id$='txtPHQDateAdministered']` | No | - | Must be >= Target Child DOB and >= Intake Date |
| **Participant** | `select[id$='ddlPHQ9Participant']` | No | - | PC1, PC2, Other, etc. |
| **Participant Specify** | `input[id$='txtPHQ9ParticipantSpecify']` | No | If Participant = Other | |
| **PHQ Worker** | `select[id$='ddlPHQWorker']` | No | - | |
| **9 PHQ Questions** | `select.phq9score` | No | - | Dropdowns with 0-3 scale |
| **Difficulty** | `select[id$='ddlDifficulty']` | No | - | Not/Somewhat/Very/Extremely difficult |
| **Referral Made** | `input[id$='chkDepressionReferralMade']` | No | - | Checkbox |

### MIECHV Tab

| Field | Selector | Required | Conditional | Notes |
|-------|----------|----------|-------------|-------|
| **Medical Care Source** | `select[id$='ddlTCMedicalCareSource']` | No | - | Various options + Other |
| **Medical Care Source Specify** | `input[id$='txtTCMedicalCareSourceOtherSpecify']` | No | If Medical Care Source = Other | |

---

## Helper Methods Summary

### Navigation

#### NavigateToTargetChildPage()
Navigate from Forms pane to Target Child Information page.

**Parameters**: `driver`, `formsPane`, `pc1Id`

**Assertions**:
- URL contains `TCIDs.aspx`
- URL contains `pc1id={pc1Id}`

---

#### OpenNewTcidForm()
Click "New TCID" button to open new form.

---

#### OpenExistingTcidEntry()
Find and click the first existing TCID link in the grid.

**Returns**: Opens existing TCID form
**Assertions**: URL contains `tcid.aspx` and `tcpk=`

---

#### NavigateBackToExistingTcid()
Navigate directly to TCID grid and reopen existing entry.

**Parameters**: `driver`, `pc1Id`

---

### Tab Management

#### SwitchToTab()
Switch to a specific tab in the multi-tab form.

**Parameters**:
- `tabHref`: Tab href selector (e.g., "#TargetChild")
- `tabTitle`: Tab title for logging

**Assertions**: Parent `li` has "active" class

---

### Form Submission

#### SubmitForm()
Submit form and optionally capture validation.

**Parameters**: `expectValidation` (default true)

**Returns**: Validation summary text if `expectValidation` is true

---

#### SubmitFormFromAdditionalItemsTab()
Submit form and automatically switch back to Additional Items tab.

**Use Case**: When submitting from Additional Items tab and validation redirects to another tab

---

### Validation

#### CountOccurrences()
Count how many times a string appears in text (case-insensitive).

**Use Case**: Verify validation message appears expected number of times

**Example**: "First Name is required!" appears 2 times initially

---

### Dropdown Helpers

#### SelectRandomDropdownOption()
Select a random option from a dropdown (excludes empty values).

**Parameters**: `selector`, `description`

**Logs**: Selected option text and value

---

#### SelectDropdownPlaceholderOption()
Select the placeholder option (empty value) in a dropdown.

**Use Case**: Clear dropdown selection to trigger validation

---

### Field Setters

#### SetPrenatalCareDate()
Set the prenatal care date input.

**Parameters**: `driver`, `dateValue`

---

#### SetBirthWeight()
Set birth weight pounds and ounces.

**Parameters**: `driver`, `pounds`, `ounces`

---

#### ValidateBirthWeight()
Test birth weight validation by setting invalid values and verifying error messages.

**Parameters**: `driver`, `targetTab`, `miechvTab`, `pounds`, `ounces`

**Expected Validations**:
- "Birth weight pounds must be less than 17"
- "Birth weight ounces must be less than 16"

---

### PHQ-9 Helpers

#### SetPhqDate()
Set PHQ-9 date input with formatted date.

**Format**: `MM/dd/yy`

---

#### SetPhqScores()
Set all 9 PHQ-9 question scores.

**Parameters**: `driver`, `values` (array of 9 strings, "" for blank)

**Example**: `["01", "02", "01", "", "01", "02", "", "02", "01"]`

---

#### ClearPhqScores()
Clear all 9 PHQ-9 question scores (select placeholder).

---

#### EnsureDifficultyAndReferralCleared()
Clear PHQ-9 difficulty dropdown and uncheck referral checkbox.

---

#### WaitForLabelText()
Wait for a label's text to meet a specific condition.

**Parameters**:
- `selector`: Label selector
- `predicate`: Function that returns true when condition met
- `timeoutSeconds`: Default 5

**Returns**: Final label text

**Use Case**: Wait for PHQ-9 score labels to update after changing answers

---

### Health Insurance Helpers

#### GetHealthInsuranceCheckboxes()
Get all health insurance checkboxes.

**Returns**: List of 5 checkboxes (Family/Child Health, Private, Other, Uninsured, Unknown)

---

#### GetHealthInsuranceOtherCheckbox()
Get the "Other" checkbox.

---

#### GetHealthInsuranceOtherSpecifyInput()
Get the "Other specify" text input.

---

#### IsElementVisible()
Check if element is visible (displayed and not `display: none`).

**Returns**: `bool`

---

#### AssertMedicalProviderDropdownsEnabledState()
Assert medical provider and facility dropdowns are enabled/disabled.

**Parameters**: `driver`, `shouldBeEnabled`

---

#### AssertDropdownEnabledState()
Assert a single dropdown is enabled/disabled.

**Parameters**: `dropdown`, `shouldBeEnabled`, `description`

---

#### GetMedicalProviderNotInListLink()
Get the "Not in List" link for medical provider.

---

#### GetMedicalFacilityNotInListLink()
Get the "Not in List" link for medical facility.

---

#### HasDisabledClass()
Check if element has "disabled" CSS class.

**Returns**: `bool`

---

#### EnsureMedicaidCaseNumberInputVisible()
Assert Medicaid case number input is visible and return it.

**Returns**: IWebElement (Medicaid case number input)

---

### Modal Helpers

#### WaitForElementToDisappear()
Wait for an element to disappear from the page.

**Parameters**: `driver`, `cssSelector`, `timeoutSeconds` (default 10)

**Use Case**: Wait for modal to close after submission

---

#### FindInputInContainerByIdParts()
Find an input within a container by matching multiple ID parts.

**Parameters**: `container`, `description`, `idParts` (variable number)

**Example**: Find input with ID containing both "MedicalFacility" and "Name"

**Returns**: IWebElement

---

### Utility

#### TrySwitchBackToAdditionalItems()
Attempt to switch back to Additional Items tab (gracefully handles errors).

**Use Case**: After form submission, automatically return to Additional Items tab

---

#### ParseDate()
Parse date string in MM/dd/yy or MM/dd/yyyy format.

**Returns**: DateTime

**Throws**: InvalidOperationException if parse fails

---

## Important Concepts

### 1. Multi-Tab Form Structure

**5 Tabs**:
- Target Child (birth info)
- Health Insurance (Medicaid, insurance, medical provider)
- Additional Items (parity, delivery, breastfeeding)
- PHQ-9 (depression screening)
- MIECHV (MIECHV-specific data)

**Navigation Pattern**: Use `SwitchToTab()` helper between tabs

**Submission**: Submit button is in panel footer, visible on all tabs

**Validation**: Validation summary appears on all tabs, shows errors from all tabs

---

### 2. Progressive Validation

**Pattern**: Submit form multiple times, filling one required field at a time.

**Flow**:
```
Submit empty → 4 validations
Fill Birth Term → 3 validations
Fill First Name → 2 validations
Fill Last Name → 1 validation
Fill Parity → 0 validations for these fields, new validation for Gestational Age
Fill Gestational Age → 0 validations
```

**Purpose**: Demonstrates real-time validation feedback and field dependencies

---

### 3. Medicaid Conditional Logic

**Rule**: If Medicaid = "Yes", all health insurance checkboxes are **disabled**.

**Logic**:
```
IF Medicaid = "Yes" (value "1")
THEN
    Health insurance checkboxes → Disabled
    Medicaid case number input → Visible
ELSE IF Medicaid = "No" (value "0") OR "Unknown" (value "9")
THEN
    Health insurance checkboxes → Enabled
    Medicaid case number input → Hidden (for "No")
```

**Test Coverage**: Test 6 validates this logic thoroughly

---

### 4. Medical Provider Question (Q19) Conditional Logic

**Rule**: Question 20 (medical provider/facility dropdowns) is controlled by Question 19.

**Logic**:
```
IF Q19 = "No" (value "0")
THEN
    Medical provider dropdown → Disabled
    Medical facility dropdown → Disabled
    "Not in List" doctor link → Disabled class
    "Not in List" facility link → Disabled class
ELSE IF Q19 = "Yes" (value "1")
THEN
    Medical provider dropdown → Enabled
    Medical facility dropdown → Enabled
    "Not in List" doctor link → Enabled
    "Not in List" facility link → Enabled
```

---

### 5. Medical Provider/Facility Modals

**"Not in List" Pattern**: When medical provider/facility is not in dropdown, click "Not in List" link to open modal and add new entry.

**Modal Workflow**:
```
Click "Not in List" link
    ↓
Modal opens
    ↓
Fill required fields (Last Name for provider, Name for facility)
    ↓
Submit modal
    ↓
Modal closes
    ↓
New entry appears in dropdown
    ↓
New entry is auto-selected
```

**Validation**: Both modals validate required fields before allowing submission

**Test Coverage**: Test 6 validates both provider and facility modals

---

### 6. PHQ-9 Scoring System

**9 Questions**: Each scored 0-3 (Not at all, Several days, More than half the days, Nearly every day)

**Total Score**: Sum of 9 questions (0-27)

**Result Calculation**:
- **Score < 10**: Negative
- **Score >= 10**: Positive

**Validity**:
- **All 9 answered**: Valid
- **Any blank**: Invalid, shows "N/A"

**Real-Time Calculation**: Score, Result, and Validity update immediately as user answers questions

**Labels**:
- `lblPHQ9Score`: Displays total score (0-27) or "N/A"
- `lblPHQ9Result`: Displays "Positive" or "Negative" or "N/A"
- `lblPHQ9ScoreValidity`: Displays "Valid" or "Invalid"

**Test Coverage**: Test 8 validates scoring logic with incomplete and complete scenarios

---

### 7. Birth Weight Validation

**Validation Rules**:
- **Pounds**: Must be < 17
- **Ounces**: Must be < 16

**Both Rules Enforced**: Both validations can appear simultaneously

**Test Scenarios**:
- (-1, -2): Both fail
- (20, 20): Both fail
- (17, 16): Both fail (boundary)
- (0-16, 0-15): Pass

**Test Coverage**: Test 5 validates all scenarios

---

### 8. Date Validations

**Prenatal Care Date**:
- Must be **before** Target Child DOB
- Validation: "Date Began Receiving Prenatal Care can not start after the birth of the child"

**PHQ-9 Date**:
- Must be **on or after** Target Child DOB
- Must be **on or after** Intake Date
- Validation: "[PHQ-9] The PHQ date administered must be on or after the TC's date of birth!"

**Date Formats Accepted**: `MM/dd/yy` or `MM/dd/yyyy`

---

### 9. Conditional "Other Specify" Fields

**Pattern**: When "Other" option is selected, a "specify" text input appears.

**3 Instances in Form**:

1. **Health Insurance → Other**:
   - Checkbox: `input[id$='chkTCHIOther']`
   - Specify: `input[id$='txtTCHIOtherSpecify']`

2. **PHQ-9 → Participant = Other**:
   - Dropdown: `select[id$='ddlPHQ9Participant']` (value "04")
   - Specify: `input[id$='txtPHQ9ParticipantSpecify']`

3. **MIECHV → Medical Care Source = Other**:
   - Dropdown: `select[id$='ddlTCMedicalCareSource']` (value "06")
   - Specify: `input[id$='txtTCMedicalCareSourceOtherSpecify']`

**Test Coverage**:
- Test 6: Health Insurance Other
- Test 8: PHQ-9 Participant Other
- Test 9: MIECHV Medical Care Source Other

---

### 10. Tooltip Icons

**Location**: Additional Items tab

**Icon**: `span.glyphicon.glyphicon-question-sign`

**Tooltip Display**: `.tooltip-inner`

**Purpose**: Provide helpful explanations for optional fields

**Test Coverage**: Test 7 validates tooltip functionality

---

## What to Keep in Mind

### When Modifying Tests

1. **Test Order Matters**: Tests 4-9 require existing TCID entries created by manual setup or previous test runs.

2. **Multi-Tab Form**: Always switch to correct tab before interacting with fields.

3. **Submit Button Location**: Submit button is in panel footer, visible from all tabs.

4. **Validation Appears on All Tabs**: Validation summary shows errors from all tabs, regardless of which tab you're on.

5. **Partial Class Pattern**: Changes to helper methods in one file affect all test files.

6. **Selector Constants**: All selectors defined as constants at top of TargetChildIDTests.cs.

---

### When Adding New Tests

1. **Assign Priority**: Add `[TestPriority(N)]` with number > 9.

2. **Use Parameterization**: Add `[Theory]` and `[MemberData(nameof(GetTestPc1Ids))]`.

3. **Start with Navigation**: Use `NavigateToTargetChildPage()` and `OpenExistingTcidEntry()` or `OpenNewTcidForm()`.

4. **Use Tab Helpers**: Use `SwitchToTab()` to navigate between tabs.

5. **Log Actions**: Use `_output.WriteLine()` for all significant actions.

6. **Wait After Tab Switch**: Use `WaitForReady()` and `Thread.Sleep()` after tab switches.

7. **Verify with Toast**: Success should show jQuery toast - use `WebElementHelper.GetToastMessage()`.

---

### Common Pitfalls

1. **Tab Not Active**: Forgot to switch to correct tab before interacting with field.
   - **Solution**: Always use `SwitchToTab()` before finding elements.

2. **Validation Summary Not Found**: Validation redirected to different tab.
   - **Solution**: Validation summary exists on all tabs - don't switch tabs to find it.

3. **Medicaid Checkbox Enabled When Should Be Disabled**: Medicaid dropdown not set to "Yes".
   - **Solution**: Ensure Medicaid dropdown selection completes before checking checkboxes.

4. **Modal Not Closing**: Modal close animation takes time.
   - **Solution**: Use `WaitForElementToDisappear()` with sufficient timeout.

5. **PHQ Score Not Updating**: JavaScript calculation not triggered.
   - **Solution**: Wait with `WaitForReady()` and `Thread.Sleep()` after setting scores.

6. **Provider/Facility Not Auto-Selected**: Dropdown not refreshed after modal close.
   - **Solution**: Switch back to Health Insurance tab after modal closes.

7. **Prenatal Date Validation Not Appearing**: Date is actually valid (before birth).
   - **Solution**: Verify Target Child DOB before setting test date.

8. **Birth Weight Validation Not Clearing**: Values still out of range.
   - **Solution**: Ensure values are strictly less than limits (< 17 lbs, < 16 oz).

---

## Test Data Requirements

### Prerequisites
- Test user with DataEntry role
- Valid PC1 IDs in `appsettings.json` under `TestPc1Ids`
- Cases must have:
  - At least one existing TCID entry (for Tests 4-9)
  - Target Child DOB and Intake Date visible on form

### Test Creates
- New TCID entries (Test 2, Test 3)
- Medical providers via modal (Test 6)
- Medical facilities via modal (Test 6)

### Test Modifies
- Existing TCID entries (Tests 4-9)
- PHQ-9 scores
- Additional Items selections

### Test Deletes
- None

**Net Impact**: Some TCID entries created, some modified.

---

## Running the Tests

### Run All TargetChildID Tests
```bash
dotnet test --filter "FullyQualifiedName~TargetChildID"
```

### Run Specific Test
```bash
# Navigation (Priority 1)
dotnet test --filter "FullyQualifiedName~TargetChildIDTests.NavigateToTargetChildInformationForm"

# Info alert (Priority 2)
dotnet test --filter "FullyQualifiedName~TargetChildIDTests.NewTcidButtonDisplaysInfoAlert"

# Progressive validation (Priority 3)
dotnet test --filter "FullyQualifiedName~TargetChildIDTests.ValidationAndSubmission"

# Prenatal care date (Priority 4)
dotnet test --filter "FullyQualifiedName~TargetChildIDTests.ExistingTcidPrenatalCareValidation"

# Birth weight (Priority 5)
dotnet test --filter "FullyQualifiedName~TargetChildIDTests.ExistingTcidBirthWeightValidation"

# Health insurance (Priority 6)
dotnet test --filter "FullyQualifiedName~TargetChildIDTests.HealthInsuranceOptionsRespectMedicaidSelection"

# Additional Items (Priority 7)
dotnet test --filter "FullyQualifiedName~TargetChildIDTests.AdditionalItemsParityAndDeliveryFlow"

# PHQ-9 (Priority 8)
dotnet test --filter "FullyQualifiedName~TargetChildIDTests.Phq9DateValidationAndSave"

# MIECHV (Priority 9)
dotnet test --filter "FullyQualifiedName~TargetChildIDTests.MiechvMedicalCareSourceOtherSpecifyToggle"
```

---

## Troubleshooting

### Test Fails: "Target Child Information link was not found"
- **Issue**: Link not in Forms pane or selector changed
- **Solution**: Verify Forms tab loaded, check link selector

### Test Fails: "TCID grid was not found"
- **Issue**: Page didn't load or no TCID entries exist
- **Solution**: Verify case has at least one TCID entry, check grid selector

### Test Fails: Birth weight validation doesn't appear
- **Issue**: Values are actually within valid range
- **Solution**: Verify test values are >= 17 lbs or >= 16 oz

### Test Fails: Health insurance checkboxes not disabled
- **Issue**: Medicaid dropdown selection didn't trigger update
- **Solution**: Increase wait time after Medicaid selection, verify page refresh

### Test Fails: Medical provider modal validation not appearing
- **Issue**: Validation logic changed or selector incorrect
- **Solution**: Verify modal validation selector, check required fields

### Test Fails: PHQ scores not calculating
- **Issue**: JavaScript not running or scores not set correctly
- **Solution**: Verify all 9 dropdowns set, increase wait time for calculation

### Test Fails: Prenatal care date validation appears unexpectedly
- **Issue**: Target Child DOB changed or test date is actually after birth
- **Solution**: Verify Target Child DOB, adjust test dates accordingly

### Test Fails: Tab switch doesn't work
- **Issue**: Tab selector changed or JavaScript not loaded
- **Solution**: Verify tab selectors, check for JavaScript errors

### Test Fails: "Other specify" input not visible
- **Issue**: "Other" option not triggering show/hide logic
- **Solution**: Verify "Other" value is correct, increase wait time after selection

---

## Selectors Reference

### Navigation

| Element | Selector |
|---------|----------|
| **Target Child Link** | `a.moreInfo[data-formtype='TCIBO']` |
| **New TCID Button** | `a.btn.btn-default[title*='New']` |
| **TCID Grid** | `table[id*='grTCIDs']` |
| **TCID Row Link** | `td a[href*='tcid.aspx']` |

### Form Elements

| Element | Selector |
|---------|----------|
| **Submit Button** | `div.panel-footer a.btn.btn-primary` |
| **Validation Summary** | `div[id$='ValidationSummary1']` |
| **Info Alert** | `div.alert.alert-info` |

### Target Child Tab

| Element | Selector |
|---------|----------|
| **Birth Term Dropdown** | `select[id$='ddlBirthTerm']` |
| **First Name Input** | `input[id$='txtTCFirstName']` |
| **Last Name Input** | `input[id$='txtTCLastName']` |
| **Gestational Age Input** | `input[id$='txtGestationalAge']` |
| **Birth Weight Lbs Input** | `input[id$='txtBirthWtLbs']` |
| **Birth Weight Oz Input** | `input[id$='txtBirthWtOz']` |
| **Prenatal Care Input** | `input[id*='Prenatal'][type='text']` |

### Health Insurance Tab

| Element | Selector |
|---------|----------|
| **Medicaid Dropdown** | `select[id$='ddlTCReceivingMedicaid']` |
| **Medicaid Case Number Input** | `input[id$='txtTcHIMedicaidCaseNumber']` |
| **Health Insurance Checkboxes** | `input[id$='chkTCHI...']` |
| **Has Medical Provider Dropdown** | `select[id$='ddlTCHasMedicalProvider']` |
| **Medical Provider Dropdown** | `select[id$='ddlTCMedicalProviderFK']` |
| **Medical Facility Dropdown** | `select[id$='ddlTCMedicalFacilityFK']` |
| **Provider "Not in List" Link** | `a[id$='lnkNewMedicalProvider']` |
| **Facility "Not in List" Link** | `a[id$='lnkNewMedicalFacility']` |

### Additional Items Tab

| Element | Selector |
|---------|----------|
| **Parity Dropdown** | `select[id$='ddlParity']` |
| **Delivery Type Dropdown** | `select[id$='ddlDeliveryType']` |
| **Breast Milk Dropdown** | `select[id$='ddlChildFedBreastMilk']` |
| **Tooltip Icons** | `#OptionalItems span.glyphicon-question-sign` |

### PHQ-9 Tab

| Element | Selector |
|---------|----------|
| **PHQ Date Input** | `input[id$='txtPHQDateAdministered']` |
| **Participant Dropdown** | `select[id$='ddlPHQ9Participant']` |
| **Worker Dropdown** | `select[id$='ddlPHQWorker']` |
| **PHQ Question Dropdowns** | `#PHQ9 select.phq9score` |
| **Difficulty Dropdown** | `select[id$='ddlDifficulty']` |
| **Referral Checkbox** | `input[id$='chkDepressionReferralMade']` |
| **Score Label** | `span[id$='lblPHQ9Score']` |
| **Result Label** | `span[id$='lblPHQ9Result']` |
| **Validity Label** | `span[id$='lblPHQ9ScoreValidity']` |

### MIECHV Tab

| Element | Selector |
|---------|----------|
| **Medical Care Source Dropdown** | `select[id$='ddlTCMedicalCareSource']` |
| **Medical Care Source Specify Input** | `input[id$='txtTCMedicalCareSourceOtherSpecify']` |

### Modals

| Element | Selector |
|---------|----------|
| **Medical Provider Modal** | `.modal.show .modal-content` |
| **Medical Facility Modal** | `.modal.show .modal-content` |
| **Modal Submit Button (Provider)** | `a[id$='ctlMedicalProvider_btnSubmitProvider']` |
| **Modal Submit Button (Facility)** | `a[id$='ctlMedicalFacility_btnSubmitFacility']` |
