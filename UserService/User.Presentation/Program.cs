using User.Application;
using User.Infrastructure;
using User.Presentation.Middleware;
using User.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddUserApplication()
    .AddUserInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.ApplyUserMigrations();
app.MapControllers();

app.Run();

public partial class Program;
