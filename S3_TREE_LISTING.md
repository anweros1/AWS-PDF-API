# 📁 S3 Bucket Tree Listing API

## Date: November 2, 2025

---

## 🎯 Feature Overview

New API endpoint to list all files and folders in the S3 bucket as a hierarchical tree structure, making it easy to browse and visualize bucket contents.

---

## 🚀 API Endpoint

### **GET** `/api/s3/tree`

Lists all files and folders in the S3 bucket as a tree structure.

#### **Query Parameters**:
- `prefix` (optional): Filter results by prefix/folder path
  - Example: `?prefix=reports/2024/`

#### **Response**: `200 OK`
```json
{
  "tree": [
    {
      "name": "reports",
      "path": "reports/",
      "type": "folder",
      "size": null,
      "lastModified": null,
      "itemCount": 15,
      "children": [
        {
          "name": "2024",
          "path": "reports/2024/",
          "type": "folder",
          "size": null,
          "lastModified": null,
          "itemCount": 10,
          "children": [
            {
              "name": "report-jan.pdf",
              "path": "reports/2024/report-jan.pdf",
              "type": "file",
              "size": 1048576,
              "lastModified": "2024-01-15T10:30:00Z",
              "itemCount": 0,
              "children": []
            }
          ]
        }
      ]
    }
  ],
  "totalFiles": 45,
  "totalFolders": 8,
  "totalSize": 52428800,
  "bucketName": "amine-api",
  "prefix": null
}
```

#### **Error Responses**:

**503 Service Unavailable** - S3 not available
```json
{
  "message": "S3 service is not available. Using local fallback storage."
}
```

**500 Internal Server Error** - Failed to list
```json
{
  "message": "Failed to list bucket contents",
  "error": "Error details..."
}
```

---

## 📊 Response Structure

### **S3TreeResponse**

| Field | Type | Description |
|-------|------|-------------|
| `tree` | `S3TreeNode[]` | Root nodes of the tree structure |
| `totalFiles` | `int` | Total number of files in bucket/prefix |
| `totalFolders` | `int` | Total number of folders in bucket/prefix |
| `totalSize` | `long` | Total size of all files in bytes |
| `bucketName` | `string` | Name of the S3 bucket |
| `prefix` | `string?` | Prefix filter applied (if any) |

### **S3TreeNode**

| Field | Type | Description |
|-------|------|-------------|
| `name` | `string` | Name of file or folder |
| `path` | `string` | Full S3 key/path |
| `type` | `string` | `"file"` or `"folder"` |
| `size` | `long?` | File size in bytes (files only) |
| `lastModified` | `DateTime?` | Last modified date (files only) |
| `children` | `S3TreeNode[]` | Child nodes (folders only) |
| `itemCount` | `int` | Number of items in folder (folders only) |

---

## 🧪 Usage Examples

### **Example 1: List Entire Bucket**

```bash
GET http://localhost:5018/api/s3/tree
```

**Response**:
```json
{
  "tree": [
    {
      "name": "pdfs",
      "path": "pdfs/",
      "type": "folder",
      "itemCount": 25,
      "children": [...]
    },
    {
      "name": "test.pdf",
      "path": "test.pdf",
      "type": "file",
      "size": 204800,
      "lastModified": "2024-11-01T12:00:00Z"
    }
  ],
  "totalFiles": 50,
  "totalFolders": 5,
  "totalSize": 10485760,
  "bucketName": "amine-api"
}
```

### **Example 2: List Specific Folder**

```bash
GET http://localhost:5018/api/s3/tree?prefix=reports/2024/
```

**Response**:
```json
{
  "tree": [
    {
      "name": "january",
      "path": "reports/2024/january/",
      "type": "folder",
      "itemCount": 5,
      "children": [
        {
          "name": "report-01.pdf",
          "path": "reports/2024/january/report-01.pdf",
          "type": "file",
          "size": 524288,
          "lastModified": "2024-01-15T10:00:00Z"
        }
      ]
    }
  ],
  "totalFiles": 12,
  "totalFolders": 3,
  "totalSize": 6291456,
  "bucketName": "amine-api",
  "prefix": "reports/2024/"
}
```

### **Example 3: PowerShell**

```powershell
# List entire bucket
$response = Invoke-RestMethod -Uri "http://localhost:5018/api/s3/tree" -Method Get
Write-Host "Total Files: $($response.totalFiles)"
Write-Host "Total Folders: $($response.totalFolders)"
Write-Host "Total Size: $([math]::Round($response.totalSize / 1MB, 2)) MB"

# List specific folder
$reports = Invoke-RestMethod -Uri "http://localhost:5018/api/s3/tree?prefix=reports/" -Method Get
$reports.tree | ForEach-Object {
    Write-Host "$($_.name) - $($_.itemCount) items"
}
```

### **Example 4: cURL**

```bash
# List entire bucket
curl -X GET "http://localhost:5018/api/s3/tree"

# List specific folder
curl -X GET "http://localhost:5018/api/s3/tree?prefix=pdfs/2024/"
```

---

## 🌳 Tree Structure Example

### **S3 Bucket Contents**:
```
bucket-root/
├── reports/
│   ├── 2024/
│   │   ├── january/
│   │   │   ├── report-01.pdf
│   │   │   └── report-02.pdf
│   │   └── february/
│   │       └── report-03.pdf
│   └── 2023/
│       └── annual.pdf
├── invoices/
│   └── invoice-001.pdf
└── test.pdf
```

### **API Response Tree**:
```json
{
  "tree": [
    {
      "name": "reports",
      "type": "folder",
      "itemCount": 4,
      "children": [
        {
          "name": "2024",
          "type": "folder",
          "itemCount": 3,
          "children": [
            {
              "name": "january",
              "type": "folder",
              "itemCount": 2,
              "children": [
                { "name": "report-01.pdf", "type": "file", "size": 102400 },
                { "name": "report-02.pdf", "type": "file", "size": 204800 }
              ]
            },
            {
              "name": "february",
              "type": "folder",
              "itemCount": 1,
              "children": [
                { "name": "report-03.pdf", "type": "file", "size": 153600 }
              ]
            }
          ]
        },
        {
          "name": "2023",
          "type": "folder",
          "itemCount": 1,
          "children": [
            { "name": "annual.pdf", "type": "file", "size": 512000 }
          ]
        }
      ]
    },
    {
      "name": "invoices",
      "type": "folder",
      "itemCount": 1,
      "children": [
        { "name": "invoice-001.pdf", "type": "file", "size": 81920 }
      ]
    },
    {
      "name": "test.pdf",
      "type": "file",
      "size": 40960
    }
  ],
  "totalFiles": 6,
  "totalFolders": 5,
  "totalSize": 1095680
}
```

---

## 🔧 Technical Implementation

### **Files Modified**:

1. **`Models/DTOs/S3TreeNode.cs`** (NEW)
   - `S3TreeNode`: Represents a node in the tree
   - `S3TreeResponse`: Response wrapper with statistics

2. **`Services/Interfaces/IS3Service.cs`**
   - Added `ListBucketTreeAsync()` method

3. **`Services/S3Service.cs`**
   - Implemented `ListBucketTreeAsync()`
   - Added `BuildTree()` helper method
   - Added `CalculateItemCount()` recursive method

4. **`Controllers/S3Controller.cs`**
   - Added `GET /api/s3/tree` endpoint

### **Key Features**:

✅ **Hierarchical Structure**: Files and folders organized as a tree  
✅ **Pagination Support**: Handles large buckets with pagination  
✅ **Prefix Filtering**: Filter by folder path  
✅ **Statistics**: Total files, folders, and size  
✅ **Item Counts**: Each folder shows number of items  
✅ **File Metadata**: Size and last modified date for files  
✅ **Recursive**: Supports nested folder structures  

---

## 📈 Performance

### **Optimization**:
- Uses S3 `ListObjectsV2` with pagination
- Builds tree structure in memory (single pass)
- Calculates statistics efficiently

### **Considerations**:
- **Large buckets**: May take time to list all objects
- **Prefix filtering**: Use to limit scope and improve performance
- **Caching**: Consider caching results for frequently accessed paths

---

## 🛡️ Error Handling

### **S3 Not Available**:
```json
HTTP 503
{
  "message": "S3 service is not available. Using local fallback storage."
}
```

### **Access Denied**:
```json
HTTP 500
{
  "message": "Failed to list bucket contents",
  "error": "Access Denied"
}
```

### **Invalid Prefix**:
- Returns empty tree if prefix doesn't match any objects
- No error thrown

---

## 🎨 Frontend Integration Example

### **JavaScript/React**:

```javascript
async function loadS3Tree(prefix = null) {
  const url = prefix 
    ? `/api/s3/tree?prefix=${encodeURIComponent(prefix)}`
    : '/api/s3/tree';
  
  const response = await fetch(url);
  const data = await response.json();
  
  console.log(`Total Files: ${data.totalFiles}`);
  console.log(`Total Size: ${(data.totalSize / 1024 / 1024).toFixed(2)} MB`);
  
  return data.tree;
}

// Render tree recursively
function renderTree(nodes, level = 0) {
  return nodes.map(node => {
    if (node.type === 'folder') {
      return `
        <div style="margin-left: ${level * 20}px">
          📁 ${node.name} (${node.itemCount} items)
          ${renderTree(node.children, level + 1)}
        </div>
      `;
    } else {
      return `
        <div style="margin-left: ${level * 20}px">
          📄 ${node.name} (${(node.size / 1024).toFixed(2)} KB)
        </div>
      `;
    }
  }).join('');
}
```

---

## 📝 Swagger Documentation

The endpoint is automatically documented in Swagger UI:

```
http://localhost:5018/swagger
```

Look for:
- **S3** section
- **GET /api/s3/tree** endpoint

---

## ✅ Testing

### **Test Case 1: Empty Bucket**
```bash
GET /api/s3/tree
```
**Expected**: Empty tree, 0 files, 0 folders

### **Test Case 2: Flat Structure**
```bash
# Bucket with only files at root
GET /api/s3/tree
```
**Expected**: All files at root level, no folders

### **Test Case 3: Nested Folders**
```bash
# Bucket with deep folder hierarchy
GET /api/s3/tree
```
**Expected**: Proper tree structure with all levels

### **Test Case 4: Prefix Filter**
```bash
GET /api/s3/tree?prefix=reports/2024/
```
**Expected**: Only items under reports/2024/

---

## 🚀 Benefits

1. **✅ Visual Bucket Browsing**: Easy to see folder structure
2. **✅ Statistics**: Quick overview of bucket contents
3. **✅ Filtering**: Focus on specific folders
4. **✅ File Metadata**: Size and modification dates
5. **✅ Item Counts**: Know how many files in each folder
6. **✅ API Integration**: Easy to build file browsers

---

## 📋 Summary

**New Endpoint**: `GET /api/s3/tree`  
**Purpose**: List S3 bucket contents as a tree  
**Features**: Hierarchical structure, statistics, filtering  
**Status**: ✅ Implemented and tested  
**Build**: ✅ Success  

---

**Your S3 bucket now has a tree listing API!** 🌳
