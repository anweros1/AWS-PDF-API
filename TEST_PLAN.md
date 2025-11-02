# 🧪 PDF API Test Plan

## 📁 Test Files

Located in `test files/` folder:
1. **basic-text.pdf** - Simple text document
2. **fillable-form.pdf** - PDF with form fields
3. **sample-report.pdf** - Multi-page report

---

## 🚀 Prerequisites

### 1. Start the API

```bash
dotnet run --project API-PDF
```

API runs on: `http://localhost:5018`

### 2. Access Swagger UI

Navigate to: **http://localhost:5018/swagger**

This provides an interactive API documentation where you can test all endpoints directly in your browser.

### 3. Setup Postman (Optional)

1. Download Postman: https://www.postman.com/downloads/
2. Create a new Collection: "PDF API Tests"
3. Set base URL variable: `{{baseUrl}}` = `http://localhost:5018`

### 4. Verify S3 Connection

**Using Swagger UI**:
1. Go to `http://localhost:5018/swagger`
2. Expand `GET /api/s3/health`
3. Click "Try it out" → "Execute"

**Using Postman**:
1. Create GET request to `http://localhost:5018/api/s3/health`
2. Click "Send"

**Expected Response**:
```json
{
  "isAvailable": true,
  "status": "Connected",
  "message": "S3 service is available and connected"
}
```

---

## 📋 Test Scenarios

### **Scenario 1: Upload Single PDF**

#### Test 1.1: Upload Basic Text PDF

**Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Expand `POST /api/pdf/upload-from-file`
3. Click **"Try it out"**
4. Fill in parameters:
   - **file**: Click "Choose File" → Select `test files\basic-text.pdf`
   - **applicationName**: `TestApp`
5. Add header:
   - Click "Add string item" under Headers
   - **X-Username**: `TestUser`
6. Click **"Execute"**

**Using Postman**:
1. Create new request: `POST`
2. URL: `http://localhost:5018/api/pdf/upload-from-file`
3. Go to **Headers** tab:
   - Add: `X-Username` = `TestUser`
4. Go to **Body** tab:
   - Select **form-data**
   - Add key: `file` (change type to **File**)
     - Click "Select Files" → Choose `test files\basic-text.pdf`
   - Add key: `applicationName` (type: **Text**)
     - Value: `TestApp`
5. Click **"Send"**

**Expected Response**:
```json
{
  "pdfGuid": "abc-123-def-456",
  "pageCount": 3,
  "uploadedAt": "2025-10-29T21:00:00Z",
  "s3Url": "https://amine-api.s3.eu-north-1.amazonaws.com/pdfs/abc-123-def-456.pdf"
}
```

**Verification**:
- ✅ Response contains valid GUID
- ✅ Page count is correct
- ✅ File appears in S3 bucket: https://s3.console.aws.amazon.com/s3/buckets/amine-api
- ✅ **Save the GUID** for later tests!

---

#### Test 1.2: Upload Form PDF

**Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Expand `POST /api/pdf/upload-from-file`
3. Click **"Try it out"**
4. Fill in parameters:
   - **file**: Select `test files\fillable-form.pdf`
   - **applicationName**: `FormApp`
5. Add header: **X-Username** = `TestUser`
6. Click **"Execute"**

**Using Postman**:
1. Create new request: `POST`
2. URL: `http://localhost:5018/api/pdf/upload-from-file`
3. **Headers**: `X-Username` = `TestUser`
4. **Body** (form-data):
   - `file` (File): `test files\fillable-form.pdf`
   - `applicationName` (Text): `FormApp`
5. Click **"Send"**

**✅ Save the GUID** for later tests!

---

#### Test 1.3: Upload Report PDF

**Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Expand `POST /api/pdf/upload-from-file`
3. Click **"Try it out"**
4. Fill in parameters:
   - **file**: Select `test files\sample-report.pdf`
   - **applicationName**: `ReportApp`
5. Add header: **X-Username** = `TestUser`
6. Click **"Execute"**

**Using Postman**:
1. Create new request: `POST`
2. URL: `http://localhost:5018/api/pdf/upload-from-file`
3. **Headers**: `X-Username` = `TestUser`
4. **Body** (form-data):
   - `file` (File): `test files\sample-report.pdf`
   - `applicationName` (Text): `ReportApp`
5. Click **"Send"**

---

### **Scenario 2: Get Page Count**

**Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Expand `GET /api/pdf/page-count/{guid}`
3. Click **"Try it out"**
4. Enter **guid**: `abc-123-def-456` (use your actual GUID)
5. Click **"Execute"**

**Using Postman**:
1. Create new request: `GET`
2. URL: `http://localhost:5018/api/pdf/page-count/abc-123-def-456`
   - Replace `abc-123-def-456` with your actual GUID
3. Click **"Send"**

**Expected Response**: Integer (e.g., `3`)

**Verification**:
- ✅ Returns correct page count
- ✅ Matches upload response

---

### **Scenario 3: Check File Exists**

**Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Expand `GET /api/s3/exists/{guid}`
3. Click **"Try it out"**
4. Enter **guid**: Your uploaded PDF GUID
5. Click **"Execute"**

**Using Postman**:
1. Create new request: `GET`
2. URL: `http://localhost:5018/api/s3/exists/abc-123-def-456`
   - Replace with your actual GUID
3. Click **"Send"**

**Expected Response**: `true`

**Verification**:
- ✅ Returns `true` for uploaded file
- ✅ Returns `false` for non-existent GUID

---

### **Scenario 4: Download PDF**

**Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Expand `GET /api/s3/download/{guid}`
3. Click **"Try it out"**
4. Enter **guid**: Your uploaded PDF GUID
5. Click **"Execute"**
6. Click **"Download file"** button to save

**Using Postman**:
1. Create new request: `GET`
2. URL: `http://localhost:5018/api/s3/download/abc-123-def-456`
   - Replace with your actual GUID
3. Click **"Send"**
4. Click **"Save Response"** → **"Save to a file"**
5. Save as `downloaded-test.pdf`

**Verification**:
- ✅ File downloads successfully
- ✅ File size matches original
- ✅ PDF opens correctly

---

### **Scenario 5: Merge Multiple PDFs**

**Prerequisites**: Upload 3 PDFs first (see Scenario 1) and collect their GUIDs

**Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Expand `POST /api/pdf/merge`
3. Click **"Try it out"**
4. Fill in the request body:
   ```json
   {
     "pdfGuids": [
       "guid-1-here",
       "guid-2-here",
       "guid-3-here"
     ],
     "applicationName": "MergedDocument"
   }
   ```
5. Add header: **X-Username** = `TestUser`
6. Click **"Execute"**

**Using Postman**:
1. Create new request: `POST`
2. URL: `http://localhost:5018/api/pdf/merge`
3. **Headers**: `X-Username` = `TestUser`
4. **Body** (raw JSON):
   ```json
   {
     "pdfGuids": [
       "guid-1-here",
       "guid-2-here",
       "guid-3-here"
     ],
     "applicationName": "MergedDocument"
   }
   ```
5. Click **"Send"**

**Expected Response**:
```json
{
  "pdfGuid": "merged-guid-here",
  "pageCount": 9,
  "uploadedAt": "2025-10-29T21:00:00Z"
}
```

**Verification**:
- ✅ Merge succeeds
- ✅ Total page count = sum of individual PDFs
- ✅ Merged PDF exists in S3
- ✅ Download and verify all pages are present

---

### **Scenario 6: Add Bookmarks**

**Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Expand `POST /api/pdf/add-bookmarks`
3. Click **"Try it out"**
4. Fill in the request body:
   ```json
   {
     "pdfGuid": "your-pdf-guid-here",
     "bookmarks": [
       { "value": "Introduction", "pageNumber": 1 },
       { "value": "Chapter 1", "pageNumber": 2 },
       { "value": "Chapter 2", "pageNumber": 3 }
     ],
     "applicationName": "BookmarkTest"
   }
   ```
5. Add header: **X-Username** = `TestUser`
6. Click **"Execute"**

**Using Postman**:
1. Create new request: `POST`
2. URL: `http://localhost:5018/api/pdf/add-bookmarks`
3. **Headers**: `X-Username` = `TestUser`
4. **Body** (raw JSON):
   ```json
   {
     "pdfGuid": "your-pdf-guid-here",
     "bookmarks": [
       { "value": "Introduction", "pageNumber": 1 },
       { "value": "Chapter 1", "pageNumber": 2 },
       { "value": "Chapter 2", "pageNumber": 3 }
     ],
     "applicationName": "BookmarkTest"
   }
   ```
5. Click **"Send"**

**Expected Response**:
```json
{
  "pdfGuid": "new-guid-with-bookmarks",
  "pageCount": 3,
  "uploadedAt": "2025-10-29T21:00:00Z"
}
```

**Verification**:
- ✅ New PDF created with bookmarks
- ✅ Download and verify bookmarks in PDF viewer
- ✅ Bookmarks point to correct pages

---

### **Scenario 7: Add Keywords**

**Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Expand `POST /api/pdf/add-keywords`
3. Click **"Try it out"**
4. Fill in the request body:
   ```json
   {
     "pdfGuid": "your-pdf-guid-here",
     "keywords": ["test", "sample", "pdf", "document"],
     "applicationName": "KeywordTest"
   }
   ```
5. Add header: **X-Username** = `TestUser`
6. Click **"Execute"**

**Using Postman**:
1. Create new request: `POST`
2. URL: `http://localhost:5018/api/pdf/add-keywords`
3. **Headers**: `X-Username` = `TestUser`
4. **Body** (raw JSON):
   ```json
   {
     "pdfGuid": "your-pdf-guid-here",
     "keywords": ["test", "sample", "pdf", "document"],
     "applicationName": "KeywordTest"
   }
   ```
5. Click **"Send"**

**Expected Response**:
```json
{
  "pdfGuid": "new-guid-with-keywords",
  "pageCount": 3,
  "uploadedAt": "2025-10-29T21:00:00Z"
}
```

**Verification**:
- ✅ New PDF created with keywords
- ✅ Download and check PDF properties
- ✅ Keywords appear in document metadata

---

### **Scenario 8: Fill Form Fields**

**Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Expand `POST /api/pdf/assign-variables`
3. Click **"Try it out"**
4. Fill in the request body:
   ```json
   {
     "pdfGuid": "your-form-pdf-guid-here",
     "variables": {
       "Name": "John Doe",
       "Email": "john@example.com",
       "Date": "2025-10-29"
     },
     "applicationName": "FormFillTest"
   }
   ```
5. Add header: **X-Username** = `TestUser`
6. Click **"Execute"**

**Using Postman**:
1. Create new request: `POST`
2. URL: `http://localhost:5018/api/pdf/assign-variables`
3. **Headers**: `X-Username` = `TestUser`
4. **Body** (raw JSON):
   ```json
   {
     "pdfGuid": "your-form-pdf-guid-here",
     "variables": {
       "Name": "John Doe",
       "Email": "john@example.com",
       "Date": "2025-10-29"
     },
     "applicationName": "FormFillTest"
   }
   ```
5. Click **"Send"**

**Expected Response**:
```json
{
  "pdfGuid": "new-guid-with-filled-form",
  "pageCount": 1,
  "uploadedAt": "2025-10-29T21:00:00Z"
}
```

**Verification**:
- ✅ New PDF created with filled fields
- ✅ Download and verify field values
- ✅ Form fields contain correct data

**Note**: This test requires actual form fields in the PDF. If `fillable-form.pdf` doesn't have form fields, the API will still succeed but won't fill any fields.

---

### **Scenario 9: Delete PDF**

**Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Expand `DELETE /api/s3/{guid}`
3. Click **"Try it out"**
4. Enter **guid**: Your PDF GUID to delete
5. Click **"Execute"**

**Using Postman**:
1. Create new request: `DELETE`
2. URL: `http://localhost:5018/api/s3/abc-123-def-456`
   - Replace with your actual GUID
3. Click **"Send"**

**Expected Response**: `200 OK` (no body)

**Verification**:
- ✅ File deleted from S3
- ✅ Exists check returns `false`
- ✅ Download fails with 404

---

## 🎯 Complete Test Script

Save this as `run-tests.ps1`:

```powershell
# PDF API Complete Test Script
$baseUrl = "http://localhost:5018/api"
$testResults = @()

Write-Host "=== PDF API Test Suite ===" -ForegroundColor Yellow
Write-Host "Starting tests at: $(Get-Date)" -ForegroundColor Cyan

# Test 1: S3 Health Check
Write-Host "`n[Test 1] S3 Health Check..." -ForegroundColor Cyan
try {
    $health = Invoke-RestMethod -Uri "$baseUrl/s3/health" -Method Get
    if ($health.isAvailable) {
        Write-Host "✓ PASS: S3 is connected" -ForegroundColor Green
        $testResults += @{Test="S3 Health"; Status="PASS"}
    } else {
        Write-Host "✗ FAIL: S3 not available" -ForegroundColor Red
        $testResults += @{Test="S3 Health"; Status="FAIL"}
    }
} catch {
    Write-Host "✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{Test="S3 Health"; Status="FAIL"}
}

# Test 2: Upload PDFs
Write-Host "`n[Test 2] Upload Test Files..." -ForegroundColor Cyan
$uploadedGuids = @()

Get-ChildItem "test files\*.pdf" | ForEach-Object {
    try {
        $result = Invoke-RestMethod -Uri "$baseUrl/pdf/upload-from-file" `
            -Method Post -Form @{
                file = $_
                applicationName = "TestSuite"
            } -Headers @{"X-Username" = "AutoTest"}
        
        $uploadedGuids += $result.pdfGuid
        Write-Host "✓ PASS: Uploaded $($_.Name) -> $($result.pdfGuid) ($($result.pageCount) pages)" -ForegroundColor Green
        $testResults += @{Test="Upload $($_.Name)"; Status="PASS"; GUID=$result.pdfGuid}
    } catch {
        Write-Host "✗ FAIL: Upload $($_.Name) - $($_.Exception.Message)" -ForegroundColor Red
        $testResults += @{Test="Upload $($_.Name)"; Status="FAIL"}
    }
}

# Test 3: Get Page Count
Write-Host "`n[Test 3] Get Page Count..." -ForegroundColor Cyan
if ($uploadedGuids.Count -gt 0) {
    try {
        $pageCount = Invoke-RestMethod -Uri "$baseUrl/pdf/page-count/$($uploadedGuids[0])" -Method Get
        Write-Host "✓ PASS: Page count = $pageCount" -ForegroundColor Green
        $testResults += @{Test="Get Page Count"; Status="PASS"}
    } catch {
        Write-Host "✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += @{Test="Get Page Count"; Status="FAIL"}
    }
}

# Test 4: Check Exists
Write-Host "`n[Test 4] Check File Exists..." -ForegroundColor Cyan
if ($uploadedGuids.Count -gt 0) {
    try {
        $exists = Invoke-RestMethod -Uri "$baseUrl/s3/exists/$($uploadedGuids[0])" -Method Get
        if ($exists) {
            Write-Host "✓ PASS: File exists in S3" -ForegroundColor Green
            $testResults += @{Test="File Exists"; Status="PASS"}
        } else {
            Write-Host "✗ FAIL: File not found in S3" -ForegroundColor Red
            $testResults += @{Test="File Exists"; Status="FAIL"}
        }
    } catch {
        Write-Host "✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += @{Test="File Exists"; Status="FAIL"}
    }
}

# Test 5: Download
Write-Host "`n[Test 5] Download PDF..." -ForegroundColor Cyan
if ($uploadedGuids.Count -gt 0) {
    try {
        Invoke-RestMethod -Uri "$baseUrl/s3/download/$($uploadedGuids[0])" `
            -Method Get -OutFile "test-download.pdf"
        
        if (Test-Path "test-download.pdf") {
            Write-Host "✓ PASS: Downloaded successfully" -ForegroundColor Green
            $testResults += @{Test="Download"; Status="PASS"}
            Remove-Item "test-download.pdf" -Force
        }
    } catch {
        Write-Host "✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += @{Test="Download"; Status="FAIL"}
    }
}

# Test 6: Merge PDFs
Write-Host "`n[Test 6] Merge PDFs..." -ForegroundColor Cyan
if ($uploadedGuids.Count -ge 2) {
    try {
        $mergeBody = @{
            pdfGuids = $uploadedGuids
            applicationName = "MergedTest"
        } | ConvertTo-Json
        
        $mergeResult = Invoke-RestMethod -Uri "$baseUrl/pdf/merge" `
            -Method Post -Body $mergeBody `
            -ContentType "application/json" `
            -Headers @{"X-Username" = "AutoTest"}
        
        Write-Host "✓ PASS: Merged $($uploadedGuids.Count) PDFs -> $($mergeResult.pdfGuid) ($($mergeResult.pageCount) pages)" -ForegroundColor Green
        $testResults += @{Test="Merge PDFs"; Status="PASS"; GUID=$mergeResult.pdfGuid}
        $uploadedGuids += $mergeResult.pdfGuid
    } catch {
        Write-Host "✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += @{Test="Merge PDFs"; Status="FAIL"}
    }
}

# Test 7: Add Bookmarks
Write-Host "`n[Test 7] Add Bookmarks..." -ForegroundColor Cyan
if ($uploadedGuids.Count -gt 0) {
    try {
        $bookmarkBody = @{
            pdfGuid = $uploadedGuids[0]
            bookmarks = @(
                @{ value = "Test Bookmark 1"; pageNumber = 1 }
                @{ value = "Test Bookmark 2"; pageNumber = 2 }
            )
            applicationName = "BookmarkTest"
        } | ConvertTo-Json
        
        $bookmarkResult = Invoke-RestMethod -Uri "$baseUrl/pdf/add-bookmarks" `
            -Method Post -Body $bookmarkBody `
            -ContentType "application/json" `
            -Headers @{"X-Username" = "AutoTest"}
        
        Write-Host "✓ PASS: Added bookmarks -> $($bookmarkResult.pdfGuid)" -ForegroundColor Green
        $testResults += @{Test="Add Bookmarks"; Status="PASS"}
        $uploadedGuids += $bookmarkResult.pdfGuid
    } catch {
        Write-Host "✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += @{Test="Add Bookmarks"; Status="FAIL"}
    }
}

# Test 8: Add Keywords
Write-Host "`n[Test 8] Add Keywords..." -ForegroundColor Cyan
if ($uploadedGuids.Count -gt 0) {
    try {
        $keywordBody = @{
            pdfGuid = $uploadedGuids[0]
            keywords = @("test", "automated", "pdf")
            applicationName = "KeywordTest"
        } | ConvertTo-Json
        
        $keywordResult = Invoke-RestMethod -Uri "$baseUrl/pdf/add-keywords" `
            -Method Post -Body $keywordBody `
            -ContentType "application/json" `
            -Headers @{"X-Username" = "AutoTest"}
        
        Write-Host "✓ PASS: Added keywords -> $($keywordResult.pdfGuid)" -ForegroundColor Green
        $testResults += @{Test="Add Keywords"; Status="PASS"}
        $uploadedGuids += $keywordResult.pdfGuid
    } catch {
        Write-Host "✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += @{Test="Add Keywords"; Status="FAIL"}
    }
}

# Test 9: Delete PDFs (cleanup)
Write-Host "`n[Test 9] Cleanup - Delete Test Files..." -ForegroundColor Cyan
$deleteCount = 0
foreach ($guid in $uploadedGuids) {
    try {
        Invoke-RestMethod -Uri "$baseUrl/s3/$guid" -Method Delete
        $deleteCount++
    } catch {
        Write-Host "  Warning: Could not delete $guid" -ForegroundColor Yellow
    }
}
Write-Host "✓ Deleted $deleteCount test files" -ForegroundColor Green
$testResults += @{Test="Cleanup"; Status="PASS"; Count=$deleteCount}

# Summary
Write-Host "`n=== Test Summary ===" -ForegroundColor Yellow
$passCount = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$total = $testResults.Count

Write-Host "Total Tests: $total" -ForegroundColor Cyan
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Red" })
Write-Host "Success Rate: $([math]::Round(($passCount / $total) * 100, 2))%" -ForegroundColor Cyan

Write-Host "`nCompleted at: $(Get-Date)" -ForegroundColor Cyan
```

---

## 📊 Test Results Template

| Test # | Test Name | Status | Notes |
|--------|-----------|--------|-------|
| 1 | S3 Health Check | ⏳ | |
| 2 | Upload basic-text.pdf | ⏳ | |
| 3 | Upload fillable-form.pdf | ⏳ | |
| 4 | Upload sample-report.pdf | ⏳ | |
| 5 | Get Page Count | ⏳ | |
| 6 | Check File Exists | ⏳ | |
| 7 | Download PDF | ⏳ | |
| 8 | Merge 3 PDFs | ⏳ | |
| 9 | Add Bookmarks | ⏳ | |
| 10 | Add Keywords | ⏳ | |
| 11 | Fill Form Fields | ⏳ | |
| 12 | Delete PDF | ⏳ | |

**Legend**: ⏳ Pending | ✅ Pass | ❌ Fail

---

## 🔍 Verification Checklist

After running tests:

- [ ] All PDFs uploaded successfully
- [ ] Files visible in S3 console
- [ ] Page counts are correct
- [ ] Downloads work properly
- [ ] Merged PDF contains all pages
- [ ] Bookmarks appear in PDF viewer
- [ ] Keywords visible in PDF properties
- [ ] Form fields filled correctly (if applicable)
- [ ] Deleted files removed from S3
- [ ] No errors in API logs

---

## 🆘 Troubleshooting

### Upload Fails
- Check file path is correct
- Verify API is running
- Check S3 credentials in appsettings.json

### S3 Health Check Fails
- Verify AWS credentials
- Check bucket name: `amine-api`
- Check region: `eu-north-1`
- Verify IAM permissions

### Download Fails
- Verify GUID is correct
- Check file exists in S3
- Verify AWS permissions

---

## 📝 Notes

- Replace `{guid}` placeholders with actual GUIDs from responses
- Save GUIDs for multi-step tests
- Clean up test files from S3 after testing
- Monitor AWS Free Tier usage

---

## 🎯 Quick Reference

### **All Tests Using Swagger UI**:
1. Navigate to: `http://localhost:5018/swagger`
2. Each endpoint has "Try it out" button
3. Fill in parameters/body
4. Add `X-Username` header where required
5. Click "Execute"

### **All Tests Using Postman**:
1. Create requests with appropriate HTTP methods
2. Set URL: `http://localhost:5018/api/...`
3. Add headers: `X-Username` = `TestUser`
4. For file uploads: Use form-data with File type
5. For JSON requests: Use raw JSON body
6. Click "Send"

### **Test File Paths**:
- `test files\basic-text.pdf`
- `test files\fillable-form.pdf`
- `test files\sample-report.pdf`

### **Common Headers**:
- `X-Username`: Required for all endpoints
- `Content-Type`: `application/json` (for JSON requests)

---

## ✅ Summary

**Test Plan Updated**: October 29, 2025

**What's Included**:
- ✅ 9 complete test scenarios
- ✅ Swagger UI instructions for each test
- ✅ Postman instructions for each test
- ✅ Expected responses and verification steps
- ✅ Uses all 3 test PDF files

**Testing Methods**:
1. **Swagger UI** - Interactive browser-based testing
2. **Postman** - Professional API testing tool
3. **PowerShell** - Automated scripting (see "Complete Test Script" section)

**Ready to Test!** 🚀

---

**Test Plan Version**: 2.0  
**Last Updated**: October 29, 2025  
**Status**: ✅ Updated with Swagger UI & Postman Instructions
