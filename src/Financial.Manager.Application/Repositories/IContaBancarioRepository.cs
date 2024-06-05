using Financial.Manager.Application.Entities;

namespace Financial.Manager.Application.Repositories;

public interface IContaBancarioRepository
{
    Task<ContaBancaria> Get(string cliente, CancellationToken cancellationToken = default);
    Task Update(ContaBancaria conta, CancellationToken cancellationToken = default);
}
