using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Register Services ──────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<EssayContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// AI Service
builder.Services.AddHttpClient<EssayService>();
builder.Services.AddScoped<EssayService>(); 

// CORS — allow React frontend to call this API
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ── Build & Configure Pipeline ─────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ⚠️ CORS must come BEFORE all other middleware
app.UseCors("AllowAll");

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();