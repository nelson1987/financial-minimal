
using AutoMapper;
using Financial.Manager.Application.Extensions;
using Financial.Manager.Application.Features.Shared;
using Financial.Manager.Application.Repositories;
using Financial.Manager.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Financial.Manager.Application.Features.Depositos;
public static class CreateDeposito
{
    public record Command(string Cliente = "Sra.Silva", decimal Valor = 200);
    public record Response();
    public record Event() : IEvent;
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

        public async Task Depositar(Command command, CancellationToken cancellationToken = default)
        {
            var conta = await _repository.Get(command.Cliente, cancellationToken);
            if (conta == null) throw new Exception("Conta inexistente.");
            await conta.Depositar(command.Valor);//, cancellationToken);
            await _repository.Update(conta, cancellationToken);
            await _eventService.Send(command.MapTo<Event>(), cancellationToken);
        }
    }
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Cliente).NotEmpty();
        }
    }
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<Command, Event>();
        }
    }
    public static IServiceCollection DependencieInjection(this IServiceCollection services)
    {
        services.AddScoped<IContaBancarioRepository, ContaBancarioRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}
