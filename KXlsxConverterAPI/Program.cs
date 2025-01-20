using KXlsxConverterAPI.AuthHandlers;
using KXlsxConverterAPI.Data;
using KXlsxConverterAPI.Models;
using KXlsxConverterAPI.Repositories;
using KXlsxConverterAPI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

services.AddIdentity<EmployeeUser, IdentityRole>()
    .AddEntityFrameworkStores<EmployeeContext>();

services.Configure<IdentityOptions>(options => 
{
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = false;
});

services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options => 
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Add authentication to service
services.AddAuthorization(options => 
{
    options.AddPolicy("BelongsToStore", policy => 
        policy.RequireClaim("StoreNumber"));
    
    options.AddPolicy("AdminOrBelongsToStore", policy =>
        policy.Requirements.Add(new AdminOrBelongsToStoreRequirement()));
});

services.AddSingleton<IAuthorizationHandler, AdminOrBelongsToStoreHandler>();

// Add services to the container
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
services.AddScoped<IEmployeeService, EmployeeService>();

services.AddControllers();
services.AddAuthentication();

// Swagger/OpenAPI
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

// Seeding admin data
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var userManager = serviceProvider.GetRequiredService<UserManager<EmployeeUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.Initialize(serviceProvider, userManager, roleManager, builder.Configuration);
}

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

