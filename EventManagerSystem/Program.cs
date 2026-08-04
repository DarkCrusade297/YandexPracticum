using EventManagerSystem.DataAccess;
using EventManagerSystem.Middleware;
using EventManagerSystem.Repositories.Booking;
using EventManagerSystem.Repositories.Event;
using EventManagerSystem.Services;
using EventManagerSystem.Services.BookingService;
using EventManagerSystem.Services.EventService;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers
builder.Services.AddControllers();

// Services
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddHostedService<BookingProcessorService>();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();