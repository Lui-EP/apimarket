using Microsoft.EntityFrameworkCore;
using AgroMarketApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// 🔧 Configuración de servicios
// ===========================

// 1️⃣ Controladores + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2️⃣ Cadena de conexión (desde appsettings o DATABASE_URL)
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

// 3️⃣ Configurar CORS correctamente
builder.Services.AddCors(options =>
{
    options.AddPolicy("AgroCors", policy =>
    {
        policy
            // Agrega aquí tus orígenes permitidos (puedes ajustar)
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "https://apimarket-o0wh.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true) // 👈 permite cualquier origen temporalmente (útil en pruebas)
            .AllowCredentials();
    });
});

// ===========================
// 🚀 Construcción de la app
// ===========================
var app = builder.Build();

// 4️⃣ Swagger UI (solo para desarrollo)
app.UseSwagger();
app.UseSwaggerUI();

// 5️⃣ Middleware
app.UseHttpsRedirection();

// 🔥 Importante: CORS antes de Routing y Controllers
app.UseCors("AgroCors");

app.UseRouting();
app.UseAuthorization();

// 6️⃣ Mapear controladores
app.MapControllers();

// ===========================
// ✅ Ejecutar
// ===========================
app.Run();
