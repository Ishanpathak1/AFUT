# Case Home Tests Documentation
### Note- This test does not use the parameterised , it will be updated in the future
## Overview

The Case Home test suite contains automated end-to-end tests for the Case Home page functionality, including tab navigation, case notes CRUD operations, and case filters editing. These tests validate the core features available on the Case Home page after opening a case.

## Test Structure

### Test Files (4 Total)

1. **CaseHomeTabsTests.cs** - Tab navigation and content visibility
2. **CaseNotesTests.cs** - Case notes CRUD operations with validation (9 tests)
3. **CaseFilters/EditCaseFiltersTests.cs** - Case filters editing functionality (3 tests)
4. **CaseHomeTestHelper.cs** - Shared navigation helper

### Common Test Data

All tests use a **known test case** for consistency:

```csharp
PC1 ID: "AB12010361993"
PC1 First Name: "Anonymized"
PC1 Last Name: "Anonymized"
Target Child DOB: "060920" (June 9, 2020)
Worker: "3396, Worker"
Alternate ID: "Anonymized"
```

**Important**: This case must exist in the test environment for all tests to succeed.

---

## Test Files Overview

### 1. CaseHomeTabsTests.cs

Tests tab navigation and content display on Case Home page.

**Test Count**: 1

**Purpose**: Validates that clicking each tab displays its corresponding content.

---

### 2. CaseNotesTests.cs

Tests complete CRUD (Create, Read, Update, Delete) operations for case notes.

**Test Count**: 9

**Operations Tested**:
- Add new case notes (with validation)
- Edit existing case notes (with validation)
- Delete case notes (with confirmation)

---

### 3. EditCaseFiltersTests.cs

Tests case filters editing functionality.

**Test Count**: 3

**Operations Tested**:
- Edit dropdown and text filters
- Cancel without saving
- Invalid date validation

---

## Detailed Test Breakdown

---

## CaseHomeTabsTests.cs

### Test: `SelectingEachTabDisplaysCorrespondingContent`

**Purpose**: Validates that all default tabs are present and clickable, and that each displays its content when activated.

**What it tests**:
1. Navigates to Case Home
2. Retrieves all tabs
3. Verifies tab count matches expected default tabs
4. **For each tab**:
   - Verifies tab exists by display name
   - Verifies tabs appear in correct order
   - Activates the tab
   - Verifies tab becomes active
   - Verifies tab content is displayed

**Default Tabs** (from configuration):
The exact tabs depend on `CaseHomePage.DefaultTabDisplayNames`, typically including:
- Summary/Details
- Forms
- Case Notes
- Documents
- Reports
- etc.

**Key Assertions**:
- Tab count matches expected count
- Each tab can be found by name
- After activation, `tab.IsActive` returns `true`
- After activation, `tab.IsContentDisplayed` returns `true`

**Use Case**: Ensures tab navigation works correctly and all expected tabs are present.

---

## CaseNotesTests.cs

### Test 1: `ClickNewCaseNote_OpensForm`

**Purpose**: Validates that clicking "New Case Note" button opens the form.

**Test Flow**:
1. Navigate to Case Home
2. Get Case Notes tab
3. Click "Add Note" button
4. Verify click succeeds without errors

**Key Assertion**: Button click completes successfully (no exceptions thrown)

**Use Case**: Smoke test to ensure button is functional.

---

### Test 2: `AddNewCaseNote_WithDateAndText_SavesSuccessfully`

**Purpose**: Validates that a new case note with valid data saves successfully.

**Test Flow**:
1. Navigate to Case Home → Case Notes tab
2. Click "New Case Note"
3. Wait for form to appear
4. Enter date: "11/10/2025"
5. Enter note text: "Just Test"
6. Click Save
7. Wait for save to complete (2 seconds)
8. Verify success notification appears: "Case Note Added"
9. Verify note appears in grid with correct date and text

**Key Assertions**:
- Date field appears and is visible
- Note field appears and is visible
- Success toast contains "Case Note Added"
- Grid contains at least one row
- First row contains "Just Test" and "11/10/2025"

**Use Case**: Happy path test for adding a new case note.

---

### Test 3: `AddNewCaseNote_WithoutData_ShowsValidation`

**Purpose**: Validates that submitting an empty form shows both required field validations.

**Test Flow**:
1. Navigate to Case Home → Case Notes tab
2. Click "New Case Note"
3. **Do NOT enter any data**
4. Click Submit button
5. Wait for validation (1 second)
6. Verify validation summary appears
7. Verify contains "Case Note Date is required!"
8. Verify contains "Note is required!"
9. Verify individual date validator is also visible
10. Verify form fields remain visible (note not saved)

**Key Assertions**:
- Validation summary is displayed
- Both error messages appear in summary
- Date validator span is visible
- Form is still open (fields still displayed)

**Use Case**: Validates required field enforcement.

---

### Test 4: `AddNewCaseNote_WithDateButNoNote_ShowsValidation`

**Purpose**: Validates that providing date but not note text shows only note validation.

**Test Flow**:
1. Navigate to Case Home → Case Notes tab
2. Click "New Case Note"
3. Enter date: "11/10/2025"
4. **Leave note text empty**
5. Click Submit
6. Verify validation summary appears
7. Verify contains "Note is required!"
8. Verify **does NOT contain** "Case Note Date is required!"
9. Verify form remains visible

**Key Assertions**:
- Only note validation appears
- Date validation does not appear
- Form is still open

**Use Case**: Validates conditional validation (only empty fields trigger errors).

---

### Test 5: `AddNewCaseNote_WithNoteButNoDate_ShowsValidation`

**Purpose**: Validates that providing note text but not date shows only date validation.

**Test Flow**:
1. Navigate to Case Home → Case Notes tab
2. Click "New Case Note"
3. **Leave date empty**
4. Enter note text: "Just Test"
5. Click Submit
6. Verify validation summary appears
7. Verify contains "Case Note Date is required!"
8. Verify **does NOT contain** "Note is required!"
9. Verify date validator span is visible
10. Verify form remains visible

**Key Assertions**:
- Only date validation appears
- Note validation does not appear
- Form is still open

**Use Case**: Validates conditional validation (only empty fields trigger errors).

---

### Test 6: `EditCaseNote_WithUpdatedData_SavesSuccessfully`

**Purpose**: Validates that editing an existing case note saves changes.

**Test Flow**:
1. Navigate to Case Home → Case Notes tab
2. Activate tab and wait
3. Click Edit on **first existing case note**
4. Wait for form to appear
5. Capture original date and note text
6. Update date to "11/11/2025"
7. Update note to "Updated test note"
8. Click Submit
9. Wait for save (2 seconds)
10. Verify success notification appears
11. Verify grid shows updated values

**Key Assertions**:
- Original values are captured
- Updated values are applied
- Success toast appears
- Grid contains updated date "11/11/2025"
- Grid contains updated text "Updated test note"

**Use Case**: Happy path test for editing existing case notes.

---

### Test 7: `EditCaseNote_ClearAllFields_ShowsValidation`

**Purpose**: Validates that clearing all fields during edit shows both validations.

**Test Flow**:
1. Navigate to Case Home → Case Notes tab
2. Click Edit on first note
3. Wait for form
4. **Clear both date and note fields**
5. Click Submit
6. Verify validation summary appears
7. Verify contains "Case Note Date is required!"
8. Verify contains "Note is required!"

**Key Assertions**:
- Both validation messages appear
- Form validation prevents save

**Use Case**: Validates that existing notes cannot be updated to invalid state.

---

### Test 8: `EditCaseNote_ClearDateOnly_ShowsValidation`

**Purpose**: Validates that clearing only the date during edit shows only date validation.

**Test Flow**:
1. Navigate to Case Home → Case Notes tab
2. Click Edit on first note
3. **Clear only date field** (keep note text)
4. Click Submit
5. Verify validation summary appears
6. Verify contains "Case Note Date is required!"
7. Verify **does NOT contain** "Note is required!"

**Key Assertions**:
- Only date validation appears
- Note validation does not appear

**Use Case**: Validates conditional validation during edit.

---

### Test 9: `EditCaseNote_ClearNoteOnly_ShowsValidation`

**Purpose**: Validates that clearing only the note during edit shows only note validation.

**Test Flow**:
1. Navigate to Case Home → Case Notes tab
2. Click Edit on first note
3. Keep date field filled
4. **Clear only note field**
5. Click Submit
6. Verify validation summary appears
7. Verify contains "Note is required!"
8. Verify **does NOT contain** "Case Note Date is required!"

**Key Assertions**:
- Only note validation appears
- Date validation does not appear

**Use Case**: Validates conditional validation during edit.

---

### Test 10: `DeleteCaseNote_ConfirmYes_DeletesSuccessfully`

**Purpose**: Validates that confirming deletion removes the case note.

**Test Flow**:
1. Navigate to Case Home → Case Notes tab
2. Activate tab and wait
3. Get initial row count and first row text
4. Find and click **first Delete button**
5. Wait for confirmation modal (1.5 seconds)
6. Verify modal is displayed
7. Verify modal contains "Are you sure"
8. Click **Yes** to confirm deletion
9. Wait for deletion to complete (2 seconds)
10. Verify success toast contains "deleted"
11. Verify deleted row is no longer at top of grid

**Key Assertions**:
- Delete modal appears
- Modal contains confirmation message
- Yes button is clickable
- Success toast appears with "deleted" message
- First row text changes (deleted row is gone)

**Use Case**: Happy path for deleting case notes with confirmation.

**Note**: Test uses `Assert.NotEqual(firstRowText, currentFirstRowText)` to verify deletion, assuming grid re-sorts after delete.

---

## EditCaseFiltersTests.cs

### Test 1: `EditingDropdownAndTextboxFiltersPersistsToCaseHome`

**Purpose**: Validates that changes to case filters (dropdown and text) can be saved.

**Test Flow**:
1. Navigate to Case Home
2. Open Case Filters editor
3. Get all filters
4. Find **first enabled dropdown filter** with an alternative value
   - Alternative = a value different from currently selected
5. Change dropdown to alternative value
6. Find **first enabled text input filter**
7. Change text input to "11/06/25"
8. Click Submit

**Key Logic**:
```csharp
// Find dropdown with alternate value
var dropdownField = filters
    .Where(filter => filter.IsDropdown && filter.IsEnabled)
    .Select(filter => new {
        Filter = filter,
        Alternative = filter.GetDropdownOptions()
            .FirstOrDefault(option => 
                !string.IsNullOrWhiteSpace(option.Value) &&
                !string.Equals(option.Value, filter.GetSelectedValue()))
    })
    .FirstOrDefault(result => result.Alternative is not null);
```

**Key Assertions**: 
- At least one editable dropdown filter exists with alternate value
- At least one editable text input filter exists
- Submit completes without errors

**Use Case**: Validates editing and saving case filters.

---

### Test 2: `CancelButtonReturnsToCaseHomeWithoutSaving`

**Purpose**: Validates that clicking Cancel button returns to Case Home without saving changes.

**Test Flow**:
1. Navigate to Case Home
2. Open Case Filters editor
3. Click **Cancel** button

**Key Assertions**:
- Cancel button is functional
- Returns to Case Home (implied by test completing)

**Use Case**: Validates cancel flow works.

**Note**: This is a smoke test - doesn't verify that changes are actually discarded.

---

### Test 3: `SubmittingInvalidDateFormat_ShowsErrorAlert`

**Purpose**: Validates that entering an invalid date shows an error alert.

**Test Flow**:
1. Navigate to Case Home
2. Open Case Filters editor
3. Find first enabled text input filter (assumes it's a date field)
4. Enter **invalid date**: "43/43/42"
5. Click Submit
6. Wait for error alert (2 seconds)
7. Verify error alert appears
8. Verify contains "You have encountered an error in the Healthy Families application"

**Key Assertions**:
- Error alert with class `.alert.alert-info` appears
- Alert contains error message text

**Use Case**: Validates date format validation on case filters.

---

## Helper Methods

### CaseHomeTestHelper.NavigateToCaseHome()

**Purpose**: Shared navigation method used by all tests to reach Case Home.

**Flow**:
```
1. Create SearchCasesSearchRoutine
2. Set search criteria (known PC1 ID)
3. LoadApplication()
4. EnsureSignedIn()
5. EnsureRoleSelected()
6. NavigateToSearchCases()
7. PopulateSearchCriteria()
8. SubmitSearch()
9. Assert all steps succeeded
10. Get first result
11. OpenCaseHome()
12. Return CaseHomePage
```

**Parameters Used**:
```csharp
Pc1Id = "AB12010361993"
Pc1FirstName = "Anonymized"
Pc1LastName = "Anonymized"
TcDob = "060920"
WorkerDisplayText = "3396, Worker"
AlternateId = "Anonymized"
```

**Assertions**:
- User signed in
- Role selected
- Search page loaded
- Search completed
- At least one result returned
- Case Home loaded

---

### CaseNotesTests.ClickFirstEditLink()

**Purpose**: Finds and clicks the first visible Edit link in the Case Notes grid.

**Flow**:
1. Activate Case Notes tab
2. Wait 1 second
3. Find all Edit links: `a[id*='lbEditCaseNote']`
4. Get first displayed link
5. Scroll into view
6. Click link
7. Return clicked element

---

## Page Object Model

### CaseHomePage

**Key Properties**:
- `IsLoaded` - Page has loaded successfully
- `DefaultTabDisplayNames` - Static list of expected tab names

**Key Methods**:

#### GetTabs()
Returns array of all tab objects on the page.

#### GetCaseNotesTab()
Returns the Case Notes tab object.

#### OpenCaseFiltersEditor()
Clicks to open Case Filters editor, returns editor page object.

#### OpenBasicInformationEditor()
Clicks to open Basic Information editor (used in other tests).

---

### CaseHomePage.Tab

Represents a single tab on Case Home page.

**Properties**:
- `DisplayName` - Tab's display text
- `IsActive` - Tab is currently selected
- `IsContentDisplayed` - Tab's content panel is visible

**Methods**:
- `Activate()` - Clicks the tab to make it active

---

### CaseHomePage.CaseNotesTab

Represents the Case Notes tab functionality.

**Methods**:

#### ClickAddNote()
Clicks "New Case Note" button to open form.

#### EnterNoteDate(date)
Enters date into date field (format: "MM/dd/yyyy").

#### EnterNoteText(text)
Enters text into note textarea.

#### SaveNote()
Clicks Submit button to save the note.

#### IsNoteSaved()
Returns `true` if note was saved successfully.

#### Activate()
Makes the Case Notes tab active.

---

### CaseFiltersPage

Represents the Case Filters editor.

**Methods**:

#### GetFilters()
Returns array of all filter objects.

#### Submit()
Clicks Submit button to save changes.

#### Cancel()
Clicks Cancel button to discard changes.

---

### CaseFiltersPage.Filter

Represents a single filter field.

**Properties**:
- `IsDropdown` - Filter is a dropdown
- `IsTextInput` - Filter is a text input
- `IsEnabled` - Filter can be edited

**Methods**:

#### GetDropdownOptions()
Returns array of options (for dropdown filters).

#### GetSelectedValue()
Returns currently selected value (for dropdown filters).

#### SetDropdownValue(value)
Sets dropdown to specified value.

#### SetTextValue(text)
Sets text input to specified value.

---

## Important Concepts

### 1. Known Test Case Pattern

All tests use the same known case (`AB12010361993`) for consistency:

**Advantages**:
- Predictable baseline data
- No need to create test data
- Faster test execution

**Disadvantages**:
- Tests fail if case doesn't exist
- All tests share same case
- Potential for test interference

---

### 2. Case Notes CRUD Operations

Case notes follow standard CRUD pattern:

**Create**: Add new note with date and text
**Read**: View notes in grid
**Update**: Edit existing note date and/or text
**Delete**: Remove note with confirmation

**Business Rules**:
- Date is **required** (format: MM/dd/yyyy)
- Note text is **required**
- Both validations trigger if both are empty
- Delete requires confirmation via modal

---

### 3. Validation Message Pattern

Tests verify validation in two locations:

#### 1. Validation Summary
```csharp
var validationSummary = driver.FindElements(
    By.CssSelector(".validation-summary.alert.alert-danger"))
    .FirstOrDefault(vs => vs.Displayed);
```

Contains all validation messages combined.

#### 2. Individual Field Validators
```csharp
var dateValidator = driver.FindElement(
    By.CssSelector("span[id$='rfvCaseNoteDate']"));
```

Displayed next to specific field.

---

### 4. Delete Confirmation Flow

Delete operation uses modal confirmation:

```
1. Click Delete button
   ↓
2. Modal appears with "Are you sure" message
   ↓
3. User clicks Yes or No
   ↓
4. If Yes: Delete completes, toast appears
   If No: Modal closes, no deletion
```

**Modal ID**: `divDeleteCaseNoteModal`
**Yes Button**: `ctl00_ContentPlaceHolder1_ucCaseNotes_lbDeleteCaseNote`

---

### 5. Case Filters Architecture

Case filters use a flexible filter system:

**Filter Types**:
- **Dropdown**: Select from predefined options
- **Text Input**: Free-form text (often dates)

**Filter Properties**:
- `IsEnabled`: Can be edited by user
- `IsDropdown` / `IsTextInput`: Filter type

**Common Filters** (examples):
- Case Status (dropdown)
- Referral Date (text/date)
- Enrollment Date (text/date)
- Program Type (dropdown)

---

### 6. Toast Notifications

Success/error feedback uses toast notifications:

**Success Pattern**:
```csharp
var successNotification = driver.FindElements(
    By.CssSelector(".jq-toast-single.jq-has-icon.jq-icon-success"))
    .FirstOrDefault(n => n.Displayed);
```

**Common Messages**:
- "Case Note Added"
- "Case note was successfully deleted"
- "Form Saved"

**Timing**: Wait 2 seconds after action for toast to appear.

---

## What to Keep in Mind

### When Modifying Tests

1. **Known Case Dependency**: Tests require case `AB12010361993` to exist.

2. **Tab Order**: Tab tests verify tabs appear in specific order from configuration.

3. **Grid State**: Case notes tests assume at least one existing note for Edit/Delete tests.

4. **Wait Times**: Toast notifications appear after ~2 seconds, adjust if needed.

5. **Stale Elements**: After deletion, re-find grid elements to avoid stale references:
   ```csharp
   var updatedGrid = driver.FindElement(By.Id("tblCaseNotes"));
   var updatedRows = updatedGrid.FindElements(By.CssSelector("tbody tr"));
   ```

6. **Validation Timing**: Wait 1 second after submit for validation to appear.

---

### When Adding New Tests

1. **Use Shared Helper**: Call `CaseHomeTestHelper.NavigateToCaseHome()`
2. **Activate Tabs**: Always activate tab before interacting with its content
3. **Wait for Forms**: After clicking Add/Edit, wait for form to appear
4. **Scroll Into View**: Use JavaScript to scroll elements into view before clicking
5. **Log Important Steps**: Use `_output.WriteLine()` for debugging

---

### Common Pitfalls

1. **Forgetting to Activate Tab**: Case Notes tab must be active before finding Edit/Delete buttons
2. **Not Waiting for Modal**: After clicking Delete, must wait for modal to appear
3. **Not Re-Finding Elements**: After page updates (save/delete), elements become stale
4. **Assuming Grid State**: Edit/Delete tests fail if grid is empty
5. **Incorrect Wait Time**: Toast notifications may not appear if wait is too short
6. **Case Not Found**: All tests fail if known case doesn't exist

---

## Test Data Requirements

### Prerequisites
- **Case Must Exist**: PC1 ID `AB12010361993`
- **User Permissions**: DataEntry role with case access
- **At Least One Case Note**: Required for Edit and Delete tests

### Test Creates
- New case notes with date and text
- Modified case notes (during edit tests)

### Test Modifies
- Existing case note dates and text
- Case filter values

### Test Deletes
- One case note (during delete test)

**Note**: Tests create new data but don't necessarily clean up. Case notes accumulate over multiple test runs.

---

## Running the Tests

### Run All Case Home Tests
```bash
dotnet test --filter "FullyQualifiedName~CaseHome"
```

### Run Specific Test File
```bash
dotnet test --filter "FullyQualifiedName~CaseNotesTests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~CaseNotesTests.AddNewCaseNote_WithDateAndText_SavesSuccessfully"
```

### Run Only Case Filters Tests
```bash
dotnet test --filter "FullyQualifiedName~EditCaseFiltersTests"
```

---

## Troubleshooting

### Test Fails: "No search results were returned"
- **Issue**: Known case `AB12010361993` doesn't exist
- **Solution**: Create the case or update test constants

### Test Fails: Case Notes Edit/Delete
- **Issue**: No existing case notes in grid
- **Solution**: Run Add test first, or manually add a case note

### Test Fails: Modal doesn't appear
- **Issue**: Delete modal timing issue
- **Solution**: Increase wait time after clicking Delete button

### Test Fails: Validation doesn't appear
- **Issue**: Validation timing issue or validation not triggered
- **Solution**: Increase wait time, verify fields are actually empty

### Test Fails: Success toast not found
- **Issue**: Toast timing or incorrect save
- **Solution**: Increase wait time to 2-3 seconds, verify save actually completed

### Test Fails: Stale element reference
- **Issue**: Trying to access element after page update
- **Solution**: Re-find element after save/delete operations

### Test Fails: Tab not found
- **Issue**: Tab names don't match expected names from configuration
- **Solution**: Verify `CaseHomePage.DefaultTabDisplayNames` matches actual tab names

---

## Architecture Diagram

```
Test Method
    ↓
CaseHomeTestHelper.NavigateToCaseHome()
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
┌─────────────────────────────────────┐
│         CaseHomePage                │
├─────────────────────────────────────┤
│  Tabs:                              │
│    ├─ Summary                       │
│    ├─ Forms                         │
│    ├─ Case Notes ──► CaseNotesTab  │
│    ├─ Documents                     │
│    └─ Reports                       │
│                                     │
│  Methods:                           │
│    ├─ GetTabs()                     │
│    ├─ GetCaseNotesTab()             │
│    ├─ OpenCaseFiltersEditor()       │
│    └─ OpenBasicInformationEditor()  │
└─────────────────────────────────────┘
         ↓                  ↓
┌──────────────────┐  ┌──────────────────┐
│  CaseNotesTab    │  │ CaseFiltersPage  │
├──────────────────┤  ├──────────────────┤
│  ClickAddNote()  │  │  GetFilters()    │
│  EnterNoteDate() │  │  Submit()        │
│  EnterNoteText() │  │  Cancel()        │
│  SaveNote()      │  └──────────────────┘
│  IsNoteSaved()   │
│  Activate()      │
└──────────────────┘
```

---

## Future Enhancements

Potential areas for expansion:
- Test Cancel button in Case Notes (verify changes are discarded)
- Test Delete → No (cancel deletion)
- Test sorting case notes by date
- Test filtering case notes by date range
- Test pagination if many case notes exist
- Test editing case filters → Cancel (verify changes discarded)
- Test all case filter types (not just first dropdown/text)
- Test concurrent editing (two users editing same note)
- Add data cleanup (delete created notes after tests)
- Test with multiple test cases (not just one known case)

---

## Related Tests

These tests are related to:
- **BasicCaseInformationTests**: Tests for editing case information
- **SearchCasesTests**: Tests for finding and opening cases
- **Forms Tests**: Tests for various form tabs within Case Home

---

