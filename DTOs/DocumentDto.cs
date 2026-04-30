namespace DocumentProcessor.DTOs
{
    public class LineItemDto
    {
        public string Description { get; set; } = "";
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double Total { get; set; }
    }

    public class DocumentDto
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
        public List<LineItemDto> LineItems { get; set; } = new();
        public string Status { get; set; } = null!;
        public List<string> Issues { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
