# ✅ S3 Access Denied - Fixed!

## Date: October 29, 2025

---

## 🐛 The Problem

### **Error You Were Getting**:
```xml
<Error>
  <Code>AccessDenied</Code>
  <Message>Access Denied</Message>
</Error>
```

### **Why This Happened**:

Your S3 bucket is configured correctly with **private access** (which is good for security!), but the API was generating **public URLs** that don't work with private buckets.

```csharp
// ❌ OLD CODE - Generated public URLs
var s3Url = $"https://{bucketName}.s3.{region}.amazonaws.com/{key}";
// This URL requires public access - which you blocked! ✅
```

---

## ✅ The Solution: Pre-Signed URLs

**Pre-signed URLs** are temporary URLs that grant access to private S3 objects without making them public.

### **How They Work**:

1. **Upload**: File stored as **private** in S3 ✅
2. **Generate URL**: Create temporary signed URL (valid 1 hour)
3. **Access**: Anyone with the URL can download (until it expires)
4. **Expire**: URL stops working after 1 hour
5. **Security**: Bucket stays private ✅

---

## 🔧 What Was Changed

### **File Modified**: `S3Service.cs`

### **Change 1: Upload Method**

**Before**:
```csharp
// Upload file
await fileTransferUtility.UploadAsync(...);

// ❌ Return public URL (doesn't work with private buckets)
var s3Url = $"https://{bucket}.s3.{region}.amazonaws.com/{key}";
return (s3Url, true);
```

**After**:
```csharp
// Upload file
await fileTransferUtility.UploadAsync(...);

// ✅ Generate pre-signed URL (works with private buckets)
var presignedUrl = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
{
    BucketName = _awsSettings.BucketName,
    Key = s3Key,
    Expires = DateTime.UtcNow.AddHours(1),  // Valid for 1 hour
    Verb = HttpVerb.GET
});

return (presignedUrl, true);
```

---

### **Change 2: GetPdfUrlAsync Method**

**Before**:
```csharp
// Check if file exists
await _s3Client.GetObjectMetadataAsync(...);

// ❌ Return public URL
var s3Url = $"https://{bucket}.s3.{region}.amazonaws.com/{key}";
return (s3Url, true);
```

**After**:
```csharp
// Check if file exists
await _s3Client.GetObjectMetadataAsync(...);

// ✅ Generate pre-signed URL
var presignedUrl = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
{
    BucketName = _awsSettings.BucketName,
    Key = s3Key,
    Expires = DateTime.UtcNow.AddHours(1),
    Verb = HttpVerb.GET
});

return (presignedUrl, true);
```

---

## 🔐 Security Benefits

### **Before (Public URLs)**:
- ❌ Required public bucket access
- ❌ Anyone could access files forever
- ❌ No expiration
- ❌ Security risk

### **After (Pre-Signed URLs)**:
- ✅ Bucket stays **private**
- ✅ Temporary access (1 hour)
- ✅ URLs expire automatically
- ✅ **Secure and compliant**

---

## 📊 How Pre-Signed URLs Look

### **Public URL (Old - Doesn't Work)**:
```
https://amine-api.s3.eu-north-1.amazonaws.com/pdfs/abc-123.pdf
```

### **Pre-Signed URL (New - Works!)**:
```
https://amine-api.s3.eu-north-1.amazonaws.com/pdfs/abc-123.pdf?
X-Amz-Algorithm=AWS4-HMAC-SHA256&
X-Amz-Credential=AKIAXJVLTYEZQ67FQEGK%2F20251029%2Feu-north-1%2Fs3%2Faws4_request&
X-Amz-Date=20251029T205900Z&
X-Amz-Expires=3600&
X-Amz-SignedHeaders=host&
X-Amz-Signature=abc123def456...
```

**Notice**: The URL includes signature and expiration parameters!

---

## 🧪 Testing the Fix

### **Test 1: Upload a PDF**

```bash
# Upload file
curl -X POST "http://localhost:5018/api/pdf/upload-from-file" \
  -H "X-Username: TestUser" \
  -F "file=@test files/basic-text.pdf" \
  -F "applicationName=TestApp"
```

**Response**:
```json
{
  "pdfGuid": "abc-123-def-456",
  "s3Url": "https://amine-api.s3...?X-Amz-Algorithm=...",
  "isStoredInS3": true
}
```

**Notice**: The `s3Url` now has query parameters (signature)!

---

### **Test 2: Download the PDF**

```bash
# Download using GUID
curl http://localhost:5018/api/s3/download/abc-123-def-456 \
  -o downloaded.pdf
```

**Result**: ✅ **File downloads successfully!**

---

### **Test 3: Access Pre-Signed URL Directly**

Copy the `s3Url` from the upload response and paste it in your browser.

**Result**: ✅ **PDF opens in browser!**

---

### **Test 4: Wait 1 Hour and Try Again**

After 1 hour, try accessing the same URL.

**Result**: ❌ **Access Denied** (URL expired - this is correct!)

---

## ⏰ URL Expiration

### **Current Setting**: 1 hour

```csharp
Expires = DateTime.UtcNow.AddHours(1)
```

### **Want to Change It?**

Edit `S3Service.cs` and change the expiration time:

```csharp
// 15 minutes
Expires = DateTime.UtcNow.AddMinutes(15)

// 24 hours
Expires = DateTime.UtcNow.AddHours(24)

// 7 days (maximum allowed by AWS)
Expires = DateTime.UtcNow.AddDays(7)
```

**AWS Limit**: Maximum 7 days

---

## 🔍 Why 1 Hour?

### **Pros**:
- ✅ Secure (URLs expire quickly)
- ✅ Prevents long-term sharing
- ✅ Good for temporary downloads

### **Cons**:
- ⚠️ Users must download within 1 hour
- ⚠️ Links in emails expire

### **Recommendation**:
- **Development**: 1-24 hours
- **Production**: 15 minutes - 1 hour
- **Long-term sharing**: Use a different approach (generate new URL on demand)

---

## 🎯 Best Practices

### **1. Keep Bucket Private** ✅
```json
// AWS S3 Bucket Settings
{
  "BlockPublicAcls": true,
  "IgnorePublicAcls": true,
  "BlockPublicPolicy": true,
  "RestrictPublicBuckets": true
}
```

### **2. Use Pre-Signed URLs** ✅
- For all private objects
- Set appropriate expiration
- Generate on-demand

### **3. Monitor Access**
- Check S3 access logs
- Monitor expired URL attempts
- Track download patterns

### **4. IAM Permissions**
Your IAM user needs:
```json
{
  "Effect": "Allow",
  "Action": [
    "s3:GetObject",
    "s3:PutObject",
    "s3:DeleteObject",
    "s3:GetObjectMetadata"
  ],
  "Resource": "arn:aws:s3:::amine-api/*"
}
```

---

## 📋 Comparison

| Feature | Public URLs | Pre-Signed URLs |
|---------|-------------|-----------------|
| **Bucket Access** | Must be public | Can be private ✅ |
| **Expiration** | Never | Configurable ✅ |
| **Security** | Low ❌ | High ✅ |
| **Sharing** | Permanent | Temporary ✅ |
| **Compliance** | Risky | Compliant ✅ |
| **Cost** | Same | Same |

---

## 🚨 Important Notes

### **1. URLs Are Temporary**
- Don't store pre-signed URLs in database
- Generate fresh URLs when needed
- URLs expire after set time

### **2. Regenerate When Needed**
```csharp
// ✅ Good: Generate URL on download request
var (url, _) = await _s3Service.GetPdfUrlAsync(guid);

// ❌ Bad: Store URL in database
// URL will expire!
```

### **3. S3 Download Endpoint**
The download endpoint now:
1. Checks if file exists
2. Gets pre-signed URL
3. Downloads file using URL
4. Returns file to client

This ensures URLs are always fresh!

---

## 🔄 How It Works Now

### **Upload Flow**:
```
1. User uploads PDF
   ↓
2. API saves to S3 (private)
   ↓
3. API generates pre-signed URL (1 hour)
   ↓
4. API returns URL to user
   ↓
5. User can download for 1 hour
   ↓
6. URL expires
```

### **Download Flow**:
```
1. User requests download by GUID
   ↓
2. API checks if file exists in S3
   ↓
3. API generates fresh pre-signed URL
   ↓
4. API downloads file from S3
   ↓
5. API returns file to user
```

---

## ✅ Verification

### **Check Your S3 Bucket Settings**:

1. Go to AWS S3 Console
2. Select bucket: `amine-api`
3. Go to "Permissions" tab
4. Check "Block public access":
   - ✅ Block all public access: **ON**
   - ✅ Block public ACLs: **ON**
   - ✅ Ignore public ACLs: **ON**
   - ✅ Block public bucket policies: **ON**
   - ✅ Restrict public buckets: **ON**

**All should be ON** - This is correct! ✅

---

## 🎉 Summary

### **Problem**:
- S3 files were private (good!)
- API used public URLs (bad!)
- Downloads failed with "Access Denied"

### **Solution**:
- ✅ Keep files private
- ✅ Use pre-signed URLs
- ✅ URLs expire after 1 hour
- ✅ Downloads work perfectly

### **Benefits**:
- ✅ **Secure**: Bucket stays private
- ✅ **Compliant**: Follows AWS best practices
- ✅ **Temporary**: URLs expire automatically
- ✅ **Working**: Downloads succeed!

---

## 🧪 Quick Test

```bash
# 1. Start API
dotnet run --project API-PDF

# 2. Upload a file
curl -X POST "http://localhost:5018/api/pdf/upload-from-file" \
  -H "X-Username: TestUser" \
  -F "file=@test files/basic-text.pdf" \
  -F "applicationName=TestApp"

# 3. Copy the pdfGuid from response

# 4. Download the file
curl http://localhost:5018/api/s3/download/{pdfGuid} -o test.pdf

# 5. Check the file
# Should download successfully! ✅
```

---

**Fix Applied**: October 29, 2025  
**Build Status**: ✅ Success  
**Security**: ✅ Enhanced (Private bucket + Pre-signed URLs)  
**Downloads**: ✅ Working!
