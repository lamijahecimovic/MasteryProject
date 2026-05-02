using DocumentProcessor.DTOs;
using DocumentProcessor.Data;
using System.Globalization;

namespace DocumentProcessor.Services
{
    public class ValidationService
    {
        private readonly AppDbContext _context;

        public ValidationService(AppDbContext context)
        {
            _context = context;
        }

        public (List<string> issues, string status) Validate(DocumentDto doc, int? currentDocId = null)
        {
            var issues = new List<string>();

            
            ValidateMissingFields(doc, issues);

            
            ValidateTotals(doc, issues);

            
            ValidateLineItems(doc, issues);

           
            ValidateDates(doc, issues);

           
            ValidateDuplicates(doc, issues, currentDocId);

            
            var status = issues.Count == 0 ? "validated" : "needs_review";

            return (issues, status);
        }

        
        private void ValidateMissingFields(DocumentDto doc, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(doc.DocType))
                issues.Add("Document type not found (invoice or purchase_order).");

            if (string.IsNullOrWhiteSpace(doc.Supplier))
                issues.Add("Supplier name is missing.");

            if (string.IsNullOrWhiteSpace(doc.DocNumber))
                issues.Add("Document number is missing.");

            if (string.IsNullOrWhiteSpace(doc.IssueDate))
                issues.Add("Issue date is missing.");

            if (doc.Total == null)
                issues.Add("Total amount is missing");
        }

        
        private void ValidateTotals(DocumentDto doc, List<string> issues)
        {
            if (doc.Subtotal == null || doc.Tax == null || doc.Total == null)
                return;

            var expectedTotal = Math.Round(doc.Subtotal.Value + doc.Tax.Value, 2);
            var actualTotal = Math.Round(doc.Total.Value, 2);

            if (Math.Abs(expectedTotal - actualTotal) > 0.01)
                issues.Add($"Total amount missmatch: {doc.Subtotal} + {doc.Tax} = {expectedTotal}, but found {actualTotal}.");
        }

        
        private void ValidateLineItems(DocumentDto doc, List<string> issues)
        {
            if (doc.LineItems == null || doc.LineItems.Count == 0)
                return;

            for (int i = 0; i < doc.LineItems.Count; i++)
            {
                var item = doc.LineItems[i];
                var expected = Math.Round(item.Quantity * item.UnitPrice, 2);
                var actual = Math.Round(item.Total, 2);

                if (Math.Abs(expected - actual) > 0.01)
                    issues.Add($"Item {i + 1} ({item.Description}): {item.Quantity} × {item.UnitPrice} = {expected}, but found {actual}.");
            }

            
            if (doc.Subtotal != null)
            {
                var sumOfItems = Math.Round(doc.LineItems.Sum(i => i.Total), 2);
                var subtotal = Math.Round(doc.Subtotal.Value, 2);

                if (Math.Abs(sumOfItems - subtotal) > 0.01)
                    issues.Add($"Sum of items ({sumOfItems}) missmatch ({subtotal}).");
            }
        }

        
        private void ValidateDates(DocumentDto doc, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(doc.IssueDate))
                return;

            var formats = new[] { "yyyy-MM-dd", "dd.MM.yyyy", "MM/dd/yyyy", "dd/MM/yyyy" };
            bool validDate = DateTime.TryParseExact(
                doc.IssueDate.Trim(),
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate
            );

            if (!validDate)
                issues.Add($"Date '{doc.IssueDate}' does not match the required format.");
            else if (parsedDate > DateTime.UtcNow)
                issues.Add($"Issue date ({doc.IssueDate}) is in the future.");
        }

        
        private void ValidateDuplicates(DocumentDto doc, List<string> issues, int? currentDocId)
        {
            if (string.IsNullOrWhiteSpace(doc.DocNumber))
                return;

            
            var exists = _context.Documents.Any(d =>
                d.DocNumber == doc.DocNumber &&
                (currentDocId == null || d.Id != currentDocId));

            if (exists)
                issues.Add($"A document with number '{doc.DocNumber}' already exists (duplicate).");
        }
    }
}