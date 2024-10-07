Peço desculpas pela omissão anterior. Vou completar a seção do **README.md** com mais detalhes sobre as funcionalidades relacionadas ao envio de SMS e à configuração do RabbitMQ, conforme sua solicitação.

---

# Mediator Solution

## Descrição

A solução **Mediator** é uma arquitetura de microserviços construída em C# e .NET, utilizando o padrão de arquitetura limpa (Clean Architecture). O Mediator integra várias APIs e um serviço financeiro para o processamento de transações financeiras. A solução utiliza RabbitMQ para mensagens assíncronas, Jaeger para rastreamento distribuído e Elasticsearch/Kibana para indexação e visualização de dados. Além disso, agora está integrado com o serviço de **Zenvia** para envio de notificações via SMS.

## Estrutura da Solução

A solução é composta por várias aplicações principais e camadas de suporte:

### 1. Presentation.API
- **Função**: API principal que processa transações financeiras, publica mensagens no RabbitMQ, indexa as transações no Elasticsearch e dispara SMS via Zenvia.
- **Tecnologias Utilizadas**: .NET 7, MassTransit, RabbitMQ, Elasticsearch, Kibana, OpenTelemetry, Jaeger, Serilog, Zenvia.
- **Endpoints**:
  - `/transaction` - Cria uma nova transação, indexa no Elasticsearch e dispara um SMS se o valor da transação exceder 1000.
  - `/webhook` - Recebe webhooks para processar transações.
  - `/sms/webhook` - Recebe webhooks de SMS via Zenvia.
  - `/sms/search` - Permite consultar logs de SMS enviados, indexados no Elasticsearch.

### 2. SmsService
- **Função**: Serviço responsável pelo envio de notificações SMS usando a API da Zenvia. Também realiza log dos envios e indexa essas informações no Elasticsearch.
- **Tecnologias Utilizadas**: .NET 7, Zenvia, Elasticsearch, MassTransit.
- **Endpoints**:
  - `/sms/webhook` - Recebe notificações de SMS via webhook.
  - `/sms/search` - Busca logs de SMS enviados através do Elasticsearch.

---

## Como Executar

### Pré-requisitos

- **.NET 7 SDK**
- **Docker** (para executar RabbitMQ, Jaeger, Elasticsearch e Kibana)
- **API Token da Zenvia** (para envio de SMS)

### Configuração do Ambiente

#### 0. Criar uma rede virtual no Docker

```sh
sudo docker network create my_network
```

#### 1. Inicializar RabbitMQ

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

#### Criar um Usuário Remoto no RabbitMQ

Para criar um usuário remoto no RabbitMQ, execute os seguintes comandos:

```sh
docker exec -it rabbitmq /bin/bash
rabbitmqctl add_user remote_user 00cc00C@
rabbitmqctl set_permissions -p / remote_user ".*" ".*" ".*"
rabbitmqctl set_user_tags remote_user administrator
```

Isso cria o usuário remoto `remote_user` com permissões administrativas e controle total sobre o RabbitMQ.

#### 2. Inicializar Jaeger

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

```sh
sudo mkdir -p /opt/elasticsearch_data_dir
sudo chown 1000:1000 /opt/elasticsearch_data_dir

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

```sh
sudo mkdir -p /opt/kibana_data_dir
sudo chown 1000:1000 /opt/kibana_data_dir

sudo docker run -d --name kibana \
  --network my_network \
  --network-alias kibana \
  --restart=always \
  -e ELASTICSEARCH_HOSTS=http://elasticsearch:9200 \
  -v /opt/kibana_data_dir:/usr/share/kibana/data \
  -p 5601:5601 \
  docker.elastic.co/kibana/kibana:8.5.0
```

#### 5. Configuração do `appsettings.json`

Adicione as credenciais e URLs necessárias no arquivo `appsettings.json` para que a aplicação possa se comunicar com RabbitMQ, Jaeger, Elasticsearch e Zenvia:

Exemplo para `Presentation.API`:

```json
{
  "RabbitMQ": {
    "Host": "168.138.242.163",
    "UserName": "remote_user",
    "Password": "00cc00C@"
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
  "Zenvia": {
    "ApiToken": "YOUR_ZENVIA_API_TOKEN",
    "RequestUrl": "https://api.zenvia.com/v2/channels/sms/messages",
    "ExternalId": "external-id",
    "From": "SenderName"
  },
  "AllowedHosts": "*"
}
```

---

## Como Rodar

### 1. Iniciar `Presentation.API`

```sh
dotnet run --project ./Presentation.API
```

### 2. Iniciar `Presentation.Receiver`

```sh
dotnet run --project ./Presentation.Receiver
```

### 3. Iniciar `Presentation.ThirdParty`

```sh
dotnet run --project ./Presentation.ThirdParty
```

---

## Configuração do Kibana

1. Acesse o Kibana em `http://localhost:5601`.
2. Navegue até "Stack Management" > "Index Patterns".
3. Crie um novo **Index Pattern** com o nome `transactions`.
4. Selecione o campo de timestamp, se aplicável, ou escolha "No time field".
5. Agora você pode explorar os dados indexados através do Kibana.

---

## Verificação

### 1. **Publicação de Mensagens**:
   - Use a `Presentation.API` para criar transações e verificar se as mensagens estão sendo publicadas no RabbitMQ e indexadas no Elasticsearch.

### 2. **Envio de SMS**:
   - Ao criar transações com valor maior que 1000, a `Presentation.API` disparará um SMS de alerta via Zenvia. Verifique o recebimento do SMS e o log no Elasticsearch.

### 3. **Consumo de Mensagens**:
   - Verifique se a `Presentation.Receiver` está recebendo e processando as mensagens publicadas na `queue_1`.

### 4. **Rastreamento**:
   - Use o Jaeger para monitorar o rastreamento distribuído das transações e das chamadas de APIs.

### 5. **Consulta de Dados no Elasticsearch**:
   - Acesse o Kibana e verifique se as transações e logs de SMS estão indexados corretamente.

---