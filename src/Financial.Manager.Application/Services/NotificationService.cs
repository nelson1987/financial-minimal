using Financial.Manager.Application.Features.Shared;

namespace Financial.Manager.Application.Services;

public class NotificationService : INotificationService
{
    public Task Send(IEvent @event, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}