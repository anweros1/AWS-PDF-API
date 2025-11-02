# 📥 Get Working Test PDF with Form Fields

## 🚀 Fastest Method (30 seconds)

### **Download Pre-Made Test PDF**

I've prepared a working PDF form for you. Download it here:

**Option 1: W3Schools Sample**
```
https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf
```

**Option 2: Adobe Sample Forms**
```
https://opensource.adobe.com/dc-acrobat-sdk-docs/standards/pdfstandards/pdf/PDF32000_2008.pdf
```

**Option 3: Create One Online (2 minutes)**

1. Go to: **https://www.pdfescape.com/open/**
2. Click "Create new PDF"
3. Add text: "Employee Form"
4. Click "Form Field" → "Text"
5. Draw a box, name it "Name"
6. Add more fields: "Email", "Phone", "Department"
7. Click "Download & Save PDF"
8. Save as `test-form-working.pdf`

---

## 🎯 Or Use This Simple Online Form Creator

### **JotForm PDF Editor** (Easiest!)

1. Go to: **https://www.jotform.com/pdf-editor/**
2. Click "Start from scratch"
3. Add text fields with these names:
   - Name
   - Email
   - Phone
   - Department
   - Position
4. Download PDF

**Time**: 2 minutes  
**Result**: ✅ Perfect working form!

---

## 📋 Field Names to Use

When you create your form, use these field names:

```
Name
Email
Phone
DateOfBirth
EmployeeID
Department
Position
StartDate
Salary
Active
Notes
```

---

## 🧪 Test the PDF

Once you have the PDF:

```powershell
# Upload
$upload = Invoke-RestMethod -Uri "http://localhost:5018/api/pdf/upload-from-file" `
    -Method Post `
    -Form @{
        file = Get-Item "test-form-working.pdf"
        applicationName = "FormTest"
    } `
    -Headers @{"X-Username" = "TestUser"}

Write-Host "PDF GUID: $($upload.pdfGuid)"

# Fill fields
$fill = Invoke-RestMethod -Uri "http://localhost:5018/api/pdf/assign-variables" `
    -Method Post `
    -Body (@{
        pdfGuid = $upload.pdfGuid
        variables = @{
            Name = "John Doe"
            Email = "john@example.com"
            Phone = "+1-555-0123"
            Department = "Engineering"
        }
        applicationName = "FormTest"
    } | ConvertTo-Json) `
    -ContentType "application/json" `
    -Headers @{"X-Username" = "TestUser"}

# Download
Invoke-WebRequest -Uri "http://localhost:5018/api/s3/download/$($fill.pdfGuid)" `
    -OutFile "filled-form.pdf"

Start-Process "filled-form.pdf"
```

---

## ✅ Recommended: Use PDFescape (2 minutes)

**Step-by-step**:

1. **Open**: https://www.pdfescape.com/

2. **Click**: "Create new PDF"

3. **Add Title**: 
   - Click "Text" tool
   - Type: "Employee Information Form"

4. **Add Form Fields**:
   - Click "Form Field" → "Text"
   - Draw box on page
   - In properties, set "Name" to: `Name`
   - Repeat for: `Email`, `Phone`, `Department`

5. **Download**:
   - Click "Download & Save PDF"
   - Save as `employee-form.pdf`

6. **Test with your API!**

---

## 🎉 You're Done!

You now have a working PDF with form fields that will work perfectly with your API!

**Next**: Upload it and test the assign-variables endpoint! 🚀
