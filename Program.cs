using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AgroMarketApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ========= Controllers + Swagger =========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========= DB (appsettings o DATABASE_URL en Render) =========
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

// ========= CORS (modo amplio para pruebas) =========
const string AgroCors = "AgroCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AgroCors, policy =>
    {
        policy
            .AllowAnyOrigin()   // <- para DEV. (en prod cambia a WithOrigins(...))
            .AllowAnyHeader()
            .AllowAnyMethod();
        // Nota: si usas cookies/tokens con credenciales, NO puedes usar AllowAnyOrigin.
        // En ese caso: .WithOrigins("http://127.0.0.1:5500","http://localhost:5500").AllowCredentials();
    });
});

var app = builder.Build();

// ========= Swagger en Development =========
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ========= Orden de middlewares =========
app.UseRouting();

// Respuesta universal a PRE-FLIGHT (OPTIONS) con CORS, sin tocar DB
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.NoContent())
   .RequireCors(AgroCors);

// Aplica CORS ANTES de exponer los endpoints
app.UseCors(AgroCors);

// (opcional) app.UseHttpsRedirection();
app.UseAuthorization();

// ========= Endpoints =========
app.MapControllers().RequireCors(AgroCors);

// Healthcheck
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
