# Basic Case Information Tests Documentation
### Note- This test does not use the parameterised , it will be updated in the future
## Overview

The `BasicCaseInformationTests.cs` file contains automated end-to-end tests for the Basic Case Information editor functionality. These tests validate the ability to edit, save, and cancel changes to core case fields displayed on the Case Home page.

## Test Structure

### Technology Stack
- **Framework**: xUnit
- **Test Type**: Fact-based tests (not parameterized)
- **Architecture**: Page Object Model with Routine pattern
- **Known Test Case**: Uses a specific known PC1 ID (`AB12010361993`) for consistency

### Test File Overview
- **File**: `BasicCaseInformationTests.cs`
- **Test Count**: 4 main tests
- **Test Type**: Edit/Save/Cancel flow validation
- **Dependencies**: Requires specific test case to exist in the system

## Known Test Data

The tests use a **known test case** with the following identifiers:

```csharp
PC1 ID: "AB12010361993"
PC1 First Name: "Anonymized"
PC1 Last Name: "Anonymized"
Target Child DOB: "060920" (June 9, 2020)
Worker: "3396, Worker"
Alternate ID: "Anonymized"
```

**Important**: These tests require this specific case to exist in the test environment. They do NOT create new cases but work with an existing one.

---

## Test Methods

### 1. EditInformationButton_EnablesAllEditableFields

**Purpose**: Validates that clicking "Edit Information" button enables all editable fields.

**Test Flow**:
1. Navigate to known case via search
2. Open Basic Case Information editor
3. Click "Edit Information" button (enter edit mode)
4. Verify **all editable fields** become enabled

**Editable Fields Tested**:
- Alternate ID
- Screen Date
- Target Child DOB
- Intake Date
- Parent Survey Date

**Key Assertions**:
- Basic Information page loads successfully
- After entering edit mode, `IsFieldEditable(field)` returns `true` for each field

**Use Case**: Ensures the edit mode correctly enables all fields that users should be able to modify.

---

### 2. EditInformation_AllEditableFieldsCanBeEditedAndReverted

**Purpose**: Validates that all editable fields can be changed and then reverted back to original values.

**Test Flow**:
1. Navigate to known case
2. Open Basic Information editor
3. Enter edit mode
4. **For each editable field**:
   - a. Capture original value
   - b. Generate replacement value
   - c. Set field to new value
   - d. Verify field displays new value
   - e. Revert field to original value
   - f. Verify field displays original value

**Value Generation Logic**:

#### For Alternate ID:
- If original is empty → "AutoAlternate-Test"
- Otherwise → `"{originalValue}-Test"`
- If replacement equals original → `"{originalValue}-Verify"`

#### For Date Fields (Screen Date, Target Child DOB, Intake Date, Parent Survey Date):
- Parses original date
- Adds 1 day for first replacement
- Adds 2 days for alternate replacement (if needed)
- Format: `"MM/dd/yy"` (e.g., "12/25/24")

**Key Assertions**:
- Each field accepts the new value
- Each field reverts to the original value
- Date values are compared by date only (time ignored)

**Special Handling**:
- **Date equivalency**: Compares dates by day, ignoring time component
- **Value collision detection**: If generated replacement equals original, uses alternate replacement

---

### 3. EditInformation_Submit_SavesChanges

**Purpose**: Validates that changes are persisted when clicking Submit button.

**Test Flow**:
1. Navigate to known case
2. Open Basic Information editor
3. Capture original Alternate ID
4. Enter edit mode
5. Change Alternate ID to "Anonymized1"
6. Click **Submit** button
7. Verify changes appear in Case Home summary
8. **Cleanup**: Revert Alternate ID back to original value

**Why Only Alternate ID**:
- Safest field to modify (won't affect case logic)
- String field (no date parsing complications)
- Easy to verify in summary

**Key Assertions**:
- After submit, Case Home page loads
- Summary displays updated Alternate ID
- Changes persist after page reload

**Cleanup Process**:
```csharp
try {
    // Test submission
} finally {
    // Always revert to original value
    // Even if test fails, cleanup runs
}
```

**Important**: Uses `finally` block to ensure cleanup runs even if test fails, preventing data pollution.

---

### 4. EditInformation_Cancel_RevertsValues

**Purpose**: Validates that clicking Cancel button discards all changes without saving.

**Test Flow**:
1. Navigate to known case
2. Capture Case Home summary values (before edit)
3. Open Basic Information editor
4. Capture original field values
5. Enter edit mode
6. **Change ALL editable fields** to new values
7. Verify fields display new values while in edit mode
8. Click **Cancel** button
9. Verify Case Home summary shows original values (unchanged)
10. Re-open Basic Information editor
11. Verify all fields still show original values (changes discarded)

**Key Assertions**:
- Changes are visible while in edit mode
- After cancel, Case Home displays original summary values
- After cancel, reopening editor shows original field values
- No changes were saved to database

**What Gets Verified in Summary**:
- Alternate ID
- Screen Date
- Target Child DOB
- Intake Date
- Parent Survey Date

**Use Case**: Ensures users can safely cancel edits without affecting the actual case data.

---

## Navigation Pattern

### NavigateToCaseHome() Helper Method

All tests use a common navigation routine that performs these steps:

```
1. Login
   ↓
2. Select Role (DataEntry)
   ↓
3. Navigate to Search Cases page
   ↓
4. Populate search criteria (PC1 ID, names, DOB, worker, alternate ID)
   ↓
5. Submit search
   ↓
6. Verify first search result exists
   ↓
7. Open Case Home for first result
   ↓
8. Return CaseHomePage object
```

**Routine Used**: `SearchCasesSearchRoutine` - reusable search flow

**Parameters Set**:
```csharp
Pc1Id = "AB12010361993"
Pc1FirstName = "Anonymized"
Pc1LastName = "Anonymized"
TcDob = "060920"
WorkerDisplayText = "3396, Worker"
AlternateId = "Anonymized"
```

**Assertions During Navigation**:
- User signed in successfully
- Role selected successfully
- Search Cases page loaded
- Search completed successfully
- At least one result returned
- Case Home page loaded

---

## Page Object Model

### BasicCaseInformationPage

The tests interact with the editor via the `BasicCaseInformationPage` page object.

**Key Properties**:
- `IsLoaded` - Indicates page has loaded
- `EditableFields` - Array of all editable field enums

**Key Methods**:

#### 1. EnterEditMode()
Clicks the "Edit Information" button to enable fields.

#### 2. IsFieldEditable(field)
Returns `true` if the specified field is enabled for editing.

#### 3. GetFieldValue(field)
Retrieves the current value of the specified field.

#### 4. SetFieldValue(field, value)
Sets the specified field to the given value.

#### 5. SubmitChanges()
Clicks Submit button and returns the Case Home page.

#### 6. CancelChanges()
Clicks Cancel button and returns the Case Home page.

### BasicCaseInformationField Enum

Represents the editable fields:

```csharp
enum BasicCaseInformationField
{
    AlternateId,
    ScreenDate,
    TargetChildDob,
    IntakeDate,
    ParentSurveyDate
}
```

### CaseHomePage

**Key Methods**:

#### OpenBasicInformationEditor()
Clicks the Basic Case Information section to open the editor, returns `BasicCaseInformationPage`.

#### GetBasicInformationSummary()
Returns a summary object containing current values displayed on Case Home:
```csharp
{
    AlternateId,
    ScreenDate,
    TargetChildDob,
    IntakeDate,
    ParentSurveyDate
}
```

---

## Date Handling Logic

### Date Field Challenges

Date fields require special handling because:
1. **Format variations**: Input may accept multiple formats
2. **Display vs Input**: Display format may differ from input format
3. **Time component**: Dates may have time portions that should be ignored in comparisons

### Date Comparison Strategy

#### AreDateValuesEquivalent()
```csharp
// Parse both dates
// Compare ONLY the date portion (ignore time)
// If parsing fails, fall back to string comparison
```

**Example**:
- `"12/25/24"` == `"12/25/2024"` → **True** (same date)
- `"12/25/24 10:30 AM"` == `"12/25/24 2:45 PM"` → **True** (same date, different time)
- `"12/25/24"` == `"12/26/24"` → **False** (different dates)

### Date Generation Strategy

#### GenerateDateReplacement()
```csharp
1. Parse original date
2. Add specified days (1 or 2)
3. If result equals original date, add one more day
4. Format as "MM/dd/yy"
5. If parsing fails, use today's date + days
```

**Purpose**: Ensures generated replacement date is always different from original.

---

## Helper Methods

### Value Generation

#### GenerateReplacementValue(field, originalValue)
- **Alternate ID**: Appends "-Test" suffix
- **Date Fields**: Adds 1 day to original date
- **Default**: Appends "-Test" suffix

#### GenerateAlternateReplacementValue(field, originalValue)
- **Alternate ID**: Appends "-Verify" suffix
- **Date Fields**: Adds 2 days to original date
- **Default**: Appends "-Verify" suffix

**Purpose**: Provides two distinct replacement values in case first replacement equals original.

### Value Comparison

#### ValuesAreEquivalent(field, expected, actual)
- **Date Fields**: Uses date-only comparison
- **Other Fields**: Case-insensitive string comparison

#### AssertValuesEqual(field, expected, actual, message)
- **Date Fields**: Uses date equivalency assertion
- **Other Fields**: Uses exact string equality assertion

### Date Utilities

#### TryParseDate(value, out date)
Attempts to parse a string into a `DateTime` using invariant culture.

#### IsDateField(field)
Returns `true` if field is one of: Screen Date, Target Child DOB, Intake Date, or Parent Survey Date.

---

## Important Concepts

### 1. Known Test Case Pattern

Unlike most other tests that use parameterized PC1 IDs from configuration, these tests use a **single known case**:

**Advantages**:
- Consistent baseline data
- Predictable field values
- Easier to maintain cleanup logic

**Disadvantages**:
- Tests fail if case doesn't exist
- All tests share same case (potential for interference)
- Less coverage across different case types

### 2. Non-Destructive Testing

Tests follow a strict **read-modify-revert** pattern:

```
1. Read original value
2. Modify to test value
3. Verify modification
4. Revert to original value
5. Verify reversion
```

**Purpose**: Leaves case in original state after testing, preventing data pollution.

### 3. Cleanup Guarantee

The Submit test uses `finally` block to ensure cleanup:

```csharp
try {
    // Modify and submit
    // Verify changes saved
} finally {
    // ALWAYS revert to original
    // Even if test fails
}
```

**Purpose**: Prevents test failures from leaving case in modified state.

### 4. Edit Mode Pattern

All edits follow this pattern:

```
1. Open Basic Information editor
2. EnterEditMode() - Click "Edit Information"
3. Modify fields
4. Submit OR Cancel
5. Verify result
```

**Key Point**: Fields are read-only until `EnterEditMode()` is called.

### 5. Summary vs Editor Values

Tests verify values in two contexts:

**Editor Context**: Field values while editing (via `GetFieldValue()`)
**Summary Context**: Displayed values on Case Home (via `GetBasicInformationSummary()`)

Both must reflect changes after Submit, but Summary stays unchanged after Cancel.

---

## Coding Standards Applied

### 1. Page Object Model 
Tests interact through page objects, never direct Selenium calls:

```csharp
// Good - Via page object
basicInfoPage.SetFieldValue(field, newValue);

// Bad - Direct Selenium
driver.FindElement(By.Id("txtAlternateId")).SendKeys(newValue);
```

### 2. Routine Pattern 
Navigation uses reusable `SearchCasesSearchRoutine`:

```csharp
var routine = new SearchCasesSearchRoutine(driver, _config);
routine.LoadApplication(parameters);
routine.EnsureSignedIn(parameters);
// ... etc
```

### 3. Clear Assertions 
All assertions include descriptive messages:

```csharp
Assert.True(basicInfoPage.IsLoaded, 
    "Basic Case Information page did not load successfully.");
```

### 4. Proper Cleanup
Uses `finally` blocks to guarantee cleanup:

```csharp
try {
    // Test code
} finally {
    // Cleanup code - always runs
}
```

### 5. Value Comparison Logic 
Handles date comparisons correctly (ignores time component).

---

## What to Keep in Mind

### When Modifying Tests

1. **Known Case Dependency**: Tests require the specific case `AB12010361993` to exist. If case is deleted or modified, tests will fail.

2. **Field Editability**: Only test fields that are actually editable. Non-editable fields will cause tests to fail.

3. **Date Format Consistency**: When modifying date generation, ensure format is `"MM/dd/yy"` to match system expectations.

4. **Cleanup is Critical**: Always revert changes in `finally` block. Test failures should not leave case in modified state.

5. **Summary Refresh**: After Submit, Case Home summary must refresh to show new values. Wait times may be needed.

6. **Field Value Trimming**: All value comparisons trim whitespace. Don't rely on leading/trailing spaces.

### When Adding New Tests

1. **Use Same Navigation**: Call `NavigateToCaseHome()` helper for consistency
2. **Follow Edit Pattern**: Always `EnterEditMode()` before modifying fields
3. **Test One Thing**: Each test should validate one specific behavior
4. **Revert Changes**: If modifying data, always revert in `finally` block
5. **Handle Date Fields**: Use date comparison helpers for date fields

### Common Pitfalls

1. **Forgetting Cleanup**: Not reverting test data leaves case in bad state for subsequent tests
2. **Time Component**: Comparing dates with time components fails (must compare date-only)
3. **Edit Mode**: Trying to modify fields without calling `EnterEditMode()` first
4. **Summary vs Editor**: Confusing editor values with summary values
5. **Case Not Found**: Tests fail if known case doesn't exist or search returns no results
6. **Stale Elements**: After Submit/Cancel, must re-navigate to get fresh page objects

---

## Test Data Requirements

### Prerequisites
- **Case Must Exist**: PC1 ID `AB12010361993` must exist in test environment
- **User Permissions**: Test user must have DataEntry role with access to the case
- **Worker Assignment**: Worker "3396, Worker" should be valid in system

### Test Does NOT Create
- No new cases created
- No new records created
- Works with existing case only

### Test Modifies
- Temporarily modifies Alternate ID in Submit test
- Reverts all changes after each test

### Test Deletes
- Nothing (read/modify/revert pattern only)

---

## Running the Tests

### Run All Basic Case Information Tests
```bash
dotnet test --filter "FullyQualifiedName~BasicCaseInformationTests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~BasicCaseInformationTests.EditInformationButton_EnablesAllEditableFields"
```

### Run in Debug Mode
```bash
dotnet test --filter "FullyQualifiedName~BasicCaseInformationTests" --logger "console;verbosity=detailed"
```

**Note**: All tests can run in parallel as they use read-modify-revert pattern, but Submit test should be run carefully as it temporarily modifies data.

---

## Troubleshooting

### Test Fails: "No search results were returned"
- **Issue**: Known case `AB12010361993` doesn't exist in test environment
- **Solution**: Create the case or update test constants to use a different existing case

### Test Fails: "Field was not editable after clicking Edit Information"
- **Issue**: Field is not actually editable in the UI or edit mode didn't activate
- **Solution**: Verify field is supposed to be editable, check page object implementation

### Test Fails: Date comparison mismatch
- **Issue**: Date formats don't match or time component is included
- **Solution**: Use `AreDateValuesEquivalent()` for date comparisons, not string equality

### Test Fails: "Failed to restore original values after submit test"
- **Issue**: Cleanup code failed to revert changes
- **Solution**: Case may be in modified state, manually revert Alternate ID to original value

### Test Fails: Summary doesn't reflect changes
- **Issue**: Page didn't refresh after submit or timing issue
- **Solution**: Add wait after submit, verify submit actually completed

### Test Fails: Cancel doesn't revert values
- **Issue**: Cancel button didn't work or page object implementation issue
- **Solution**: Verify Cancel button actually discards changes in manual testing

---

## Field Definitions

### Alternate ID
- **Purpose**: Secondary identifier for the case (optional)
- **Type**: Text field
- **Validation**: Accepts alphanumeric and special characters
- **Common Values**: Often empty or contains external system IDs

### Screen Date
- **Purpose**: Date when initial screening was completed
- **Type**: Date field
- **Format**: MM/dd/yy or MM/dd/yyyy
- **Validation**: Must be valid date

### Target Child DOB
- **Purpose**: Date of birth of the target child (primary child in case)
- **Type**: Date field
- **Format**: MM/dd/yy or MM/dd/yyyy
- **Validation**: Must be valid date, typically in the past

### Intake Date
- **Purpose**: Date when case was formally opened/enrolled
- **Type**: Date field
- **Format**: MM/dd/yy or MM/dd/yyyy
- **Validation**: Must be valid date

### Parent Survey Date
- **Purpose**: Date when parent survey/assessment was completed
- **Type**: Date field
- **Format**: MM/dd/yy or MM/dd/yyyy
- **Validation**: Must be valid date

---

## Architecture Diagram

```
Test Method
    ↓
NavigateToCaseHome()
    ↓
SearchCasesSearchRoutine
    ├── LoadApplication
    ├── EnsureSignedIn
    ├── EnsureRoleSelected
    ├── NavigateToSearchCases
    ├── PopulateSearchCriteria (Known PC1 ID)
    ├── SubmitSearch
    └── Return CaseHomePage
    ↓
CaseHomePage.OpenBasicInformationEditor()
    ↓
BasicCaseInformationPage
    ├── EnterEditMode()
    ├── GetFieldValue(field)
    ├── SetFieldValue(field, value)
    ├── IsFieldEditable(field)
    ├── SubmitChanges() → Returns CaseHomePage
    └── CancelChanges() → Returns CaseHomePage
    ↓
CaseHomePage.GetBasicInformationSummary()
    └── Returns summary object with current values
```



