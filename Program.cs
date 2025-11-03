using Microsoft.EntityFrameworkCore;
using AgroMarketApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ========== Controllers + Swagger ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========== Connection string ==========
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

// ========== CORS (abierto; sin credenciales) ==========
const string AgroCors = "AgroCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AgroCors, policy =>
    {
        policy
            .AllowAnyOrigin()   // <-- si no usas cookies, esto es lo más robusto
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

// ========== Pipeline (orden) ==========
app.UseRouting();

// 1) Preflight corto (evita pelearse con Render/CDN/DB en OPTIONS)
app.Use(async (ctx, next) =>
{
    if (string.Equals(ctx.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
    {
        var origin = ctx.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin))
            ctx.Response.Headers["Access-Control-Allow-Origin"] = origin;

        // Refleja lo que pidió el navegador (seguro y suficiente)
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

// 2) CORS para todas las demás peticiones
app.UseCors(AgroCors);

// (Opcional) app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Healthcheck
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
