# Referrals Tests Documentation

## Overview

The Referrals test suite contains comprehensive automated end-to-end tests for the Referrals module of the application. This module manages incoming case referrals, tracking from initial contact through assignment to workers. The test suite is split across 4 test files covering different aspects of referral management.

## What is the Referrals Module?

The **Referrals** module handles the lifecycle of case referrals in the home visiting program:

1. **Person Search**: Search for existing participants before creating new profiles
2. **Profile Creation**: Create new participant profiles with demographic information
3. **Referral Information**: Capture referral source, date, reason, and details
4. **Contact Attempts**: Track attempts to reach the referred person
5. **Worker Assignment**: Assign referrals to specific workers with dates
6. **Document Management**: Upload and manage referral-related documents
7. **Status Tracking**: Monitor referrals as Active or Completed
8. **Case Progression**: Move referrals from initial contact to active cases

## Test File Overview

| File | Test Count | Primary Focus |
|------|------------|---------------|
| **ReferralsTests.cs** | 11 | Core referral CRUD, person search, validation, contact attempts |
| **DeleteCaseCheckTests.cs** | 1 | Delete referral with confirmation |
| **UpdateWorkerAssignmentTests.cs** | 2 | Update worker assignments and date validation |
| **UploadDocumentsTests.cs** | 3 | Document upload and deletion |

**Total Tests**: 17

---

## Test File 1: ReferralsTests.cs

### Purpose
Core tests for referral creation, person search, validation, and contact attempt management.

### Test Categories

#### A. Navigation and UI Tests

##### 1. ReferralsPage_ClickFirstEdit_OpensEditPage
**Purpose**: Verify edit button opens referral edit page.

**Test Flow**:
1. Login and navigate to Referrals page
2. Find Active Referrals table
3. Click Edit button on first referral
4. Verify URL changed (navigated to edit page)

---

##### 2. ChangeCompletedReferralYear_UpdatesTableWithCorrectYearEntries
**Purpose**: Verify year dropdown filters Completed Referrals table.

**Test Flow**:
1. Navigate to Referrals page
2. Find Completed Referral Year dropdown
3. Get current selected year
4. Change to different year
5. Wait for table to update
6. Verify table shows referrals from selected year only

**Year Filter Logic**:
- Dropdown contains available years (e.g., 2022, 2023, 2024, 2025)
- Selecting a year filters Completed Referrals table
- Only referrals from selected year are shown

---

##### 3. ReferralsPage_DefaultYearAndEntriesDropdown_AreCorrect
**Purpose**: Verify default selections for year and entries per page dropdowns.

**Test Flow**:
1. Navigate to Referrals page
2. Verify Completed Referral Year dropdown defaults to **current year**
3. Verify "Show entries" dropdown defaults to **25** entries

**Expected Defaults**:
- Year: `DateTime.Now.Year` (current year)
- Entries: 25

---

#### B. Person Search Tests

##### 4. NewReferral_SearchForPersonTest_ShowsMatchesInGrid
**Purpose**: Test person search with test data that exists in database.

**Test Flow**:
1. Click New Referral button
2. Fill search form:
   - First Name: "Tasha"
   - Last Name: "Smith"
   - DOB: "01/04/1991"
   - Phone: "1111111110"
   - Emergency Phone: "1111111118"
3. Click Search button
4. Verify search results grid appears
5. Verify at least one result found
6. Verify grid contains expected data

**Use Case**: Search finds existing person - user can select them instead of creating duplicate.

---

##### 5. NewReferral_SearchWithTestData_ShowsNoRecordFoundMessage
**Purpose**: Test person search with data that doesn't exist.

**Test Flow**:
1. Click New Referral button
2. Fill search form:
   - First Name: "John"
   - Last Name: "Doe"
   - DOB: "01/01/1990"
   - Phone: "1234567890"
   - Emergency Phone: "0987654321"
3. Click Search button
4. Verify "No records found" message appears in grid

**Use Case**: No match found - user can proceed to create new person profile.

---

#### C. Person Profile Creation Tests

##### 6. NewReferral_SearchNoRecordsFound_CreateNewPersonProfileWithRaceAndGender
**Purpose**: Create new person profile with race and gender selections.

**Test Flow**:

**Part 1: Search (No Results)**
1. Click New Referral
2. Search for non-existent person
3. Verify "No records found"

**Part 2: Create New Person Profile**
4. Click "Create New Person Profile" button
5. Fill PC1 information:
   - First Name: "Melissa"
   - Last Name: "Robinson"
   - DOB: "01/01/1990"
   - SSN: "111112222"
   - Phone: "1112223333"
   - Emergency Phone: "2223334444"
6. Select **Race** (multi-select Chosen dropdown):
   - "American Indian"
7. Select **Gender**: "Female"
8. Scroll down
9. Click "Create Person Profile and Continue" button
10. Wait for page to load
11. Verify referral form appears (person created)

**Important**: Uses **Chosen.js** multi-select for Race dropdown.

---

##### 7. NewReferral_SearchWithYear2016_CreatePersonProfileAndCheckValidation
**Purpose**: Create person profile and test various field validations.

**Test Flow**:

**Part 1: Search and Create Profile**
1. Search for person (no results)
2. Click "Create New Person Profile"
3. Fill minimal person information
4. Click "Create Person Profile and Continue"

**Part 2: Test Referral Date Validations**
5. Enter referral date: **"01/01/2000"** (very old date)
6. Click Submit
7. Verify validation:
   - "Referral Date cannot be before 2016"

8. Change date to: **"12/10/2050"** (future date)
9. Click Submit
10. Verify validation:
    - "Referral Date cannot be in the future"

11. Change date to: **"08/09/2024"** (valid date)
12. Verify validation clears

**Part 3: Test Other Validations**
13. Fill other required fields progressively
14. Verify validation errors clear as fields filled

**Referral Date Rules**:
- Must be >= 2016
- Must be <= today
- Cannot be future date

---

#### D. Referral Validation Tests

##### 8. NewReferral_SelectExistingPerson_SubmitWithoutRequiredFields_ShowsValidationErrors
**Purpose**: Comprehensive validation test for all required fields.

**Test Flow**:

**Part 1: Select Existing Person**
1. Click New Referral
2. Search for existing person ("Tasha Smith")
3. Click "Select" on first search result
4. Wait for referral form to load with person info

**Part 2: Submit Empty Form**
5. Scroll to bottom
6. Click Submit button
7. Capture all validation errors

**Part 3: Verify Required Field Validations**
Expected errors (may vary):
- Referral Date is required
- Referral Source is required
- Referral Reason is required
- Contact Type is required
- Contact Date is required
- Other field-specific validations

**Purpose**: Ensures all required fields are validated server-side.

---

##### 9. NewReferral_ProgressivelyFillRequiredFields_ValidatesCorrectly
**Purpose**: Test that validation errors clear as fields are filled.

**Test Flow**:

1. Select existing person from search
2. Submit empty form → Capture initial validation errors
3. Fill Referral Date → Submit → Verify date error cleared
4. Fill Referral Source → Submit → Verify source error cleared
5. Fill Referral Reason → Submit → Verify reason error cleared
6. Continue filling fields one by one
7. Verify validation errors progressively decrease
8. Fill all required fields
9. Submit successfully
10. Verify success toast or redirect

**Purpose**: Ensures validation is dynamic and helps users complete the form step by step.

---

#### E. Contact Attempt Tests

##### 10. ReferralsPage_NewContactAttempt_ValidationRequired
**Purpose**: Test contact attempt form validation.

**Test Flow**:

1. Navigate to Referrals page
2. Click Edit on first active referral
3. Scroll to Contact Attempts section
4. Click "New Contact Attempt" button
5. Submit form without filling any fields
6. Verify validation errors:
   - Contact Date is required
   - Contact Type is required
   - Outcome is required (if applicable)
   - Other required fields

**Contact Attempt Fields**:
- **Contact Date**: Date of attempt
- **Contact Type**: Phone, In-Person, Email, etc.
- **Outcome**: Successful, No Answer, Left Message, etc.
- **Notes**: Optional details

---

##### 11. ReferralsPage_AddContactAttempt_FillsFormAndSubmits
**Purpose**: Successfully create a contact attempt.

**Test Flow**:

1. Navigate to Referrals edit page
2. Find Contact Attempts table
3. Get initial row count
4. Click "New Contact Attempt" button
5. Fill form:
   - Contact Date: Today's date
   - Contact Type: "Phone" (or first available option)
   - Outcome: "Successful" (or first available option)
   - Notes: "Test contact attempt"
6. Click Submit
7. Wait for form to close
8. Verify Contact Attempts table row count increased by 1
9. Verify new contact attempt appears in table with correct data

**Success Indicator**: Row count increases, new attempt visible in table.

---

##### 12. ReferralsPage_DeleteContactAttempt_CancelsAndConfirmsDelete
**Purpose**: Test delete contact attempt with cancel and confirm.

**Test Flow**:

**Part 1: Cancel Delete**
1. Navigate to Referrals edit page
2. Find Contact Attempts table
3. Get initial row count
4. Click Delete button on first contact attempt
5. Verify delete confirmation modal appears
6. Click "No" / "Cancel"
7. Verify modal closes
8. Verify row count unchanged (not deleted)

**Part 2: Confirm Delete**
9. Click Delete button again
10. Verify confirmation modal appears
11. Click "Yes" / "Confirm"
12. Wait for deletion to complete
13. Verify row count decreased by 1
14. Verify success toast (if applicable)

**Purpose**: Ensures accidental deletes are prevented by confirmation.

---

## Test File 2: DeleteCaseCheckTests.cs

### Purpose
Test deletion of entire referrals with confirmation modal.

### Test

#### ReferralsPage_DeleteReferral_CancelAndConfirmDelete

**Purpose**: Test referral deletion with cancel and confirm flows.

**Test Flow**:

**Part 1: Setup**
1. Login and navigate to Referrals page
2. Find Active Referrals table
3. Get initial row count
4. Verify at least one referral exists

**Part 2: Cancel Delete**
5. Click Delete button on first referral
6. Wait for confirmation modal
7. Verify modal appears
8. Verify modal title: "Delete Confirmation"
9. Click "No, return" button
10. Verify modal closes
11. Verify row count unchanged (referral NOT deleted)

**Part 3: Confirm Delete**
12. Click Delete button again on same referral
13. Wait for confirmation modal
14. Click "Yes, delete" button
15. Wait for deletion to complete (2 seconds)
16. Verify modal closes

**Part 4: Verify Deletion**
17. If initial count was 1:
    - Verify "No data available in table" message appears
18. If initial count > 1:
    - Verify row count decreased by 1

**Modal Elements**:
- **Title**: "Delete Confirmation"
- **No Button**: `button.btn.btn-default[data-dismiss='modal']` - text contains "No"
- **Yes Button**: `a.btn.btn-primary` with id containing "lbConfirmDelete"

**Delete Button Selector**: `a.btn.btn-danger` with id containing "btnDeleteReferral" or text "Delete"

---

## Test File 3: UpdateWorkerAssignmentTests.cs

### Purpose
Test worker assignment updates and date validation for referrals.

### Tests

#### 1. ReferralsPage_UpdateWorkerAssignment_EditsWorkerAndDate

**Purpose**: Successfully update worker assignment with new worker and date.

**Test Flow**:

**Part 1: Navigate to Worker Assignment Form**
1. Login and navigate to Referrals page
2. Click Edit on first active referral
3. Scroll to bottom of page
4. Click "Edit Worker Assignment" button (link with id containing `lbEditWorkerAssignment`)
5. Wait for form to load

**Part 2: Change Date (Triggers Postback!)**
6. Find date field: `input[id*='txtWorkerAssignmentDate']`
7. Get current date value
8. Calculate new date: current + 1 day (or use tomorrow)
9. Clear date field
10. Enter new date (format: "MM/dd/yyyy")
11. Press Tab key to trigger onchange event
12. **CRITICAL**: Wait for postback to complete (3 seconds)
    - Date change triggers `__doPostBack` in ASP.NET
    - Page refreshes/updates
    - Worker dropdown gets re-rendered

**Part 3: Change Worker (After Postback)**
13. **Re-find worker dropdown** after postback (old reference is stale)
14. Find dropdown: `select[id*='ddlWorkerAssignmentWorker']`
15. Get currently selected worker
16. Select different worker from list:
    - Preferred workers: "2431, Worker", "3477, Worker", "79, Worker", "Test, Derek"
    - Or use JavaScript to select first non-default option
17. Wait 500ms

**Part 4: Submit**
18. Find submit button: `a.btn.btn-primary[id*='lbSubmitWorkerAssignment']`
19. Scroll to button
20. Click submit
21. Wait for submission (2 seconds)

**Part 5: Verify Success**
22. Find toast message
23. Assert toast contains:
    - "Worker Assignment Edited" OR
    - "Worker assignment successfully edited"

**Critical Concept: Postback Handling**
- Date field has `onchange` event that triggers ASP.NET postback
- Postback refreshes parts of the page (especially worker dropdown)
- Must re-find elements after postback to avoid stale element exceptions
- Wait 3 seconds after Tab key to allow postback to complete

**Toast Selectors** (tried in order):
```css
.toast
.toast-message
[class*='toast']
[id*='toast']
.alert-success
.alert-danger
[class*='alert']
[role='alert']
[class*='Toastify']
```

---

#### 2. ReferralsPage_UpdateWorkerAssignment_InvalidDate_ShowsValidationError

**Purpose**: Test date validation - assignment date must be on or after referral date.

**Test Flow**:

**Part 1: Setup**
1-5. Same as previous test (navigate to worker assignment form)

**Part 2: Set Invalid Date**
6. Set date to **"11/07/2025"** (known to be before referral date for test data)
7. Trigger postback (Tab key)
8. Wait for postback (3 seconds)

**Part 3: Change Worker**
9. Re-find worker dropdown
10. Select different worker

**Part 4: Submit Invalid Form**
11. Click Submit button
12. Wait for response

**Part 5: Verify Validation Error**
13. Find toast message
14. Assert toast contains: "Validation Failed"
15. Find validation error message on page
16. Assert error contains: **"Contact Date cannot be before the Referral Date"**

**Validation Selectors** (tried in order):
```css
.validation-summary-errors
[class*='validation-summary']
.field-validation-error
.text-danger
[style*='color:Red']
```

**Validation Rule**: Worker Assignment Date >= Referral Date

**Why This Matters**: You can't assign a worker before the referral was even received.

---

## Test File 4: UploadDocumentsTests.cs

### Purpose
Test document upload and deletion functionality for referrals.

### Test File Constant
```csharp
private const string TestFilePath = @"C:\Users\IP282924\Desktop\Repo\Pookie.Tests\TestFiles\TestHFNY.pdf";
```

### Tests

#### 1. UploadDocument_WithPDFFile_HandlesValidationAndUploads

**Purpose**: Successfully upload a PDF document to a referral.

**Test Flow**:

**Part 1: Navigate to Upload Form**
1. Login and navigate to Referrals page
2. Click Edit on first active referral
3. Click "Upload New Document" button (link with id containing `lbNewUploadedFile`)
4. Wait for upload form to appear

**Part 2: Upload File**
5. Get test file path: `TestHFNY.pdf`
6. Verify file exists (falls back to alternative paths if needed)
7. Find file input: `input[type='file'][id*='fuUploadedFile']`
8. Send keys (file path) to file input
9. Wait 1 second

**Part 3: Submit Upload**
10. Find upload submit button:
    - Primary: `a[id*='lbSubmitUploadedFile']`
    - Has upload icon: `.glyphicon-upload`
    - Text contains "Upload"
11. Click submit button
12. Wait for upload to complete (3 seconds)

**Part 4: Verify Success Toast**
13. Search for toast message (wait up to 10 seconds)
14. Toast may use jQuery toast plugin:
    - `.jq-toast-single`
    - `.jq-icon-success`
    - May have heading (`.jq-toast-heading`)
15. Assert toast contains:
    - "Document Uploaded" OR
    - "Successfully uploaded the document" OR
    - "Successfully uploaded"

**Success Toast Selectors** (tried in order):
```css
.jq-toast-single
[class*='jq-toast']
.jq-icon-success
.toast
.toast-message
[class*='toast']
.alert-success
.alert
[class*='success']
[role='alert']
[class*='notification']
[id*='toast']
[class*='Toastify']
```

**File Upload Input Selectors**:
```css
input[type='file'][id*='fuUploadedFile']
input[type='file'][name*='fuUploadedFile']
input[type='file'][id*='UploadedFile']
input[type='file']
```

---

#### 2. UploadDocument_OpensUploadDialog

**Purpose**: Smoke test to verify upload dialog opens.

**Test Flow**:
1. Navigate to Referrals page
2. Click Edit on first referral
3. Click "Upload New Document" button
4. Verify file input appears: `input[type='file']`
5. Assert file input is visible

**Purpose**: Quick test to ensure upload functionality is accessible.

---

#### 3. UploadDocument_DeleteDocument_CancelsAndConfirmsDelete

**Purpose**: Test document deletion with cancel and confirm.

**Test Flow**:

**Part 1: Setup (Upload if Needed)**
1. Navigate to Referrals edit page
2. Find Uploaded Documents table
3. Get initial document row count
4. If count == 0:
   - Upload test document first
   - Refresh table
   - Get new count

**Part 2: Cancel Delete**
5. Assert at least one document exists
6. Get first document row and its text
7. Find delete button in row:
   - `button.btn-danger.delete-gridview`
   - `button[class*='delete-gridview']`
   - Has trash icon: `.glyphicon-trash`
   - Text contains "Delete"
8. Click delete button
9. Wait for confirmation modal (1 second)
10. Find modal: `#divDeleteUploadedFileModal` or `.modal`
11. Find "No" button in modal
12. Click "No" button
13. Wait for modal to close
14. Verify row count unchanged (document NOT deleted)

**Part 3: Confirm Delete**
15. Click delete button again on same document
16. Wait for confirmation modal
17. Find "Yes" button in modal
18. Click "Yes" button
19. Wait for deletion (2 seconds)
20. Refresh table
21. Verify row count decreased by 1
22. If original row count > 1:
    - Verify specific document no longer exists
23. If original row count == 1:
    - Verify table is now empty

**Uploaded Documents Table Selectors**:
```css
table[id*='UploadedFile']
table[id*='Document']
table[class*='table']
table
```

**Table Identification**: Table text contains "pdf", "View", or "Delete"

**Document Row Filtering**: Row must:
- Be displayed
- NOT contain "No data available"
- Contain "pdf" in text

**Delete Button Selectors**:
```css
button.btn-danger.delete-gridview
button[class*='delete-gridview']
button.btn-danger
button[data-target*='DeleteUploadedFileModal']
button[class*='btn-danger']
```

**Modal Buttons**:
- **No/Cancel**: `button` or `a.btn` with text "No", "Cancel"
- **Yes/Confirm**: `button` or `a.btn` with text "Yes", "OK", "Confirm"

**Fallback**: If modal not found, tries browser alert (`driver.SwitchTo().Alert()`)

---

## Important Concepts

### 1. Active vs Completed Referrals

**Active Referrals**:
- Referrals that are currently being processed
- Worker has not yet made successful contact
- Visible in Active Referrals table
- Can be edited, deleted, and have contact attempts added

**Completed Referrals**:
- Referrals that have been closed/finalized
- Moved to Completed Referrals table
- Filtered by year using dropdown
- Typically read-only or limited edit capability

---

### 2. Person Search Workflow

**Purpose**: Prevent duplicate person profiles in the system.

**Flow**:
```
Start New Referral
    ↓
Search for Person (Name, DOB, Phone)
    ↓
  Match Found?
    ├─ Yes → Select Person → Create Referral
    └─ No → Create New Person Profile → Create Referral
```

**Search Fields**:
- PC1 First Name
- PC1 Last Name
- DOB (Date of Birth)
- Phone
- Emergency Phone

**Search Results**:
- Grid displays matching persons
- "Select" button to choose existing person
- "No records found" if no matches
- "Create New Person Profile" button to create new

---

### 3. Person Profile Creation

**Required Fields**:
- First Name
- Last Name
- DOB
- Gender
- Race (multi-select)
- Phone numbers

**Race Selection**: Uses **Chosen.js** multi-select dropdown
- Can select multiple races
- American Indian, Asian, Black/African American, White, etc.
- Native Hawaiian/Pacific Islander, Other

**Gender Options**: Male, Female, Other

**SSN**: Optional but recommended

**Flow After Creation**:
1. Person profile saved to database
2. Automatically proceeds to referral form
3. Person info pre-filled in referral

---

### 4. Referral Date Validation

**Business Rules**:
```
Referral Date >= 2016
Referral Date <= Today
Referral Date <= Worker Assignment Date
```

**Why 2016?**: Program/system started in 2016, no referrals before that year are valid.

**Why Not Future?**: Can't refer someone in the future.

**Worker Assignment Rule**: Can't assign worker before referral was received.

**Validation Messages**:
- "Referral Date cannot be before 2016"
- "Referral Date cannot be in the future"
- "Contact Date cannot be before the Referral Date"

---

### 5. Required Referral Fields

**Typical Required Fields**:
- Referral Date
- Referral Source (where referral came from)
- Referral Reason (why they were referred)
- Contact Type (phone, in-person, etc.)
- Contact Date (first contact attempt)
- Worker Assignment (who is handling the referral)

**Optional Fields**:
- Notes
- Additional contact information
- Special circumstances

---

### 6. Contact Attempts Tracking

**Purpose**: Document all attempts to reach the referred person.

**Why Track This?**:
- Compliance requirement (must document outreach efforts)
- Helps determine if referral should be closed (multiple failed attempts)
- Shows timeline of engagement

**Contact Attempt Fields**:
- **Date**: When attempt was made
- **Type**: Phone, In-Person, Email, Text, Letter
- **Outcome**: Successful, No Answer, Left Message, Wrong Number, etc.
- **Notes**: Details about the attempt

**Typical Outcomes**:
- **Successful**: Reached person, discussed services
- **No Answer**: Phone rang, no one answered
- **Left Message**: Left voicemail or message with someone
- **Wrong Number**: Phone disconnected or wrong person
- **Refused Services**: Person declined services

---

### 7. Worker Assignment

**Purpose**: Assign referrals to specific workers for follow-up.

**Assignment Fields**:
- **Worker**: Dropdown of available workers
- **Date**: Date of assignment (must be >= referral date)

**Assignment Date Postback**:
- Changing date triggers ASP.NET `__doPostBack`
- Page refreshes/updates
- Worker dropdown may be re-rendered with filtered workers
- **CRITICAL**: Must re-find worker dropdown after date change

**Why Postback?**: Date determines which workers were available/employed at that time.

---

### 8. Document Management

**Supported Operations**:
- Upload new documents (PDF, images, etc.)
- View uploaded documents
- Delete documents with confirmation

**Upload Workflow**:
```
Edit Referral
    ↓
Click "Upload New Document"
    ↓
Select file from computer
    ↓
Click Upload button
    ↓
Success toast appears
    ↓
Document appears in table
```

**Delete Workflow**:
```
Edit Referral
    ↓
Find document in table
    ↓
Click Delete button
    ↓
Confirmation modal appears
    ├─ Click No → Modal closes, document remains
    └─ Click Yes → Document deleted, row removed
```

**Common Document Types**:
- Medical records
- Birth certificates
- Consent forms
- Assessment forms
- Referral letters

---

### 9. Chosen.js Multi-Select Dropdowns

**What is Chosen.js?**
- jQuery plugin that enhances `<select>` elements
- Provides searchable, multi-select dropdowns
- Used for Race, Ethnicity, and other multi-value fields

**Interaction Pattern**:
```
1. Find Chosen container: .chosen-container[id$='suffix_chosen']
2. Scroll into view
3. Click trigger: .chosen-choices
4. Wait for dropdown to open
5. Find options: .chosen-drop .chosen-results li.active-result
6. Click desired option(s)
7. Dropdown stays open for multi-select
8. Click outside or press Enter to close
```

**Test Helper**:
```csharp
SelectChosenOption(driver, "ddlRace", "American Indian");
SelectChosenOptionViaScript(driver, "ddlRace", "American Indian", "01");
```

**Why Special Handling?**
- Standard Selenium `SelectElement` doesn't work
- Chosen hides original `<select>` and creates custom DOM
- Requires JavaScript interaction or clicking visible elements

---

### 10. ASP.NET Postback Handling

**What is a Postback?**
- ASP.NET Web Forms mechanism for server-side updates
- Triggered by certain field changes (dropdowns, date fields)
- Page submits to server, server processes, page updates
- JavaScript function: `__doPostBack(eventTarget, eventArgument)`

**Common Postback Triggers**:
- Date fields with validation
- Dropdowns that affect other field options
- Checkboxes that show/hide sections

**Test Implications**:
1. Must wait for postback to complete before continuing
2. Element references become stale after postback
3. Must re-find elements after postback

**Postback Wait Strategy**:
```csharp
// Trigger postback
dateField.SendKeys(Keys.Tab);

// Wait for postback
driver.WaitForReady(30);
Thread.Sleep(3000); // Extra time for postback

// Re-find elements
workerDropdown = driver.FindElements(By.CssSelector("select[id*='ddlWorker']"))
    .FirstOrDefault(d => d.Displayed);
```

---

## Helper Methods Summary

### Navigation Helpers

#### LoginAndNavigateToReferrals()
Logs in, selects DataEntry role, navigates to Referrals page.

**Common Pattern**: All tests start with this method.

---

### Search Helpers

#### FillPersonSearchForm()
Fills person search form with first name, last name, DOB, phone, emergency phone.

---

#### ClickSearchButton()
Finds and clicks the Search button on person search form.

---

### Referral Form Helpers

#### FillReferralTextField()
Fills a text input field by suffix.

**Pattern**: `driver.FindTextInputBySuffix(suffix)`

---

#### SelectReferralDropdownByText()
Selects dropdown option by visible text.

---

#### SelectReferralDropdownByFirstNonEmpty()
Selects first non-empty option from dropdown.

---

### Chosen.js Helpers

#### SelectChosenOption()
Interacts with Chosen.js dropdown to select an option.

**Interaction**: Clicks trigger, finds option by text, clicks option.

---

#### SelectChosenOptionViaScript()
Uses JavaScript to select Chosen option directly.

**Faster**: Bypasses UI interaction, sets value directly.

---

### Validation Helpers

#### SubmitReferralFormAndGetErrors()
Submits form and returns set of validation error messages.

**Returns**: `HashSet<string>` of error messages

---

### Table Helpers

#### FindActiveReferralsTable()
Finds the Active Referrals table on the main Referrals page.

---

#### FindCompletedReferralsTable()
Finds the Completed Referrals table.

---

#### FindReferralSearchResultsGrid()
Finds the person search results grid.

---

#### FindContactAttemptsTable()
Finds the Contact Attempts table on referral edit page.

---

#### GetContactAttemptDataRows()
Returns list of data rows from Contact Attempts table (excludes "No data available" row).

---

### Button Helpers

#### FindNewReferralButton()
Finds "New Referral" button: `.btn.btn-default.pull-right` with text "New Referral"

---

#### FindReferralEditButton()
Finds Edit button in referral table row:
- Uses icon: `.glyphicon-pencil`
- Finds parent button with `.btn.btn-default`
- Falls back to text "Edit"

---

### Contact Attempt Helpers

#### FindContactAttemptForm()
Finds the Add/Edit Contact Attempt form panel.

---

### Document Upload Helpers

#### FindFileUploadInputField()
Finds file upload input: `input[type='file'][id*='fuUploadedFile']`

---

#### FindUploadSubmitButton()
Finds upload submit button with upload icon or text "Upload".

---

#### FindUploadedDocumentsTable()
Finds the table showing uploaded documents.

---

#### GetDocumentRows()
Returns list of document rows (excludes empty table message).

---

#### FindDeleteDocumentButton()
Finds delete button in document row (trash icon or "Delete" text).

---

### Modal Helpers

#### HandleDeleteConfirmationModal()
Handles delete confirmation modal, clicks Yes or No based on parameter.

**Parameters**: `confirm` (bool) - true for Yes, false for No

---

### Toast/Validation Helpers

#### FindToastMessage()
Searches for toast notification using multiple selectors, returns message text.

---

#### VerifySuccessToast()
Waits up to 10 seconds for success toast, returns found status and text.

---

#### GetValidationErrorMessages()
Returns list of all visible validation error messages on page.

---

#### VerifyValidationError()
Asserts that a specific validation error message is present.

---

## What to Keep in Mind

### When Modifying Tests

1. **Postback Awareness**: After changing certain fields (dates, certain dropdowns), wait for postback and re-find elements.

2. **Chosen.js Handling**: Race and similar multi-select fields require special interaction - can't use standard `SelectElement`.

3. **Year Filter**: Completed Referrals are filtered by year - ensure test data exists for the year you're testing.

4. **Person Search Data**: Tests use hardcoded test data ("Tasha Smith", "John Doe") - these must exist (or not exist) in the test database.

5. **Test File Path**: Document upload tests require `TestHFNY.pdf` at:
   ```
   C:\Users\IP282924\Desktop\Repo\Pookie.Tests\TestFiles\TestHFNY.pdf
   ```
   Update path if file location changes.

6. **Active Referrals Dependency**: Many tests assume at least one active referral exists. Create test data if needed.

7. **Validation Messages**: Validation error text may change - update assertions if wording changes.

8. **Toast Notifications**: Multiple toast implementations possible (jQuery toast, Toastify, Bootstrap alerts) - tests check all.

---

### When Adding New Tests

1. **Start with LoginAndNavigateToReferrals()**: All tests should begin with navigation.

2. **Use Existing Helpers**: Check for existing helper methods before writing new code.

3. **Log Every Step**: Use `_output.WriteLine()` extensively for debugging.

4. **Handle Postbacks**: If interacting with date or dependent fields, expect postbacks.

5. **Multiple Selectors**: Always provide fallback selectors for robustness.

6. **Wait Generously**: Use `Thread.Sleep()` liberally - tests are not time-critical.

7. **Clean Up**: If creating test data (referrals, contact attempts, documents), consider cleanup in teardown.

---

### Common Pitfalls

1. **Stale Element After Postback**: Always re-find elements after date changes or other postback triggers.

2. **Chosen Dropdown Not Opening**: Ensure you scroll element into view and wait before clicking.

3. **Toast Not Found**: Toast may appear and disappear quickly - capture immediately after action.

4. **Validation Errors Not Visible**: Scroll to top of page or to validation summary location.

5. **Table Empty**: Test may fail if no referrals/documents exist - create test data first.

6. **Wrong Year Selected**: Completed Referrals tests may fail if year dropdown not set correctly.

7. **File Not Found**: Upload tests fail if `TestHFNY.pdf` missing - check file path.

8. **Modal Not Found**: Delete confirmation may use browser alert instead of modal - handle both.

---

## Test Data Requirements

### Prerequisites
- Test user with DataEntry role
- At least one active referral (for edit, delete, contact attempt tests)
- Test file: `C:\Users\IP282924\Desktop\Repo\Pookie.Tests\TestFiles\TestHFNY.pdf`

### Recommended Test Data

**Existing Person (for search)**:
- First Name: "Tasha"
- Last Name: "Smith"
- DOB: "01/04/1991"
- Phone: "1111111110"
- Emergency Phone: "1111111118"

**Non-Existent Person (for "no results" test)**:
- First Name: "John"
- Last Name: "Doe"
- DOB: "01/01/1990"
- Phone: "1234567890"
- Emergency Phone: "0987654321"

### Test Creates
- New person profiles (if running creation tests)
- New referrals
- Contact attempts
- Uploaded documents

### Test Modifies
- Worker assignments on existing referrals

### Test Deletes
- Referrals (DeleteCaseCheckTests)
- Contact attempts (ReferralsTests)
- Uploaded documents (UploadDocumentsTests)

**Net Impact**: Some data created (person profiles, referrals), some deleted (referrals, contact attempts, documents).

---

## Running the Tests

### Run All Referrals Tests
```bash
dotnet test --filter "FullyQualifiedName~Referrals"
```

### Run Specific Test File
```bash
# Core referral tests
dotnet test --filter "FullyQualifiedName~ReferralsTests"

# Delete referral tests
dotnet test --filter "FullyQualifiedName~DeleteCaseCheckTests"

# Worker assignment tests
dotnet test --filter "FullyQualifiedName~UpdateWorkerAssignmentTests"

# Document upload tests
dotnet test --filter "FullyQualifiedName~UploadDocumentsTests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~ReferralsTests.NewReferral_SearchForPersonTest_ShowsMatchesInGrid"
```

---

## Troubleshooting

### Test Fails: "No active referrals found"
- **Issue**: Active Referrals table is empty
- **Solution**: Create at least one active referral manually or via setup

### Test Fails: "Test file not found"
- **Issue**: `TestHFNY.pdf` missing from expected location
- **Solution**: Create/copy test PDF to `C:\Users\IP282924\Desktop\Repo\Pookie.Tests\TestFiles\TestHFNY.pdf`

### Test Fails: "Unable to locate Referrals link"
- **Issue**: User doesn't have permission or navbar structure changed
- **Solution**: Verify user has DataEntry role, check navbar selectors

### Test Fails: "Person search returns no results" (when expected)
- **Issue**: Test data not in database
- **Solution**: Create person "Tasha Smith" with specified details

### Test Fails: Chosen dropdown not working
- **Issue**: Chosen.js not loaded or element not ready
- **Solution**: Increase wait time before interacting, verify Chosen container exists

### Test Fails: Stale element exception after date change
- **Issue**: Postback refreshed page, old element reference invalid
- **Solution**: Re-find element after postback

### Test Fails: Validation error not found
- **Issue**: Error selector doesn't match or error not displayed
- **Solution**: Scroll to error location, add more selector variants, check error text

### Test Fails: Toast notification not found
- **Issue**: Toast appeared and disappeared too quickly
- **Solution**: Capture toast immediately after action, increase wait time

### Test Fails: Worker assignment date validation passes when it shouldn't
- **Issue**: Test date is actually after referral date
- **Solution**: Verify referral date in database, use earlier assignment date

### Test Fails: "No records found" message not appearing
- **Issue**: Search actually found a match (duplicate person exists)
- **Solution**: Use completely unique search data or delete duplicates

---

## Field Reference

### Person Search Form

| Field | Input Type | Suffix | Notes |
|-------|------------|--------|-------|
| **PC1 First Name** | Text | `txtpcfirstname` | Required for search |
| **PC1 Last Name** | Text | `txtpclastname` | Required for search |
| **DOB** | Text | `txtpcdob` | Format: MM/DD/YYYY |
| **Phone** | Text | `txtpcphone` | 10 digits |
| **Emergency Phone** | Text | `txtpcemergencyphone` | 10 digits |

---

### Person Profile Form

| Field | Input Type | Suffix | Required | Notes |
|-------|------------|--------|----------|-------|
| **First Name** | Text | `txtFirstName` | Yes | |
| **Last Name** | Text | `txtLastName` | Yes | |
| **DOB** | Text | `txtDOB` | Yes | MM/DD/YYYY |
| **SSN** | Text | `txtSSN` | No | ###-##-#### |
| **Phone** | Text | `txtPhone` | Yes | 10 digits |
| **Emergency Phone** | Text | `txtEmergencyPhone` | Yes | 10 digits |
| **Race** | Multi-select (Chosen) | `ddlRace` | Yes | Multiple selections allowed |
| **Gender** | Dropdown | `ddlGender` | Yes | Male, Female, Other |

---

### Referral Form

| Field | Input Type | Suffix | Required | Validation |
|-------|------------|--------|----------|------------|
| **Referral Date** | Text | `txtReferralDate` | Yes | >= 2016, <= Today |
| **Referral Source** | Dropdown | `ddlReferralSource` | Yes | |
| **Referral Reason** | Dropdown | `ddlReferralReason` | Yes | |
| **Contact Type** | Dropdown | `ddlContactType` | Yes | Phone, In-Person, etc. |
| **Contact Date** | Text | `txtContactDate` | Yes | |
| **Notes** | Textarea | `txtNotes` | No | |

---

### Worker Assignment Form

| Field | Input Type | Selector | Required | Notes |
|-------|------------|----------|----------|-------|
| **Assignment Date** | Text | `input[id*='txtWorkerAssignmentDate']` | Yes | Triggers postback on change |
| **Worker** | Dropdown | `select[id*='ddlWorkerAssignmentWorker']` | Yes | Re-rendered after date postback |

---

### Contact Attempt Form

| Field | Input Type | Suffix | Required | Notes |
|-------|------------|--------|----------|-------|
| **Contact Date** | Text | `txtContactDate` | Yes | |
| **Contact Type** | Dropdown | `ddlContactType` | Yes | |
| **Outcome** | Dropdown | `ddlOutcome` | Yes | |
| **Notes** | Textarea | `txtNotes` | No | |

---

### Document Upload Form

| Field | Input Type | Selector | Required | Notes |
|-------|------------|----------|----------|-------|
| **File Upload** | File | `input[type='file'][id*='fuUploadedFile']` | Yes | PDF, images, etc. |

---

## Grid/Table Reference

### Active Referrals Table

**Selector**: `.table.table-condensed.table-responsive.dataTable.no-footer.dtr-column` (with "active" and "referral" in class/id)

**Typical Columns**:
- PC1 Name
- Referral Date
- Referral Source
- Worker Assignment
- Status
- Actions (Edit, Delete)

---

### Completed Referrals Table

**Selector**: `.table.table-condensed.table-responsive.dataTable.no-footer.dtr-column` (with "completed" and "referral" in class/id)

**Filter**: Year dropdown controls which year's referrals are shown

**Typical Columns**:
- PC1 Name
- Referral Date
- Completion Date
- Worker
- Outcome
- Actions (View)

---

### Person Search Results Grid

**Selector**: `table[id*='grResults']` or `div[id*='grResults'] table.dataTable`

**Columns**:
- Select (button)
- First Name
- Last Name
- DOB
- Phone
- Emergency Phone

---

### Contact Attempts Table

**Selector**: `table[id*='tblContactAttempts']` (via `FindElementBySuffix`)

**Columns**:
- Contact Date
- Contact Type
- Outcome
- Notes
- Actions (Edit, Delete)

---

### Uploaded Documents Table

**Selector**: `table[id*='UploadedFile']` or `table[id*='Document']`

**Identification**: Table text contains "pdf", "View", or "Delete"

**Columns**:
- File Name
- Upload Date
- Uploaded By
- Actions (View, Delete)

---

## Business Logic Summary

### Referral Lifecycle

```
1. Referral Received
    ↓
2. Search for Existing Person
    ├─ Found → Select Person
    └─ Not Found → Create New Person Profile
    ↓
3. Create Referral Record
    ↓
4. Assign to Worker
    ↓
5. Contact Attempts
    ├─ Successful → Move to Active Case
    ├─ No Response → Continue Attempts
    └─ Refused Services → Close/Complete Referral
    ↓
6. Outcome
    ├─ Became Active Case → Move to Cases module
    └─ Did Not Enroll → Move to Completed Referrals
```

---

### Key Business Rules

1. **No Duplicates**: Always search before creating new person profiles

2. **Date Constraints**:
   - Referral Date >= 2016
   - Referral Date <= Today
   - Worker Assignment Date >= Referral Date

3. **Required Data**: Cannot create referral without source, reason, and contact information

4. **Worker Assignment**: Referral must be assigned to a worker for follow-up

5. **Contact Attempts**: Document all outreach efforts for compliance

6. **Document Retention**: Uploaded documents become part of permanent record

7. **Status Transition**: Referrals move from Active → Completed based on outcome


