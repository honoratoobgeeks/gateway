# Mediator API

## Descrição

A Mediator API é uma aplicação construída com arquitetura limpa (Clean Architecture) em C# e .NET 7.0. Ela integra diversas APIs e um serviço financeiro para processamento de transações financeiras, utilizando RabbitMQ para mensagens assíncronas e Jaeger para rastreamento distribuído.

## Funcionalidades

- **Processamento de Transações:** Envia e recebe transações financeiras.
- **Webhooks:** Recebe respostas de transações via webhook.
- **Integração com RabbitMQ:** Utiliza RabbitMQ para mensagens assíncronas.
- **Rastreamento com Jaeger:** Implementa rastreamento distribuído utilizando OpenTelemetry e Jaeger.
- **Logging com Serilog:** Configuração de logging utilizando Serilog.

## Estrutura do Projeto

- **Domain:** Contém as entidades e interfaces de domínio.
- **Application:** Contém as interfaces e serviços de aplicação.
- **Infra.Data:** Contém a implementação do contexto do banco de dados e repositórios.
- **Infra.IoC:** Configuração de injeção de dependências.
- **DTO:** Contém objetos de transferência de dados (Data Transfer Objects).
- **Presentation.API:** Contém os controladores da API e a configuração do projeto.

## Tecnologias Utilizadas

- **.NET 7.0**
- **Entity Framework Core**
- **MassTransit**
- **RabbitMQ**
- **OpenTelemetry**
- **Jaeger**
- **Serilog**
- **AutoMapper**

## Pré-requisitos

- **.NET 7 SDK**
- **Docker** (para executar RabbitMQ e Jaeger)

## Como Executar

### 1. Configuração do RabbitMQ

Execute o RabbitMQ em um container Docker:

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
