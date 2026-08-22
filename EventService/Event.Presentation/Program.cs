using Event.Application;
using Event.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEventApplication()
    .AddEventInfrastructure();

var app = builder.Build();

app.MapGet("/", () => "Event service is running.");

app.Run();

public partial class Program;
