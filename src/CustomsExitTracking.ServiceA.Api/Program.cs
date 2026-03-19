using CustomsExitTracking.ServiceA.Api.Application;
using CustomsExitTracking.ServiceA.Api.Integrations;
using CustomsExitTracking.ServiceA.Api.Persistence;
using CustomsExitTracking.ServiceA.Api.Repositories;
using CustomsExitTracking.ServiceA.Api.Settings;
using CustomsExitTracking.Shared.Contracts;
using CustomsExitTracking.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CustomsDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("Oracle")));
builder.Services.Configure<ScreeningRulesOptions>(
    builder.Configuration.GetSection(ScreeningRulesOptions.SectionName));
builder.Services.AddScoped<IPersonReadRepository, PersonReadRepository>();
builder.Services.AddScoped<IExitRecordReadRepository, ExitRecordReadRepository>();
builder.Services.AddScoped<PersonReadService>();
builder.Services.AddScoped<ExitRecordReadService>();
builder.Services.AddScoped<ExitVerificationService>();
builder.Services.AddHttpClient<IServiceBClient, ServiceBClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["ServiceB:BaseUrl"]
        ?? throw new InvalidOperationException("ServiceB:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Service A API");
    options.RoutePrefix = "swagger";
});

app.MapGet("/health", () => Results.Ok(CreateHealthResponse("healthy")))
    .WithName("GetHealth");

app.MapGet("/ready", () => Results.Ok(CreateHealthResponse("ready")))
    .WithName("GetReadiness");

app.MapGet("/api/persons/{nationalId}", GetPersonAsync)
    .WithName("GetPersonByNationalId");

app.MapGet("/api/persons/{nationalId}/exits", GetPersonExitsAsync)
    .WithName("GetExitRecordsByNationalId");

app.MapPost("/api/persons/{nationalId}", VerifyAndInsertExitAsync)
    .WithName("VerifyAndInsertExit");

app.Run();

static async Task<Results<Ok<PersonDto>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> GetPersonAsync(
    string nationalId,
    PersonReadService service,
    CancellationToken cancellationToken)
{
    if (!RequestValidation.IsNationalIdValid(nationalId))
    {
        return TypedResults.BadRequest(new ErrorResponse(
            "VALIDATION_ERROR",
            "The request is invalid.",
            new Dictionary<string, string[]>
            {
                ["nationalId"] = ["National ID is required."]
            }));
    }

    var person = await service.GetByNationalIdAsync(nationalId, cancellationToken);
    if (person is null)
    {
        return TypedResults.NotFound(new ErrorResponse(
            "PERSON_NOT_FOUND",
            $"No person was found for national ID '{nationalId}'."));
    }

    return TypedResults.Ok(person);
}

static async Task<Results<Ok<IReadOnlyList<ExitRecordDto>>, BadRequest<ErrorResponse>>> GetPersonExitsAsync(
    string nationalId,
    DateTimeOffset? from,
    DateTimeOffset? to,
    string? toCountry,
    int? limit,
    int? offset,
    ExitRecordReadService service,
    CancellationToken cancellationToken)
{
    if (!RequestValidation.IsNationalIdValid(nationalId))
    {
        return TypedResults.BadRequest(new ErrorResponse(
            "VALIDATION_ERROR",
            "The request is invalid.",
            new Dictionary<string, string[]>
            {
                ["nationalId"] = ["National ID is required."]
            }));
    }

    if (!string.IsNullOrWhiteSpace(toCountry) && !RequestValidation.IsCountryCodeValid(toCountry))
    {
        return TypedResults.BadRequest(new ErrorResponse(
            "VALIDATION_ERROR",
            "The request is invalid.",
            new Dictionary<string, string[]>
            {
                ["toCountry"] = ["To-country filter must be a valid ISO alpha-3 code."]
            }));
    }

    var request = new ExitRecordQueryRequest(
        from,
        to,
        toCountry,
        limit ?? SharedValidationConstants.DefaultPageSize,
        offset ?? 0);

    if (!RequestValidation.IsPaginationValid(request))
    {
        return TypedResults.BadRequest(new ErrorResponse(
            "VALIDATION_ERROR",
            "The request is invalid.",
            new Dictionary<string, string[]>
            {
                ["pagination"] = [$"Limit must be between 1 and {SharedValidationConstants.MaxPageSize}, and offset must be zero or greater."]
            }));
    }

    var records = await service.GetByNationalIdAsync(nationalId, request, cancellationToken);
    return TypedResults.Ok(records);
}

static async Task<Results<Ok<CustomsExitTracking.ServiceA.Api.Contracts.VerifyAndInsertExitResponse>, BadRequest<ErrorResponse>>> VerifyAndInsertExitAsync(
    string nationalId,
    CustomsExitTracking.ServiceA.Api.Contracts.VerifyAndInsertExitRequest request,
    ExitVerificationService service,
    CancellationToken cancellationToken)
{
    var errors = new Dictionary<string, string[]>();

    if (!RequestValidation.IsNationalIdValid(nationalId))
    {
        errors["nationalId"] = ["National ID is required."];
    }

    if (!RequestValidation.IsCountryCodeValid(request.FromCountryCode))
    {
        errors["fromCountryCode"] = ["From-country code must be a valid ISO alpha-3 code."];
    }

    if (!RequestValidation.IsCountryCodeValid(request.ToCountryCode))
    {
        errors["toCountryCode"] = ["To-country code must be a valid ISO alpha-3 code."];
    }

    if (string.IsNullOrWhiteSpace(request.PortOfExit))
    {
        errors["portOfExit"] = ["Port of exit is required."];
    }

    if (errors.Count > 0)
    {
        return TypedResults.BadRequest(new ErrorResponse(
            "VALIDATION_ERROR",
            "The request is invalid.",
            errors));
    }

    var result = await service.VerifyAndInsertAsync(nationalId, request, cancellationToken);
    return TypedResults.Ok(result);
}

static HealthStatusResponse CreateHealthResponse(string status) =>
    new("service-a", status, DateTimeOffset.UtcNow);

public partial class Program;
