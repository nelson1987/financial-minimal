
namespace Financial.Manager.Application;
public static class CreateDeposito
{
    public record Command(string Cliente = "Sra.Silva", decimal Valor = 200);
    public record Response();
    public record Handler
    {
        private readonly IContaBancarioRepository _repository;
        private readonly INotificationService _eventService;

        public Handler(IContaBancarioRepository repository,
            INotificationService notification)
        {
            _repository = repository;
            _eventService = notification;
        }

        public void Depositar(Command command)
        {
            var conta = _repository.Get(command.Cliente);
            conta.Depositar(command.Valor);
            _repository.Update(conta);
            _eventService.Send(new Event());
        }
    }
    public record Validator();
    public record Event() : IEvent;
    public static void DependencieInjection() { }
}

public interface IEvent { }
public interface INotificationService
{
    void Send(IEvent @event);
}

public interface IContaBancarioRepository
{
    ContaBancaria Get(string cliente);
    void Update(ContaBancaria conta);
}

public class ContaBancaria
{
    public IList<Movimentacao>? Movimentacoes { get; }
    public decimal Saldo { get { return Movimentacoes!.Sum(x => x.Valor); } }
    public void Depositar(decimal valor)
    {
        Movimentacoes?.Add(new Movimentacao
        {
            Valor = valor,
            CreatedAt = DateTime.Now
        });
    }
}

public class Movimentacao
{
    public DateTime CreatedAt { get; set; }
    public decimal Valor { get; set; }
}