using Booking.Application;
using Booking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddBookingApplication()
    .AddBookingInfrastructure();

var app = builder.Build();

app.MapGet("/", () => "Booking service is running.");

app.Run();

public partial class Program;
