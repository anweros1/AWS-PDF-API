@echo off
echo ========================================
echo Creating Test PDF with Form Fields
echo ========================================
echo.

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python from: https://www.python.org/downloads/
    echo.
    pause
    exit /b 1
)

echo Python found!
echo.

REM Check if reportlab is installed
python -c "import reportlab" >nul 2>&1
if errorlevel 1 (
    echo Installing reportlab...
    pip install reportlab
    echo.
)

REM Run the script
echo Creating test PDF...
python create-test-form.py

if errorlevel 1 (
    echo.
    echo ERROR: Failed to create PDF
    pause
    exit /b 1
)

echo.
echo ========================================
echo SUCCESS!
echo ========================================
echo.
echo Test PDF created: test-form-with-fields.pdf
echo.
echo Next steps:
echo 1. Upload the PDF to your API
echo 2. Use the assign-variables endpoint to fill fields
echo 3. Download and verify the filled PDF
echo.
echo See PDF_FORMS_GUIDE.md for detailed instructions
echo.
pause
