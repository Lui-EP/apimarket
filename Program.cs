using Microsoft.EntityFrameworkCore;
using AgroMarketApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connection string (appsettings o DATABASE_URL en Render)
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

// CORS (sin AllowCredentials, sin SetIsOriginAllowed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AgroCors", policy =>
    {
        policy
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "https://apimarket-o0wh.onrender.com"   // tu backend (útil para Swagger/try it out)
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        // Si en el futuro usas cookies/autenticación cross-site:
        // usa .WithOrigins([...]).AllowCredentials()  <-- pero sin AllowAnyOrigin/SetIsOriginAllowed
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

// CORS va entre Routing y Authorization
app.UseCors("AgroCors");

app.UseAuthorization();

app.MapControllers();

app.Run();
