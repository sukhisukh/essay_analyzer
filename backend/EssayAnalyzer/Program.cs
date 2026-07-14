using Microsoft.EntityFrameworkCore;
using EssayAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Fix port for Azure Linux ──────────────────────
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ── Register Services ─────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IAnthropicService, AnthropicService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();

// Database
builder.Services.AddDbContext<EssayContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        )
    ));

// AI Service
builder.Services.AddHttpClient<EssayService>();

// CORS — SetIsOriginAllowed bypasses Azure override behavior
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy =>
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader());
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("ChromeExtension", policy =>
    {
        policy.WithOrigins(
            "chrome-extension://YOUR_EXTENSION_ID_HERE",
            "https://classroom.google.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// ── Build & Configure Pipeline ────────────────────
var app = builder.Build();

// Show detailed errors
app.UseDeveloperExceptionPage();    // ← ADD THIS

// ⚠️ CORS MUST be first — before everything else
app.UseCors("AllowAll");
app.UseCors("ChromeExtension"); 
// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();