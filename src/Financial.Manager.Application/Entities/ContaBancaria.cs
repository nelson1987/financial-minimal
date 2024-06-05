namespace Financial.Manager.Application.Entities;

public class ContaBancaria
{
    public IList<Movimentacao>? Movimentacoes { get; }
    public decimal Saldo { get { return Movimentacoes!.Sum(x => x.Valor); } }
    public async Task Depositar(decimal valor)//, CancellationToken cancellationToken = default)
    {
        Movimentacoes?.Add(new Movimentacao
        {
            Valor = valor,
            CreatedAt = DateTime.Now
        });
        await Task.CompletedTask;
    }
}
