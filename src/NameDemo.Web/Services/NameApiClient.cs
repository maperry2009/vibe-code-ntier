using System.Net.Http.Json;

namespace NameDemo.Web.Services;

public record GuestNameDto(int Id, string Name, string LastName, DateTime CreatedAt);

public record CreateNameRequest(string Name, string? LastName);

public class NameApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<GuestNameDto>> GetNamesAsync(CancellationToken cancellationToken = default)
    {
        var names = await httpClient.GetFromJsonAsync<List<GuestNameDto>>("api/names", cancellationToken);
        return names ?? [];
    }

    public async Task<bool> AddNameAsync(string name, string lastName, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/names",
            new CreateNameRequest(name, lastName),
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
