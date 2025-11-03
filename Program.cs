using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    conn =
        $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};" +
        $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

// ========== CORS (abierto; sin credenciales) ==========
const string AgroCors = "AgroCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AgroCors, policy =>
    {
        policy
            .AllowAnyOrigin()   // si NO usas cookies/autenticación de navegador
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

// ========== Pipeline (orden correcto) ==========
app.UseRouting();

// 0) Inyecta headers CORS también en respuestas de error (500, etc.)
app.Use(async (ctx, next) =>
{
    ctx.Response.OnStarting(() =>
    {
        var origin = ctx.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin))
        {
            if (!ctx.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                ctx.Response.Headers["Access-Control-Allow-Origin"] = origin;
            if (!ctx.Response.Headers.ContainsKey("Vary"))
                ctx.Response.Headers["Vary"] = "Origin";
        }
        return Task.CompletedTask;
    });

    await next();
});

// 1) Preflight corto (OPTIONS) para no tocar DB/EF en preflight
app.Use(async (ctx, next) =>
{
    if (string.Equals(ctx.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
    {
        var origin = ctx.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin))
            ctx.Response.Headers["Access-Control-Allow-Origin"] = origin;

        var reqHdrs = ctx.Request.Headers["Access-Control-Request-Headers"].ToString();
        var reqMth = ctx.Request.Headers["Access-Control-Request-Method"].ToString();
        if (!string.IsNullOrEmpty(reqHdrs))
            ctx.Response.Headers["Access-Control-Allow-Headers"] = reqHdrs;
        if (!string.IsNullOrEmpty(reqMth))
            ctx.Response.Headers["Access-Control-Allow-Methods"] = reqMth;

        ctx.Response.Headers["Vary"] = "Origin";
        ctx.Response.StatusCode = 204; // No Content
        return;
    }
    await next();
});

// 2) CORS para el resto de peticiones
app.UseCors(AgroCors);

// (Opcional) app.UseHttpsRedirection();
app.UseAuthorization();

// ========== Endpoints ==========
app.MapControllers();

// Healthcheck simple
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
