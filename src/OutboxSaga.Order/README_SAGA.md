# Documentação do Fluxo SAGA e Responsabilidades dos Serviços

## Visão Geral do Padrão SAGA
O padrão SAGA é uma abordagem para gerenciar transações distribuídas em sistemas de microsserviços. Ele garante consistência eventual por meio de uma sequência de eventos e compensações, ao invés de transações distribuídas tradicionais.

## Fluxo SAGA no Contexto do Projeto

### 1. Início do Pedido
- **Serviço:** Order API
- **Responsabilidade:** Receber requisições de criação de pedido, validar dados e iniciar a transação SAGA.
- **Ação:** Cria o pedido e grava uma mensagem na tabela Outbox para iniciar o fluxo de eventos.

### 2. Outbox Pattern
- **Serviço:** Order API (camada de infraestrutura)
- **Responsabilidade:** Garantir que eventos de domínio (ex: PedidoCriado) sejam persistidos junto com o pedido, de forma atômica.
- **Ação:** Grava a mensagem de evento na Outbox junto com o pedido na mesma transação.

### 3. Processador de Outbox
- **Serviço:** Order Outbox Processor (serviço background ou worker)
- **Responsabilidade:** Ler mensagens não publicadas da Outbox, publicar em um barramento de eventos (ex: Azure Service Bus, Kafka) e marcar como publicadas.
- **Ação:** Garante entrega confiável dos eventos para outros serviços.

### 4. Orquestração/Coordenação
- **Serviço:** Coordinator (futuro)
- **Responsabilidade:** Orquestrar o fluxo SAGA, recebendo eventos e disparando comandos para os serviços participantes (ex: Payment, Shipping).
- **Ação:** Controla o estado da transação distribuída, lida com falhas e compensações.

### 5. Serviços Participantes
- **Serviços:** Payment, Shipping, etc.
- **Responsabilidade:** Executar suas etapas do processo (ex: processar pagamento, agendar entrega) e publicar eventos de sucesso/falha.
- **Ação:** Cada serviço reage a eventos, executa sua lógica e publica novos eventos para o SAGA.

### 6. Compensação
- **Serviço:** Todos os participantes
- **Responsabilidade:** Em caso de falha em qualquer etapa, executar ações compensatórias (ex: estornar pagamento, cancelar pedido).
- **Ação:** Implementar lógica de compensação para garantir consistência.

## Diagrama Simplificado

Pedido API → [Outbox] → Outbox Processor → [Event Bus] → Coordinator → Payment/Shipping

## Resumo das Responsabilidades
- **Order API:** Inicia SAGA, grava pedido e evento na Outbox.
- **Outbox Processor:** Publica eventos confiáveis.
- **Coordinator:** Orquestra o fluxo SAGA e gerencia compensações.
- **Payment/Shipping:** Executam etapas do processo e publicam eventos.

---

> **Para TechRecruiters:**
> - O projeto segue padrões modernos de arquitetura (DDD, SAGA, Outbox, Microsserviços).
> - Cada serviço tem responsabilidade única e clara.
> - O fluxo é resiliente, escalável e preparado para falhas.
> - Documentação e separação de camadas facilitam onboarding e manutenção.
