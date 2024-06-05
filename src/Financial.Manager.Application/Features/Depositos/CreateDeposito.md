# Create Deposito
## 1.Fluxo Normal
##### O cliente decide realizar um deposito, no valor de R$ 100,25.
##### O sistema valida se a conta do Cliente existe.
##### O sistema valida se a conta do Cliente está disponível para a operação.
##### O sistema valida se a conta tem o valor do depósito + taxa de depósito.
##### O sistema realiza a operação
##### O sistema envia uma mensagem de que o depósito foi realizado com sucesso para o cliente.
## 2.Fluxo de Exceção
| Problema | Mensagem |
| ------------- | ------------- |
| Conta do cliente não foi encontrada | A conta não foi encontrada. |
| Conta do cliente está bloqueada | A operação não pode ser concluída. |
| Conta do cliente não tem saldo para a operação + Taxa | O saldo é insuficiente. |
