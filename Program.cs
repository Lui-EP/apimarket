using Microsoft.EntityFrameworkCore;
using AgroMarketApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ========== Controllers + Swagger ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========== Connection string (appsettings o DATABASE_URL en Render) ==========
var conn = builder.Configuration.GetConnectionString("PostgreSQLConnection");
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrWhiteSpace(conn) && !string.IsNullOrWhiteSpace(dbUrl))
{
    var uri = new Uri(dbUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    conn = $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};" +
           $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

// ========== CORS ==========
const string AgroCors = "AgroCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AgroCors, policy =>
    {
        policy
            // SOLO los orígenes que realmente llaman a la API:
            .WithOrigins(
                "https://agromarket-s920.onrender.com", // tu FRONT en Render
                "http://localhost:5500",                // dev local
                "http://127.0.0.1:5500"                 // dev local
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
            // .AllowCredentials(); // solo si usas cookies/auth cross-site
    });
});

var app = builder.Build();

// (Opcional) Swagger en dev solamente:
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ========== Pipeline (orden correcto) ==========
app.UseRouting();
app.UseCors(AgroCors);
app.UseAuthorization();

// ========== Endpoints ==========
app.MapControllers();

// Healthcheck sencillo
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
