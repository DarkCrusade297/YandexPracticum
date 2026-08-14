using Infrastructure.DataAccess;
using EventManagerSystem.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Infrastructure;
using Application;

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

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();