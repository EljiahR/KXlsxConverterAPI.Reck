using KXlsxConverterAPI.Data;
using KXlsxConverterAPI.Repositories;
using KXlsxConverterAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

string? dbConnection = builder.Configuration["POSTGRESQLCONNSTR_DatabaseConnectionString"];
if (string.IsNullOrEmpty(dbConnection))
{
    services.AddDbContext<EmployeeContext>(options =>
        options.UseSqlite("Data Source=employees.db"), ServiceLifetime.Scoped);
}
else
{
    services.AddDbContext<EmployeeContext>(options =>
        options.UseNpgsql(dbConnection));
}

// Add CORS
services.AddCors(options =>
{
    options.AddPolicy("AllowFront", policy =>
    {
        policy.WithOrigins("https://15minutechart.netlify.app")  
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add authentication to service

// Add services to the container
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
services.AddScoped<IEmployeeService, EmployeeService>();

services.AddControllers();
services.AddAuthentication();

// Swagger/OpenAPI
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

// Enable CORS
app.UseCors("AllowFront");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

// Authentication middleware must come before Authorization and Endpoints
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

