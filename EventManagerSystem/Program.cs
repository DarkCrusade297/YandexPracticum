using Application;
using Application.Common.Settings;
using Application.Services.JwtTokenService;
using EventManagerSystem.Middleware;
using Infrastructure;
using Infrastructure.DataAccess;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

//Services
builder.Services.AddServices(builder.Configuration);

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Event Manager System API",
        Version = "v1",
        Description = "API for event management and bookings"
    });
});
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

// Swagger лучше включить до кастомного middleware на время диагностики
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "Event Manager System API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.ApplyMigrations();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();