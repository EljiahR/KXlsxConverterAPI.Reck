using KXlsxConverterAPI.Data;
using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Repositories;
using KXlsxConverterAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
services.AddDbContext<EmployeeContext>(options =>
    options.UseSqlite("Data Source=employees.db"),ServiceLifetime.Scoped);

services.AddScoped<IEmployeeRepository, EmployeeRepository>();
services.AddScoped<IEmployeeService, EmployeeService>();

services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
