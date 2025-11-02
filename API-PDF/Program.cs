using API_PDF;
using API_PDF.Data;
using API_PDF.Models;
using API_PDF.Repositories;
using API_PDF.Repositories.Interfaces;
using API_PDF.Services;
using API_PDF.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// Configure Options
builder.Services.Configure<AwsSettings>(
    builder.Configuration.GetSection(AwsSettings.SectionName));
builder.Services.Configure<PdfSettings>(
    builder.Configuration.GetSection(PdfSettings.SectionName));

// Register Repositories
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<IPdfHistoryRepository, PdfHistoryRepository>();

// Register Services
builder.Services.AddSingleton<IS3Service, S3Service>();
builder.Services.AddScoped<IPdfService, PdfService>();

// Register HttpClient
builder.Services.AddHttpClient();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PDF Management API",
        Version = "v1",
        Description = "API for PDF manipulation with AWS S3 storage integration"
    });

    // Add support for file uploads in Swagger UI
    options.OperationFilter<SwaggerFileOperationFilter>();
    
    // Add support for custom headers (X-Username)
    options.OperationFilter<SwaggerHeaderOperationFilter>();
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Add API logging middleware
app.UseMiddleware<API_PDF.Middleware.ApiLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
