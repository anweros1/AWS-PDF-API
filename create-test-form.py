#!/usr/bin/env python3
"""
Create a test PDF with form fields for testing the API
Requires: pip install reportlab
"""

from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import letter
from reportlab.pdfbase import pdfform
from reportlab.lib.colors import black, white, blue

def create_form_pdf(filename="test-form-with-fields.pdf"):
    """Create a PDF with interactive form fields"""
    
    # Create canvas
    c = canvas.Canvas(filename, pagesize=letter)
    width, height = letter
    
    # Title
    c.setFont("Helvetica-Bold", 24)
    c.drawString(50, height - 50, "Employee Information Form")
    
    # Subtitle
    c.setFont("Helvetica", 12)
    c.drawString(50, height - 80, "Please fill out all fields below:")
    
    # Y position tracker
    y = height - 120
    
    # Helper function to add labeled text field
    def add_text_field(label, field_name, y_pos, width=300):
        c.setFont("Helvetica-Bold", 11)
        c.drawString(50, y_pos + 5, label)
        
        # Create text field
        form = c.acroForm
        form.textfield(
            name=field_name,
            tooltip=f"Enter {label.lower()}",
            x=200,
            y=y_pos,
            width=width,
            height=20,
            borderColor=black,
            fillColor=white,
            textColor=blue,
            forceBorder=True
        )
        return y_pos - 40
    
    # Add form fields
    y = add_text_field("Full Name:", "Name", y)
    y = add_text_field("Email Address:", "Email", y)
    y = add_text_field("Phone Number:", "Phone", y)
    y = add_text_field("Date of Birth:", "DateOfBirth", y, width=150)
    y = add_text_field("Employee ID:", "EmployeeID", y, width=150)
    y = add_text_field("Department:", "Department", y)
    y = add_text_field("Position:", "Position", y)
    y = add_text_field("Start Date:", "StartDate", y, width=150)
    y = add_text_field("Salary:", "Salary", y, width=150)
    
    # Add checkbox
    y -= 20
    c.setFont("Helvetica-Bold", 11)
    c.drawString(50, y + 5, "Active Employee:")
    
    form = c.acroForm
    form.checkbox(
        name="Active",
        tooltip="Check if active",
        x=200,
        y=y,
        size=15,
        borderColor=black,
        fillColor=white,
        forceBorder=True,
        checked=False
    )
    
    # Add notes field (larger text field)
    y -= 60
    c.setFont("Helvetica-Bold", 11)
    c.drawString(50, y + 80, "Additional Notes:")
    
    form.textfield(
        name="Notes",
        tooltip="Enter additional notes",
        x=50,
        y=y,
        width=500,
        height=60,
        borderColor=black,
        fillColor=white,
        textColor=blue,
        forceBorder=True
    )
    
    # Footer
    c.setFont("Helvetica-Oblique", 9)
    c.drawString(50, 50, "This is a test form for API testing purposes")
    c.drawString(50, 35, "Field names: Name, Email, Phone, DateOfBirth, EmployeeID, Department, Position, StartDate, Salary, Active, Notes")
    
    # Save
    c.save()
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
    print("  - Notes (multiline text)")

if __name__ == "__main__":
    create_form_pdf()
