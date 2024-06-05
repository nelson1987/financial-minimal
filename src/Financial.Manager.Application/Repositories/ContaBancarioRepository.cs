using Financial.Manager.Application.Entities;

namespace Financial.Manager.Application.Repositories;

public class ContaBancarioRepository : IContaBancarioRepository
{
    public Task<ContaBancaria> Get(string cliente, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Update(ContaBancaria conta, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
