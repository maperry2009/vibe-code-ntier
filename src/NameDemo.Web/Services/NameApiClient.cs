using System.Net.Http.Json;

namespace NameDemo.Web.Services;

public record GuestNameDto(int Id, string Name, string LastName, DateTime CreatedAt);

public record NameRequest(string Name, string LastName);

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
            new NameRequest(name, lastName),
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateNameAsync(int id, string name, string lastName, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"api/names/{id}",
            new NameRequest(name, lastName),
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
