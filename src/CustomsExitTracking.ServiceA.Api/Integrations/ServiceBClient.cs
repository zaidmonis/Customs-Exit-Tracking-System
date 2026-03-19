using System.Net.Http.Json;
using CustomsExitTracking.ServiceA.Api.Contracts;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceA.Api.Integrations;

public sealed class ServiceBClient(HttpClient httpClient) : IServiceBClient
{
    public async Task<ExitRecordDto> CreateExitRecordAsync(
        string nationalId,
        VerifyAndInsertExitRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync($"/api/persons/{nationalId}/exits", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ExitRecordDto>(cancellationToken: cancellationToken);
        return payload ?? throw new InvalidOperationException("Service B returned an empty create response.");
    }
}
