# ECommerce Microservices Documentation

This folder explains the project in plain language. The goal is to make the system understandable even if the reader is new to microservices, messaging, Docker, Redis, PostgreSQL, and the outbox/inbox patterns.

Turkish documentation is available here: [Turkish Documentation](./tr/README.md).

AI-first graph-ready documentation is available here: [AI Documentation](./ai/README.md).

## Reading Order

1. [Architecture Overview](./architecture-overview.md)
2. [Services and Responsibilities](./services-and-responsibilities.md)
3. [Data Model and Tables](./data-model-and-tables.md)
4. [Messaging, RabbitMQ, Inbox, and Outbox](./messaging-inbox-outbox.md)
5. [Main Business Flows](./main-business-flows.md)
6. [Scripts and Local Commands](./scripts-and-local-commands.md)
7. [Runtime and Operations](./runtime-and-operations.md)
8. [API Examples](./api-examples.md)
9. [Diagrams](./diagrams.md)
10. [RabbitMQ Topology](./rabbitmq-topology.md)
11. [Local Setup From Zero](./local-setup-from-zero.md)
12. [Service Deep Dive](./service-deep-dive.md)
13. [Table Field Reference](./table-field-reference.md)
14. [Glossary](./glossary.md)
15. [Patterns Used](./patterns-used.md)
16. [Troubleshooting](./troubleshooting.md)
17. [Architecture Decision Log](./architecture-decision-log.md)
18. [Current Gaps and Improvements](./current-gaps-and-improvements.md)
19. [API Security Baseline](./security-baseline.md)

## Project in One Paragraph

This is a .NET 10 e-commerce backend built as independent microservices. Each business capability has its own service and database. Services talk synchronously through HTTP when needed, and asynchronously through RabbitMQ events and commands for workflow steps such as order submission, inventory reservation, payment authorization, shipment creation, and notifications. PostgreSQL stores durable business data, Redis stores active basket state, RabbitMQ carries integration messages, Ocelot acts as the API gateway, and MassTransit provides messaging, consumer wiring, inbox state, and transactional outbox support.

## High-Level Components

```text
Client
  |
  v
Ocelot API Gateway
  |
  +--> Catalog API
  +--> Basket API
  +--> Ordering API
  +--> Inventory API
  +--> Identity API
  +--> Notification SignalR Hub

RabbitMQ
  |
  +--> Ordering Saga Worker
  +--> Inventory API
  +--> Payment API
  +--> Shipping API
  +--> Notification API

PostgreSQL
  |
  +--> one database per service

Redis
  |
  +--> active basket state
```

## Current Status

Implemented:

- Solution structure and shared building blocks.
- Catalog, Basket, Ordering, Inventory, Payment, Shipping, Notification, Identity, Ordering Saga Worker, and API Gateway.
- Database-per-service PostgreSQL persistence.
- Redis-backed basket state with local in-memory fallback.
- RabbitMQ messaging through MassTransit.
- EF Core migrations for all service databases.
- Transactional outbox support for message publishing.
- Inbox state through MassTransit EF integration for consumer services.
- Ocelot gateway routing.
- SignalR notification hub.
- Local Docker Compose orchestration.
- Smoke test and validation scripts.
- Initial unit and contract tests.

Not fully implemented yet:

- Real login/token issuance.
- Production Redis provider.
- Full integration test suite.
- Protected container publish/deployment pipeline; secretless build/test/validation CI is present.
- Production-grade observability dashboards.
