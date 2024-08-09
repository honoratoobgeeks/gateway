Aqui está um exemplo de como você pode documentar a solução Mediator (anteriormente chamada de Gateway) em um arquivo `README.md`.

```markdown
# Mediator Solution

## Descrição

A solução **Mediator** é uma arquitetura de microserviços construída em C# e .NET, utilizando o padrão de arquitetura limpa (Clean Architecture). O Mediator integra várias APIs e um serviço financeiro para o processamento de transações financeiras. A solução utiliza RabbitMQ para mensagens assíncronas e Jaeger para rastreamento distribuído.

## Estrutura da Solução

A solução é composta por três aplicações principais e várias camadas de suporte:

### 1. Presentation.API
- **Função:** API principal que processa transações financeiras e publica mensagens no RabbitMQ.
- **Tecnologias Utilizadas:** .NET 7, MassTransit, RabbitMQ, OpenTelemetry, Jaeger, Serilog.
- **Endpoints:**
  - `/transaction` - Cria uma nova transação.
  - `/webhook` - Recebe webhooks para processar transações.

### 2. Presentation.Receiver
- **Função:** Consome mensagens de `queue_1` no RabbitMQ e processa as transações recebidas.
- **Tecnologias Utilizadas:** .NET 7, MassTransit, RabbitMQ.
- **Funcionamento:** A aplicação se conecta ao `queue_1` e, ao receber mensagens, executa um `Console.WriteLine` com os dados da mensagem.

### 3. Presentation.ThirdParty
- **Função:** (Explicar a função se aplicável, por exemplo, comunicação com serviços de terceiros).
- **Tecnologias Utilizadas:** (Listar as tecnologias utilizadas).
- **Endpoints:**
  - (Listar os endpoints disponíveis nesta aplicação).

### 4. Application Layer
- **Função:** Contém as regras de negócios e os serviços da aplicação.
- **Componentes:** 
  - `TransactionService` - Serviço principal que manipula transações.
  - DTOs - Objetos de Transferência de Dados (Data Transfer Objects).

### 5. Domain Layer
- **Função:** Contém as entidades e interfaces de domínio da solução.
- **Componentes:**
  - `Transaction` - Entidade que representa uma transação financeira.
  - Interfaces de repositório e serviços.

### 6. Infra.Data Layer
- **Função:** Contém a implementação do contexto do banco de dados e repositórios.
- **Tecnologias Utilizadas:** Entity Framework Core.
- **Componentes:**
  - `MediatorDbContext` - Contexto do banco de dados.
  - `TransactionRepository` - Implementação do repositório de transações.

### 7. Infra.IoC Layer
- **Função:** Configuração de Injeção de Dependências.
- **Componentes:**
  - Configuração dos serviços, repositórios, e contexto do banco de dados.

## Como Executar

### Pré-requisitos

- **.NET 7 SDK**
- **Docker** (para executar RabbitMQ e Jaeger)

### Configuração do Ambiente

#### 1. Inicializar RabbitMQ

Inicie o RabbitMQ em um container Docker:

```sh
sudo docker run -d \
  --name rabbitmq \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=00cc00C@ \
  -v /opt/rabbit_mq_data_dir:/var/lib/rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  --restart always \
  --network my_network \
  --network-alias rabbitmq \
  rabbitmq:management
```

#### 2. Inicializar Jaeger

Inicie o Jaeger em um container Docker:

```sh
sudo docker run -d --name jaeger \
  --network my_network \
  --network-alias jaeger \
  --restart=always \
  -e COLLECTOR_ZIPKIN_HTTP_PORT=9411 \
  -p 5775:5775/udp \
  -p 6831:6831/udp \
  -p 6832:6832/udp \
  -p 5778:5778 \
  -p 16686:16686 \
  -p 14268:14268 \
  -p 14250:14250 \
  -p 9411:9411 \
  jaegertracing/all-in-one:1.26
```

#### 3. Configurar `appsettings.json`

Certifique-se de que cada aplicação tem um arquivo `appsettings.json` configurado corretamente, apontando para os serviços do RabbitMQ e Jaeger.

Exemplo para `Presentation.API`:

```json
{
  "RabbitMQ": {
    "Host": "rabbitmq",
    "UserName": "guest",
    "Password": "guest"
  },
  "OpenTelemetry": {
    "Jaeger": {
      "AgentHost": "jaeger",
      "AgentPort": "6831"
    }
  },
  "AllowedHosts": "*"
}
```

### Como Rodar

1. **Iniciar `Presentation.API`:**

```sh
dotnet run --project ./Presentation.API
```

2. **Iniciar `Presentation.Receiver`:**

```sh
dotnet run --project ./Presentation.Receiver
```

3. **Iniciar `Presentation.ThirdParty`:**

```sh
dotnet run --project ./Presentation.ThirdParty
```

### Verificação

1. **Publicação de Mensagens:**
   - Utilize a `Presentation.API` para criar transações e verificar se as mensagens estão sendo publicadas no RabbitMQ.

2. **Consumo de Mensagens:**
   - Verifique se a `Presentation.Receiver` está recebendo e processando as mensagens publicadas no `queue_1`.

3. **Rastreamento:**
   - Utilize o Jaeger para monitorar e rastrear as transações e fluxos dentro do sistema.

## Contribuição

1. Faça um fork do repositório.
2. Crie uma nova branch (`git checkout -b feature/nova-feature`).
3. Faça commit das suas alterações (`git commit -am 'Adiciona nova feature'`).
4. Faça push para a branch (`git push origin feature/nova-feature`).
5. Crie um novo Pull Request.

## Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para mais detalhes.
```

### Conclusão

Este `README.md` fornece uma visão geral da solução Mediator, detalha como cada componente e aplicação funciona, e instrui os desenvolvedores sobre como configurar e executar o sistema. Ele também inclui informações sobre contribuição e licenciamento, tornando-o um documento completo para orientar o desenvolvimento e uso da solução.