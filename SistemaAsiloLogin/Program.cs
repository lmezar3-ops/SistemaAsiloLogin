using Microsoft.EntityFrameworkCore;
using MicroservicioAncianos.Data;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllers();

// Configuración de Entity Framework
builder.Services.AddDbContext<AsiloDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AsiloConnection")));

// Configuración CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirSistemaAsilo", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configurar el pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseCors("PermitirSistemaAsilo");

app.UseAuthorization();

app.MapControllers();

app.Run();