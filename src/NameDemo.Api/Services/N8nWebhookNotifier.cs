using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NameDemo.Data;

namespace NameDemo.Api.Services;

public class N8nWebhookNotifier(
    HttpClient httpClient,
    IOptions<WebhookOptions> options,
    ILogger<N8nWebhookNotifier> logger) : IWebhookNotifier
{
    public async Task TryNotifyGuestNameSavedAsync(
        GuestName guestName,
        CancellationToken cancellationToken = default)
    {
        var webhookUrl = options.Value.Url;
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            logger.LogDebug("Webhook URL is not configured; skipping notification.");
            return;
        }

        try
        {
            var payload = new
            {
                guestName.Id,
                guestName.Name,
                guestName.CreatedAt,
                Event = "guest_name.created"
            };

            using var response = await httpClient.PostAsJsonAsync(webhookUrl, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    "Webhook notification sent for guest name {GuestNameId}.",
                    guestName.Id);
                return;
            }

            logger.LogWarning(
                "Webhook notification failed for guest name {GuestNameId}. Status: {StatusCode}",
                guestName.Id,
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Webhook notification failed for guest name {GuestNameId}.",
                guestName.Id);
        }
    }
}
