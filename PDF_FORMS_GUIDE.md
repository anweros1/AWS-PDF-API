# 📝 PDF Forms & Variables - Complete Guide

## Date: October 29, 2025

---

## 🎯 What Are PDF Form Fields?

PDF forms contain **interactive fields** that can be filled programmatically or by users.

### **Think of them like HTML forms**:

```html
<!-- HTML Form -->
<input type="text" name="Name" />
<input type="email" name="Email" />
<input type="date" name="Date" />
```

```pdf
<!-- PDF Form (conceptual) -->
[Text Field: Name]
[Text Field: Email]
[Text Field: Date]
```

---

## 🔧 How Your API Works

### **Endpoint**: `POST /api/pdf/assign-variables`

### **What It Does**:
1. Takes a PDF with form fields
2. Fills the fields with your data
3. Creates a new PDF with filled fields
4. Returns the new PDF GUID

### **Example Request**:
```json
{
  "pdfGuid": "abc-123-def-456",
  "variables": {
    "Name": "John Doe",
    "Email": "john@example.com",
    "Phone": "+1-555-0123",
    "Date": "2025-10-29"
  },
  "applicationName": "FormFillTest"
}
```

### **How It Matches Fields**:
- **Key** = Field name in PDF (e.g., "Name")
- **Value** = What to fill in (e.g., "John Doe")

---

## 📋 Field Types Explained

### **1. Text Fields**
```json
{
  "Name": "John Doe",
  "Address": "123 Main Street"
}
```
- Single-line or multi-line text
- Most common field type

### **2. Checkboxes**
```json
{
  "Active": "Yes",
  "Agree": "true"
}
```
- Values: "Yes", "true", "1", "On" = checked
- Values: "No", "false", "0", "Off" = unchecked

### **3. Radio Buttons**
```json
{
  "Gender": "Male",
  "PaymentMethod": "CreditCard"
}
```
- Only one option can be selected
- Value must match option name exactly

### **4. Dropdown Lists**
```json
{
  "Country": "United States",
  "Department": "Engineering"
}
```
- Value must match one of the dropdown options

### **5. Date Fields**
```json
{
  "StartDate": "2025-10-29",
  "BirthDate": "1990-01-15"
}
```
- Format depends on field configuration
- Common formats: YYYY-MM-DD, MM/DD/YYYY

---

## 🛠️ Creating Test PDFs

### **Method 1: Python Script (Easiest!)**

I've created a script for you: `create-test-form.py`

**Install requirements**:
```bash
pip install reportlab
```

**Run script**:
```bash
python create-test-form.py
```

**Output**: `test-form-with-fields.pdf`

**Fields created**:
- Name (text)
- Email (text)
- Phone (text)
- DateOfBirth (text)
- EmployeeID (text)
- Department (text)
- Position (text)
- StartDate (text)
- Salary (text)
- Active (checkbox)
- Notes (multiline text)

---

### **Method 2: LibreOffice (FREE)**

**Step-by-step**:

1. **Download LibreOffice**
   ```
   https://www.libreoffice.org/download/
   ```

2. **Open LibreOffice Writer**
   - Create new document

3. **Add Form Content**
   ```
   Employee Information Form
   
   Name: __________________
   Email: __________________
   Phone: __________________
   Date: __________________
   ```

4. **Enable Form Controls**
   - View → Toolbars → Form Controls
   - Click "Design Mode" (pencil icon)

5. **Insert Text Fields**
   - Click "Text Box" icon
   - Draw box where you want field
   - Right-click → Control Properties
   - Set "Name" property (e.g., "Name")

6. **Insert Other Fields**
   - Check Box: For yes/no options
   - Date Field: For dates
   - List Box: For dropdowns

7. **Exit Design Mode**
   - Click "Design Mode" again to turn it off

8. **Export as PDF**
   - File → Export as PDF
   - ✅ Check "Create PDF form"
   - ✅ Check "Submit format: PDF"
   - Save

---

### **Method 3: Adobe Acrobat Pro**

**Step-by-step**:

1. **Open PDF**
   - Open any PDF or create new

2. **Prepare Form**
   - Tools → Prepare Form
   - Acrobat auto-detects fields

3. **Add Fields**
   - Click "Add New Field"
   - Choose type: Text, Checkbox, etc.
   - Draw on page

4. **Set Field Properties**
   - Double-click field
   - Set "Name" (this is the key!)
   - Set format, validation, etc.

5. **Save**
   - File → Save

---

### **Method 4: Online Tools (Quick & Easy)**

**PDFescape** (Free):
1. Go to: https://www.pdfescape.com/
2. Upload or create PDF
3. Click "Form Field" → "Text"
4. Draw field on page
5. Set field name
6. Download PDF

**JotForm PDF Editor** (Free):
1. Go to: https://www.jotform.com/pdf-editor/
2. Upload PDF
3. Add form fields
4. Download

---

## 🧪 Testing Your API

### **Step 1: Create Form PDF**

Use any method above to create a PDF with fields named:
- Name
- Email
- Phone
- Date

### **Step 2: Upload to API**

```bash
curl -X POST "http://localhost:5018/api/pdf/upload-from-file" \
  -H "X-Username: TestUser" \
  -F "file=@test-form-with-fields.pdf" \
  -F "applicationName=TestApp"
```

**Response**:
```json
{
  "pdfGuid": "abc-123-def-456",
  "pageCount": 1,
  "success": true
}
```

### **Step 3: Fill Form Fields**

```bash
curl -X POST "http://localhost:5018/api/pdf/assign-variables" \
  -H "Content-Type: application/json" \
  -H "X-Username: TestUser" \
  -d '{
    "pdfGuid": "abc-123-def-456",
    "variables": {
      "Name": "John Doe",
      "Email": "john@example.com",
      "Phone": "+1-555-0123",
      "Date": "2025-10-29"
    },
    "applicationName": "FormFillTest"
  }'
```

**Response**:
```json
{
  "pdfGuid": "xyz-789-new-guid",
  "pageCount": 1,
  "success": true,
  "message": "Variables assigned successfully"
}
```

### **Step 4: Download Filled PDF**

```bash
curl "http://localhost:5018/api/s3/download/xyz-789-new-guid" \
  -o filled-form.pdf
```

### **Step 5: Verify**

Open `filled-form.pdf` and check:
- ✅ Name field shows "John Doe"
- ✅ Email field shows "john@example.com"
- ✅ Phone field shows "+1-555-0123"
- ✅ Date field shows "2025-10-29"

---

## 🔍 How to Check Field Names in Existing PDF

### **Method 1: Adobe Acrobat**
1. Open PDF
2. Tools → Prepare Form
3. Hover over fields to see names

### **Method 2: Python Script**

```python
import PyPDF2

def list_form_fields(pdf_path):
    with open(pdf_path, 'rb') as file:
        reader = PyPDF2.PdfReader(file)
        fields = reader.get_fields()
        
        if fields:
            print("Form fields found:")
            for field_name, field_data in fields.items():
                print(f"  - {field_name}: {field_data.get('/FT', 'Unknown type')}")
        else:
            print("No form fields found in PDF")

list_form_fields("your-pdf.pdf")
```

### **Method 3: Online Tool**
- Upload to: https://www.pdfescape.com/
- Click on fields to see names

---

## 📊 Complete Example

### **Scenario**: Employee Onboarding Form

**1. Create PDF with fields**:
- EmployeeName
- EmployeeEmail
- Department
- StartDate
- Salary
- FullTime (checkbox)

**2. Upload to API**:
```json
{
  "file": "employee-form.pdf",
  "applicationName": "HR-System"
}
```

**3. Fill fields for new employee**:
```json
{
  "pdfGuid": "form-guid-123",
  "variables": {
    "EmployeeName": "Jane Smith",
    "EmployeeEmail": "jane.smith@company.com",
    "Department": "Engineering",
    "StartDate": "2025-11-01",
    "Salary": "$85,000",
    "FullTime": "Yes"
  },
  "applicationName": "HR-System"
}
```

**4. Result**:
- New PDF created with all fields filled
- Ready to send to employee
- Can be stored or printed

---

## 🎯 Common Use Cases

### **1. Employment Contracts**
```json
{
  "EmployeeName": "John Doe",
  "Position": "Software Engineer",
  "StartDate": "2025-11-01",
  "Salary": "$100,000",
  "SignDate": "2025-10-29"
}
```

### **2. Invoices**
```json
{
  "InvoiceNumber": "INV-2025-001",
  "CustomerName": "Acme Corp",
  "Date": "2025-10-29",
  "Amount": "$5,000.00",
  "DueDate": "2025-11-29"
}
```

### **3. Application Forms**
```json
{
  "ApplicantName": "Jane Smith",
  "Email": "jane@example.com",
  "Phone": "+1-555-0123",
  "Position": "Marketing Manager",
  "Experience": "5 years"
}
```

### **4. Certificates**
```json
{
  "RecipientName": "John Doe",
  "CourseName": "Advanced Programming",
  "CompletionDate": "2025-10-29",
  "InstructorName": "Dr. Smith",
  "CertificateNumber": "CERT-2025-123"
}
```

---

## ⚠️ Important Notes

### **1. Field Names Must Match Exactly**
```json
// ❌ Won't work - case mismatch
{
  "name": "John"  // PDF has "Name"
}

// ✅ Works - exact match
{
  "Name": "John"
}
```

### **2. Empty Fields Are Skipped**
```json
{
  "Name": "John",
  "Email": "",      // Won't fill this field
  "Phone": null     // Won't fill this field
}
```

### **3. Unknown Fields Are Ignored**
```json
{
  "Name": "John",
  "Age": "30"  // If PDF doesn't have "Age" field, this is ignored
}
```

### **4. Original PDF Is Not Modified**
- API creates a **new PDF** with filled fields
- Original PDF remains unchanged
- New PDF gets a new GUID

---

## 🔧 Troubleshooting

### **Problem**: Fields not filling

**Check**:
1. ✅ PDF has form fields (not just text)
2. ✅ Field names match exactly (case-sensitive)
3. ✅ PDF is not password-protected
4. ✅ Fields are not read-only

### **Problem**: Can't see field names

**Solution**:
- Open in Adobe Acrobat → Prepare Form
- Or use Python script above
- Or check PDF properties

### **Problem**: Checkbox not working

**Try different values**:
```json
{
  "Active": "Yes"    // Try this
  "Active": "true"   // Or this
  "Active": "1"      // Or this
  "Active": "On"     // Or this
}
```

---

## 📚 Additional Resources

### **PDF Form Specifications**:
- Adobe PDF Reference: https://www.adobe.com/devnet/pdf/pdf_reference.html
- PDF Forms Tutorial: https://www.pdfa.org/resource/pdf-forms/

### **Tools**:
- LibreOffice: https://www.libreoffice.org/
- PDFescape: https://www.pdfescape.com/
- JotForm: https://www.jotform.com/pdf-editor/

### **Libraries**:
- ReportLab (Python): https://www.reportlab.com/
- iText (Java): https://itextpdf.com/
- PDFKit (Node.js): https://pdfkit.org/

---

## 🎉 Quick Start Checklist

- [ ] Install Python and reportlab
- [ ] Run `python create-test-form.py`
- [ ] Upload generated PDF to API
- [ ] Test filling fields with API
- [ ] Download and verify filled PDF
- [ ] Create your own form PDF
- [ ] Test with your real data

---

## 📝 Example API Test Script

```bash
#!/bin/bash

# 1. Upload form PDF
UPLOAD_RESPONSE=$(curl -s -X POST "http://localhost:5018/api/pdf/upload-from-file" \
  -H "X-Username: TestUser" \
  -F "file=@test-form-with-fields.pdf" \
  -F "applicationName=TestApp")

FORM_GUID=$(echo $UPLOAD_RESPONSE | jq -r '.pdfGuid')
echo "Form uploaded: $FORM_GUID"

# 2. Fill form fields
FILL_RESPONSE=$(curl -s -X POST "http://localhost:5018/api/pdf/assign-variables" \
  -H "Content-Type: application/json" \
  -H "X-Username: TestUser" \
  -d "{
    \"pdfGuid\": \"$FORM_GUID\",
    \"variables\": {
      \"Name\": \"John Doe\",
      \"Email\": \"john@example.com\",
      \"Phone\": \"+1-555-0123\",
      \"DateOfBirth\": \"1990-01-15\",
      \"Department\": \"Engineering\"
    },
    \"applicationName\": \"FormFillTest\"
  }")

FILLED_GUID=$(echo $FILL_RESPONSE | jq -r '.pdfGuid')
echo "Form filled: $FILLED_GUID"

# 3. Download filled PDF
curl "http://localhost:5018/api/s3/download/$FILLED_GUID" \
  -o filled-form.pdf

echo "✅ Downloaded: filled-form.pdf"
```

---

**Guide Created**: October 29, 2025  
**Status**: ✅ Complete  
**Python Script**: ✅ Included (`create-test-form.py`)
