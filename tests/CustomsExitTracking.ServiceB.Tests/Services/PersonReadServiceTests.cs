using CustomsExitTracking.ServiceB.Api.Application;
using CustomsExitTracking.ServiceB.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceB.Tests;

public class PersonReadServiceTests
{
    [Fact]
    public async Task PersonReadService_ReturnsRepositoryResult()
    {
        var expected = TestData.CreatePerson();
        var service = new PersonReadService(new StubPersonReadRepository(expected));

        var result = await service.GetByNationalIdAsync("MY9001010001", CancellationToken.None);

        Assert.Equal(expected, result);
    }

    private sealed class StubPersonReadRepository(PersonDto? person) : IPersonReadRepository
    {
        public Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken) =>
            Task.FromResult(person);
    }
}
