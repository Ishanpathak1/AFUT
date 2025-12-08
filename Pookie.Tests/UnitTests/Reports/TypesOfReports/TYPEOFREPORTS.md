# Types of Reports Tests Documentation

## Overview

The **TypesOfReports** test suite contains automated end-to-end tests for all report categories in the application. These tests validate report criteria, filters, PDF generation, and content for each category: **Lists**, **Ticklers**, **Analysis**, **Quarterlies**, **Accreditation**, **Training**, **MIECHV**, and **Retired**.

## What are Types of Reports?

Reports in the application are organized into **8 categories**, each serving different purposes:

| Category | Purpose | Example Reports |
|----------|---------|-----------------|
| **Lists** | List-style reports with tabular data | Case lists, participant lists, worker assignments |
| **Ticklers** | Follow-up reminders and task lists | Upcoming assessments, overdue forms, missing documents |
| **Analysis** | Data analysis and insights | Demographics, outcomes analysis, trends |
| **Quarterlies** | Quarterly reporting | Quarterly summaries, MIECHV quarterly reports |
| **Accreditation** | Accreditation-related reports | Compliance reports, certification status |
| **Training** | Training reports | Staff training completion, certifications |
| **MIECHV** | MIECHV-specific reports | MIECHV forms, MIECHV benchmarks |
| **Retired** | Deprecated/legacy reports | Old reports no longer actively used |

**Common Workflow**:
```
Navigate to Reports Homepage
    ↓
Select Report Category Filter
    ↓
Browse/Search Reports
    ↓
Click Report Name
    ↓
Set Report Criteria
    ├─ Sites (divSites)
    ├─ Case Filters (divCaseFilters)
    ├─ By Whom (divByWhom)
    ├─ Quarters (divQuarters)
    ├─ Programs/Regions (divPrograms)
    └─ Date Ranges
    ↓
Run Report
    ↓
View in DevExpress Viewer
    ↓
Export to PDF
```

---

## Test Files Overview

| File | Tests | Primary Focus |
|------|-------|---------------|
| **Accreditation.cs** | 2 | Accreditation reports criteria & PDF validation |
| **Analysis.cs** | 2 | Analysis reports criteria & PDF validation (includes special handling for Quality Assurance & Demographics Export) |
| **Lists.cs** | 2 | Lists reports criteria & PDF validation |
| **MIECHV.cs** | 2 | MIECHV reports criteria & PDF validation |
| **Quarterlies.cs** | 3 | Quarterlies reports criteria, quarter validation, & PDF validation |
| **Retired.cs** | 2 | Retired reports criteria & PDF validation |
| **Ticklers.cs** | 2 | Ticklers reports criteria & PDF validation |
| **Training.cs** | 2 | Training reports criteria & PDF validation |

**Total Test Files**: 8 (one per category)
**Total Tests**: ~17 across all categories

---

## Report Filter Helper

### ReportFilterHelper.cs

**Location**: `UnitTests/Reports/ReportFilterHelper.cs`

**Purpose**: A comprehensive static helper class providing reusable methods for testing report functionality across ALL report categories.

### Key Responsibilities:

1. **Navigation & Filter Selection**
   - Navigate to Reports homepage
   - Select report category filters
   - Change page size (10, 15, 20, 25, All)

2. **Report Interaction**
   - Click reports by index or name
   - Navigate back to report list

3. **Criteria Testing**
   - Test criteria sections (Sites, Case Filters, By Whom)
   - Test dropdowns, checkboxes, radio buttons
   - Test Quarters section
   - Test Programs/Regions tabbed section
   - Test Tickler Summary "Report to Run" section

4. **Criteria Randomization**
   - `RandomizeReportCriteria()` - Generic randomization for all reports
   - `RandomizeTicklersCriteria()` - Ticklers-specific randomization
   - Mimics real user behavior (sometimes leaves defaults, sometimes changes values)

5. **PDF Generation & Validation**
   - Run reports and wait for DevExpress viewer
   - Export to PDF
   - Validate PDF content matches report name and selected criteria

6. **Validation Testing**
   - Test quarter selection validation
   - Test date range validation (Start > End)

---

## Common Test Pattern

All test files follow a consistent 2-test pattern:

### Test 1: Verify Criteria and Filters (Priority 1)

**Purpose**: Test that all reports in a category have functioning criteria, filters, and UI elements.

**Test Flow**:
1. Navigate to Reports homepage
2. Select category filter (e.g., "Lists", "Ticklers")
3. Change page size to "All"
4. For EACH report in category:
   - Click report name
   - Verify `divCriteria` displayed
   - Test ALL criteria sections:
     - `divSites` (Sites)
     - `divCaseFilters` (Case Filters)
     - `divByWhom` (By Whom)
   - Exercise all dropdowns (select multiple options)
   - Exercise all checkboxes (toggle on/off)
   - Exercise all radio buttons (select each option)
   - Test quarter section (if present)
   - Test Programs/Regions tabs (if present)
   - Test Tickler Summary section (if present for Ticklers)
   - Navigate back to list

**Uses**: `ReportFilterHelper.TestReportFilterCategoryComplete()`

**Example**:

```csharp
[Fact]
[TestPriority(1)]
public void VerifyListsReportCriteriaAndFilters()
{
    using var driver = _driverFactory.CreateDriver();
    
    // Tests all Lists reports' criteria and filters
    ReportFilterHelper.TestReportFilterCategoryComplete(driver, _config, _output, "Lists");
}
```

---

### Test 2: Verify PDF Content Matches Selected Criteria (Priority 2)

**Purpose**: Test that generated PDFs contain correct report names and match user-selected criteria.

**Test Flow**:
1. Set up download directory
2. Navigate to Reports homepage
3. Select category filter
4. Change page size to "All"
5. For EACH report in category:
   - Click report name
   - **Randomize criteria** (like a real user)
   - Track what was selected
   - Click "Run Report"
   - Wait for DevExpress viewer to load (up to 90s)
   - Click "Export to PDF"
   - Wait for PDF download (up to 30s)
   - **Validate PDF**:
     - Report name appears in PDF heading/content
     - Filename matches report name
     - Selected criteria values appear in PDF
   - Navigate back to list
6. Clean up download directory

**Uses**:
- `ReportFilterHelper.ClickReportAtIndex()`
- `ReportFilterHelper.RandomizeReportCriteria()` or `RandomizeTicklersCriteria()`
- `ReportFilterHelper.RunReport()`
- `ReportFilterHelper.ExportReportToPdf()`
- `ReportFilterHelper.ValidatePdfContent()`

**Example**:

```csharp
[Fact]
[TestPriority(2)]
public void VerifyListsReportPdfContentMatchesSelectedCriteria()
{
    var downloadDir = Path.Combine(Path.GetTempPath(), "PookieTestDownloads", Guid.NewGuid().ToString());
    Directory.CreateDirectory(downloadDir);

    try
    {
        using var driver = _driverFactory.CreateDriver(downloadDir);
        
        // Clean up old PDFs
        PdfTestHelper.CleanupOldPdfs(downloadDir, olderThanHours: 0);
        
        // Navigate and select filter
        var homePage = CommonTestHelper.NavigateToReportsHomePage(driver, _config, _output);
        ReportFilterHelper.SelectReportFilter(driver, _output, "Lists");
        ReportFilterHelper.ChangePageSizeToAll(driver, _output);
        
        var reportCount = ReportFilterHelper.GetReportCount(driver, _output);
        var random = new Random();
        
        // Test each report
        for (int i = 0; i < reportCount; i++)
        {
            var reportName = ReportFilterHelper.ClickReportAtIndex(driver, _output, "Lists", i);
            var selectedCriteria = ReportFilterHelper.RandomizeReportCriteria(driver, _output, random);
            
            ReportFilterHelper.RunReport(driver, _output, timeoutSeconds: 60);
            var pdfFile = ReportFilterHelper.ExportReportToPdf(driver, _output, downloadDir, timeoutSeconds: 30);
            ReportFilterHelper.ValidatePdfContent(pdfFile, reportName, selectedCriteria, _output);
            
            if (i < reportCount - 1)
            {
                ReportFilterHelper.NavigateBackToReportList(driver, _output);
            }
        }
    }
    finally
    {
        // Cleanup
        if (Directory.Exists(downloadDir))
        {
            Directory.Delete(downloadDir, recursive: true);
        }
    }
}
```

---

## Special Test Cases

### Quarterlies: Quarter Selection Validation Test

**Additional Test**: `VerifyQuarterSelectionValidation()` (Priority 2)

**Purpose**: Verify that Quarterly reports require quarter selection before running.

**Test Flow**:
1. Navigate to Quarterlies reports
2. Click first report
3. Attempt to run report without selecting quarter
4. Verify validation message appears
5. Verify date fields are disabled initially

**Uses**: `ReportFilterHelper.TestQuarterValidation()`

---

### Analysis: Special Report Handling

**Quality Assurance Report**:
- Special behavior: Navigates to `QASummary.aspx` instead of report viewer
- Test validates navigation instead of PDF

**Demographics Export Report**:
- Skipped in PDF validation test (takes too long or has issues)

---

## ReportFilterHelper Methods Reference

### Navigation Methods

#### TestReportFilterCategoryComplete()
**Purpose**: Complete test flow for a report category.

**Parameters**:
- `driver`: Web driver
- `config`: App configuration
- `output`: Test output helper
- `filterName`: Category name (e.g., "Lists", "Ticklers")

**Flow**:
1. Navigate to Reports homepage
2. Select filter
3. Change page size to "All"
4. For each report:
   - Click report
   - Verify divCriteria
   - Test all criteria sections
   - Test quarters (if present)
   - Test Programs/Regions (if present)
   - Test Tickler Summary (if present)
   - Navigate back

---

#### SelectReportFilter()
**Purpose**: Select a report category filter.

**Example**: `SelectReportFilter(driver, output, "Lists")`

---

#### ChangePageSizeToAll()
**Purpose**: Change DataTables page size to "All" to show all reports.

---

#### NavigateBackToReportList()
**Purpose**: Navigate back from report criteria page to report list.

**Strategy**:
1. Look for back button/link with href containing "ReportCatalog"
2. If not found, use browser back

---

### Report Selection Methods

#### ClickReportAtIndex()
**Purpose**: Click a report at specific index and return report name.

**Parameters**:
- `index`: Zero-based index

**Returns**: Report name string

---

#### ClickFirstReport()
**Purpose**: Click the first visible report in the filtered list.

**Returns**: Report name string

---

#### GetReportCount()
**Purpose**: Get count of visible reports in current filtered list.

**Returns**: Integer count

---

### Criteria Testing Methods

#### VerifyDivCriteriaDisplayed()
**Purpose**: Verify `div#divCriteria` is displayed after clicking report.

---

#### TestReportCriteriaSections()
**Purpose**: Test all three main criteria sections: Sites, Case Filters, By Whom.

---

#### TestReportCriteriaSectionsWithLogging()
**Purpose**: Test criteria sections and return list of which sections are present.

**Returns**: `List<string>` of section names (e.g., ["Sites", "Case Filters"])

---

#### TestCriteriaSection()
**Purpose**: Test a specific criteria section by validating all UI elements within it.

**Parameters**:
- `divId`: Section ID (e.g., "divSites")
- `sectionDisplayName`: Display name for logging

**Actions**:
- Tests all dropdowns
- Tests all checkboxes
- Tests all radio buttons

---

#### TestQuarterCriteriaSection()
**Purpose**: Test the Quarters section (divQuarters) if present.

**Actions**:
- Finds quarter dropdown
- Exercises dropdown options

---

#### TestProgramsAndRegionsSection()
**Purpose**: Test Programs/Regions tabbed section (divPrograms) if present.

**Actions**:
- Click Programs tab → exercise checkboxes
- Click Regions tab → exercise checkboxes

---

#### TestTicklerSummaryReportToRunSection()
**Purpose**: Test Tickler Summary "Report to Run" radio section (divTSumReports) if present.

**Actions**:
- Find all radio buttons
- Select each option
- Restore original selection

---

### UI Element Testing Methods

#### TestDropdownFunctionality()
**Purpose**: Test dropdown by selecting multiple options.

**Actions**:
1. Select up to 5 options
2. Verify selection
3. Restore original selection

---

#### TestCheckboxFunctionality()
**Purpose**: Test checkbox by toggling on/off.

**Actions**:
1. Toggle ON
2. Verify checked
3. Toggle OFF
4. Verify unchecked
5. Restore original state

---

#### TestRadioButtonFunctionality()
**Purpose**: Test radio button groups and associated dropdowns.

**Actions**:
1. Select each radio option (up to 3 per group)
2. Test any dropdowns that become visible
3. Restore original selection

---

### Validation Methods

#### VerifyStartDateField()
**Purpose**: Verify Start Date field is present in criteria (optional check).

**Selectors Tried**:
- `div#divStartDate input.StartDate`
- `div#divQuarters input[id$='txtQtrStartDate']`
- Any input with `StartDate` in ID

**Logging**: Logs PASS if found, WARN if not found (non-fatal)

---

#### TestQuarterValidation()
**Purpose**: Test quarter selection validation.

**Actions**:
1. Find quarter section
2. Verify date fields are disabled initially
3. Try to run report without selecting quarter
4. Verify validation message appears

---

#### TestDateValidation()
**Purpose**: Test date range validation (Start > End).

**Actions**:
1. Set End Date to earlier date (e.g., 60 days ago)
2. Set Start Date to later date (e.g., 30 days ago)
3. Verify validation message: "Start Date cannot be after the End Date"

**Returns**: `bool` - true if validation appeared as expected

---

### Criteria Randomization Methods

#### RandomizeReportCriteria()
**Purpose**: **Generic** randomization for ANY report category.

**Strategy**:
- 15% chance to leave all defaults (unless quarter required)
- 75% chance to change each dropdown
- 75% chance to change each date field
- 70% chance to toggle each checkbox (max 3)
- 70% chance to change each radio group

**Special Handling**:
- **Quarter Section**: If present, 70% select quarter, 30% select date range
- **Date Pairs**: If exactly 2 date fields, treats as start/end pair within 90 days
- **Start/End Detection**: Looks for fields with "Start" or "End" in ID/name

**Returns**: `SelectedReportCriteria` object with tracked selections

---

#### RandomizeTicklersCriteria()
**Purpose**: **Ticklers-specific** randomization.

**Strategy**:
- 50% chance to leave all defaults
- Otherwise, randomly change:
  - Program dropdown (50% chance)
  - Status dropdown (50% chance)
  - Date fields (50% chance each, up to 2)

**Returns**: `SelectedReportCriteria` object with tracked selections

---

### PDF Testing Methods

#### RunReport()
**Purpose**: Click "Run Report" button and wait for DevExpress viewer to load.

**Parameters**:
- `timeoutSeconds`: Max wait time (default 60s)

**Flow**:
1. Find and click "Run Report" button
2. Wait for viewer element: `img.dxrd-pointer-events-none` or `div.dxrd-preview-wrapper`
3. Wait for toolbar to be visible (up to 90s)
4. Detect error pages and throw exception if found
5. Log progress every 6 seconds

**Error Detection**: Detects redirects to error pages and captures error messages

---

#### ExportReportToPdf()
**Purpose**: Export report to PDF from DevExpress viewer.

**Parameters**:
- `downloadDirectory`: Where PDFs are saved
- `timeoutSeconds`: Max wait for download (default 30s)

**Flow**:
1. Wait for export toolbar to be visible
2. Click export button: `div.dxrd-preview-export-to`
3. Click PDF option in menu
4. Click Export confirmation button (if present)
5. Wait for PDF file to appear in download directory

**Returns**: `FileInfo` of downloaded PDF file

**Selectors**:
- Export button: `div.dxrd-preview-export-to`, `div.dxrd-preview-export-toolbar-item`
- PDF option: `div[title*='PDF']`, `div.dx-item[title*='PDF']`
- Confirm button: `button` or `div` containing text "Export"

---

#### ValidatePdfContent()
**Purpose**: Validate PDF heading, filename, and selected criteria.

**Parameters**:
- `pdfFile`: Downloaded PDF file
- `expectedReportName`: Report name to find in PDF
- `selectedCriteria`: Criteria that were selected

**Validations**:
1. **Report Name in PDF**:
   - Check heading (first 10 lines)
   - Check full PDF text
   - Try partial match (significant words)
   - Fallback: Check filename

2. **Filename Matches Report Name**:
   - Exact match
   - Partial match (significant words)

3. **Selected Criteria in PDF**:
   - For each selected criterion, verify value appears in PDF
   - For dates, try multiple formats (MM/dd/yyyy, M/d/yy, MMM d, yyyy, etc.)
   - For non-dates, try partial match (significant words)

**Uses**: `PdfTestHelper.ExtractFullPdfText()`, `PdfTestHelper.ExtractPdfHeading()`

---

### Utility Methods

#### GetVisibleRows()
**Purpose**: Get rows currently visible (not hidden by DataTables).

**Logic**: Filters out rows with `display: none` CSS

---

#### IsSectionPresent()
**Purpose**: Check if a criteria section is present and displayed.

**Returns**: `bool`

---

### SelectedReportCriteria Class

**Purpose**: Data structure to track selected criteria for PDF validation.

**Properties**:
- `Criteria`: `Dictionary<string, string>` of criterion name to value
- `HasCriteria`: `bool` - true if any criteria were selected

**Methods**:
- `Add(string criteriaName, string value)`: Add a selected criterion

**Usage Example**:

```csharp
var selectedCriteria = new SelectedReportCriteria();
selectedCriteria.Add("Program", "MIECHV");
selectedCriteria.Add("Start Date", "01/15/2024");
selectedCriteria.Add("Status", "Active");
```

---

## Important Concepts

### 1. Report Criteria Sections

Reports can have multiple criteria sections:

| Section ID | Display Name | Contains |
|------------|--------------|----------|
| **divSites** | Sites | Site selection dropdowns, checkboxes |
| **divCaseFilters** | Case Filters | Case-level filters (status, type, etc.) |
| **divByWhom** | By Whom | Worker selection (radio + dropdowns) |
| **divQuarters** | Quarters | Quarter dropdown OR date range fields |
| **divPrograms** | Programs/Regions | Tabbed interface with Programs and Regions checkboxes |
| **divTSumReports** | Tickler Summary Report to Run | Radio buttons for report options (Ticklers only) |

---

### 2. DevExpress Report Viewer

**What It Is**: Third-party component for displaying reports.

**Load Sequence**:
1. Click "Run Report" button
2. Page transitions to report viewer
3. Viewer skeleton loads (images, containers)
4. **Report generates on server** (can take 5-90 seconds)
5. Toolbar becomes visible ← **This is the key indicator**
6. Report content displays

**Why Toolbar Wait?**: The toolbar doesn't become visible until the report is fully generated. Tests wait for toolbar visibility as the signal that generation is complete.

---

### 3. Criteria Randomization Strategy

**Purpose**: Mimic real user behavior - sometimes use defaults, sometimes change values.

**Randomization Rates**:
- **Leave All Defaults**: 15% chance (0% if quarter required)
- **Change Dropdown**: 75% chance each
- **Change Date Field**: 75% chance each
- **Toggle Checkbox**: 70% chance each (max 3 checkboxes)
- **Change Radio Group**: 70% chance each

**Quarter Special Case**:
- 70% chance: Select quarter from dropdown
- 30% chance: Select date range radio + enter dates

**Why Randomize?**: 
1. Tests real user patterns
2. Covers different criteria combinations
3. Validates that ANY criteria selection produces valid PDF

---

### 4. PDF Validation Strategy

**3-Level Validation**:

1. **Report Name Validation**:
   - **Level 1**: Exact match in heading/full text
   - **Level 2**: Partial match (significant words)
   - **Level 3**: Filename contains report name

2. **Filename Validation**:
   - **Level 1**: Exact match
   - **Level 2**: Partial match (significant words)

3. **Criteria Validation** (if criteria were changed):
   - **For Dates**: Try multiple formats
   - **For Text**: Exact match or partial match (significant words)

**Fuzzy Matching**: Uses "significant words" (> 3 characters) to allow for slight differences in report names, titles, and PDF text.

---

### 5. Quarter Selection Logic

**Quarter Section Components**:
- **Radio Button**: "Quarter" option
- **Quarter Dropdown**: List of quarters (Q1 2023, Q2 2023, etc.)
- **Radio Button**: "Date Range" option
- **Start Date Field**: Enabled when "Date Range" selected
- **End Date Field**: Enabled when "Date Range" selected

**Validation**:
- Date fields are **disabled** when Quarter radio is selected
- Clicking "Run Report" without selecting quarter/date range shows validation error

**Test Coverage**:
- `TestQuarterValidation()`: Tests validation
- `RandomizeReportCriteria()`: Randomly selects quarter or date range

---

### 6. Error Detection & Handling

**Error Page Detection**: `RunReport()` detects server error pages:
- Checks URL for `/errorpage.aspx` or `/error`
- Checks page title for "Error" or "Exception"
- Extracts and logs error messages from page

**PDF Export Errors**: `ExportReportToPdf()` handles:
- Export button not visible (waits up to 30s)
- PDF download timeout (30s)
- Missing PDF file

**Navigation Errors**: `NavigateBackToReportList()` handles:
- Missing back button (falls back to browser back)
- Stale element exceptions

---

## What to Keep in Mind

### When Modifying Tests

1. **All Categories Use Same Pattern**: If you add a feature to one category test, consider adding to all categories.

2. **Helper Methods Are Shared**: Changes to `ReportFilterHelper` affect ALL category tests.

3. **PDF Validation is Fuzzy**: Don't expect exact matches - validation uses partial matching for flexibility.

4. **Randomization is Intentional**: Tests are supposed to run differently each time to cover various scenarios.

5. **Download Directory Cleanup**: Always use try-finally to ensure cleanup even if test fails.

6. **Report Generation Can Be Slow**: 90-second timeout is necessary for complex reports.

---

### When Adding New Tests

1. **Follow the 2-Test Pattern**: Criteria/filters test + PDF validation test.

2. **Use ReportFilterHelper**: Don't duplicate code - add to helper if needed.

3. **Test Priority Order**: Priority 1 for criteria, Priority 2 for PDF validation.

4. **Handle Special Cases**: Some reports may need special handling (like Quality Assurance, Demographics Export).

5. **Log Extensively**: Use `[INFO]`, `[PASS]`, `[WARN]`, `[ERROR]` prefixes.

6. **Clean Up Resources**: Always clean up download directories.

---

### Common Pitfalls

1. **Timeout Too Short for Report Generation**: Some reports take 60+ seconds.
   - **Solution**: Use 90-second timeout in `RunReport()`.

2. **Export Button Not Visible Yet**: Toolbar may not be visible immediately.
   - **Solution**: `ExportReportToPdf()` waits up to 30s for button.

3. **PDF Text Doesn't Match Exactly**: Report names in PDF may differ slightly.
   - **Solution**: `ValidatePdfContent()` uses fuzzy matching.

4. **Quarter Validation Fails**: Some reports don't have quarter section.
   - **Solution**: `TestQuarterValidation()` checks if section exists first.

5. **Date Fields Disabled**: When quarter is selected, date fields are disabled.
   - **Solution**: `RandomizeReportCriteria()` handles this logic.

6. **Navigation Back Fails**: Back button may not exist or be stale.
   - **Solution**: `NavigateBackToReportList()` falls back to browser back.

7. **PDF Download Timeout**: Large PDFs or slow network.
   - **Solution**: Increase timeout or use faster test environment.

8. **Error Page Not Detected**: Report generation fails silently.
   - **Solution**: `RunReport()` checks for error pages and logs errors.

---

## Test Data Requirements

### Prerequisites
- Test user with access to Reports module
- Reports exist in each category
- DevExpress report viewer configured
- PDF generation functional

### Test Creates
- Temporary download directories
- PDF files (cleaned up after test)

### Test Modifies
- None (read-only tests)

### Test Deletes
- Temporary download directories and PDFs

**Net Impact**: All tests are read-only and clean up after themselves.

---

## Running the Tests

### Run All TypesOfReports Tests
```bash
dotnet test --filter "FullyQualifiedName~TypesOfReports"
```

### Run Specific Category
```bash
# Lists
dotnet test --filter "FullyQualifiedName~ListsTests"

# Ticklers
dotnet test --filter "FullyQualifiedName~TicklersTests"

# Analysis
dotnet test --filter "FullyQualifiedName~AnalysisTests"

# Quarterlies
dotnet test --filter "FullyQualifiedName~QuarterliesTests"

# Accreditation
dotnet test --filter "FullyQualifiedName~AccreditationTests"

# Training
dotnet test --filter "FullyQualifiedName~TrainingTests"

# MIECHV
dotnet test --filter "FullyQualifiedName~MIECHVTests"

# Retired
dotnet test --filter "FullyQualifiedName~RetiredTests"
```

### Run Specific Test Type
```bash
# Criteria and filters only
dotnet test --filter "FullyQualifiedName~ReportCriteriaAndFilters"

# PDF validation only
dotnet test --filter "FullyQualifiedName~PdfContentMatchesSelectedCriteria"
```

---

## Troubleshooting

### Test Fails: "Report viewer did not load"
- **Issue**: Report generation timed out or server error
- **Solution**: 
  - Check test output for error page detection
  - Increase timeout from 60s to 90s
  - Verify report works manually in application

### Test Fails: "Export button not visible"
- **Issue**: Toolbar hasn't loaded yet or report still generating
- **Solution**:
  - Test waits up to 30s - may need to increase
  - Check that report actually generated (look for toolbar)

### Test Fails: "PDF file was not downloaded"
- **Issue**: Download directory issue or PDF export failed
- **Solution**:
  - Verify download directory path is valid
  - Check browser download settings
  - Verify PDF export works manually

### Test Fails: "Expected report name not found in PDF"
- **Issue**: PDF text differs from report name
- **Solution**:
  - This is a warning, not error - check logs for partial match
  - Validation uses fuzzy matching, so some mismatch is OK
  - Manually verify PDF content if concerned

### Test Fails: "Validation message did not appear"
- **Issue**: Validation logic changed or selector incorrect
- **Solution**:
  - Check validation message selectors in helper
  - Verify validation still works manually
  - Update selectors if UI changed

### Test Fails: Quarter validation test
- **Issue**: Report doesn't have quarter section
- **Solution**: This is expected - test checks if section exists first

### Test Fails: "Could not clean up download directory"
- **Issue**: Files still in use or permission issue
- **Solution**:
  - This is a warning, not error
  - Files will be cleaned up on next test run
  - Manually delete if concerned

---

## Selectors Reference

### Report Catalog

| Element | Selector |
|---------|----------|
| **Filter Label** | `span.FilterLabel` |
| **Reports Table** | `table#mainTable tbody` |
| **Page Size Dropdown** | `select[name='mainTable_length']` |
| **Report Name Cell** | `tr td:nth-child(2)` (second column) |

### Criteria Sections

| Element | Selector |
|---------|----------|
| **Criteria Container** | `div#divCriteria` |
| **Sites Section** | `div#divSites` |
| **Case Filters Section** | `div#divCaseFilters` |
| **By Whom Section** | `div#divByWhom` |
| **Quarters Section** | `div#divQuarters` |
| **Programs/Regions Section** | `div#divPrograms` |
| **Tickler Summary Section** | `fieldset#divTSumReports` |

### UI Elements

| Element | Selector |
|---------|----------|
| **Dropdown** | `select.form-control`, `select[id*='ddl']` |
| **Checkbox** | `input[type='checkbox']` |
| **Radio Button** | `input[type='radio']` |
| **Date Field** | `input[type='text'][id*='Date']` |
| **Run Report Button** | `a.btn.btn-primary[id$='btnRunReport']` |

### DevExpress Viewer

| Element | Selector |
|---------|----------|
| **Viewer Container** | `div.dxrd-preview-wrapper` |
| **Toolbar** | `div.dxrd-toolbar` |
| **Export Button** | `div.dxrd-preview-export-to` |
| **PDF Option** | `div[title*='PDF']` |
| **Export Confirm** | `button:contains('Export')` |

### Validation

| Element | Selector |
|---------|----------|
| **Validation Message** | `span.Error[style*='color: red']` |
| **Start Date Validation** | `span[id*='ValStartdate']` |

---

