using AutoFixture;
using AutoFixture.AutoMoq;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;

namespace Financial.Manager.Tests.Features;
public class CreateDepositoHandlerUnitTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
    private readonly CreateDepositoHandler _handler;
    private CreateDepositoCommand _command;
    private readonly Guid _idContaCliente = Guid.NewGuid();
    private readonly decimal _valorDeposito = 100.25M;
    private readonly ContaCliente contaCliente = new ContaCliente();
    private readonly CancellationToken cancellationToken = CancellationToken.None;

    public CreateDepositoHandlerUnitTests()
    {
        _command = new CreateDepositoCommand(_idContaCliente, _valorDeposito);
        contaCliente = _fixture.Build<ContaCliente>()
            .With(x => x.Id, _idContaCliente)
            .With(x => x.Bloqueada, false)
            .With(x => x.Saldo, 0.00M)
            .Create();

        _fixture.Freeze<Mock<IContaClienteRepository>>()
                .Setup(x => x.GetAsync(_idContaCliente, cancellationToken))
                .ReturnsAsync(contaCliente);

        _handler = _fixture.Create<CreateDepositoHandler>();
    }

    [Fact]
    public async Task Dado_Deposito_Valido_Deve_Retornar_Sucesso_Deposito_Realizado()
    {
        //Arrange
        var mensagemEsperada = "Depósito realizado com sucesso.";
        //Act
        var resultado = await _handler.HandleAsync(_command, cancellationToken);
        //Assert
        Assert.Equal(100.25M, contaCliente.Saldo);
        Assert.True(resultado.Sucesso);
        Assert.Equal(mensagemEsperada, resultado.Data);
    }

    [Fact]
    public async Task Dado_Deposito_Invalido_Deve_Retornar_Erro_Conta_Nao_EncontradaAsync()
    {
        //Arrange
        var mensagemEsperada = "Conta não encontrada.";
        _fixture.Freeze<Mock<IContaClienteRepository>>()
                .Setup(x => x.GetAsync(_idContaCliente, cancellationToken))
                .ReturnsAsync((ContaCliente?)null);
        //Act
        var resultado = await _handler.HandleAsync(_command, cancellationToken);
        //Assert
        Assert.Equal(0M, contaCliente.Saldo);
        Assert.False(resultado.Sucesso);
        Assert.Equal(mensagemEsperada, resultado.Data);
    }

    [Fact]
    public async Task Dado_Deposito_Invalido_Deve_Retornar_Erro_Conta_BloqueadaAsync()
    {
        //Arrange
        var mensagemEsperada = "Operação não concluída.";
        var contaCliente = _fixture.Build<ContaCliente>()
            .With(x => x.Id, _idContaCliente)
            .With(x => x.Bloqueada, true)
            .With(x => x.Saldo, 0.00M)
            .Create();
        _fixture.Freeze<Mock<IContaClienteRepository>>()
                .Setup(x => x.GetAsync(_idContaCliente, cancellationToken))
                .ReturnsAsync(contaCliente);
        //Act
        var resultado = await _handler.HandleAsync(_command, cancellationToken);
        //Assert
        Assert.Equal(0M, contaCliente.Saldo);
        Assert.False(resultado.Sucesso);
        Assert.Equal(mensagemEsperada, resultado.Data);
    }

    [Fact(Skip = "Saldo Insuficiente é para Transferência")]
    public async Task Dado_Deposito_Invalido_Deve_Retornar_Erro_Saldo_InsuficienteAsync()
    {
        //Arrange
        var mensagemEsperada = "Saldo insuficiente.";
        var contaCliente = _fixture.Build<ContaCliente>()
            .With(x => x.Id, _idContaCliente)
            .With(x => x.Bloqueada, false)
            .With(x => x.Saldo, 100M)
            .Create();
        _fixture.Freeze<Mock<IContaClienteRepository>>()
                .Setup(x => x.GetAsync(_idContaCliente, cancellationToken))
                .ReturnsAsync(contaCliente);
        //Act
        var resultado = await _handler.HandleAsync(_command, cancellationToken);
        //Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal(mensagemEsperada, resultado.Data);
    }
}

public class CreateDepositoValidatorUnitTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
    private CreateDepositoCommand _command;
    private readonly IValidator<CreateDepositoCommand> _validator;
    private readonly Guid _idContaCliente = Guid.NewGuid();
    private readonly decimal _valorDeposito = 100.25M;

    public CreateDepositoValidatorUnitTests()
    {
        _command = new CreateDepositoCommand(_idContaCliente, _valorDeposito);
        _validator = _fixture.Create<CreateDepositoCommandValidator>();
    }

    [Fact]
    public void Dado_Que_Todos_Os_Campos_Do_Deposito_Estao_Validos_Deve_Validar_Corretamente()
    => _validator
            .TestValidate(_command)
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Dado_Um_Request_De_Deposito_Com_IdContaCliente_Invalido_Deve_Retornar_Falha_Na_Validacao()
    => _validator
        .TestValidate(_command with { IdContaCliente = Guid.Empty })
        .ShouldHaveValidationErrorFor(x => x.IdContaCliente)
        .Only();

    [Fact]
    public void Dado_Um_Request_De_Deposito_Com_Valor_Invalido_Deve_Retornar_Falha_Na_Validacao()
    => _validator
        .TestValidate(_command with { Valor = 0.00M })
        .ShouldHaveValidationErrorFor(x => x.Valor)
        .Only();
}

public class ContaCliente
{
    public Guid Id { get; set; }
    public bool Bloqueada { get; set; }
    public decimal Saldo { get; set; }

    public void Depositar(decimal valor)
    {
        Saldo += valor;
    }
}

public record CreateDepositoCommand(Guid IdContaCliente, decimal Valor);

//public interface IDepositoHandler
//{
//    string Handle(Deposito deposito);
//}

public class CreateDepositoHandler : Handler<string> //: IDepositoHandler
{
    private readonly IContaClienteRepository _contaClienteRepository;
    private readonly IValidator<CreateDepositoCommand> _validator;

    public CreateDepositoHandler(IContaClienteRepository contaClienteRepository,
        IValidator<CreateDepositoCommand> validator)
    {
        _contaClienteRepository = contaClienteRepository;
        _validator = validator;
    }

    public async Task<HandlerResult<string>> HandleAsync(CreateDepositoCommand deposito, CancellationToken cancellationToken = default)
    {

        var validation = await _validator.ValidateAsync(deposito, cancellationToken);
        //if (!validation.IsValid) return validation.ToFailResult();
        if (!validation.IsValid) return Falha(MensagemErro.ContaNaoEncontrada.Mensagem);
        var contaCliente = await _contaClienteRepository.GetAsync(deposito.IdContaCliente, cancellationToken);
        if (contaCliente == null) return Falha(MensagemErro.ContaNaoEncontrada.Mensagem);
        if (contaCliente.Bloqueada) return Falha(MensagemErro.ContaBloqueada.Mensagem);
        //if (contaCliente.Saldo < deposito.Valor) return MensagemErro.SaldoInsuficiente.Mensagem;
        contaCliente.Depositar(deposito.Valor);
        return Sucesso(MensagemSucesso.DepositoRealizado.Mensagem);
    }
}

public abstract class Handler<T> where T : class
{
    public static HandlerResult<T> Sucesso(T message)
    {
        return new HandlerResult<T>() { Sucesso = true, Data = message };
    }
    public static HandlerResult<T> Falha(T message)
    {
        return new HandlerResult<T>() { Sucesso = false, Data = message };
    }
    //public static HandlerResult<T> Falha(FluentValidation.Results.ValidationResult validationResult)
    //{
    //    return new HandlerResult<T>() { Sucesso = false, Data = message };
    //}
}

public class HandlerResult<T> where T : class
{
    public bool Sucesso { get; set; }
    public required T Data { get; set; }
}

public interface IContaClienteRepository
{
    Task<ContaCliente?> GetAsync(Guid idContaCliente, CancellationToken cancellationToken = default);
}

public static class MensagemSucesso
{
    public static class DepositoRealizado
    {
        public static string Mensagem => "Depósito realizado com sucesso.";
    }
}

public static class MensagemErro
{
    public static class ComandoInvalido
    {
        public static string Mensagem => "Comando inválido.";
    }
    public static class ContaNaoEncontrada
    {
        public static string Mensagem => "Conta não encontrada.";
    }
    public static class ContaBloqueada
    {
        public static string Mensagem => "Operação não concluída.";
    }
    public static class SaldoInsuficiente
    {
        public static string Mensagem => "Saldo insuficiente.";
    }
}

public class CreateDepositoCommandValidator : AbstractValidator<CreateDepositoCommand>
{
    public CreateDepositoCommandValidator()
    {
        RuleFor(x => x.IdContaCliente).NotEmpty();
        RuleFor(x => x.Valor).NotEmpty();
    }
}