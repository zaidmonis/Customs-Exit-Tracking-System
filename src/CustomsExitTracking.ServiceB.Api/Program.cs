using CustomsExitTracking.ServiceB.Api.Application;
using CustomsExitTracking.ServiceB.Api.Persistence;
using CustomsExitTracking.ServiceB.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;
using CustomsExitTracking.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<CustomsDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("Oracle")));
builder.Services.AddScoped<IPersonReadRepository, PersonReadRepository>();
builder.Services.AddScoped<IExitRecordRepository, ExitRecordRepository>();
builder.Services.AddScoped<PersonReadService>();
builder.Services.AddScoped<ExitRecordService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(CreateHealthResponse("healthy")))
    .WithName("GetHealth");

app.MapGet("/ready", () => Results.Ok(CreateHealthResponse("ready")))
    .WithName("GetReadiness");

app.MapGet("/api/persons/{nationalId}", GetPersonAsync)
    .WithName("GetPersonByNationalId");

app.MapGet("/api/persons/{nationalId}/exits", GetPersonExitsAsync)
    .WithName("GetExitRecordsByNationalId");

app.MapPost("/api/persons/{nationalId}/exits", CreateExitRecordAsync)
    .WithName("CreateExitRecord");

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
    ExitRecordService service,
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

static async Task<Results<Created<ExitRecordDto>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> CreateExitRecordAsync(
    string nationalId,
    CustomsExitTracking.ServiceB.Api.Contracts.ExitRecordCreateRequest request,
    ExitRecordService service,
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

    var created = await service.CreateAsync(nationalId, request, cancellationToken);
    if (created is null)
    {
        return TypedResults.NotFound(new ErrorResponse(
            "PERSON_NOT_FOUND",
            $"No person was found for national ID '{nationalId}'."));
    }

    return TypedResults.Created($"/api/persons/{nationalId}/exits/{created.ExitId}", created);
}

static HealthStatusResponse CreateHealthResponse(string status) =>
    new("service-b", status, DateTimeOffset.UtcNow);

public partial class Program;
