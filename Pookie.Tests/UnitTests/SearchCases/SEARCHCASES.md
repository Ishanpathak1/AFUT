# SearchCases Tests Documentation

## Overview

The SearchCases test suite contains automated end-to-end tests for the Search Cases functionality. These tests validate the ability to search for existing cases using various criteria such as PC1 ID, names, dates of birth, worker assignments, and alternate IDs.

## What is Search Cases?

**Search Cases** is the primary interface for finding existing cases in the system. It allows users to search by multiple criteria to locate specific cases before viewing or editing them.

**Purpose**:
- Find cases by PC1 (Primary Caregiver 1) information
- Find cases by TC (Target Child) information
- Filter cases by assigned worker
- Support wildcard and partial matching searches
- Navigate directly to case details from search results

**Workflow**:
```
Enter Search Criteria
    ↓
Submit Search
    ↓
View Results
    ├─ Click PC1 ID → Open Case Home
    └─ No Results → "No records found." message
```

## Test File Overview

| File | Test Count | Primary Focus |
|------|------------|---------------|
| **SearchCasesTests.cs** | 5 | Search with various criteria, result verification, case navigation |

**Total Tests**: 5 (includes 1 theory test with 6 parameterized scenarios = 11 total executions)

---

## Architecture: Routine Pattern

### What is a Routine?

The SearchCases tests use a **Routine Pattern** - a reusable, step-by-step procedure that can be called from multiple tests.

**Benefits**:
- **Reusability**: Same search logic used across all tests
- **Maintainability**: Change search logic in one place
- **Clarity**: Step-by-step execution with clear names
- **Consistency**: All tests use same navigation and search approach

**Routine Class**: `SearchCasesSearchRoutine`

**Routine Steps**:
1. Load application
2. Sign in if required
3. Ensure role selected
4. Navigate to Search Cases
5. Populate search criteria
6. Submit search

---

## Test Breakdown

### Test 1: Fill_All_Search_Fields_DisplaysMatchingResult

**Purpose**: Verify that filling all search fields returns the expected case.

**Test Flow**:

#### Step 1-6: Execute Search Routine
1. Create driver and routine
2. Set search criteria with all fields:
   - PC1 ID: "AB12010361993"
   - PC1 First Name: "Anonymized"
   - PC1 Last Name: "Anonymized"
   - TC DOB: "060920"
   - Worker: "3396, Worker"
   - Alternate ID: "Anonymized"
3. Execute routine steps:
   - `LoadApplication()` - Navigate to app URL
   - `EnsureSignedIn()` - Login
   - `EnsureRoleSelected()` - Select DataEntry role
   - `NavigateToSearchCases()` - Open Search Cases page
   - `PopulateSearchCriteria()` - Fill all fields
   - `SubmitSearch()` - Click Search button

#### Step 7: Verify Results
7. Assert user signed in successfully
8. Assert role selected successfully
9. Assert Search Cases page loaded
10. Assert search completed
11. Assert first result PC1 ID matches searched PC1 ID
12. Assert `SearchMatched` flag is true

**Expected Result**: First search result has PC1 ID = "AB12010361993"

**Test Data**: Uses hardcoded test case known to exist in test database.

---

### Test 2: Fill_All_Search_Fields_OpensMatchingCaseHome

**Purpose**: Verify that clicking search result opens correct Case Home page.

**Test Flow**:

#### Part 1: Execute Search (Same as Test 1)
1-6. Execute search routine with all fields filled

#### Part 2: Open Case Home
7. Get first search result from parameters
8. Call `firstResult.OpenCaseHome()` - Clicks PC1 ID link
9. Wait for Case Home page to load

#### Part 3: Verify Case Home
10. Assert Case Home page is not null
11. Assert Case Home page is loaded
12. Assert Case Home PC1 ID matches first search result PC1 ID
13. Assert Case Home PC1 ID matches original search criteria PC1 ID

**Expected Result**: Case Home page opens with correct PC1 ID.

**Navigation**: PC1 ID in search results is a clickable link that opens Case Home page.

---

### Test 3: Search_With_No_Criteria_DisplaysNoRecordsFoundMessage

**Purpose**: Verify that searching with empty criteria shows "No records found" message.

**Test Flow**:

#### Step 1-6: Execute Search with Empty Criteria
1. Create driver and routine
2. Set search criteria as empty: `new SearchCasesCriteria()`
3. Execute routine steps (all fields left blank)

#### Step 7: Verify No Results Message
4. Assert user signed in
5. Assert role selected
6. Assert Search Cases page loaded
7. Assert search completed
8. Assert `FirstResult` is null (no results)
9. Get Search Cases page reference
10. Call `IsNoRecordsMessageDisplayed()`
11. Assert method returns true

**Expected Result**: Grid shows "No records found." message in header cell.

**No Records Message**: Appears in `thead td` cell when no results match criteria.

---

### Test 4: Single_Field_Search_Returns_Expected_Case (Theory)

**Purpose**: Test searching with only one field at a time to verify partial matching.

**Test Type**: Theory test with `[MemberData]` - runs 6 scenarios

**Test Flow**:

#### Scenarios (6 total):

**Scenario 1: Search by PC1 ID Only**
- Criteria: PC1 ID = "AB12010361993", Worker = "3396, Worker"
- Expects Exact Match: Yes
- Assertion: Result contains case with exact PC1 ID

**Scenario 2: Search by PC1 First Name Only**
- Criteria: PC1 First Name = "Anonymized", Worker = "3396, Worker"
- Expects Exact Match: No (may return multiple matches)
- Assertion: Results are not empty, "No records found" not displayed

**Scenario 3: Search by PC1 Last Name Only**
- Criteria: PC1 Last Name = "Anonymized", Worker = "3396, Worker"
- Expects Exact Match: No
- Assertion: Results are not empty

**Scenario 4: Search by TC DOB Only**
- Criteria: TC DOB = "060920", Worker = "3396, Worker"
- Expects Exact Match: No
- Assertion: Results are not empty

**Scenario 5: Search by Worker Only**
- Criteria: Worker = "3396, Worker"
- Expects Exact Match: No
- Assertion: Results are not empty (all cases for that worker)

**Scenario 6: Search by Alternate ID Only**
- Criteria: Alternate ID = "Anonymized", Worker = "3396, Worker"
- Expects Exact Match: No
- Assertion: Results are not empty

#### Assertions for Each Scenario:
- User signed in
- Role selected
- Search Cases page loaded
- Search completed
- No "No records found" message displayed
- Results collection is not empty
- If expects exact match and PC1 ID provided, verify result contains exact PC1 ID

**Why Include Worker?**: Most scenarios include Worker filter to narrow results and ensure test data exists.

---

### Test 5: Cancel_Search_Returns_To_Home_Page

**Purpose**: Verify Cancel button returns to Home page without executing search.

**Test Flow**:

#### Part 1: Navigate to Search Cases
1. Create driver and routine
2. Execute routine steps 1-4:
   - Load application
   - Sign in
   - Select role
   - Navigate to Search Cases
3. Do NOT populate criteria or submit search

#### Part 2: Click Cancel
4. Assert user signed in
5. Assert role selected
6. Assert Search Cases page loaded
7. Get Search Cases page reference
8. Call `searchCasesPage.CancelSearch()` - Clicks Cancel button
9. Wait for page load

#### Part 3: Verify Home Page
10. Create HomePage object
11. Assert Home page is not null
12. Assert Home page is loaded
13. Assert current URL ends with "/Default.aspx" or "Default.aspx"

**Expected Result**: User returns to Home page (Default.aspx) without executing search.

**Cancel Button**: Allows user to abort search and return to previous page.

---

## Search Criteria Fields

### PC1 (Primary Caregiver 1) Fields

| Field | Property | Input Selector | Description |
|-------|----------|----------------|-------------|
| **PC1 ID** | `Pc1Id` | `input[id$='txtPC1ID']` | Unique identifier (e.g., AB12010361993) |
| **PC1 First Name** | `Pc1FirstName` | `input[id$='txtPC1FirstName']` | Primary caregiver first name |
| **PC1 Last Name** | `Pc1LastName` | `input[id$='txtPC1LastName']` | Primary caregiver last name |
| **PC1 DOB** | `Pc1Dob` | `input[id$='txtPC1DOB']` | Primary caregiver date of birth |
| **PC Phone** | `PcPhone` | `input[id$='txtPCPhone']` | Primary caregiver phone number |

### TC (Target Child) Fields

| Field | Property | Input Selector | Description |
|-------|----------|----------------|-------------|
| **TC First Name** | `TcFirstName` | `input[id$='txtTCFirstName']` | Target child first name |
| **TC Last Name** | `TcLastName` | `input[id$='txtTCLastName']` | Target child last name |
| **TC DOB** | `TcDob` | `input[id$='txtTCDOB']` | Target child date of birth (format: MMDDYY) |

### Other Fields

| Field | Property | Input Selector | Type | Description |
|-------|----------|----------------|------|-------------|
| **Worker** | `WorkerDisplayText` | `select[id$='ddlWorker']` | Dropdown | Assigned worker (e.g., "3396, Worker") |
| **All Workers** | `IncludeAllWorkers` | `input[id$='chkAllWorkers']` | Checkbox | Include cases from all workers |
| **Alternate ID** | `AlternateId` | `input[id$='txtAlternateID']` | Input | Alternative case identifier |
| **HV Case PK** | `HvCasePk` | `input[id$='txtHVCasePK']` | Input | Home visiting case primary key |

---

## Routine Steps Explained

### Step 1: LoadApplication()

**Purpose**: Navigate to application URL.

**Logic**:
```csharp
parms.App = EntryPoint.OpenPage(_driver, _config);
```

**Actions**:
- Opens browser
- Navigates to `_config.AppUrl`
- Waits for page ready

---

### Step 2: EnsureSignedIn()

**Purpose**: Sign in if login page is displayed.

**Logic**:
```csharp
IF not on login page
THEN set SignedIn = true and return
ELSE perform login
```

**Actions**:
- Check if login page displayed (looks for `#Login1_LoginButton`)
- If on login page:
  - Create `LoginPage` object
  - Call `SignIn(username, password)`
  - Verify signed in successfully
  - Set `parms.SignedIn = true`
- If not on login page (already logged in):
  - Set `parms.SignedIn = true`

---

### Step 3: EnsureRoleSelected()

**Purpose**: Select user role if role selection page is displayed.

**Logic**:
```csharp
TRY
    Create SelectRolePage
    IF ProgramName and RoleName provided
    THEN SelectRole(program, role)
    ELSE SelectFirstAvailableRole()
CATCH (already past role selection)
    TryGetHomePage()
    IF home page found
    THEN set RoleSelected = true
```

**Actions**:
- Attempt to find role selection page
- If found:
  - Select specific role if `ProgramName` and `RoleName` provided
  - Otherwise select first available role
- If not found (already at home page):
  - Try to get HomePage
  - Set `parms.RoleSelected = true`
- Store HomePage reference in `parms.HomePage`

**Flexibility**: Handles both fresh login and already-logged-in state.

---

### Step 4: NavigateToSearchCases()

**Purpose**: Navigate to Search Cases page.

**Logic**:
```csharp
TRY
    IF HomePage available
    THEN homePage.OpenSearchCases()
    ELSE navigationBar.OpenSearchCasesPage()
CATCH (navigation failed)
    Navigate directly to /Pages/SearchCases.aspx
```

**Actions**:
- Attempt navigation via HomePage or NavigationBar
- If both fail, direct URL navigation
- Create `SearchCasesPage` object
- Set `parms.SearchCasesPage` reference
- Set `parms.SearchCasesPageLoaded` flag

**Fallback Strategy**: Multiple navigation methods ensure robustness.

---

### Step 5: PopulateSearchCriteria()

**Purpose**: Fill search form with provided criteria.

**Logic**:
```csharp
searchCasesPage.ApplyCriteria(parms.Criteria);
```

**Actions**:
- For each criteria field:
  - Find input/dropdown element
  - Clear existing value
  - Set new value if provided
- Worker dropdown: Select by display text
- All Workers checkbox: Check/uncheck if value provided

**Behavior**: Only fills fields that have non-null values in criteria object.

---

### Step 6: SubmitSearch()

**Purpose**: Click Search button and capture results.

**Logic**:
```csharp
searchCasesPage.SubmitSearch()
Set SearchCompleted = true
Get first result
Set FirstResult, FirstResultPc1Id
IF criteria has Pc1Id AND first result exists
THEN check if SearchMatched
```

**Actions**:
- Click Search button
- Wait for AJAX update panel (30 seconds)
- Wait for page ready (30 seconds)
- Get first search result
- Store in `parms.FirstResult` and `parms.FirstResultPc1Id`
- If searching by PC1 ID, set `parms.SearchMatched` flag

**SearchMatched**: True if first result PC1 ID matches searched PC1 ID (exact match).

---

## Page Object Model

### SearchCasesPage

**Purpose**: Encapsulates Search Cases page interactions.

#### Constructor
```csharp
public SearchCasesPage(IPookieWebDriver driver)
```
- Waits for page ready (30 seconds)
- Finds search form: `form[action*='SearchCases.aspx']`
- Verifies search button exists

#### Properties

**IsLoaded**: Returns true if search form and search button are displayed.

#### Methods

**ApplyCriteria(SearchCasesCriteria criteria)**
- Fills all search fields from criteria object
- Clears fields before setting new values
- Only sets fields with non-null criteria values

**SubmitSearch()**
- Clicks Search button
- Waits for update panel (30 seconds)
- Waits for page ready (30 seconds)

**CancelSearch()**
- Clicks Cancel button
- Waits for page ready (30 seconds)

**GetResults()**
- Returns collection of `SearchCasesResultRow` objects
- Filters out empty rows (rows without `<td>` elements)
- Returns empty collection if results grid not found

**GetFirstResult()**
- Returns first `SearchCasesResultRow` or null if no results

**IsNoRecordsMessageDisplayed()**
- Looks for results grid
- Finds header cell (`thead td`)
- Checks if cell text = "No records found." (case-insensitive)
- Returns true if message displayed, false otherwise

---

### SearchCasesCriteria

**Purpose**: Data transfer object for search criteria.

**Properties**: All optional (nullable strings and bool)
- `Pc1Id`
- `Pc1FirstName`
- `Pc1LastName`
- `Pc1Dob`
- `PcPhone`
- `TcFirstName`
- `TcLastName`
- `TcDob`
- `WorkerDisplayText`
- `IncludeAllWorkers`
- `AlternateId`
- `HvCasePk`

**Usage**:
```csharp
var criteria = new SearchCasesCriteria
{
    Pc1Id = "AB12010361993",
    WorkerDisplayText = "3396, Worker"
};
```

---

### SearchCasesResultRow

**Purpose**: Represents a single search result row.

#### Constructor
```csharp
internal SearchCasesResultRow(IPookieWebDriver driver, IWebElement row)
```
- Stores driver and row references

#### Properties

**Pc1Id**: Returns text content of first cell (`td:nth-child(1)`)

#### Methods

**OpenCaseHome()**
- Finds PC1 ID link in first cell (`td:nth-child(1) a`)
- Clicks link
- Waits for page ready (60 seconds)
- Returns new `CaseHomePage` object

**Grid Structure**: PC1 ID is in first column and is clickable link.

---

## Important Concepts

### 1. Routine Pattern

**Structure**:
```
Routine Class
    ├─ RoutineStep 1 (Method)
    ├─ RoutineStep 2 (Method)
    ├─ RoutineStep 3 (Method)
    └─ Params (State Object)
```

**Benefits**:
- **Declarative**: Step names describe what happens
- **Stateful**: Params object tracks state across steps
- **Reusable**: Same routine used by all tests
- **Debuggable**: Each step can be tested independently

**RoutineStep Attribute**:
```csharp
[RoutineStep(1, "Load application")]
public void LoadApplication(Params parms)
```
- Number: Execution order
- Description: Human-readable step name

**RoutineOutput Attribute**:
```csharp
[RoutineOutput]
public bool SignedIn { get; set; }
```
- Marks properties as routine outputs
- Used for logging and verification

---

### 2. Test Data Constants

Tests use hardcoded test data:

```csharp
private const string KnownPc1Id = "AB12010361993";
private const string KnownPc1FirstName = "Anonymized";
private const string KnownPc1LastName = "Anonymized";
private const string KnownTcDob = "060920";
private const string KnownWorkerDisplayText = "3396, Worker";
private const string KnownAlternateId = "Anonymized";
```
---

### 3. Date Format

**TC DOB Format**: `MMDDYY` (6 digits)
- Example: "060920" = June 9, 2020

**PC1 DOB Format**: Likely `MM/DD/YYYY` (not used in current tests)

---

### 4. Worker Selection

**Worker Display Text Format**: "{WorkerID}, Worker"
- Example: "3396, Worker"

**How It Works**:
- Worker dropdown shows workers in format "ID, LastName"
- Tests select by exact display text
- Uses `SelectElement.SelectByText()`

**All Workers Checkbox**: If checked, includes cases from all workers, not just selected worker.

---

### 5. Search Matching Logic

**PC1 ID**: Exact match expected
**Names**: Partial match (may return multiple results)
**DOB**: Exact match
**Worker**: Filter (returns all cases for that worker)
**Alternate ID**: Exact or partial match

**Multiple Fields**: AND logic - all filled fields must match.

---

### 6. No Results Handling

**Detection**:
```csharp
IF results grid exists
AND thead td cell exists
AND cell text = "No records found."
THEN no results
ELSE has results
```

**Grid Structure When No Results**:
```html
<table id="...grResults">
    <thead>
        <tr>
            <td>No records found.</td>
        </tr>
    </thead>
    <tbody></tbody>
</table>
```

**Grid Structure When Has Results**:
```html
<table id="...grResults">
    <thead>
        <tr>
            <th>PC1 ID</th>
            <th>PC1 Name</th>
            <!-- ... -->
        </tr>
    </thead>
    <tbody>
        <tr>
            <td><a href="...">AB12010361993</a></td>
            <td>Last, First</td>
            <!-- ... -->
        </tr>
    </tbody>
</table>
```

---

### 7. Result Navigation

**PC1 ID Link**: First column contains clickable link to Case Home.

**Click Flow**:
```
Search Results
    ↓
Click PC1 ID Link
    ↓
Wait 60 seconds for page load
    ↓
Case Home Page Opens
```

**Timeout**: 60 seconds (longer than usual due to Case Home page complexity).

---

## Helper Method Reference

### ExecuteSearchWithCriteria()

**Purpose**: Reusable helper for executing search with custom criteria and assertion.

**Parameters**:
- `criteria`: SearchCasesCriteria object
- `assertion`: Action delegate to run after search completes

**Usage**:
```csharp
ExecuteSearchWithCriteria(criteria, parameters =>
{
    Assert.True(parameters.SearchCompleted);
    // ... more assertions
});
```

**Flow**:
1. Create driver
2. Create routine and params
3. Set criteria
4. Execute all routine steps
5. Call assertion delegate

**Benefit**: Eliminates code duplication in theory tests.

---

### SingleCriteriaSearchData()

**Purpose**: Provides test data for parameterized theory test.

**Returns**: `IEnumerable<object[]>` where each array contains:
1. `SearchCasesCriteria` object
2. `bool` expectsExactMatch flag
3. `string` scenario description

**Yield Pattern**: Uses `yield return` to generate test cases dynamically.

**Example**:
```csharp
yield return new object[]
{
    new SearchCasesCriteria { Pc1Id = KnownPc1Id },
    true,  // expects exact match
    "searching by PC1 ID only"  // scenario description
};
```

---

## What to Keep in Mind

### When Modifying Tests

1. **Test Data Dependency**: Tests require specific cases to exist in database with exact IDs, names, and dates.

2. **Routine Pattern**: Changing search flow requires updating `SearchCasesSearchRoutine`, not individual tests.

3. **Worker Required**: Most single-field searches include Worker to narrow results and ensure test data exists.

4. **Date Format**: TC DOB uses `MMDDYY` format (6 digits), not standard date format.

5. **Constants**: Test data constants at top of class should match actual test data in database.

6. **Wait Times**: Case Home navigation uses 60-second timeout - may need adjustment for slow environments.

---

### When Adding New Tests

1. **Use Routine**: Always use `SearchCasesSearchRoutine` for search execution.

2. **Create Criteria**: Use `SearchCasesCriteria` object, not individual parameters.

3. **Verify Steps**: Assert each routine output flag (SignedIn, RoleSelected, etc.).

4. **Add to Theory**: If testing single-field search, add to `SingleCriteriaSearchData()`.

5. **Document Test Data**: If adding new test data, add constants at top of class.

6. **Test Negative Cases**: Consider adding tests for invalid data, special characters, SQL injection attempts.

---

### Common Pitfalls

1. **Test Data Not Found**: Test fails because expected case doesn't exist in database.
   - **Solution**: Verify test data exists, update constants if data changed.

2. **Worker Dropdown Not Found**: Worker dropdown doesn't exist or is hidden.
   - **Solution**: Ensure user has permission to see worker dropdown, check page structure.

3. **No Results When Expected**: Search returns no results when it should return data.
   - **Solution**: Verify search criteria matches data exactly, check date formats.

4. **Too Many Results**: Single-field search returns too many results to verify.
   - **Solution**: Add worker filter to narrow results, use more specific criteria.

5. **Page Navigation Timeout**: Case Home page takes too long to load.
   - **Solution**: Increase timeout from 60 seconds, check network/server performance.

6. **Stale Element on Result Click**: Result row element becomes stale before click.
   - **Solution**: Re-find element before clicking, reduce delays between search and click.

---

## Test Data Requirements

### Prerequisites
- Test user with DataEntry role
- Known test case with specific data:
  - PC1 ID: "AB12010361993"
  - PC1 First Name: "Anonymized"
  - PC1 Last Name: "Anonymized"
  - TC DOB: "060920" (June 9, 2020)
  - Assigned Worker: "3396, Worker"
  - Alternate ID: "Anonymized"

### Test Does NOT Modify Data
- All tests are read-only
- No cases created, modified, or deleted
- Safe to run repeatedly without cleanup

---

## Running the Tests

### Run All SearchCases Tests
```bash
dotnet test --filter "FullyQualifiedName~SearchCasesTests"
```

### Run Specific Test
```bash
# All fields search
dotnet test --filter "FullyQualifiedName~SearchCasesTests.Fill_All_Search_Fields_DisplaysMatchingResult"

# No criteria search
dotnet test --filter "FullyQualifiedName~SearchCasesTests.Search_With_No_Criteria_DisplaysNoRecordsFoundMessage"

# Single field theory test (runs all 6 scenarios)
dotnet test --filter "FullyQualifiedName~SearchCasesTests.Single_Field_Search_Returns_Expected_Case"

# Case Home navigation
dotnet test --filter "FullyQualifiedName~SearchCasesTests.Fill_All_Search_Fields_OpensMatchingCaseHome"

# Cancel button
dotnet test --filter "FullyQualifiedName~SearchCasesTests.Cancel_Search_Returns_To_Home_Page"
```

---

## Troubleshooting

### Test Fails: "User was not signed in"
- **Issue**: Login failed or credentials invalid
- **Solution**: Verify `_config.UserName` and `_config.Password` are correct

### Test Fails: "Role was not selected successfully"
- **Issue**: Role selection failed or role not available
- **Solution**: Verify user has DataEntry role, check role name in config

### Test Fails: "Search Cases page did not load"
- **Issue**: Navigation to Search Cases failed
- **Solution**: Verify user has permission to access Search Cases, check URL in config

### Test Fails: "Search did not complete"
- **Issue**: Search button click failed or results didn't load
- **Solution**: Check if search button exists, verify wait times are sufficient

### Test Fails: "First search result PC1 ID did not match"
- **Issue**: Search returned wrong case or no cases
- **Solution**: Verify test data exists with exact PC1 ID, check search logic

### Test Fails: "No records found message was displayed" (when expecting results)
- **Issue**: Search criteria doesn't match any cases
- **Solution**: Verify test data exists, check criteria values match database

### Test Fails: "Expected the 'No records found.' message to be displayed"
- **Issue**: Empty search returned results (shouldn't happen)
- **Solution**: Check if database has cases with no criteria (shouldn't), verify search logic

### Test Fails: "Case home page did not load successfully"
- **Issue**: Navigation to Case Home failed or timed out
- **Solution**: Increase timeout from 60 seconds, verify PC1 ID link works manually

### Test Fails: "PC1 identifier link was not found"
- **Issue**: Result row doesn't have clickable PC1 ID link
- **Solution**: Verify grid structure, check if first column has `<a>` tag

### Test Fails: "Worker dropdown was not found"
- **Issue**: Worker dropdown doesn't exist on page
- **Solution**: Check user permissions, verify dropdown selector is correct

---

## Selectors Reference

### Search Form Inputs

| Field | Selector |
|-------|----------|
| **PC1 ID** | `input[id$='txtPC1ID']` |
| **PC1 First Name** | `input[id$='txtPC1FirstName']` |
| **PC1 Last Name** | `input[id$='txtPC1LastName']` |
| **PC1 DOB** | `input[id$='txtPC1DOB']` |
| **PC Phone** | `input[id$='txtPCPhone']` |
| **TC First Name** | `input[id$='txtTCFirstName']` |
| **TC Last Name** | `input[id$='txtTCLastName']` |
| **TC DOB** | `input[id$='txtTCDOB']` |
| **Worker** | `select[id$='ddlWorker']` |
| **All Workers** | `input[id$='chkAllWorkers']` |
| **Alternate ID** | `input[id$='txtAlternateID']` |
| **HV Case PK** | `input[id$='txtHVCasePK']` |

### Buttons

| Element | Selector |
|---------|----------|
| **Search Button** | `[id$='btSearch']` |
| **Cancel Button** | `a[id$='btnCancel']` |

### Results Grid

| Element | Selector |
|---------|----------|
| **Results Grid** | `table[id$='grResults']` |
| **Results Rows** | `tbody tr` |
| **No Records Message** | `thead td` |
| **PC1 ID Cell** | `td:nth-child(1)` |
| **PC1 ID Link** | `td:nth-child(1) a` |

---

