---
id: service-payment
type: service
version: 1
status: active
tags:
- microservice
- payment
- mock-provider
related:
- database-payment
- command-authorize-payment
- event-payment-authorized
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Own payment authorization, refund state, and transaction audit records.

# Responsibilities

- Authorize payment commands.
- Refund payments during compensation.
- Publish payment success/failure events.
- Store payment transaction history.

# Dependencies

- [[../../../docs/ai/03_DATABASES#PaymentDb]]
- [[../../../docs/ai/04_EVENTS#AuthorizePayment]]
- [[../../../docs/ai/04_EVENTS#RefundPayment]]

# Database

See [[../../../docs/ai/03_DATABASES#PaymentDb]].

# APIs

No public payment HTTP API is currently documented. The service hosts health endpoints.

# Events Published

- [[../../../docs/ai/04_EVENTS#PaymentAuthorized]]
- [[../../../docs/ai/04_EVENTS#PaymentFailed]]

# Events Consumed

- [[../../../docs/ai/04_EVENTS#AuthorizePayment]]
- [[../../../docs/ai/04_EVENTS#RefundPayment]]

# Important Classes

- `PaymentService`
- `AuthorizePaymentConsumer`
- `RefundPaymentConsumer`
- `PaymentRepository`
- `PaymentDbContext`

# Folder Structure

- `ECommerce.Payment.Api`
- `ECommerce.Payment.Application`
- `ECommerce.Payment.Domain`
- `ECommerce.Payment.Infrastructure`

# Configuration

- `ConnectionStrings__PaymentDb`
- `RabbitMq__ConnectionString`

# Design Decisions

Payment behavior is mock. See [[../../../docs/ai/10_ROADMAP#Service Features]].

# Future Improvements

- Integrate real payment provider.
- Add webhook processing.
