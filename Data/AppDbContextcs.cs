using Microsoft.EntityFrameworkCore;
using DocumentProcessor.Models;

namespace DocumentProcessor.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Document> Documents { get; set; }
    }
}