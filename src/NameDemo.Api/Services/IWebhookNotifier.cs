using NameDemo.Data;

namespace NameDemo.Api.Services;

public interface IWebhookNotifier
{
    Task TryNotifyGuestNameSavedAsync(GuestName guestName, CancellationToken cancellationToken = default);
}
