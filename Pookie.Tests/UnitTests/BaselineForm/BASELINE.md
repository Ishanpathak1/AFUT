# Baseline Form Tests Documentation

## Overview

The Baseline Form test suite contains comprehensive end-to-end tests for the multi-tab Baseline Form (Intake.aspx). These tests validate navigation, form fields, conditional logic, validation rules, data entry, and persistence across 7 different tabs of the form.

## Test Structure

### Test Files (7 Total)

1. **BaselineFormTests.cs** - Basic navigation (Priority 1)
2. **BaselineFormValidationTests.cs** - PC1 tab comprehensive validation (Priority 2)
3. **BaselineFormPC1MedicalProviderTests.cs** - Medical Provider/Benefits tab (Priority 3)
4. **BaselineFormFamilyChildrenTests.cs** - Family/Other Children tab (Priorities 4-5)
5. **BaselineFormPHQ9Tests.cs** - PHQ-9 screening tab (Priority 6)
6. **BaselineFormMIECHVTests.cs** - MIECHV supplemental questions (Priority 7)
7. **BaselineFormPC2Tests.cs** - PC2 tab validation (inherits from BaselineFormValidationTests)

### Test Execution Order

Tests run in priority order (1-7) using the `[TestPriority]` attribute. Each test builds on previous tests and assumes certain baseline data exists.

## Detailed Test Breakdown

---

## 1. BaselineFormTests.cs (Priority 1)

### Test: `BaselineFormLinkNavigatesToIntakePage`

**Purpose**: Validates basic navigation to the Baseline Form.

**What it tests**:
- Navigation from Forms tab → Baseline Form link
- URL contains "Intake.aspx"
- URL contains correct PC1 ID and IPK parameters

**Key Assertions**:
- URL starts with expected base URL
- Query parameters include `pc1id` and `ipk=57561`

---

## 2. BaselineFormValidationTests.cs (Priority 2)

### Test 1: `SubmitShowsRelationshipValidationMessage`

**Purpose**: Validates that submitting an empty form shows required field validation.

**What it tests**:
- Navigates to Baseline Form → PC1 tab
- Ensures relationship dropdown is set to "--Select--"
- Clicks Submit without entering required data
- Verifies "Please enter relationship to target child" validation appears

---

### Test 2: `ConditionalQuestionsRespondToBaselineSelections`

**Purpose**: Comprehensive test of conditional field visibility, validation, and data entry for PC1 tab.

**What it tests**:

#### A. Basic Demographics
- Gender selection
- Relationship to target child (Step-parent)
- Marital status (Never Married)
- Race (Black or African American checkbox)
- Ethnicity (Hispanic)

#### B. Born in USA Conditional Logic
1. Selects "No" → Verify follow-up fields appear (Birth Country, Years in USA)
2. Fills in "Canada" and "5 years"
3. Changes to "Yes" → Verify follow-up fields hide

#### C. Primary Language "Other" Conditional
1. Selects "99. Other" → Verify specify field appears
2. Enters "Elvish" in specify field
3. Changes to "1. English" → Verify specify field hides

#### D. Educational Enrollment & Consistency Validation
1. Selects higher grade level (07 or 08)
2. Selects "Yes" for enrollment
3. Submits → Verifies **consistency validation**: "Highest Grade Completed and Educational Enrollment do not agree"
4. Verifies enrollment hours section appears
5. Submits without hours → Verifies "Hours per month required" validation
6. Enters **invalid hours** (>450) → Verifies "between 0 and 450" validation
7. Enters **valid hours** (1-450) → Validation clears

#### E. Program Type Conditional Logic
1. When enrollment = "No" → Verifies program checkboxes are **disabled**
2. When enrollment = "Yes":
   - Verifies program checkboxes are **enabled**
   - Submits without selecting any → Verifies "must specify a education or employment program" validation
   - Selects "Other" checkbox → Verify specify field appears
   - Fills in specify field

#### F. Employment Conditional Logic
1. Selects "Yes" for currently employed → Verifies employment fields enabled
2. Submits without date → Verifies "employment start date" validation
3. Selects "No" for currently employed → Verifies "Previously employed" and "Looked for employment" dropdowns appear and are enabled
4. Fills both dropdowns

#### G. Final Submission
- Submits with all valid data
- Verifies success toast: "Form Saved" + PC1 ID

**Key Concepts**:
- **Conditional Visibility**: Fields show/hide based on selections
- **Consistency Validation**: Cross-field validation (grade vs enrollment)
- **Range Validation**: Hours must be 0-450
- **Required Field Chaining**: Employment fields required when employed = Yes

---

## 3. BaselineFormPC1MedicalProviderTests.cs (Priority 3)

### Test: `MedicalProviderTabCompleteFlowTest`

**Purpose**: Tests Medical Provider/Benefits tab including adding providers/facilities, Medicaid logic, and public benefits validation.

**What it tests**:

#### A. OBP Involvement "Other" Validation
1. Selects "7. Other" → Verify specify field appears
2. Leaves specify empty and submits → Verifies "Please specify involvement of OBP" validation
3. Changes to a non-Other option → Validation clears

#### B. Add New Medical Provider
1. Sets "PC1 Has Medical Provider" to "Yes"
2. Clicks "Not in List" link → Modal opens
3. Clicks Submit without filling → Verifies "Provider's Last Name required" validation
4. Fills all provider fields:
   - First Name: `PC1medicalproviderFirstNameTest{timestamp}`
   - Last Name: `PC1medicalProviderLastNameTest{timestamp}`
   - Address, City, State (AA), Zip (00000), Phone (5555555555)
5. Submits → Waits for modal to close
6. Verifies new provider appears in dropdown
7. Selects the new provider

#### C. Add New Medical Facility
1. Clicks "Not in List" for facility → Modal opens
2. Clicks Submit without filling → Verifies "Facility Name required" validation
3. Fills all facility fields: Name, Address, City, State, Zip, Phone
4. Submits → Waits for modal to close
5. Verifies new facility appears in dropdown
6. Selects new facility

#### D. Medicaid and Health Insurance Interaction
1. Selects "No" for Medicaid:
   - Verifies Medicaid Case Number textbox is **hidden**
   - Verifies Health Insurance checkboxes are **enabled**
2. Selects "Unknown" for Medicaid:
   - Verifies Medicaid Case Number textbox is **hidden**
3. Selects "Yes" for Medicaid:
   - Verifies Medicaid Case Number textbox **appears**
   - Verifies Health Insurance checkboxes are **disabled**
4. Changes back to "No" → Checkboxes enabled again
5. Clicks "Other" health insurance checkbox → Verify specify field appears

#### E. Current Service Involvement
- Selects random values for 4 dropdowns:
  - Mental Health
  - Substance Abuse
  - Domestic Violence
  - CPS/ACS

#### F. Public Benefits Validation
1. Selects "Yes" for receiving public benefits
2. Submits without filling → Verifies ALL 5 benefit validations:
   - "TANF required"
   - "Food Stamps required"
   - "Emergency Assistance required"
   - "WIC required"
   - "SSI/SSD required"
3. Fills TANF → Verifies individual validation clears
4. Fills remaining 4 benefit dropdowns
5. Final submit → Success toast

**Key Concepts**:
- **Modal CRUD Operations**: Adding providers/facilities via modals
- **Mutual Exclusivity**: Medicaid = Yes disables Health Insurance
- **Cascading Required Fields**: Public benefits = Yes makes 5 fields required

---

## 4. BaselineFormFamilyChildrenTests.cs (Priorities 4-5)

### Test 1: `FamilyChildrenTabValidationTest` (Priority 4)

**Purpose**: Tests ALL validations for all 6 children entries systematically.

**What it tests**:

#### A. Household Income Fields
- Number of people in house (1-99, random)
- Average monthly income (0-99999, random)
- Average monthly benefits (0-99999, random)
- Number of persons contributing (0-99, random)

#### B. Validation Tests for Each Child (1-6)

For **each of the 6 children**, the test performs:

1. **Living Arrangement "Other" Validation**:
   - Selects "05. Other" → Verify specify field appears
   - Submits without specify → Verify validation: "Please specify Child{N} Living Arrangement"
   - Changes to non-Other option → Verify specify field hides

2. **Relationship "Other" Validation**:
   - Selects "09. Other" → Verify specify field appears
   - Submits without specify → Verify validation: "Please specify Child{N} relationship to PC 1"
   - Changes to non-Other option → Verify specify field hides

3. **First Name Blank Validation**:
   - Clears first name field
   - Submits → Verify validation: "Other child {N}: First Name cannot be blank"
   - Restores original first name

4. **Last Name Blank Validation**:
   - Clears last name field
   - Submits → Verify validation: "Other child {N}: Last Name cannot be blank"
   - Restores original last name

5. **Age Validation (Over 21 Years)**:
   - Enters DOB making child 22 years old
   - Submits → Verify validation: "over 21 years" + "not allowed" + "Other Children"
   - Tests **future date validation**:
     - Enters DOB 1-5 years in the future
     - Submits → Verify validation: "is in the future" + "not allowed"
   - Corrects to valid date (1-20 years old)

**Result**: All 6 children have valid data after corrections.

---

### Test 2: `FamilyChildrenTabSubmitTest` (Priority 5)

**Purpose**: Tests filling all 6 children with valid data, submitting, and verifying data persistence.

**What it tests**:

#### A. Fill Household Income
- Number in house: 99
- Monthly income: 12
- Monthly benefits: 12
- Number contributing: 99

#### B. Fill All 6 Children with Valid Data

Predefined names for consistency:
- Child 1: wonder lasgirl
- Child 2: captain patrick
- Child 3: bat hired
- Child 4: super denim
- Child 5: iron catching
- Child 6: Peter parker

For each child:
- First Name, Last Name
- DOB: Random date making child 1-20 years old (under 21)
- Relationship: Random (excluding "Other")
- Living Arrangement: Random (excluding "Other")

#### C. Submit and Verify Toast
- Clicks Submit
- Verifies success toast: "Form Saved"
- Waits for redirect to CaseHome.aspx

#### D. Verify Data Persistence
1. Navigates back to Forms tab
2. Clicks Baseline Form link again
3. Navigates to Family/Other Children tab
4. Verifies **all household income fields persisted**
5. Verifies **all 6 children data persisted**:
   - First Name, Last Name, DOB match expected values

**Key Concepts**:
- **Iterative Validation**: Same validation logic applied to all 6 children
- **Data Persistence**: Form retains data after save
- **Age Business Rules**: Children must be under 21 and not future-dated

---

## 5. BaselineFormPHQ9Tests.cs (Priority 6)

### Test: `PHQ9TabCompleteFlowTest`

**Purpose**: Tests PHQ-9 depression screening tab including date validation, participant logic, refused checkbox, and score calculation.

**What it tests**:

#### A. Date Validation
1. Reads screen date from page
2. Enters date **one day before** screen date → Submits
3. Verifies validation: "The PHQ date administered must be on or after the case start date"
4. Corrects date to **screen date** (valid)

#### B. Participant "Other" Validation
1. Selects "04. Other" in Q33 (Participant) → Verify specify field appears
2. Leaves specify empty → Submits
3. Verifies validation: "You must specify the participant if the 'Other' option is selected"
4. Changes to "01. PC1"

#### C. Refused Checkbox Behavior
1. Checks "PHQ-9 refused" checkbox
2. Verifies Q36-Q44 score dropdowns are **disabled**
3. Submits without worker → Verifies validation: "You must select the worker if the PHQ was refused or information about the PHQ is entered"
4. Unchecks refused checkbox

#### D. Score Calculation with Random Values
1. Randomly selects values (01-04) for Q36-Q44 (9 questions):
   - Q36: Interest
   - Q37: Feeling Down
   - Q38: Sleep Problems
   - Q39: Tired
   - Q40: Appetite
   - Q41: Bad Self
   - Q42: Concentration
   - Q43: Slow or Fast
   - Q44: Better Off Dead
2. Calculates **expected total score**: Sum of all questions (value 01=0, 02=1, 03=2, 04=3)
3. Selects random value for Q45 (Difficulty)
4. Verifies:
   - **Actual score** matches **expected score**
   - **Result** is correct:
     - Score > 9 → "Positive"
     - Score ≤ 9 → "Negative"
   - **Validity** shows "Valid"

#### E. Worker Required Validation
1. Submits with scores but no worker selected
2. Verifies validation: "You must select the worker..."

#### F. Select Worker and Submit
1. Selects worker: "105, Worker"
2. Submits
3. Verifies success toast: "Form Saved" + PC1 ID

**PHQ-9 Scoring Logic**:
- Each question Q36-Q44 has 4 options: "Not at all (0)", "Several days (1)", "More than half (2)", "Nearly every day (3)"
- Total Score = Sum of 9 questions (range: 0-27)
- Result interpretation:
  - **0-9**: Negative (minimal/mild depression)
  - **10-27**: Positive (moderate/severe depression)
- Q45 (Difficulty) doesn't affect score, just functional impact

**Key Concepts**:
- **Conditional Required**: Worker required if refused OR scores entered
- **Date Business Rules**: Must be on or after case start date
- **Dynamic Score Calculation**: Client-side JavaScript calculates score in real-time
- **Validation vs Refusal**: Can refuse without entering scores OR enter scores with worker

---

## 6. BaselineFormMIECHVTests.cs (Priority 7)

### Test: `MIECHVTabCompleteFlowTest`

**Purpose**: Tests MIECHV (Maternal, Infant, and Early Childhood Home Visiting) supplemental questions tab.

**What it tests**:

#### A. Navigate to MIECHV Tab
- Navigates to Baseline Form
- Activates MIECHV tab

#### B. Fill MIECHV Form with Random Values
Selects random valid options for 6 MIECHV questions:

1. **PC1-3a**: PC1 Living Arrangement
2. **PC1-3b**: PC1 Living Situation Specific
3. **PC1-4**: PC1 Self Low Student Achievement
4. **PC1-5**: Children Low Student Achievement
5. **PC1-6**: Other Children Developmental Delays
6. **PC1-7**: Family Armed Forces

For each dropdown:
- Finds dropdown by CSS selector
- Selects random valid option (excluding empty values)
- Waits for update panel
- Logs selected value

#### C. Submit and Verify
- Clicks Submit button
- Waits for toast or redirect
- Verifies success via:
  - Success toast containing "Form Saved" + PC1 ID, OR
  - Redirect to CaseHome.aspx
- Handles error page redirect as failure

**Key Concepts**:
- **Supplemental Data Collection**: MIECHV collects additional risk factors
- **All Optional**: No required field validations (unlike other tabs)
- **Random Value Testing**: Tests form accepts any valid dropdown selections

---

## 7. BaselineFormPC2Tests.cs

### Tests: `SubmitShowsRelationshipValidationMessage` and `ConditionalQuestionsRespondToBaselineSelections`

**Purpose**: Tests PC2 (Primary Caregiver 2) tab with **exact same logic** as PC1 tab.

**Architecture**: Inherits from `BaselineFormValidationTests` and only overrides:
- `FormToken`: "PC2Form" (used in selector replacement)
- `TabSelector`: "#tab_PC2 a[href='#PC2']"
- `CheckConsistencyValidation`: false (PC2 doesn't have enrollment consistency checks)

**PC2-Specific Behavior**:
- After ANY submit (even validation failure), page resets to PC1 tab
- Overridden `ClickSubmitButton()` automatically switches back to PC2 tab after submit
- If PC2 tab disappears after submit (successful save), doesn't attempt to switch back

**What it tests**:
All the same tests as PC1:
1. Relationship validation
2. Conditional questions (Born in USA, Primary Language, Enrollment, Employment, etc.)

**Key Difference**: PC2 is for cases with 2 primary caregivers (e.g., mother and father).

---

## Common Helper Methods

### Navigation Helpers
- `NavigateToBaselineForm()` - Navigates from Forms pane to Baseline Form (Intake.aspx)
- `ActivateTab()` - Clicks a specific tab link and waits for it to load

### Form Field Helpers
- `FindSubmitButton()` - Finds main Submit button on page
- `SelectRelationshipDropdown()` - Selects relationship to target child dropdown
- `SelectRandomDropdownOption()` - Selects random valid option from dropdown
- `SelectSpecificDropdownOption()` - Selects specific option based on predicate

### Validation Helpers
- `FindValidationMessage()` - Finds validation message containing specified keywords
- `WaitUntilElementHidden()` - Waits for element to hide (for conditional visibility)
- `WaitUntilChildrenHidden()` - Waits for child elements to hide
- `ElementIsDisplayed()` - Safely checks if element is displayed (handles stale references)

### Program/Checkbox Helpers (PC1/PC2)
- `WaitForProgramCheckbox()` - Waits for program type checkbox to appear
- `SetProgramCheckboxState()` - Sets checkbox to checked/unchecked state

### Utility Helpers
- `GetRandomNumber()` - Thread-safe random number generator
- `GenerateRandomDateUnder21()` - Generates valid child DOB (1-20 years old)
- `WaitForModalToClose()` - Waits for modal to close completely

---

## Coding Standards Applied

### 1. CSS Classes Over IDs 
All selectors use CSS classes and semantic attributes:

```csharp
// Good - Uses form-control class and partial ID
"select.form-control[id*='ddlLivingArrangement']"

// Good - Uses Bootstrap classes
"a.btn.btn-primary"
```

### 2. Use Existing Helper Methods 
Tests leverage:
- `CommonTestHelper.NavigateToFormsTab()` - For login → role → search → forms
- `CommonTestHelper.FindPc1Display()` - For PC1 ID verification
- `CommonTestHelper.ClickElement()` - For clicking with JavaScript fallback
- `WebElementHelper.SelectWorker()` - For worker dropdown
- `WebElementHelper.SelectDropdownOption()` - For dropdown selections
- `WebElementHelper.SetInputValue()` - For input fields
- `WebElementHelper.FindElementInModalOrPage()` - For finding elements
- `WebElementHelper.GetToastMessage()` - For toast notifications

### 3. Clear Error Messages 
```csharp
?? throw new InvalidOperationException("Child {childNumber} Living Arrangement dropdown was not found.");
```

---

## Important Concepts

### 1. Multi-Tab Form Architecture

The Baseline Form (Intake.aspx) has **multiple tabs**:
- **PC1** (Primary Caregiver 1): Demographics, education, employment
- **PC2** (Primary Caregiver 2): Same as PC1, for second caregiver
- **Medical Provider/Benefits**: Healthcare, insurance, public benefits
- **Family/Other Children**: Household info, up to 6 other children
- **PHQ-9**: Depression screening questionnaire
- **MIECHV**: Supplemental risk factor questions

Tests activate tabs using: `ActivateTab(driver, "#tab_NAME a[href='#NAME']", "Display Name")`

### 2. Conditional Visibility

Many fields show/hide based on selections:

| Trigger | Condition | Action |
|---------|-----------|--------|
| Born in USA | = "No" | Show: Birth Country, Years in USA |
| Primary Language | = "99. Other" | Show: Specify Language textbox |
| Educational Enrollment | = "Yes" | Show: Hours input, Enable program checkboxes |
| Currently Employed | = "Yes" | Enable: Start Date, Hours, Wages |
| Currently Employed | = "No" | Enable: Previously Employed, Looked for Employment |
| Receiving Medicaid | = "Yes" | Show: Case Number, Disable Health Insurance |
| Receiving Public Benefits | = "Yes" | Require: All 5 benefit dropdowns |
| Living Arrangement | = "05. Other" | Show: Specify textbox (for each child) |
| Relationship | = "09. Other" | Show: Specify textbox (for each child) |
| PHQ-9 Refused | = Checked | Disable: Q36-Q44 score dropdowns |

### 3. Consistency Validation (PC1 Only)

**Business Rule**: Highest Grade Completed must be consistent with Educational Enrollment and Program Type.

Examples:
- Grade = "07. Some High School" + Enrollment = "Yes" + Program = "Middle School" → **Validation error**
- Grade = "08. High School Graduate" + Enrollment = "Yes" → **Validation error** (can't be enrolled if graduated)

### 4. Cross-Field Dependencies

- **Employment**: Selecting "Yes" requires Start Date, Hours, and Wages
- **Public Benefits**: Selecting "Yes" requires ALL 5 benefit types (TANF, Food Stamps, Emergency Assistance, WIC, SSI/SSD)
- **PHQ-9 Worker**: Required if EITHER refused OR scores entered
- **Child Data**: If any child field is entered, First Name and Last Name become required for that child

### 5. Data Validation Rules

| Field | Rule |
|-------|------|
| Educational Hours | Must be 0-450 |
| Child Age | Must be under 21 years and not future date |
| PHQ-9 Date | Must be on or after case start date |
| Number in House | 0-99 |
| Monthly Income/Benefits | 0-99999 |
| Zip Code | Numeric, 5 digits |
| Phone | Numeric, 10 digits |

### 6. Form Submission Behavior

After clicking Submit:
1. **Validation Failure**: Page may reset to PC1 tab, must switch back to correct tab
2. **Validation Success**: Shows toast message OR redirects to CaseHome.aspx
3. **PC2 Bug**: Always resets to PC1 after any submit (handled by test)

Toast message format: "Form Saved - [PC1ID]"

### 7. Modal CRUD Operations

Adding Medical Provider/Facility:
1. Click "Not in List" link
2. Modal opens with fields
3. Submit without filling → Validation appears **in modal**
4. Fill all required fields
5. Submit → Modal closes
6. Wait for page refresh
7. New item appears in dropdown on main page

### 8. Random Value Testing

Many tests use random values to ensure:
- Tests don't rely on specific data
- System handles any valid input
- Repeated runs don't conflict (using timestamps for provider names)

Random generation is **thread-safe** using locks:
```csharp
lock (RandomLock)
{
    return RandomGenerator.Next(minInclusive, maxInclusive + 1);
}
```

---

## What to Keep in Mind

### When Modifying Tests

1. **Test Order Matters**: Tests run in priority order (1-7). Later tests may depend on earlier tests completing.

2. **Waits Are Critical**: Always wait after interactions:
   ```csharp
   driver.WaitForUpdatePanel(10);
   driver.WaitForReady(10);
   Thread.Sleep(500); // UI settle time
   ```

3. **Tab Switching After Submit**: Page often resets to PC1 after submit. Always switch back to the tab you're testing.

4. **Stale Element References**: After postbacks, re-find elements:
   ```csharp
   // Re-find after postback
   dateInput = driver.FindElements(By.CssSelector(...)).FirstOrDefault(el => el.Displayed);
   ```

5. **Modal vs Page Context**: Be clear about whether you're finding elements in a modal or on the main page.

6. **PC2 Inherits PC1 Logic**: If you update PC1 validation tests, PC2 automatically gets the same updates.

### When Adding New Tests

1. **Assign Test Priority**: Add `[TestPriority(N)]` attribute with appropriate order number
2. **Use Parameterization**: Add `[Theory]` and `[MemberData(nameof(GetTestPc1Ids))]`
3. **Start with Navigation**: Use `CommonTestHelper.NavigateToFormsTab()` then `NavigateToBaselineForm()`
4. **Activate Tab**: Call `ActivateTab(driver, "#tab_NAME a[href='#NAME']", "Tab Name")`
5. **Log Important Steps**: Use `_output.WriteLine()` with `[INFO]`, `[PASS]`, `[WARN]` prefixes
6. **Re-activate Tab After Submit**: Always switch back to your tab after submitting

### Common Pitfalls

1. **Forgetting Tab Context**: After validation or submit, page resets to PC1. Always re-activate your tab.
2. **Not Waiting for Modals**: After adding provider/facility, wait for modal to close AND page to refresh.
3. **Assuming Field Visibility**: Always verify conditional fields are actually displayed before interacting.
4. **Hardcoded Timestamps**: For provider names, use dynamic timestamps to avoid conflicts.
5. **Inconsistent Random Values**: Use thread-safe random generation, don't create new Random() instances.
6. **Missing Data Dependencies**: Ensure a child has basic info (name, DOB, relationship) before testing specific validations.

---

## Test Data Requirements

### Prerequisites
- Valid PC1 IDs configured in `appsettings.json` under `TestPc1Ids`
- Test user with DataEntry role permissions
- Case must have:
  - Start date (for PHQ-9 date validation)
  - Target child (for relationship validation)

### Test Creates
- Medical Provider (with unique timestamp in name)
- Medical Facility ("PC1FacilityNameTest")
- 6 children entries with predefined names
- Random dropdown selections across multiple tabs

### Test Modifies
- Various baseline form fields across all tabs
- Data persists between test runs

### Test Deletes
- None (tests are read-create only, no deletions)

---

## Running the Tests

### Run All Baseline Form Tests
```bash
dotnet test --filter "FullyQualifiedName~BaselineForm"
```

### Run Specific Test File
```bash
dotnet test --filter "FullyQualifiedName~BaselineFormPHQ9Tests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~BaselineFormValidationTests.ConditionalQuestionsRespondToBaselineSelections"
```

### Run in Order (Recommended)
The `[TestPriority]` attribute ensures proper execution order automatically.

---

## Troubleshooting

### Test Fails at Navigation
- **Issue**: Cannot find Baseline Form link in Forms tab
- **Solution**: Verify Forms tab loads, check CSS selectors

### Test Fails at Tab Activation
- **Issue**: Tab link not found
- **Solution**: Verify tab selector matches actual HTML, ensure form loaded

### Test Fails at Conditional Field
- **Issue**: Field doesn't appear when expected
- **Solution**: Check parent dropdown selection, verify JavaScript loaded, increase wait times

### Test Fails at Validation Check
- **Issue**: Validation message not found
- **Solution**: Tab may have reset to PC1, re-activate tab and check again

### Test Fails at Modal Operations
- **Issue**: Can't find element in modal OR element found on main page instead
- **Solution**: Use `WebElementHelper.FindElementInModalOrPage()`, ensure modal is fully open

### Test Fails at Child Iteration
- **Issue**: Child 3 validation fails but Child 1-2 passed
- **Solution**: Check if previous child tests left form in bad state, verify child-specific selectors

### Test Fails at Submit
- **Issue**: Success toast doesn't appear
- **Solution**: Check for validation errors on page, verify all required fields filled, check for error page redirect

---

## PC2 vs PC1 Differences

| Aspect | PC1 | PC2 |
|--------|-----|-----|
| **Tab Selector** | `#tab_PC1` | `#tab_PC2` |
| **Form Token** | "PC1Form" | "PC2Form" |
| **Consistency Validation** | Yes (enrollment vs grade) | No |
| **Tab Reset Bug** | No | Yes (always resets to PC1) |
| **Field Names** | `ddlRelation`, `ddlGender` | `PC2Form_ddlRelation`, `PC2Form_ddlGender` |
| **Test Implementation** | Base class | Inherits from base class |

---

## Future Enhancements

Potential areas for expansion:
- Test negative scenarios (invalid formats, SQL injection attempts)
- Test date boundary conditions more thoroughly
- Test accessibility features (keyboard navigation, screen readers)
- Test with multiple PC1 IDs in parallel
- Test concurrent user submissions
- Add performance benchmarks (page load times, submit response times)
- Test mobile responsive behavior
- Test browser compatibility (Chrome, Firefox, Edge, Safari)

---

## Architecture Diagram

```
BaselineForm (Intake.aspx)
│
├── PC1 Tab (Primary Caregiver 1)
│   ├── Demographics (Gender, Relationship, Marital Status, Race, Ethnicity)
│   ├── Country of Birth (Conditional on Born in USA)
│   ├── Language (Conditional specify on "Other")
│   ├── Education (Grade, Enrollment, Hours, Program Type)
│   └── Employment (Status, Dates, Hours, Wages)
│
├── PC2 Tab (Primary Caregiver 2)
│   └── Same structure as PC1, different form token
│
├── Medical Provider/Benefits Tab
│   ├── OBP Involvement (Conditional specify on "Other")
│   ├── Medical Provider (CRUD via modal)
│   ├── Medical Facility (CRUD via modal)
│   ├── Medicaid (Conditional case number, disables insurance)
│   ├── Health Insurance (Checkboxes, conditional specify)
│   ├── Current Service Involvement (4 dropdowns)
│   └── Public Benefits (Conditional 5 required dropdowns)
│
├── Family/Other Children Tab
│   ├── Household Income (4 fields)
│   └── 6 Children (Each with: Name, DOB, Relationship, Living Arrangement)
│       ├── Age validation (under 21, not future)
│       ├── Name required validation
│       └── Conditional specify fields
│
├── PHQ-9 Tab
│   ├── Date Administered (Must be >= case start date)
│   ├── Participant (Conditional specify on "Other")
│   ├── Refused Checkbox (Disables Q36-Q44)
│   ├── Q36-Q44: 9 Depression Questions (0-3 points each)
│   ├── Q45: Difficulty Level (doesn't affect score)
│   ├── Score Calculation (Total 0-27, >9 = Positive)
│   └── Worker (Required if refused OR scores entered)
│
└── MIECHV Tab
    └── 6 Supplemental Questions (All optional, random selections)
```


