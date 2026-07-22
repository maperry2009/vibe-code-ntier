using Microsoft.AspNetCore.WebUtilities;
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
            logger.LogWarning("Webhook URL is not configured; skipping notification.");
            return;
        }

        try
        {
            var url = QueryHelpers.AddQueryString(webhookUrl, new Dictionary<string, string?>
            {
                ["name"] = guestName.Name,
                ["lastName"] = guestName.LastName,
                ["id"] = guestName.Id.ToString(),
                ["createdAt"] = guestName.CreatedAt.ToString("O")
            });

            logger.LogInformation("Calling n8n webhook for guest name {GuestNameId}.", guestName.Id);

            using var response = await httpClient.GetAsync(url, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    "Webhook notification sent for guest name {GuestNameId}. Response: {ResponseBody}",
                    guestName.Id,
                    responseBody);
                return;
            }

            logger.LogWarning(
                "Webhook notification failed for guest name {GuestNameId}. Status: {StatusCode}. Response: {ResponseBody}",
                guestName.Id,
                (int)response.StatusCode,
                responseBody);
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
