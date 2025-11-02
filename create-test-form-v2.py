#!/usr/bin/env python3
"""
Create a test PDF with form fields using pdfrw (better compatibility)
Requires: pip install pdfrw reportlab
"""

from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import letter
from reportlab.lib.colors import black
try:
    from pdfrw import PdfReader, PdfWriter, PdfDict, PdfName, PdfArray, PdfString
    HAS_PDFRW = True
except ImportError:
    HAS_PDFRW = False
import io

def create_form_pdf_v2(filename="test-form-with-fields-v2.pdf"):
    """Create a PDF with interactive form fields using pdfrw for better compatibility"""
    
    if not HAS_PDFRW:
        print("❌ Error: pdfrw is not installed")
        print("\nPlease install required packages:")
        print("  pip install pdfrw reportlab")
        return
    
    # Step 1: Create a base PDF with text labels using ReportLab
    buffer = io.BytesIO()
    c = canvas.Canvas(buffer, pagesize=letter)
    width, height = letter
    
    # Title
    c.setFont("Helvetica-Bold", 24)
    c.drawString(50, height - 50, "Employee Information Form")
    
    # Subtitle
    c.setFont("Helvetica", 12)
    c.drawString(50, height - 80, "Please fill out all fields below:")
    
    # Y position tracker
    y = height - 120
    
    # Add labels for fields
    c.setFont("Helvetica-Bold", 11)
    labels = [
        "Full Name:",
        "Email Address:",
        "Phone Number:",
        "Date of Birth:",
        "Employee ID:",
        "Department:",
        "Position:",
        "Start Date:",
        "Salary:",
        "Active Employee:",
        "Additional Notes:"
    ]
    
    for label in labels:
        c.drawString(50, y + 5, label)
        y -= 40
    
    # Footer
    c.setFont("Helvetica-Oblique", 9)
    c.drawString(50, 50, "This is a test form for API testing purposes")
    c.drawString(50, 35, "Field names: Name, Email, Phone, DateOfBirth, EmployeeID, Department, Position, StartDate, Salary, Active, Notes")
    
    c.save()
    buffer.seek(0)
    
    # Step 2: Read the base PDF
    base_pdf = PdfReader(fdata=buffer.getvalue())
    
    # Step 3: Add form fields using pdfrw
    y = height - 120
    
    # Create AcroForm
    if base_pdf.Root.AcroForm is None:
        base_pdf.Root.AcroForm = PdfDict(Fields=PdfArray())
    
    fields = base_pdf.Root.AcroForm.Fields
    if fields is None:
        fields = PdfArray()
        base_pdf.Root.AcroForm.Fields = fields
    
    # Create form fields
    field_defs = [
        ("Name", 200, y, 300, 20),
        ("Email", 200, y - 40, 300, 20),
        ("Phone", 200, y - 80, 300, 20),
        ("DateOfBirth", 200, y - 120, 150, 20),
        ("EmployeeID", 200, y - 160, 150, 20),
        ("Department", 200, y - 200, 300, 20),
        ("Position", 200, y - 240, 300, 20),
        ("StartDate", 200, y - 280, 150, 20),
        ("Salary", 200, y - 320, 150, 20),
    ]
    
    # Add text fields
    for field_name, x, field_y, w, h in field_defs:
        field = create_text_field(field_name, x, field_y, w, h)
        fields.append(field)
    
    # Add checkbox
    checkbox_y = y - 360
    checkbox = create_checkbox_field("Active", 200, checkbox_y, 15)
    fields.append(checkbox)
    
    # Add notes field (larger)
    notes_y = checkbox_y - 80
    notes_field = create_text_field("Notes", 50, notes_y, 500, 60)
    fields.append(notes_field)
    
    # Save the PDF
    PdfWriter(filename, trailer=base_pdf).write()
    
    print(f"✅ Created: {filename}")
    print(f"\nForm fields available:")
    print("  - Name (text)")
    print("  - Email (text)")
    print("  - Phone (text)")
    print("  - DateOfBirth (text)")
    print("  - EmployeeID (text)")
    print("  - Department (text)")
    print("  - Position (text)")
    print("  - StartDate (text)")
    print("  - Salary (text)")
    print("  - Active (checkbox)")
    print("  - Notes (text)")
    print("\n✅ This PDF should be compatible with PdfSharp!")


def create_text_field(name, x, y, width, height):
    """Create a text field annotation"""
    field = PdfDict()
    field.FT = PdfName.Tx  # Field Type: Text
    field.T = PdfString.encode(name)  # Field Name
    field.V = PdfString.encode('')  # Field Value (empty)
    field.Rect = PdfArray([x, y, x + width, y + height])
    field.F = 4  # Flags: Print
    return field


def create_checkbox_field(name, x, y, size):
    """Create a checkbox field annotation"""
    field = PdfDict()
    field.FT = PdfName.Btn  # Field Type: Button
    field.T = PdfString.encode(name)  # Field Name
    field.V = PdfName.Off  # Field Value (unchecked)
    field.Rect = PdfArray([x, y, x + size, y + size])
    field.F = 4  # Flags: Print
    return field


if __name__ == "__main__":
    create_form_pdf_v2()
