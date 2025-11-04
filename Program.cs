using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AgroMarketApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ===== Controllers + Swagger =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== Connection string (appsettings o DATABASE_URL en Render) =====
var conn = builder.Configuration.GetConnectionString("PostgreSQLConnection");
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrWhiteSpace(conn) && !string.IsNullOrWhiteSpace(dbUrl))
{
    var uri = new Uri(dbUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    conn =
        $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};" +
        $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

// ===== CORS =====
// Nota: NO usamos credenciales/cookies; por eso AllowAnyOrigin es válido.
const string AgroCors = "AgroCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AgroCors, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ===== Pipeline =====
app.UseRouting();
// en Program.cs, después de app.UseRouting();
app.UseStaticFiles();      // sirve /wwwroot

// (opcional) SPA fallback
app.MapFallbackToFile("index.html");

// (0) Headers CORS en TODAS las respuestas (incluye 4xx/5xx).
//     Si el proxy de Render deja pasar la request a Kestrel, estos headers SIEMPRE saldrán.
app.Use(async (ctx, next) =>
{
    // Inyecta desde el inicio (por si algo truena más adelante)
    ctx.Response.Headers["Access-Control-Allow-Origin"] = "*"; // sin cookies -> *
    ctx.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,OPTIONS";
    ctx.Response.Headers["Access-Control-Allow-Headers"] = "*";
    ctx.Response.Headers["Vary"] = "Origin";
    await next();
});

// (1) Preflight OPTIONS corto (no tocar DB/EF)
app.Use(async (ctx, next) =>
{
    if (string.Equals(ctx.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
    {
        // ya pusimos los headers arriba
        ctx.Response.StatusCode = 204; // No Content
        return;
    }
    await next();
});

// (2) CORS normal para el resto
app.UseCors(AgroCors);

// (opcional) app.UseHttpsRedirection();
app.UseAuthorization();

// ===== Endpoints =====
app.MapControllers();

// Healthcheck simple
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
