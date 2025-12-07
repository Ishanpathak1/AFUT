# Reports Home Page Tests Documentation

## Overview

The Reports test suite contains automated end-to-end tests for the Reports module, specifically the Reports Catalog homepage. These tests validate hover tooltips, filter functionality, report information icons, and documentation links for all available reports in the system.

## What is the Reports Module?

**Reports** is a centralized catalog of all available reports in the application. The Reports homepage provides a searchable, filterable interface for users to discover, understand, and access various reports.

**Purpose**:
- Browse all available reports by category
- View report descriptions and documentation
- Track report usage (recent and frequent)
- Filter reports by category (Lists, Ticklers, Analysis, etc.)
- Access PDF documentation for each report
- Understand report purpose through tooltips and info icons

**Report Categories**:
- **All**: All reports (default)
- **Lists**: List-style reports
- **Ticklers**: Reminder/follow-up reports
- **Analysis**: Data analysis reports
- **Quarterlies**: Quarterly reporting
- **Accreditation**: Accreditation-related reports
- **Training**: Training reports
- **MIECHV**: MIECHV-specific reports
- **Retired**: Deprecated reports

---

## Test File Overview

| Folder | File | Test Count | Primary Focus |
|--------|------|------------|---------------|
| **HomePage** | **ReportsHomePageTests.cs** | 4 | Hover tooltips, filters, info icons, documentation links |

**Total Tests**: 4

**Test Priorities**: Tests are ordered 1-4 using `[TestPriority]` attribute for sequential execution.

---

## Test Execution Order

Tests run in priority order:

1. **Priority 1**: Verify all Reports Catalog hover elements (13 tooltips)
2. **Priority 2**: Verify all report info icon tooltips
3. **Priority 3**: Verify Report Catalog filter functionality (9 filters)
4. **Priority 4**: Verify report documentation links clickable

---

## Detailed Test Breakdown

### Test 1: VerifyAllReportsCatalogHoverElements (Priority 1)

**Purpose**: Verify that all filter labels and column labels have correct hover tooltips.

**Test Flow**:

#### Part 1: Navigate to Reports Homepage
1. Sign in and navigate to Reports homepage
2. Wait for report catalog header div: `#divReportCatalogHeader`
3. Assert header is present

#### Part 2: Test 9 Filter Label Tooltips
4. For each filter label (All, Lists, Ticklers, Analysis, Quarterlies, Accreditation, Training, MIECHV, Retired):
   - Find filter label element: `span.FilterLabel`
   - Get tooltip from `data-original-title` attribute
   - Extract description from HTML-encoded tooltip
   - Assert tooltip contains expected description
   - Hover over element (500ms)
   - Move away from element

**Filter Label Tooltips**:

| Filter | Expected Tooltip |
|--------|------------------|
| **All** | "Reset any filters and display all reports" |
| **Lists** | "Display only reports that are part of the list category" |
| **Ticklers** | "Display only reports that are part of the ticklers category" |
| **Analysis** | "Display only reports that are part of the analysis category" |
| **Quarterlies** | "Display only reports that are part of the quarterlies category" |
| **Accreditation** | "Display only reports that are part of the accreditation category" |
| **Training** | "Display only reports that are part of the training category" |
| **MIECHV** | "Display only reports that are part of the MIECHV category" |
| **Retired** | "No longer applicable or replaced by better versions" |

#### Part 3: Test 4 Column Label Tooltips
5. For each column label (Recent-You, Recent-All, Frequent-You, Frequent-All):
   - Find column label element: `span.ColumnLabel`
   - Get tooltip from `data-original-title` attribute
   - Extract description from HTML-encoded tooltip
   - Assert tooltip contains expected description
   - Hover over element (500ms)
   - Move away from element

**Column Label Tooltips**:

| Column | Expected Tooltip |
|--------|------------------|
| **Recent-You** | "Display the date the reports were last run by you" |
| **Recent-All** | "Display the date the reports were last run by anyone in your program" |
| **Frequent-You** | "Display the frequency rank that the reports were run by you" |
| **Frequent-All** | "Display the frequency rank that the reports were run by anyone in your program" |

#### Part 4: Verify All Tooltips Passed
6. Assert all 13 tooltips verified successfully

**Total Tooltips Tested**: 9 filters + 4 columns = **13 tooltips**

**Tooltip HTML Format**: `&lt;b&gt;Title&lt;/b&gt;&lt;hr&gt;Description`

**Tooltip Extraction**: Split by `<hr>` or `&lt;hr&gt;` and take second part (description).

---

### Test 2: VerifyAllReportInfoIconTooltips (Priority 2)

**Purpose**: Verify that every report row has an info icon with a valid `data-description` attribute.

**Test Flow**:

#### Part 1: Navigate and Load Reports Table
1. Navigate to Reports homepage
2. Wait for reports table: `table tbody`
3. Assert table loaded

#### Part 2: Find All Info Icons
4. Find all info icons: `a.moreInfo.btn.btn-xs` containing `span.glyphicon-info-sign`
5. Filter to only displayed icons
6. Log count of info icons found
7. Assert at least 1 info icon found

#### Part 3: Validate Each Info Icon
8. For each info icon:
   - Get `data-description` attribute
   - Assert description is not null or whitespace
   - Hover over info icon (300ms)
   - Find report name in same row (2nd `td` element)
   - Log report name (first 50 characters)
   - Move away from icon
   - Increment success count

#### Part 4: Verify All Icons Validated
9. Log total success count
10. Assert all info icons have valid descriptions

**Info Icon Selector**: `a.moreInfo.btn.btn-xs span.glyphicon-info-sign`

**Data Attribute**: `data-description` contains full report description.

**Use Case**: Ensures every report has informative tooltip for users.

---

### Test 3: VerifyReportCatalogFilterFunctionality (Priority 3)

**Purpose**: Verify that each filter button correctly filters the reports table and that page size changes work.

**Test Flow**:

#### Part 1: Navigate and Load Reports Table
1. Navigate to Reports homepage
2. Wait for reports table: `table tbody`
3. Assert table loaded

#### Part 2: Test Each Filter
4. For each filter (All, Lists, Ticklers, Analysis, Quarterlies, Accreditation, Training, MIECHV, Retired):
   - **Test Filter Functionality** (see detailed steps below)

#### Part 3: Verify All Filters Tested
5. Assert all filter functionality tests completed successfully

---

### TestFilterFunctionality() Helper (Used by Test 3)

**Purpose**: Test a single filter's functionality.

**Detailed Steps**:

#### Step 1: Click Filter
1. Find filter label: `span.FilterLabel` with matching text
2. Click filter using `CommonTestHelper.ClickElement()`
3. Wait for update panel (10s)
4. Wait for page ready (10s)
5. Sleep 1500ms for table refresh

#### Step 2: Get Filtered Results
6. Wait for reports table: `table#mainTable tbody`
7. Find all rows with `td` elements (data rows)
8. Get visible rows (not hidden by DataTables with `display: none`)
9. Count all info icons in all rows

#### Step 3: Validate Filter Results
10. Assert at least 1 row returned
11. Assert at least 1 info icon found
12. Assert row count equals info icon count (1 icon per row)

#### Step 4: Validate Each Row
13. For each row:
    - Find info icon: `a.moreInfo.btn.btn-xs` containing `span.glyphicon-info-sign`
    - Assert info icon exists
    - Get `data-description` attribute
    - Assert description is not null/whitespace
    - Increment validated count

#### Step 5: Test Page Size Changes (for "All" filter only)
14. If filter is "All" AND total rows > 25:
    - **Test Page Size Changes** (see detailed steps below)

#### Step 6: Log Results
15. Log filter name, total rows, visible rows, and validation status

---

### TestPageSizeChanges() Helper (Used by Test 3)

**Purpose**: Test that DataTables page size dropdown correctly changes visible row count.

**Detailed Steps**:

#### Part 1: Find Page Size Dropdown
1. Find page size dropdown: `select[name='mainTable_length']`
2. Create `SelectElement` wrapper

#### Part 2: Test Each Page Size
3. For each page size (10, 15, 20, 25):
   - Skip if page size > total rows
   - Select page size by value
   - Wait for ready (2s)
   - Sleep 500ms
   - Get visible rows (not hidden by DataTables)
   - Calculate expected visible: `Math.Min(pageSize, totalRows)`
   - Assert visible count equals expected
   - Log page size and visible count

#### Part 3: Test "All" Option
4. Select page size "-1" (All)
5. Wait for ready (2s)
6. Sleep 500ms
7. Get visible rows
8. Assert visible count equals total rows
9. Log "All" option results

#### Part 4: Reset to Default
10. Select page size "10" (reset to default)
11. Wait for ready (2s)
12. Sleep 300ms

**Page Size Options**: 10, 15, 20, 25, -1 (All)

**DataTables Behavior**: Hides rows with `style="display: none"` CSS.

---

### Test 4: VerifyReportDocumentationLinksClickable (Priority 4)

**Purpose**: Verify that all report documentation links (PDF files) are clickable and functional.

**Test Flow**:

#### Part 1: Navigate and Load Reports Table
1. Navigate to Reports homepage
2. Wait for reports table: `table#mainTable tbody`
3. Assert table loaded

#### Part 2: Set Page Size to "All"
4. Find page size dropdown: `select[name='mainTable_length']`
5. Get current page size
6. If not "-1" (All):
   - Select "-1" (All)
   - Wait for ready (3s)
   - Sleep 1000ms
   - Log page size change

#### Part 3: Count Documentation Links
7. Find all doc links: `a.viewReportDoc.btn.btn-xs` with:
   - Non-empty `data-filename` attribute
   - Child element `span.glyphicon-file`
8. Count links with docs (visible)
9. Count links without docs (hidden with `display: none`)
10. Log total reports, reports with docs, reports without docs
11. Assert at least 1 doc link found

#### Part 4: Test Each Documentation Link
12. For each documentation link (by index):
    - **Re-find all doc links** (avoid stale element reference)
    - Get doc link at current index
    - **Test Report Documentation Link** (see detailed steps below)

#### Part 5: Verify All Links Tested
13. Log total documentation links tested successfully

**Doc Link Selector**: `a.viewReportDoc.btn.btn-xs`

**File Icon**: `span.glyphicon-file`

**Why Re-find**: Avoid `StaleElementReferenceException` after page interactions.

---

### TestReportDocumentationLink() Helper (Used by Test 4)

**Purpose**: Test a single report documentation link.

**Detailed Steps**:

#### Step 1: Get Report Information
1. Find parent row of doc link: `./ancestor::tr`
2. Get report name from 2nd `td` in row (or "Unknown Report")
3. Get filename from `data-filename` attribute
4. Get description from `data-description` attribute
5. Get link ID from `id` attribute
6. Log current index, total count, and report name
7. Log filename

#### Step 2: Validate Report Name Matches Filename
8. Call `ValidateReportNameMatchesFilename()` helper
9. If match found:
   - Log "[MATCH] Pattern match: Report name matches filename"
   - Log matched keywords
10. If no match:
    - Log "[WARN] Pattern mismatch: Report name may not match filename"
    - Log report name and filename

#### Step 3: Store Original Window State
11. Store current window handle
12. Store current URL
13. Store current window count

#### Step 4: Click Documentation Link
14. Try:
    - Click doc link using `CommonTestHelper.ClickElement()`
    - Sleep 2000ms for action to complete
    - Get new window count
    - Get current URL

#### Step 5: Detect Click Result
15. **If new window opened** (window count increased):
    - Log "[DETECTED] New window/tab opened"
    - Switch to new window (last handle)
    - Get new window URL
    - Log new window URL
    - Close new window
    - Switch back to original window

16. **Else if URL changed** (different URL, same window):
    - Log "[DETECTED] URL changed in same window"
    - Log new URL
    - If URL doesn't contain "ReportCatalog.aspx":
      - Navigate back
      - Wait for ready (5s)
      - Sleep 1000ms

17. **Else** (no window change, no URL change):
    - Log "[DETECTED] PostBack/Download triggered"

#### Step 6: Log Success
18. Log "[PASS] Documentation link verified"

#### Step 7: Handle Exceptions
19. Catch any exception:
    - Log "[WARN] Exception: {message}"
    - Ensure back on original window
    - Don't fail test (just log and continue)

**Click Behaviors**:
1. **New Window/Tab**: Opens PDF in new window
2. **URL Change**: Navigates to PDF in same window
3. **PostBack/Download**: Triggers file download or AJAX action

**Error Handling**: Graceful - logs warnings but doesn't fail entire test.

---

## Helper Methods Summary

### Navigation Helpers

#### NavigateToReportsHomePage()
Signs in and navigates to Reports homepage.

**Uses**: `CommonTestHelper.NavigateToReportsHomePage(driver, _config, _output)`

**Assertions**:
- HomePage is not null
- HomePage is loaded
- Logs success with current URL

---

### Tooltip Helpers

#### VerifyHoverTooltip()
Tests hover functionality and verifies tooltip content for a label.

**Parameters**:
- `cssSelector`: CSS selector for element (e.g., "span.FilterLabel")
- `labelText`: Text content of label (e.g., "All")
- `expectedTooltip`: Expected tooltip description

**Steps**:
1. Find element by selector and text
2. Get `data-original-title` attribute
3. Extract description using `ExtractTooltipDescription()`
4. Assert tooltip contains expected text
5. Hover over element (500ms)
6. Log label and tooltip text
7. Move away from element (100px offset, 200ms)

**Tooltip Attribute**: `data-original-title` (Bootstrap tooltip)

---

#### ExtractTooltipDescription()
Extracts description text from HTML-encoded tooltip attribute.

**Input Format**: `&lt;b&gt;Title&lt;/b&gt;&lt;hr&gt;Description`

**Output**: `Description`

**Logic**:
1. Split by `<hr>`, `&lt;hr&gt;`, `<hr />`, `&lt;hr /&gt;`, `<br />`, `&lt;br /&gt;`
2. If multiple parts, return 2nd part (description)
3. Otherwise return entire string trimmed

**Use Case**: Bootstrap tooltips encode HTML in `data-original-title`.

---

### Filter Helpers

#### TestFilterFunctionality()
Tests filter functionality by clicking it and verifying filtered results.

**Parameters**: `filterName` (e.g., "All", "Lists", "Ticklers")

**Key Validations**:
- At least 1 row returned
- At least 1 info icon found
- Row count equals info icon count
- Each info icon has `data-description`

**Special Case**: For "All" filter with > 25 rows, also tests page size changes.

---

#### GetVisibleRows()
Gets rows currently visible (not hidden by DataTables pagination).

**Logic**:
1. Find all `tr` elements with `td` children
2. Filter to rows without `display: none` in style attribute
3. Return list of visible row elements

**Use Case**: DataTables hides rows with CSS instead of removing from DOM.

---

#### TestPageSizeChanges()
Tests that changing DataTables page size dropdown updates visible row count.

**Parameters**:
- `reportsTable`: Table element
- `totalRows`: Total row count

**Tests**: 10, 15, 20, 25, -1 (All)

**Validations**: Visible row count matches expected for each page size.

---

### Documentation Link Helpers

#### TestReportDocumentationLink()
Tests a report documentation link by clicking it and logging results.

**Parameters**:
- `docLink`: Link element
- `currentIndex`: Current test index
- `totalCount`: Total link count

**Steps**:
1. Get report info (name, filename, description)
2. Validate report name matches filename
3. Click link
4. Detect result (new window, URL change, or download)
5. Handle cleanup (close window, navigate back)
6. Log results

**Error Handling**: Catches exceptions, logs warnings, continues test.

---

#### ValidateReportNameMatchesFilename()
Validates that report name matches PDF filename.

**Returns**: `(bool IsMatch, string MatchedKeywords)`

**Match Criteria** (any one of):
1. **Keyword Match**: At least 2 significant keywords match
2. **Substring Match**: Report name contains filename or vice versa
3. **Code Match**: Report code found in filename (e.g., "1-1.C", "PHQ9", "ASQ")

**Keyword Extraction**: Excludes common words like "the", "and", "report", "summary", etc.

**Report Code Patterns**:
- `\d+-\d+\.[A-Z]` - e.g., "1-1.C", "7-4.E"
- `PHQ\d+` - e.g., "PHQ9"
- `ASQ-?SE` - e.g., "ASQ-SE"
- `ASQ` - e.g., "ASQ"
- `MIECHV` - e.g., "MIECHV"
- `CHEERS` - e.g., "CHEERS"

---

#### NormalizeForComparison()
Normalizes string for comparison by removing special characters and converting to lowercase.

**Logic**: Keep only alphanumeric and spaces, convert to lowercase.

**Example**:
- Input: "PHQ-9: Depression Screening"
- Output: "phq9 depression screening"

---

#### ExtractKeywords()
Extracts meaningful keywords from text (excludes common words).

**Common Words Excluded**:
- "the", "and", "or", "of", "to", "a", "an", "in", "on", "at", "for", "with", "by", "from"
- "report", "summary", "analysis", "detail", "details", "case", "filter", "site", "options"

**Logic**:
1. Split by spaces
2. Filter to words > 2 characters
3. Exclude common words
4. Return distinct keywords

---

#### ExtractReportCode()
Extracts report code from report name.

**Patterns Matched**:
- `\d+-\d+\.[A-Z]` - Numbered codes like "1-1.C"
- `PHQ\d+` - Depression screening codes like "PHQ9"
- `ASQ-?SE` - ASQ screening codes
- `ASQ` - General ASQ
- `MIECHV` - MIECHV reports
- `CHEERS` - CHEERS reports

**Returns**: First matched code or empty string.

---

## Important Concepts

### 1. Bootstrap Tooltips

**Tooltip Attribute**: `data-original-title`

**HTML Encoding**: Tooltips contain HTML-encoded markup:
```html
&lt;b&gt;Filter Label&lt;/b&gt;&lt;hr&gt;Description text here
```

**Display**: Bootstrap tooltip plugin shows formatted tooltip on hover.

**Testing Strategy**: Extract `data-original-title`, parse HTML encoding, verify description text.

---

### 2. DataTables Pagination

**Library**: jQuery DataTables plugin for table pagination, sorting, filtering.

**Page Size Dropdown**: `select[name='mainTable_length']`

**Page Size Values**:
- `10` - Show 10 rows per page (default)
- `15` - Show 15 rows per page
- `20` - Show 20 rows per page
- `25` - Show 25 rows per page
- `-1` - Show all rows (no pagination)

**Row Hiding**: DataTables hides rows with `style="display: none"` CSS instead of removing from DOM.

**Testing Strategy**: Select page size, count visible rows (without `display: none`), verify count matches expected.

---

### 3. Report Info Icons

**Icon Element**: `a.moreInfo.btn.btn-xs` containing `span.glyphicon-info-sign`

**Data Attribute**: `data-description` contains full report description.

**Purpose**: Provides detailed explanation of report when hovered.

**Testing Strategy**: Verify every report row has info icon with non-empty `data-description`.

---

### 4. Report Documentation Links

**Link Element**: `a.viewReportDoc.btn.btn-xs` with `span.glyphicon-file` icon.

**Data Attributes**:
- `data-filename`: PDF filename (e.g., "PHQ9_Report.pdf")
- `data-description`: Report description (same as info icon)

**Click Behaviors**:
1. **New Window**: Opens PDF in new tab/window
2. **Same Window**: Navigates to PDF in same window
3. **Download**: Triggers file download
4. **PostBack**: Triggers AJAX/server action

**Hidden Links**: Links with `style="display: none"` indicate no documentation available.

---

### 5. Filter Functionality

**Filter Labels**: `span.FilterLabel`

**9 Filters**: All, Lists, Ticklers, Analysis, Quarterlies, Accreditation, Training, MIECHV, Retired

**Click Behavior**: Filters reports table to show only reports in selected category.

**Update Mechanism**: AJAX update panel refreshes table content.

**Validation**: After filter click, verify rows returned and each has valid info icon.

---

### 6. Hover Actions

**Selenium Actions**: `Actions` class for mouse interactions.

**Hover Pattern**:
```csharp
actions.MoveToElement(element).Perform();
Thread.Sleep(500); // Wait for tooltip to appear
```

**Move Away Pattern**:
```csharp
actions.MoveToElement(element).MoveByOffset(100, 0).Perform();
Thread.Sleep(200); // Wait for tooltip to disappear
```

**Use Case**: Trigger Bootstrap tooltips and verify they display correctly.

---

### 7. Stale Element Prevention

**Problem**: After page interactions (filter clicks, page size changes), previously found elements become stale.

**Solution**: Re-find elements before interacting:

```csharp
for (int i = 0; i < totalLinks; i++)
{
    // Re-find all links to avoid stale element references
    var currentLinks = driver.FindElements(By.CssSelector("a.viewReportDoc.btn.btn-xs"))
        .Where(link => /* filters */)
        .ToList();
    
    if (i < currentLinks.Count)
    {
        TestLink(currentLinks[i]);
    }
}
```

**Use Case**: Clicking documentation links may trigger page refreshes or navigation.

---

### 8. Report Name to Filename Matching

**Purpose**: Validate that PDF filenames correspond to report names.

**Matching Strategies**:
1. **Keyword Match**: Extract significant keywords from both, check if >= 2 match
2. **Substring Match**: Check if one contains the other
3. **Code Match**: Extract report code (e.g., "1-1.C"), check if in filename

**Log Result**:
- **[MATCH]**: Pattern matched, log matched keywords
- **[WARN]**: Pattern mismatch, log both name and filename for review

**Purpose**: Quality check to ensure documentation corresponds to correct report.

---

### 9. Window/Tab Management

**Challenge**: Documentation links may open in new window/tab or same window.

**Detection**:
```csharp
var originalWindowCount = driver.WindowHandles.Count;
// Click link
var newWindowCount = driver.WindowHandles.Count;

if (newWindowCount > originalWindowCount)
{
    // New window opened
    var newHandle = driver.WindowHandles.Last();
    driver.SwitchTo().Window(newHandle);
    // ... inspect ...
    driver.Close();
    driver.SwitchTo().Window(originalHandle);
}
```

**Cleanup**: Always close new windows and switch back to original window.

---

## What to Keep in Mind

### When Modifying Tests

1. **Test Order Doesn't Matter**: Unlike other test suites, these tests are independent and don't create/modify data. Priority order is just for logical flow, not dependency.

2. **DataTables Hidden Rows**: Use `GetVisibleRows()` instead of counting all rows - DataTables hides with CSS.

3. **Stale Elements**: Always re-find documentation links before clicking in loop to avoid stale element exceptions.

4. **Window Cleanup**: Always close new windows and switch back to original window after testing documentation links.

5. **Hover Timing**: 500ms hover delay is necessary for Bootstrap tooltips to appear.

6. **Filter Wait Times**: 1500ms sleep after filter click is necessary for table to fully update.

---

### When Adding New Tests

1. **Assign Priority**: Add `[TestPriority(N)]` with number > 4.

2. **Use Navigation Helper**: Start with `NavigateToReportsHomePage(driver)`.

3. **Log Extensively**: Use `_output.WriteLine()` with `[PASS]`, `[INFO]`, `[WARN]` prefixes.

4. **Handle Errors Gracefully**: Wrap in try-catch, log warnings, don't fail entire test.

5. **Wait After Clicks**: Always `WaitForUpdatePanel()`, `WaitForReady()`, and sleep after clicks.

6. **Test New Filter Categories**: If new categories added, update filter list in Test 3.

7. **Test New Columns**: If new column labels added, update column list in Test 1.

---

### Common Pitfalls

1. **Tooltip Not Found**: Element not hovered long enough.
   - **Solution**: Increase hover sleep time to 500ms or more.

2. **Filter Doesn't Work**: Table not updated after click.
   - **Solution**: Increase sleep time after filter click to 1500ms or more.

3. **Visible Row Count Wrong**: Counting all rows instead of visible rows.
   - **Solution**: Use `GetVisibleRows()` helper to filter out `display: none` rows.

4. **Documentation Link Stale**: Element became stale after previous click.
   - **Solution**: Re-find all links at start of each loop iteration.

5. **Window Not Closed**: New window left open, causing subsequent failures.
   - **Solution**: Always close new windows in `try-finally` or ensure cleanup in catch block.

6. **Report Name Mismatch Warning**: Legitimate naming difference, not an error.
   - **Solution**: Review logged warning, ignore if naming difference is acceptable. Pattern matching is fuzzy, not exact.

7. **Page Size Test Skipped**: Total rows <= 25, so page size test not needed.
   - **Solution**: This is expected behavior - page size tests only run for "All" filter with > 25 rows.

---

## Test Data Requirements

### Prerequisites
- Test user with access to Reports module
- Reports catalog with multiple reports
- Reports must have:
  - Info icons with `data-description` attributes
  - Category assignments (Lists, Ticklers, Analysis, etc.)
  - Some reports with PDF documentation
  - Filter labels and column labels with tooltips

### Test Creates
- None (read-only tests)

### Test Modifies
- None (read-only tests)

### Test Deletes
- None (read-only tests)

**Net Impact**: All tests are read-only and don't modify any data.

---

## Running the Tests

### Run All Reports Tests
```bash
dotnet test --filter "FullyQualifiedName~ReportsHomePageTests"
```

### Run Specific Test
```bash
# Hover tooltips (Priority 1)
dotnet test --filter "FullyQualifiedName~ReportsHomePageTests.VerifyAllReportsCatalogHoverElements"

# Info icon tooltips (Priority 2)
dotnet test --filter "FullyQualifiedName~ReportsHomePageTests.VerifyAllReportInfoIconTooltips"

# Filter functionality (Priority 3)
dotnet test --filter "FullyQualifiedName~ReportsHomePageTests.VerifyReportCatalogFilterFunctionality"

# Documentation links (Priority 4)
dotnet test --filter "FullyQualifiedName~ReportsHomePageTests.VerifyReportDocumentationLinksClickable"
```

### Run in Any Order
Tests are independent and can run in any order (unlike data-dependent tests in other modules).

---

## Troubleshooting

### Test Fails: "Report catalog header div was not found"
- **Issue**: Page didn't load or selector changed
- **Solution**: Verify navigation succeeded, check page HTML for header div

### Test Fails: "Label 'All' was not found"
- **Issue**: Filter label selector changed or element not visible
- **Solution**: Verify `span.FilterLabel` selector, check page HTML

### Test Fails: Tooltip doesn't contain expected text
- **Issue**: Tooltip text changed or HTML encoding different
- **Solution**: Update expected tooltip text in test, verify `data-original-title` attribute format

### Test Fails: "No report info icons were found on the page"
- **Issue**: Page didn't load fully or no reports in catalog
- **Solution**: Increase wait time, verify reports exist, check selector

### Test Fails: Filter returns no rows
- **Issue**: No reports in that category or filter not working
- **Solution**: Verify reports exist in category, check filter click succeeded, increase wait time

### Test Fails: Visible row count doesn't match expected
- **Issue**: DataTables pagination not working or page size didn't change
- **Solution**: Verify page size dropdown worked, increase wait time, check `GetVisibleRows()` logic

### Test Fails: Stale element exception on documentation link
- **Issue**: Element not re-found in loop
- **Solution**: Ensure links are re-found at start of each iteration, not reused from previous iteration

### Test Fails: "Success toast was not displayed" or window not closed
- **Issue**: Documentation link click didn't work or cleanup failed
- **Solution**: Check link is clickable, verify window detection logic, ensure cleanup in finally block

---

## Selectors Reference

### Page Structure

| Element | Selector |
|---------|----------|
| **Report Catalog Header** | `#divReportCatalogHeader` |
| **Reports Table** | `table#mainTable tbody` |
| **Page Size Dropdown** | `select[name='mainTable_length']` |

### Filter Labels

| Element | Selector |
|---------|----------|
| **Filter Label** | `span.FilterLabel` |
| **Column Label** | `span.ColumnLabel` |

### Report Rows

| Element | Selector |
|---------|----------|
| **Info Icon Link** | `a.moreInfo.btn.btn-xs` |
| **Info Icon** | `span.glyphicon-info-sign` |
| **Documentation Link** | `a.viewReportDoc.btn.btn-xs` |
| **File Icon** | `span.glyphicon-file` |
| **Data Row** | `tr` (with `td` elements) |

### Attributes

| Attribute | Purpose |
|-----------|---------|
| **data-original-title** | Bootstrap tooltip HTML |
| **data-description** | Report description text |
| **data-filename** | PDF filename |
| **style** | Used to detect hidden rows (`display: none`) |


