# Outbox Saga Lab

Um laboratório de arquitetura distribuída em **.NET 10**, criado para estudar, praticar e registrar decisões envolvendo **Clean Architecture**, **DDD**, **Outbox Pattern**, **MongoDB** e uma base evolutiva para **Sagas** em microsserviços.

A ideia deste projeto é ir um pouco além de uma API CRUD. O foco está em modelar decisões arquiteturais reais: consistência eventual, persistência confiável de eventos, separação de responsabilidades e preparação para comunicação assíncrona entre serviços.

## Objetivo

Este repositório faz parte do meu portfólio técnico e nasceu com uma intenção simples: construir um projeto onde a arquitetura aparece no código, não apenas no desenho.

O cenário base é simples:

- um pedido é criado no serviço de Orders;
- o domínio levanta eventos;
- a Application transforma esses eventos em mensagens de outbox;
- Order e Outbox são persistidos juntos em uma transação;
- futuramente, um worker publicará essas mensagens em um broker;
- outros serviços, como Payment e Shipping, poderão reagir e compor uma saga.

```mermaid
flowchart LR
    Client[Client/API Consumer] --> API[Orders API]
    API --> APP[Application Layer]
    APP --> DOMAIN[Domain Layer]
    APP --> OUTBOX[Outbox Message]
    APP --> UOW[Unit of Work]
    UOW --> MONGO[(MongoDB)]
    MONGO --> WORKER[Outbox Worker]
    WORKER --> BUS[Message Broker]
    BUS --> PAYMENT[Payment Service]
    BUS --> SHIPPING[Shipping Service]
    BUS --> COORDINATOR[Saga Coordinator]
```

## Fluxo De Criação De Pedido

```mermaid
sequenceDiagram
    autonumber
    participant Client
    participant API as Orders API
    participant App as CreateOrderHandler
    participant Domain as Order Aggregate
    participant Outbox as OutboxMessage
    participant UoW as Mongo UnitOfWork
    participant Mongo as MongoDB

    Client->>API: POST /orders
    API->>App: CreateOrderCommand
    App->>Domain: new Order(...)
    Domain-->>App: OrderCreatedDomainEvent
    App->>Outbox: FromDomainEvent(...)
    App->>UoW: ExecuteInTransactionAsync
    UoW->>Mongo: Insert Order
    UoW->>Mongo: Insert OutboxMessage
    UoW-->>App: Commit
    App-->>API: CreateOrderResult
    API-->>Client: 201 Created
```

## Decisão Principal: Transactional Outbox

Em sistemas distribuídos, salvar dados e publicar eventos no broker são duas operações diferentes. Se uma delas falha, o sistema pode ficar inconsistente.

Exemplo do problema:

```text
1. Salva o pedido no banco
2. Falha antes de publicar OrderCreated no broker
3. Payment e Shipping nunca ficam sabendo do pedido
```

O Outbox Pattern resolve isso persistindo o evento no mesmo banco e na mesma transação do aggregate.

```text
Order + OutboxMessage = mesma transação
```

Depois, um worker independente lê a outbox e publica no broker com retry, controle de falhas e observabilidade.

## Camadas

```text
src/
  OutboxSaga.Order/
    OutboxSaga.Order.Api/
    OutboxSaga.Order.Application/
    OutboxSaga.Order.Domain/
    OutboxSaga.Order.Infrastructure/
```

### Domain

Contém o modelo de negócio puro.

Responsabilidades:

- aggregates;
- value objects;
- domain events;
- regras e invariantes;
- transições válidas de estado.

Exemplos:

- `Order`
- `OrderCustomer`
- `Money`
- `OrderStatus`
- `OrderCreatedDomainEvent`

O domínio não conhece MongoDB, HTTP, Service Bus ou qualquer detalhe externo.

### Application

Orquestra casos de uso.

Responsabilidades:

- receber commands;
- criar aggregates;
- coletar domain events;
- gerar mensagens de outbox;
- coordenar repositórios e Unit of Work;
- manter as regras de fluxo da aplicação.

Exemplo:

- `CreateOrderHandler`
- `CreateOrderCommand`
- `CreateOrderResult`
- `ICommandHandler<TCommand, TResult>`
- `IOutboxRepository`
- `IUnitOfWork`

### Infrastructure

Implementa detalhes externos.

Responsabilidades:

- MongoDB;
- repositories;
- transação;
- mapeamentos BSON;
- persistência de Orders;
- persistência de Outbox Messages.

Exemplos:

- `MongoContext`
- `MongoUnitOfWork`
- `MongoOrderRepository`
- `MongoOutboxRepository`
- `MongoMappings`

### API

Camada de entrada HTTP.

Responsabilidades:

- endpoints;
- contratos HTTP;
- composition root;
- OpenAPI/Scalar.

A API não deve carregar regra de negócio. Ela traduz request em command e delega para a Application.

## Fluxo De Dependências

```mermaid
flowchart TD
    API --> Application
    Infrastructure --> Application
    Infrastructure --> Domain
    Application --> Domain

    Domain -.-> Nothing[Nenhuma dependência externa]
```

A regra é simples: o domínio fica no centro.

Infraestrutura depende da aplicação, não o contrário. A Application define portas; a Infrastructure implementa adaptadores.

## MongoDB E Transação

O projeto usa MongoDB para persistir:

- orders;
- outbox messages.

A transação é coordenada por `MongoUnitOfWork`, garantindo que o pedido e sua mensagem de outbox sejam persistidos juntos.

```mermaid
flowchart LR
    Handler[CreateOrderHandler] --> UOW[MongoUnitOfWork]
    UOW --> S[Mongo Session]
    S --> O[orders collection]
    S --> OB[outbox_messages collection]
    S --> C[Commit]
```

Observação: transações no MongoDB exigem replica set ou cluster. Em MongoDB Atlas isso já faz sentido para o cenário do projeto.

## Por Que O AggregateRoot Guarda Eventos?

O aggregate é quem sabe quando algo relevante aconteceu no domínio.

Quando um `Order` é criado, ele levanta um `OrderCreatedDomainEvent`. Ele não publica no broker, não serializa JSON e não conhece outbox. Ele apenas registra o fato de domínio.

Isso mantém as responsabilidades bem separadas:

```text
Domain: aconteceu algo
Application: transforma em mensagem
Infrastructure: persiste/publica
```

## Stack

- C# / .NET 10
- ASP.NET Core Minimal APIs
- MongoDB Driver
- MongoDB Atlas
- Clean Architecture
- DDD
- Transactional Outbox
- Base para Saga Pattern
- Scalar/OpenAPI

## Como Rodar

Configure a connection string do MongoDB em variável de ambiente ou user-secrets.

Exemplo:

```powershell
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb+srv://user-service-saga-lab:<db_password>@cluster0.lrlk00w.mongodb.net/?appName=Cluster0" --project src/OutboxSaga.Order/OutboxSaga.Order.Api
dotnet user-secrets set "MongoDb:DatabaseName" "OutboxSagaDb" --project src/OutboxSaga.Order/OutboxSaga.Order.Api
```

Depois:

```powershell
dotnet build src/OutboxSaga.Order/OutboxSaga.Order.Api/OutboxSaga.Orders.Api.csproj
dotnet run --project src/OutboxSaga.Order/OutboxSaga.Order.Api/OutboxSaga.Orders.Api.csproj
```

## Próximos Passos

- Implementar o Outbox Worker.
- Publicar eventos em um broker.
- Adicionar Payment Service.
- Adicionar Shipping Service.
- Evoluir o Saga Coordinator.
- Adicionar retries e dead-letter strategy.
- Adicionar observabilidade com logs estruturados, tracing e métricas.
- Criar testes de domínio, application e integração.

## Status

Projeto em evolução.

A base atual já cobre o fluxo de criação de pedido com persistência transacional em MongoDB e registro de mensagens na outbox. O próximo grande passo é processar a outbox e iniciar a comunicação assíncrona entre serviços.
