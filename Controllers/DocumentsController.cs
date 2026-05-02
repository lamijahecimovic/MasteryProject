using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentProcessor.Data;
using DocumentProcessor.Models;
using DocumentProcessor.DTOs;
using DocumentProcessor.Services;
using System.Text.Json;

namespace DocumentProcessor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ParserService _parser;
        private readonly ValidationService _validator;

        public DocumentsController(AppDbContext context, ParserService parser, ValidationService validator)
        {
            _context = context;
            _parser = parser;
            _validator = validator;
        }

        // POST: api/documents/upload
        [HttpPost("upload")]
        public async Task<ActionResult<DocumentDto>> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("The file is empty.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            var ext = Path.GetExtension(file.FileName).ToLower().TrimStart('.');
            DocumentDto parsed;

            parsed = ext switch
            {
                "pdf" => _parser.ParsePdf(fileBytes),
                "csv" => _parser.ParseCsv(fileBytes),
                "txt" => _parser.ParseTxt(fileBytes),
                "png" or "jpg" or "jpeg" => _parser.ParseImage(fileBytes),
                _ => throw new Exception("Unsupported file format.")
            };
            var (issues, status) = _validator.Validate(parsed);

            var doc = new Document
            {
                Filename = file.FileName,
                FileType = ext,
                DocType = parsed.DocType,
                Supplier = parsed.Supplier,
                DocNumber = parsed.DocNumber,
                IssueDate = parsed.IssueDate,
                Currency = parsed.Currency,
                Subtotal = parsed.Subtotal,
                Tax = parsed.Tax,
                Total = parsed.Total,
                LineItemsJson = JsonSerializer.Serialize(parsed.LineItems),
                Status = status,
                IssuesJson = JsonSerializer.Serialize(issues),
            };

            _context.Documents.Add(doc);
            await _context.SaveChangesAsync();

            return Ok(MapToDto(doc));
        }

        // GET: api/documents
        [HttpGet]
        public async Task<ActionResult<List<DocumentDto>>> GetAll()
        {
            var docs = await _context.Documents.ToListAsync();
            return Ok(docs.Select(MapToDto).ToList());
        }

        // GET: api/documents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetById(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();
            return Ok(MapToDto(doc));
        }

        // Mapiranje iz Document u DocumentDto
        private DocumentDto MapToDto(Document doc)
        {
            return new DocumentDto
            {
                Id = doc.Id,
                Filename = doc.Filename,
                FileType = doc.FileType,
                DocType = doc.DocType,
                Supplier = doc.Supplier,
                DocNumber = doc.DocNumber,
                IssueDate = doc.IssueDate,
                Currency = doc.Currency,
                Subtotal = doc.Subtotal,
                Tax = doc.Tax,
                Total = doc.Total,
                LineItems = JsonSerializer.Deserialize<List<LineItemDto>>(doc.LineItemsJson) ?? new(),
                Status = doc.Status,
                Issues = JsonSerializer.Deserialize<List<string>>(doc.IssuesJson) ?? new(),
                CreatedAt = doc.CreatedAt,
            };
        }
        // PATCH: api/documents/5/status
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<DocumentDto>> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var allowed = new[] { "uploaded", "needs_review", "validated", "rejected" };
            if (!allowed.Contains(newStatus))
                return BadRequest("Nevažeći status.");

            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

            doc.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(MapToDto(doc));
        }
        // PUT: api/documents/5 //manuelna korekcija
        [HttpPut("{id}")]
        public async Task<ActionResult<DocumentDto>> Update(int id, [FromBody] DocumentDto updated)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

           
            doc.DocType = updated.DocType;
            doc.Supplier = updated.Supplier;
            doc.DocNumber = updated.DocNumber;
            doc.IssueDate = updated.IssueDate;
            doc.Currency = updated.Currency;
            doc.Subtotal = updated.Subtotal;
            doc.Tax = updated.Tax;
            doc.Total = updated.Total;
            doc.LineItemsJson = JsonSerializer.Serialize(updated.LineItems);

            
            var (issues, status) = _validator.Validate(updated, id);
            doc.IssuesJson = JsonSerializer.Serialize(issues);
            doc.Status = status;

            await _context.SaveChangesAsync();

            return Ok(MapToDto(doc));
        }
        // DELETE: api/documents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}