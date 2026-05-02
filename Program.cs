using DocumentProcessor.Data;
using DocumentProcessor.Services;
using Microsoft.EntityFrameworkCore;
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=documents.db"));

builder.Services.AddScoped<OcrService>();
builder.Services.AddScoped<ParserService>();
builder.Services.AddScoped<ValidationService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy
            .WithOrigins(
                "http://localhost",
                "http://localhost:5270",
                "https://fir-project-78e65.web.app"  
            )
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.MapControllers();
app.Run();