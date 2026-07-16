---
id: ai-glossary
type: glossary
version: 1
status: active
tags:
- glossary
- terminology
related:
- ai-architecture
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Define graph-stable terminology used in this repository.

# Glossary

## API Gateway

Ocelot service that routes client HTTP requests to backend services. See [[02_SERVICES#ApiGateway]].

## BuildingBlocks

Shared libraries under `src/BuildingBlocks` for contracts, event bus, persistence, localization, observability, and security.

## Command

Message requesting work. Examples: [[04_EVENTS#ReserveInventory]], [[04_EVENTS#AuthorizePayment]].

## Event

Message stating that something happened. Examples: [[04_EVENTS#OrderSubmitted]], [[04_EVENTS#PaymentAuthorized]].

## Inbox

MassTransit storage used to detect processed messages and reduce duplicate processing.

## Outbox

MassTransit storage used to reliably publish messages after database transactions.

## Saga

Long-running workflow coordinator. In this repository: [[02_SERVICES#OrderingSaga]].

## Database Per Service

Pattern where each service owns its own database. See [[09_DECISIONS#decision-database-per-service]].

## Runtime Check

Executable verification under `tools/ECommerce.RuntimeChecks`. See [[06_DEPLOYMENT#Runtime Checks]].
