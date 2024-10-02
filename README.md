# Mediator Solution

## Descrição

A solução **Mediator** é uma arquitetura de microserviços construída em C# e .NET, utilizando o padrão de arquitetura limpa (Clean Architecture). O Mediator integra várias APIs e um serviço financeiro para o processamento de transações financeiras. A solução utiliza RabbitMQ para mensagens assíncronas, Jaeger para rastreamento distribuído e Elasticsearch/Kibana para indexação e visualização de dados.

## Estrutura da Solução

A solução é composta por três aplicações principais e várias camadas de suporte:

### 1. Presentation.API
- **Função:** API principal que processa transações financeiras, publica mensagens no RabbitMQ, e indexa as transações no Elasticsearch.
- **Tecnologias Utilizadas:** .NET 7, MassTransit, RabbitMQ, Elasticsearch, Kibana, OpenTelemetry, Jaeger, Serilog.
- **Endpoints:**
  - `/transaction` - Cria uma nova transação e a indexa no Elasticsearch.
  - `/webhook` - Recebe webhooks para processar transações.
  - `/search` - Permite consultar transações indexadas no Elasticsearch.

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
  - `TransactionService` - Serviço principal que manipula transações e indexa no Elasticsearch.
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
- **Docker** (para executar RabbitMQ, Jaeger, Elasticsearch e Kibana)

### Configuração do Ambiente

#### 0. Criar uma rede virtual no docker

```sh
sudo docker network create my_network
```

#### 1. Inicializar RabbitMQ

Inicie o RabbitMQ em um container Docker:

```sh
sudo docker run -d \
  --name rabbitmq \
  --network my_network \
  --network-alias rabbitmq \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=guest \
  -v /opt/rabbit_mq_data_dir:/var/lib/rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  --restart always \
  rabbitmq:3-management
```

#### 2. Inicializar Jaeger

Inicie o Jaeger em um container Docker:

```sh
sudo docker run -d --name jaeger \
  --network my_network \
  --network-alias jaeger \
  --restart=always \
  -v /opt/jaeger_data_dir:/var/lib/jaeger \
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

#### 3. Inicializar Elasticsearch

Crie o diretório para os dados persistentes do Elasticsearch:

```sh
sudo mkdir -p /opt/elasticsearch_data_dir
sudo chown 1000:1000 /opt/elasticsearch_data_dir
```

Inicie o Elasticsearch em um container Docker:

```sh
sudo docker run -d --name elasticsearch \
  --network my_network \
  --network-alias elasticsearch \
  --restart=always \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  -e "ES_JAVA_OPTS=-Xms1g -Xmx1g" \
  -v /opt/elasticsearch_data_dir:/usr/share/elasticsearch/data \
  -p 9200:9200 \
  -p 9300:9300 \
  docker.elastic.co/elasticsearch/elasticsearch:8.5.0
```

#### 4. Inicializar Kibana

Crie o diretório para os dados persistentes do Kibana:

```sh
sudo mkdir -p /opt/kibana_data_dir
sudo chown 1000:1000 /opt/kibana_data_dir
```

Inicie o Kibana em um container Docker:

```sh
sudo docker run -d --name kibana \
  --network my_network \
  --network-alias kibana \
  --restart=always \
  -e ELASTICSEARCH_HOSTS=http://elasticsearch:9200 \
  -v /opt/kibana_data_dir:/usr/share/kibana/data \
  -p 5601:5601 \
  docker.elastic.co/kibana/kibana:8.5.0
```

#### 5. Configurar `appsettings.json`

Certifique-se de que cada aplicação tem um arquivo `appsettings.json` configurado corretamente, apontando para os serviços do RabbitMQ, Jaeger, e Elasticsearch.

Exemplo para `Presentation.API`:

```json
{
  "RabbitMQ": {
    "Host": "rabbitmq",
    "UserName": "guest",
    "Password": "guest"
  },
  "Elasticsearch": {
    "Url": "http://elasticsearch:9200",
    "IndexName": "transactions"
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

### Configuração do Kibana

1. Acesse o Kibana em `http://localhost:5601`.
2. Navegue até "Stack Management" > "Index Patterns".
3. Crie um novo Index Pattern com o nome do índice `transactions`.
4. Selecione o campo de timestamp, se aplicável, ou escolha "No time field".
5. Agora você pode explorar os dados indexados através do Kibana.

### Verificação

1. **Publicação de Mensagens:**
   - Utilize a `Presentation.API` para criar transações e verificar se as mensagens estão sendo publicadas no RabbitMQ e indexadas no Elasticsearch.

2. **Consumo de Mensagens:**
   - Verifique se a `Presentation.Receiver` está recebendo e processando as mensagens publicadas no `queue_1`.

3. **Rastreamento:**
   - Utilize o Jaeger para monitorar e rastrear as transações e fluxos dentro do sistema.

4. **Consulta de Dados no Elasticsearch:**
   - Acesse o Kibana e verifique se as transações estão indexadas corretamente no Elasticsearch.


## Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para mais detalhes.