using Microsoft.EntityFrameworkCore;
using AgroMarketApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connection string desde appsettings o DATABASE_URL
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
builder.Services.AddCors();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseAuthorization();
app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapControllers();
app.Run();
