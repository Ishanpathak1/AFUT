# ServiceReferrals Tests Documentation

#IMPORTANT -Make sure a service referral form is already there. While creating referral form it requries certain criteria to prepare this

## Overview

The ServiceReferrals test suite contains automated end-to-end tests for the Service Referrals form functionality. These tests validate the ability to create, edit, and delete service referrals that document when participants are referred to external agencies and services (healthcare, social services, etc.).

## What are Service Referrals?

**Service Referrals** document when home visiting program participants are referred to external services or agencies. This tracks coordination of care and ensures participants receive comprehensive support.

**Purpose**:
- Document referrals to external service providers
- Track which family member was referred
- Record nature of referral (inform, discuss, arrange)
- Track if services were received and why if not
- Ensure referrals are made after case start

**Workflow**:
```
Create New Service Referral
    ↓
Fill Required Fields
    ├─ Referral Date (must be >= Case Start Date)
    ├─ Worker
    ├─ Service Code
    ├─ Family Member Referred
    ├─ Nature of Referral
    └─ Agency Referred To
    ↓
Optional: Services Received?
    ├─ Yes → Enter Start Date
    └─ No → Enter Reason Not Received
    ↓
Submit Referral
    ↓
Appears in Referrals Grid
```

## Test File Overview

| File | Test Count | Primary Focus |
|------|------------|---------------|
| **ServiceReferralsTests.cs** | 6 | CRUD operations, validation, conditional fields |

**Total Tests**: 6 (parameterized with PC1 IDs)

**Test Priorities**: Tests are ordered 1-6 using `[TestPriority]` attribute for sequential execution.

---

## Test Execution Order

Tests run in priority order:

1. **Priority 1**: Check add new referral form (navigation smoke test)
2. **Priority 2**: Check referral date validation (date >= case start)
3. **Priority 3**: Check conditional fields (services received logic)
4. **Priority 4**: Check form validation and submission (full flow)
5. **Priority 5**: Check edit button (update existing referral)
6. **Priority 6**: Check delete button (delete with confirmation)

---

## Detailed Test Breakdown

### Test 1: CheckingTheAddNewOfServiceReferralForm (Priority 1)

**Purpose**: Smoke test to verify navigation to Service Referrals page and ability to open new referral form.

**Test Flow**:

#### Part 1: Navigate to Service Referrals
1. Sign in as DataEntry user
2. Navigate to Forms tab for target PC1 ID
3. Find Service Referrals link in Forms pane
4. Click Service Referrals link
5. Wait for Service Referrals page to load

#### Part 2: Verify PC1 ID Display
6. Find PC1 ID display on page
7. Assert PC1 ID is not empty
8. Assert PC1 ID contains expected value

#### Part 3: Create New Referral Entry
9. Click "New Referral" button
10. Enter referral date: "11/20/25"
11. Click "Add New" button in modal
12. Wait for form to load

**Service Referrals Link Selector**:
```css
a.moreInfo[data-formtype='sr']
a.moreInfo[id$='lnkServiceReferral']
```

**Purpose**: Ensures basic navigation and form opening works before running complex tests.

---

### Test 2: CheckingReferralDateCannotBeEarlierThanCaseStart (Priority 2)

**Purpose**: Test date validation - referral date must be greater than case start date.

**Test Flow**:

#### Part 1: Navigate and Open Form
1-8. Same as Test 1 (navigate to Service Referrals page)

#### Part 2: Enter Invalid Date
9. Click "New Referral" button
10. Enter referral date: **"10/16/23"** (known to be before case start date)
11. Click "Add New" button
12. Do NOT expect success (`expectSuccess: false`)

#### Part 3: Verify Validation Error
13. Get validation summary text
14. Assert validation summary is not empty
15. Assert message contains: **"Invalid Referral Date; must be greater than Case Start Date."**

**Date Validation Rule**: Referral Date > Case Start Date

**Why This Matters**: Can't refer someone to services before their case started in the program.

**Validation Selector**: `.validation-summary.alert.alert-danger`

---

### Test 3: CheckingServiceReferralConditionalFields (Priority 3)

**Purpose**: Test conditional field visibility based on "Services Received" selection.

**Test Flow**:

#### Part 1: Setup
1-9. Navigate to Service Referrals and open new form
10. Populate minimum required fields (worker, service code, family member, nature, agency)

#### Part 2: Test Services Received = Yes
11. Select Services Received: **Yes (value "1")**
12. Wait for page update
13. Find Start Date input: `input[id$='startdate']`
14. Assert Start Date input is **displayed**
15. Enter Start Date: "11/21/25"
16. Assert Reason Not Received field is **NOT displayed**

#### Part 3: Test Services Received = No
17. Select Services Received: **No (value "0")**
18. Wait for page update
19. Find Reason Not Received dropdown: `select[id$='reasonnotreceived']`
20. Assert Reason Not Received dropdown is **displayed**
21. Select reason: "2. Participant not eligible for service" (value "02")
22. Assert Start Date input is **NOT displayed**

**Conditional Logic**:
```
IF Services Received = Yes
THEN show Start Date field
     hide Reason Not Received field
ELSE IF Services Received = No
THEN show Reason Not Received field
     hide Start Date field
```

---

### Test 4: CheckingServiceReferralFormValidationAndSubmission (Priority 4)

**Purpose**: Comprehensive test of progressive validation and successful form submission.

**Test Flow**:

#### Part 1: Submit Empty Form
1-9. Navigate to Service Referrals and open new form
10. Click Submit without filling any fields
11. Capture validation errors
12. Assert validation contains all required field messages:
    - "Service Code Required"
    - "Family Member Referred Required"
    - "Nature of Referral Required"
    - "Agency referred to is Required"

**Note**: Worker may already be selected from previous test, so not always validated.

#### Part 2: Fill Worker
13. Select Worker: "Test, Derek" (value "3489")
14. Click Submit
15. Capture validation (worker validation may or may not appear)

#### Part 3: Fill Service Code
16. Select Service Code: "02 Child primary care" (value "02")
17. Click Submit
18. Capture validation
19. Assert "Service Code Required" message is **CLEARED**

#### Part 4: Fill Family Member
20. Select Family Member Referred: "2. Primary Caretaker 2" (value "02")
21. Click Submit
22. Capture validation
23. Assert "Family Member Referred Required" message is **CLEARED**

#### Part 5: Fill Nature of Referral
24. Select Nature of Referral: "2. Inform/Discuss" (value "02")
25. Click Submit
26. Capture validation
27. Assert "Nature of Referral Required" message is **CLEARED**

#### Part 6: Fill Agency
28. Select Agency Referred To: "Anonymized" (value "1")
29. Click Submit
30. Capture validation
31. Assert validation text is **empty** (all validations cleared)

#### Part 7: Verify Success
32. Wait for success toast (up to 15 seconds)
33. Assert toast contains:
    - "Form Saved"
    - PC1 ID
34. Find referral row in grid by date "11/20/25" and agency "Anonymized"
35. Assert row is not null (referral appears in grid)

**Progressive Validation Pattern**: Each field clears its own validation message when filled.

**Success Toast**: `.jq-toast-single.jq-icon-success`

---

### Test 5: CheckEditButton (Priority 5)

**Purpose**: Test editing existing service referral.

**Test Flow**:

#### Part 1: Navigate and Find Existing Referral
1-8. Navigate to Service Referrals page
9. Find existing editable referral row
10. Find Edit button in row: `a.btn.btn-sm.btn-default[id$='lnkEditButton']`
11. Extract `srpk` (Service Referral Primary Key) from edit button href

#### Part 2: Open Edit Form
12. Click Edit button
13. Wait for form to load

#### Part 3: Modify Services Received
14. Select Services Received: **Yes**
15. Start Date input appears
16. Enter Start Date: "11/20/25"
17. Assert Reason Not Received dropdown is **not visible**

#### Part 4: Submit Edit
18. Click Submit
19. Capture validation
20. Assert validation is empty (no errors)
21. Wait for success toast
22. Assert toast contains "Form Saved" and PC1 ID

#### Part 5: Verify Update in Grid
23. Wait for referral row with same `srpk` (up to 20 seconds)
24. Assert row shows Services Received = **"Yes"**

**SRPK (Service Referral Primary Key)**: Unique identifier extracted from edit button URL (e.g., `srpk=123`).

**Edit Button Selector**: `a.btn.btn-sm.btn-default[id$='lnkEditButton']`

---

### Test 6: CheckDeleteButton (Priority 6)

**Purpose**: Test delete with cancel and confirm flows.

**Test Flow**:

#### Part 1: Navigate and Find Referral to Delete
1-9. Navigate to Service Referrals page
10. Find existing editable referral row
11. Extract `srpk` from edit button href

#### Part 2: Cancel Delete Flow
12. Find delete button: `.delete-control a.btn.btn-danger[id$='lbDelete']`
13. Click delete button
14. Wait for delete confirmation modal: `div.dc-confirmation-modal.modal`
15. Find Cancel button: `button.btn.btn-default[data-dismiss='modal']`
16. Click Cancel button
17. Wait for modal to close
18. Wait for referral row to still exist (verify not deleted)

#### Part 3: Confirm Delete Flow
19. Click delete button again
20. Wait for delete confirmation modal
21. Find Confirm button: `a.btn.btn-primary[id$='lbConfirmDelete']`
22. Click Confirm button
23. Wait for deletion to complete

#### Part 4: Verify Deletion
24. Wait for modal to close
25. Wait for delete success toast
26. Assert toast contains:
    - "Form Deleted"
    - "Service Referral"
    - "was successfully deleted"
    - PC1 ID
27. Wait for referral row removal (up to 25 seconds)
28. Assert row with `srpk` no longer exists

**Delete Confirmation Modal**: `.dc-confirmation-modal.modal`

**Delete Flow**: Cancel preserves row, Confirm removes row.

---

## Required Fields

### Service Referral Form

| Field | Property | Selector | Required | Notes |
|-------|----------|----------|----------|-------|
| **Referral Date** | ReferralDate | `input.form-control[id*='ReferralDate']` | Yes | Must be > Case Start Date |
| **Worker** | Worker | `select[id$='ddlWorker']` | Yes | Assigned worker |
| **Service Code** | ServiceCode | `select[id$='ddlServiceCode']` | Yes | Type of service (02=Child primary care, etc.) |
| **Family Member Referred** | FamilyMember | `select[id$='familymember']` | Yes | 01=PC1, 02=PC2, etc. |
| **Nature of Referral** | NatureOfReferral | `select[id$='ddlNatureOfReferral']` | Yes | 01=Arrange, 02=Inform/Discuss, etc. |
| **Agency Referred To** | Agency | `select[id$='ddlAgency']` | Yes | External agency/provider |

### Conditional Fields

| Field | Selector | Condition | Notes |
|-------|----------|-----------|-------|
| **Services Received?** | `select[id$='servicesreceived']` | Always visible | Yes=1, No=0 |
| **Start Date** | `input[id$='startdate']` | If Services Received = Yes | Date services started |
| **Reason Not Received** | `select[id$='reasonnotreceived']` | If Services Received = No | 01-04 (various reasons) |

---

## Helper Methods Summary

### Navigation Helpers

#### SignInAsDataEntry()
Signs in and selects DataEntry role.

**Returns**: `HomePage` object

---

#### NavigateToServiceReferrals()
Navigates from home page to Service Referrals page for specific PC1 ID.

**Steps**:
1. Navigate to Forms tab
2. Find Service Referrals link
3. Click link
4. Wait for page load

---

#### NavigateToFormsTab()
Complex navigation that searches for case and opens Forms tab.

**Steps**:
1. Click Search Cases button
2. Enter PC1 ID
3. Click Search
4. Click Forms tab
5. Ensure Forms tab is active
6. Return Forms pane element

**Returns**: `IWebElement` (Forms pane)

---

### Form Creation Helpers

#### CreateNewReferralEntry()
Opens new service referral form with date.

**Parameters**:
- `referralDate`: Date string (default "11/20/25")
- `expectSuccess`: Whether to expect successful form load (default true)

**Steps**:
1. Click "New Referral" button
2. Enter referral date
3. Click "Add New" button
4. Wait for form load

**New Referral Button**: `a.btn.btn-default.pull-right[id$='btnAdd']`

---

### Dropdown Selection Helpers

#### SelectWorker()
Selects worker from dropdown.

**Uses**: `WebElementHelper.SelectDropdownOption()`

**Selectors**:
```css
select[id$='ddlWorker']
select[id*='ddlCaseWorker']
select[id*='ddlFSW']
```

---

#### SelectServiceCode()
Selects service code.

**Example**: "02 Child primary care" (value "02")

---

#### SelectFamilyMemberReferred()
Selects family member.

**Example**: "2. Primary Caretaker 2" (value "02")

---

#### SelectNatureOfReferral()
Selects nature of referral.

**Example**: "2. Inform/Discuss" (value "02")

---

#### SelectAgencyReferredTo()
Selects agency.

**Example**: "Anonymized" (value "1")

---

#### SelectServicesReceived()
Selects Yes or No from Services Received dropdown.

**Parameters**: `bool servicesReceived`
- `true` → Select value "1" (Yes)
- `false` → Select value "0" (No)

**Important**: Triggers update panel refresh - wait after selection.

---

### Validation Helpers

#### SubmitAndCaptureValidation()
Clicks Submit and returns validation summary text.

**Returns**: `string?` (validation text or null)

---

#### GetValidationSummaryText()
Finds and returns validation summary text.

**Selector**: `.validation-summary.alert.alert-danger`

**Returns**: Trimmed text or null if not found.

---

#### AssertValidationMessageCleared()
Asserts that specific validation message is NOT in validation text.

**Use Case**: Verify validation cleared after filling field.

---

### Grid Helpers

#### FindServiceReferralRow()
Finds referral row by date and agency text.

**Parameters**:
- `formDateText`: Date string (e.g., "11/20/25")
- `detailText`: Agency name (e.g., "Anonymized")

**Returns**: `IWebElement?` or null if not found

**Grid Selector**: `table[id*='grServiceReferral'], table[id*='gvServiceReferral']`

---

#### GetExistingEditableReferralRow()
Finds an existing referral row with Edit button.

**Strategy**:
1. Try to find row containing "Test" (test data)
2. If not found, return first row with Edit button
3. Throw error if no editable rows exist

**Returns**: `IWebElement` (referral row)

---

#### FindReferralRowBySrpk()
Finds referral row by Service Referral Primary Key (srpk).

**Logic**: Looks for edit button with href containing `srpk={value}`.

**Returns**: `IWebElement?` or null if not found

---

#### WaitForReferralRowBySrpk()
Waits for referral row with specific srpk to appear.

**Timeout**: Default 10 seconds, configurable

**Returns**: `IWebElement` or throws timeout error

---

#### WaitForReferralRowRemoval()
Waits for referral row with specific srpk to disappear (after delete).

**Timeout**: Default 15 seconds, up to 25 seconds

**Throws**: Error if row still exists after timeout

---

### Delete Helpers

#### CancelDeleteFlow()
Opens delete modal, clicks Cancel, verifies row still exists.

---

#### ConfirmDeleteFlow()
Opens delete modal, clicks Confirm, verifies row removed.

---

#### OpenDeleteModal()
Clicks delete button and waits for confirmation modal to appear.

**Returns**: `IWebElement` (modal)

**Modal Selector**: `div.dc-confirmation-modal.modal`

---

#### WaitForModalToClose()
Waits for modal to be hidden (not displayed).

**Timeout**: Default 10 seconds

---

### Toast Helpers

#### WaitForToastMessage()
Waits for success toast and verifies contents.

**Verifies**:
- "Form Saved"
- PC1 ID

**Toast Selector**: `.jq-toast-single.jq-icon-success`

---

#### WaitForDeleteToastMessage()
Waits for delete success toast.

**Verifies**:
- "Form Deleted"
- "Service Referral"
- "was successfully deleted"
- PC1 ID

---

### Utility Helpers

#### FindPc1Display()
Finds PC1 ID display on page.

**Selectors**:
```css
[id$='lblPC1ID']
[id$='lblPc1Id']
.pc1-id
.pc1-id-value
```

**Fallback**: Searches panel/card bodies for text containing PC1 ID.

---

#### FindElementInModalOrPage()
Finds element in modal first, then falls back to page.

**Strategy**:
1. Look for visible modal
2. Search within modal
3. If not found or no modal, search page
4. Poll for up to timeout seconds

**Use Case**: Robust element finding for elements that may be in modal or on page.

---

#### SetInputValue()
Sets input field value with multiple fallback strategies.

**Strategy**:
1. Try Clear() + SendKeys()
2. If value doesn't stick, use JavaScript
3. If still fails, remove readonly attribute and try JavaScript again
4. Verify final value matches expected
5. Optionally trigger blur event

**Handles**: `ElementNotInteractableException`, readonly fields, JavaScript-rendered fields

---

#### ExtractQueryParameter()
Extracts query parameter value from URL.

**Example**: `ExtractQueryParameter(href, "srpk")` → returns "123" from `url?srpk=123`

**Returns**: Unescaped parameter value or null

---

#### IsElementDisplayed()
Checks if any element matching selector is displayed.

**Returns**: `bool`

---

#### PopulateMinimumRequiredServiceReferralFields()
Fills all 5 required fields at once.

**Fields Filled**:
- Worker: "Test, Derek" (3489)
- Service Code: "02 Child primary care"
- Family Member: "2. Primary Caretaker 2"
- Nature: "2. Inform/Discuss"
- Agency: "Anonymized"

**Use Case**: Quick setup for tests that don't care about specific field values.

---

## Important Concepts

### 1. Service Referral Primary Key (srpk)

**What It Is**: Unique identifier for each service referral record.

**Where Found**: In edit button and delete button href attributes.

**Format**: `?srpk=123` or `&srpk=123` in URL

**Use Cases**:
- Identify specific referral row after edit
- Verify correct row deleted
- Track referral across page refreshes

**Extraction**: `ExtractQueryParameter(href, "srpk")`

---

### 2. Conditional Field Logic

**Services Received Dropdown** triggers show/hide of other fields:

```
IF Services Received = Yes (value "1")
THEN
    Show: Start Date input
    Hide: Reason Not Received dropdown
ELSE IF Services Received = No (value "0")
THEN
    Show: Reason Not Received dropdown
    Hide: Start Date input
```

**Implementation**: Uses AJAX update panel - page refreshes after selection.

**Test Verification**: Uses `IsElementDisplayed()` to check visibility.

---

### 3. Date Validation

**Rule**: Referral Date > Case Start Date

**Test Date**:
- Valid: "11/20/25" (after most case start dates)
- Invalid: "10/16/23" (before most case start dates)

**Validation Message**: "Invalid Referral Date; must be greater than Case Start Date."

**Why**: Ensures referrals are chronologically after case enrollment.

---

### 4. Progressive Validation

**Pattern**: Submit form multiple times, filling one field at a time.

**Flow**:
```
Submit empty → 4 validations
Fill Service Code → Submit → 3 validations (Service Code cleared)
Fill Family Member → Submit → 2 validations (Family Member cleared)
Fill Nature → Submit → 1 validation (Nature cleared)
Fill Agency → Submit → 0 validations (Agency cleared)
→ Success!
```

**Purpose**: Ensures real-time feedback as user fills form.

---

### 5. jQuery Toast Notifications

**Toast Library**: jQuery Toast plugin

**Success Toast**:
```html
<div class="jq-toast-single jq-icon-success">
    Form Saved - Service Referral for PC1 AB12345678 successfully saved
</div>
```

**Delete Toast**:
```html
<div class="jq-toast-single jq-icon-success">
    Form Deleted - Service Referral for PC1 AB12345678 was successfully deleted
</div>
```

**Wait Strategy**: Poll for up to 15 seconds for toast to appear.

---

### 6. Test Priority Order

**Why Ordered?**:
- Priority 1-4 create test data
- Priority 5 edits data created by Priority 4
- Priority 6 deletes data created by Priority 4

**Ordering Attribute**: `[TestPriority(N)]`

**Test Orderer**: `[TestCaseOrderer("AFUT.Tests.UnitTests.Attributes.PriorityOrderer", "AFUT.Tests")]`

**Important**: Tests must run in order for data dependencies.

---

### 7. Parameterized Tests

**Pattern**: All tests use `[Theory]` with `[MemberData(nameof(GetTestPc1Ids))]`

**Data Source**: `_config.TestPc1Ids` from `appsettings.json`

**Benefit**: Same tests run against multiple PC1 IDs for broader coverage.

---

### 8. Modal vs Page Element Finding

**Challenge**: Some elements appear in modals, some on main page.

**Solution**: `FindElementInModalOrPage()` helper

**Strategy**:
1. Check if modal is visible
2. Search within modal first
3. Fall back to page search
4. Poll for timeout duration

**Use Cases**: Referral date input, submit button, dropdowns

---

## What to Keep in Mind

### When Modifying Tests

1. **Test Order Matters**: Tests 5-6 depend on data created by tests 1-4. Don't change priority order.

2. **Worker May Be Pre-Selected**: Worker validation may not always appear because worker persists across form loads.

3. **Conditional Fields Update**: After changing "Services Received", wait for update panel refresh before checking field visibility.

4. **Modal vs Page**: Elements may be in modal or on page - use `FindElementInModalOrPage()`.

5. **SRPK Tracking**: Edit and delete operations rely on srpk from href - ensure extraction works.

6. **Date Dependency**: Invalid date test ("10/16/23") assumes case start date is after 2023-10-16.

---

### When Adding New Tests

1. **Assign Priority**: Add `[TestPriority(N)]` with appropriate number (> 6).

2. **Use Parameterization**: Add `[Theory]` and `[MemberData(nameof(GetTestPc1Ids))]`.

3. **Start with Navigation**: Use `SignInAsDataEntry()` and `NavigateToServiceReferrals()`.

4. **Use Existing Helpers**: Check for `SelectServiceCode()`, `SelectAgencyReferredTo()`, etc.

5. **Log Actions**: Use `_output.WriteLine()` for all significant actions.

6. **Wait After Dropdowns**: "Services Received" dropdown triggers update panel - always wait.

7. **Verify with Toast**: Success should show jQuery toast - use `WaitForToastMessage()`.

---

### Common Pitfalls

1. **Test Order Failure**: Running tests out of order causes failures because test 5-6 expect data from test 4.
   - **Solution**: Always run full test suite, or manually create test data before running individual tests.

2. **Worker Validation Not Appearing**: Worker field already populated from previous test.
   - **Solution**: Don't assert worker validation specifically; it's unreliable.

3. **Element in Modal Not Found**: Looking on page instead of in modal.
   - **Solution**: Use `FindElementInModalOrPage()` instead of direct `FindElement()`.

4. **Conditional Field Not Visible**: Didn't wait for update panel after "Services Received" change.
   - **Solution**: Always use `WaitForUpdatePanel()` and `WaitForReady()` after dropdown change.

5. **SRPK Extraction Failed**: Href format changed or edit button href is empty.
   - **Solution**: Verify edit button has href attribute, check URL parameter format.

6. **Row Not Found After Edit**: Grid didn't refresh or srpk changed.
   - **Solution**: Increase wait timeout, verify srpk is correct.

7. **Modal Not Closing**: Modal animation takes time or JavaScript issue.
   - **Solution**: Increase timeout in `WaitForModalToClose()`, check for JavaScript errors.

---

## Test Data Requirements

### Prerequisites
- Test user with DataEntry role
- Valid PC1 IDs in `appsettings.json` under `TestPc1Ids`
- Cases must have:
  - Start dates before "10/16/23" (for invalid date test)
  - Start dates before "11/20/25" (for valid date tests)
- Test worker: "Test, Derek" (ID: 3489)
- Test agency: "Anonymized" (ID: 1)

### Test Creates
- Service referrals with date "11/20/25"
- Referrals with various service codes and agencies

### Test Modifies
- Priority 5 updates referral to have Services Received = Yes

### Test Deletes
- Priority 6 deletes one referral

**Net Impact**: Some referrals created, one edited, one deleted.

---

## Running the Tests

### Run All ServiceReferrals Tests
```bash
dotnet test --filter "FullyQualifiedName~ServiceReferralsTests"
```

### Run Specific Test
```bash
# Add new test (Priority 1)
dotnet test --filter "FullyQualifiedName~ServiceReferralsTests.CheckingTheAddNewOfServiceReferralForm"

# Date validation (Priority 2)
dotnet test --filter "FullyQualifiedName~ServiceReferralsTests.CheckingReferralDateCannotBeEarlierThanCaseStart"

# Conditional fields (Priority 3)
dotnet test --filter "FullyQualifiedName~ServiceReferralsTests.CheckingServiceReferralConditionalFields"

# Full validation (Priority 4)
dotnet test --filter "FullyQualifiedName~ServiceReferralsTests.CheckingServiceReferralFormValidationAndSubmission"

# Edit (Priority 5)
dotnet test --filter "FullyQualifiedName~ServiceReferralsTests.CheckEditButton"

# Delete (Priority 6)
dotnet test --filter "FullyQualifiedName~ServiceReferralsTests.CheckDeleteButton"
```

### Run in Order (Important!)
Tests should run in priority order automatically due to `[TestCaseOrderer]` attribute.

---

## Troubleshooting

### Test Fails: "Service Referrals link was not found"
- **Issue**: Link not in Forms pane or selector changed
- **Solution**: Verify Forms tab loaded, check link selector

### Test Fails: "Invalid Referral Date; must be greater than Case Start Date" appears unexpectedly
- **Issue**: Test date is actually before case start date
- **Solution**: Verify case start dates in database, use more recent test date

### Test Fails: "Service Code Required" doesn't clear after selection
- **Issue**: Dropdown selection failed or validation didn't refresh
- **Solution**: Verify dropdown selector, increase wait after selection

### Test Fails: "Services received should not be visible" / "Start date should be visible"
- **Issue**: Conditional fields not showing/hiding correctly
- **Solution**: Ensure `SelectServicesReceived()` waits for update panel, verify field selectors

### Test Fails: "Edit button was not found"
- **Issue**: No editable referrals exist in grid
- **Solution**: Run Priority 4 test first to create referrals, or manually create test data

### Test Fails: "Unable to determine srpk"
- **Issue**: Edit button href is empty or doesn't contain srpk parameter
- **Solution**: Verify edit button href format, check URL parameter extraction logic

### Test Fails: "Referral row with srpk still present after delete"
- **Issue**: Delete didn't complete or grid didn't refresh
- **Solution**: Increase timeout, verify delete succeeded (check toast), manually verify grid

### Test Fails: "Success toast was not displayed"
- **Issue**: Toast appeared too quickly or selector changed
- **Solution**: Increase timeout from 15 seconds, verify toast selector

### Test Fails: Test 5 or 6 fails but Tests 1-4 pass
- **Issue**: Tests run out of order or test data missing
- **Solution**: Ensure test orderer is working, run full suite from beginning

---

## Selectors Reference

### Buttons

| Element | Selector |
|---------|----------|
| **New Referral** | `a.btn.btn-default.pull-right[id$='btnAdd']` |
| **Add New (modal)** | `.modal-footer .btn.btn-primary[id$='btnSubmit']` |
| **Submit** | `a.btn.btn-primary[id*='Submit1'][id$='btnSubmit']` |
| **Edit** | `a.btn.btn-sm.btn-default[id$='lnkEditButton']` |
| **Delete** | `.delete-control a.btn.btn-danger[id$='lbDelete']` |

### Form Fields

| Field | Selector |
|-------|----------|
| **Referral Date** | `input.form-control[id*='ReferralDate']` |
| **Worker** | `select[id$='ddlWorker']` |
| **Service Code** | `select[id$='ddlServiceCode']` |
| **Family Member** | `select[id$='familymember']` |
| **Nature of Referral** | `select[id$='ddlNatureOfReferral']` |
| **Agency** | `select[id$='ddlAgency']` |
| **Services Received** | `select[id$='servicesreceived']` |
| **Start Date** | `input[id$='startdate']` (conditional) |
| **Reason Not Received** | `select[id$='reasonnotreceived']` (conditional) |

### Grid

| Element | Selector |
|---------|----------|
| **Service Referrals Grid** | `table[id*='grServiceReferral'], table[id*='gvServiceReferral']` |
| **Data Rows** | `tr` (with `td` elements) |

### Validation / Feedback

| Element | Selector |
|---------|----------|
| **Validation Summary** | `.validation-summary.alert.alert-danger` |
| **Success Toast** | `.jq-toast-single.jq-icon-success` |
| **Delete Modal** | `div.dc-confirmation-modal.modal` |


