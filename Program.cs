using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AgroMarketApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ===== Logging un poco más verboso en Prod =====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ===== Controllers + Swagger =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== DB (appsettings o DATABASE_URL en Render) =====
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

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseNpgsql(conn, o =>
    {
        o.CommandTimeout(15); // que truene rápido si la DB no responde
    });

    // útil para rastrear problemas en Prod temporalmente:
    opt.EnableDetailedErrors();
    opt.EnableSensitiveDataLogging();
});

// ===== CORS ultra permisivo para DEV (luego lo cerramos) =====
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

// ===== Middleware para medir latencia y ver si llegamos al controlador =====
app.Use(async (ctx, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    try
    {
        await next();
    }
    finally
    {
        sw.Stop();
        app.Logger.LogInformation("HTTP {Method} {Path} -> {StatusCode} en {Elapsed}ms",
            ctx.Request.Method, ctx.Request.Path, ctx.Response?.StatusCode, sw.ElapsedMilliseconds);
    }
});

// ===== Orden correcto =====
app.UseRouting();

// Respuesta universal a OPTIONS (preflight) con CORS
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.NoContent())
   .RequireCors(AgroCors);

// Aplica CORS ANTES de exponer endpoints
app.UseCors(AgroCors);

app.UseAuthorization();

// ===== Endpoints =====
app.MapControllers().RequireCors(AgroCors);

// Healthcheck y debug
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapGet("/debug/ping", () => Results.Ok(new { ping = DateTime.UtcNow }));
app.MapGet("/debug/db", async (AppDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return Results.Ok(new { db = ok ? "ok" : "fail" });
});

app.Run();
