using Financial.Manager.Application.Features.Shared;

namespace Financial.Manager.Application.Services;

public interface INotificationService
{
    Task Send(IEvent @event, CancellationToken cancellationToken = default);
}
