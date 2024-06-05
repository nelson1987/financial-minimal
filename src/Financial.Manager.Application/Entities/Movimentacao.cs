using Financial.Manager.Application.Entities.Shared;

namespace Financial.Manager.Application.Entities;

public class Movimentacao : EntityBase
{
    public decimal Valor { get; set; }
}