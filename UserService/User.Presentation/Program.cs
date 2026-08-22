using User.Application;
using User.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddUserApplication()
    .AddUserInfrastructure();

var app = builder.Build();

app.MapGet("/", () => "User service is running.");

app.Run();

public partial class Program;
