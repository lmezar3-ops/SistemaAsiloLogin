using Microsoft.EntityFrameworkCore;
using MicroservicioMedicamentos.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MedicamentosDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AsiloConnection")));

// Add services to the container.

builder.Services.AddControllers();

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


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("PermitirSistemaAsilo");

app.UseAuthorization();

app.MapControllers();

app.Run();
