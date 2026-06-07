using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Fix port for Azure Linux ──────────────────────
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ── Register Services ─────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// AI Service — only ONE registration needed
builder.Services.AddHttpClient<EssayService>();  // ← removed AddScoped

// CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ── Build & Configure Pipeline ────────────────────
var app = builder.Build();

// Swagger in all environments — needed for testing on Azure
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();