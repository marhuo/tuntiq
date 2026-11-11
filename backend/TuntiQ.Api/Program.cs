using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TuntiQ.Api.Logging;
using TuntiQ.Api.Shared;
using TuntiQ.Application;
using TuntiQ.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration.GetConnectionString("Default")!)
    .AddAppLogging(builder.Configuration.GetConnectionString("Logs")!);

var app = builder.Build();
app.UseRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseLogViewer("Admin");
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "TuntiQ API"));
}

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        CurrentUser.Set(new UserContext(
            Guid.Parse(context.User.FindFirstValue("sub")!),
            Guid.Parse(context.User.FindFirstValue("tenant_id")!),
            false));
    }

    await next();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
