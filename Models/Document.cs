using System.Text.Json;

namespace DocumentProcessor.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Filename { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public string? DocType { get; set; }
        public string? Supplier { get; set; }
        public string? DocNumber { get; set; }
        public string? IssueDate { get; set; }
        public string? Currency { get; set; }
        public double? Subtotal { get; set; }
        public double? Tax { get; set; }
        public double? Total { get; set; }
        public string LineItemsJson { get; set; } = "[]"; 
        public string Status { get; set; } = "uploaded";
        public string IssuesJson { get; set; } = "[]";   
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}