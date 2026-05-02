using UglyToad.PdfPig;
using System.Text;
using System.Globalization;
using DocumentProcessor.DTOs;

namespace DocumentProcessor.Services
{
    public class ParserService
    {
        private readonly OcrService _ocr;

        public ParserService(OcrService ocr)
        {
            _ocr = ocr;
        }

        public DocumentDto ParsePdf(byte[] fileBytes)
        {
            var sb = new StringBuilder();
            using var pdf = PdfDocument.Open(fileBytes);
            foreach (var page in pdf.GetPages())
            {
                // Grupiraj riječi po Y poziciji (isti red = slična Y koordinata)
                var wordsByLine = page.GetWords()
                    .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 0))
                    .OrderByDescending(g => g.Key);

                foreach (var line in wordsByLine)
                {
                    var lineText = string.Join(" ", line.Select(w => w.Text));
                    sb.AppendLine(lineText);
                }
            }
            return ExtractFieldsFromText(sb.ToString());
        }


        public DocumentDto ParseCsv(byte[] fileBytes)
        {
            var text = Encoding.UTF8.GetString(fileBytes);
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var lineItems = new List<LineItemDto>();

            
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length < 4) continue;

                lineItems.Add(new LineItemDto
                {
                    Description = parts[0].Trim(),
                    Quantity = double.TryParse(parts[1].Trim(), out var qty) ? qty : 0,
                    UnitPrice = double.TryParse(parts[2].Trim(), out var price) ? price : 0,
                    Total = double.TryParse(parts[3].Trim(), out var total) ? total : 0,
                });
            }

            return new DocumentDto
            {
                DocType = "invoice",
                LineItems = lineItems,
                Subtotal = lineItems.Sum(i => i.Total),
            };
        }

        
        public DocumentDto ParseTxt(byte[] fileBytes)
        {
            var text = Encoding.UTF8.GetString(fileBytes);
            return ExtractFieldsFromText(text);
        }
        public DocumentDto ParseImage(byte[] fileBytes)
        {
            var text = _ocr.ExtractTextFromImage(fileBytes);
            return ExtractFieldsFromText(text);
        }


        private DocumentDto ExtractFieldsFromText(string text)
        {
            var data = new DocumentDto();
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var lower = line.ToLower();

                if (lower.Contains("invoice"))
                    data.DocType = "invoice";
                else if (lower.Contains("purchase order"))
                    data.DocType = "purchase_order";

                if (lower.Contains("supplier:"))
                    data.Supplier = line.Split(':', 2)[1].Trim();

                if (lower.Contains("number:"))
                    data.DocNumber = line.Split(':', 2)[1].Trim();

                if (lower.Contains("date:"))
                    data.IssueDate = line.Split(':', 2)[1].Trim();

                if (lower.StartsWith("subtotal"))
                    data.Subtotal = ExtractLastNumber(line);

                if (lower.StartsWith("tax"))
                    data.Tax = ExtractLastNumber(line);

                if (lower.StartsWith("total"))
                    data.Total = ExtractLastNumber(line);

                foreach (var currency in new[] { "EUR", "USD", "BAM" })
                    if (line.Contains(currency))
                        data.Currency = currency;
            }

            return data;
        }

        private double? ExtractLastNumber(string line)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = parts.Length - 1; i >= 0; i--)
                if (double.TryParse(parts[i], NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var val))
                    return val;
            return null;
        }
    }
}