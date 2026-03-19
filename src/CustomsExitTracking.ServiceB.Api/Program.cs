using CustomsExitTracking.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(CreateHealthResponse("healthy")))
    .WithName("GetHealth");

app.MapGet("/ready", () => Results.Ok(CreateHealthResponse("ready")))
    .WithName("GetReadiness");

app.Run();

static HealthStatusResponse CreateHealthResponse(string status) =>
    new("service-b", status, DateTimeOffset.UtcNow);

public partial class Program;
