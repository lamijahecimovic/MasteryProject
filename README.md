#Document processing system
## Live API
https://masteryproject-production.up.railway.app/swagger

## Frontend
https://fir-project-78e65.web.app

## Tech Stack
- **ASP.NET Core Web API**
- **SQLite**
- **PdfPig** (PDF parsing)
- **Tesseract** (OCR for images)

## Local Setup
cd DocumentProcessor 
dotnet run

The API will be available at: **http://localhost:5270/swagger**

## Overview
The backend accepts documents in PDF, CSV, TXT, and image formats. It parses the files, extracts structured data, and automatically validates calculations, dates, and duplicates.

## Validation Logic
- **Total Verification:** Ensures `subtotal + tax = total`.
- **Line Item Validation:** Verifies `qty × unit_price = line_item_total`.
- **Missing Field Detection:** Identifies required fields that are not present.
- **Date Validation:** Checks for valid date formats and logical consistency.
- **Duplicate Detection:** Prevents redundant entries based on document number.

## AI Tools Used
- **Claude (Anthropic)** — Used for development assistance and debugging.

## Future Improvements
- User Authentication
- Batch Upload (processing multiple files simultaneously)
- Export to Excel/PDF
- Enhanced OCR for complex layouts



