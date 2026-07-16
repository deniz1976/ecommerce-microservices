# Current Gaps and Improvements

## What Is Already Good

The project already has a strong foundation:

- .NET 10 solution structure.
- Clean service boundaries.
- Database-per-service.
- RabbitMQ messaging through MassTransit.
- EF Core migrations.
- Transactional outbox.
- Inbox state tables through MassTransit.
- Saga worker for order workflow.
- Redis basket state.
- Ocelot API gateway.
- SignalR notifications.
- Turkish and English localization foundation.
- OIDC/JWT-ready security hooks.
- Docker Compose local orchestration.
- Validation and smoke test scripts.
- Initial unit and contract tests.

## Biggest Missing Piece

The biggest missing feature is real authentication.

Current status:

- Identity service can register users.
- Password hashing exists.
- JWT/OIDC validation hooks exist.
- Login/token issuing does not exist yet.

Options:

1. Use managed identity provider.
2. Add OpenIddict to Identity service.
3. Use external auth such as Auth0, Keycloak, Azure AD B2C, or Clerk.

Recommendation:

For learning and control, OpenIddict is a good next step. For production speed, managed auth is often better.

## Redis Improvement

Current:

```text
Local Redis container
```

Future:

```text
Managed Redis connection string
```

Why:

- Local container is fine for development.
- Production needs persistence/availability/monitoring.

## Messaging Improvements

Current:

- MassTransit consumers.
- EF inbox/outbox tables.
- Basic event and command contracts.

Improve later:

- Explicit retry policies per consumer.
- Dead-letter queue documentation and monitoring.
- Message versioning strategy beyond `Version = 1`.
- More explicit idempotency records per command.
- Runtime queue topology inspection documentation.
- Consumer integration tests with RabbitMQ container.

## Saga Improvements

Current:

- Durable saga state in PostgreSQL.
- Order workflow orchestration.
- Compensation commands on failure.

Improve later:

- Timeout handling.
- More detailed failure reasons.
- Admin endpoint to inspect workflow state.
- Retry and manual replay tooling.
- Stronger state transition tests.

## Database Improvements

Current:

- EF migrations exist.
- Neon databases are created.
- Database-per-service is respected.

Improve later:

- Add seed data scripts for catalog and inventory.
- Add migration bundle generation.
- Add migration execution to CI/CD.
- Add backup and restore documentation.
- Add indexes based on real query patterns.

## Testing Improvements

Current:

- Contract tests.
- Inventory unit tests.
- Ordering unit tests.
- Smoke test script.

Improve later:

- Integration tests with Testcontainers.
- API tests for each service.
- Contract compatibility tests for messages.
- End-to-end order workflow test with RabbitMQ and PostgreSQL.
- Gateway routing tests.

## Observability Improvements

Current:

- OpenTelemetry wiring exists.

Improve later:

- Central log sink.
- Metrics dashboard.
- Distributed tracing backend.
- Correlation ID propagation verification.
- Alerts for failed consumers and dead-letter queues.

## Security Improvements

Current:

- JWT/OIDC validation hooks.
- Current user abstraction.
- Password hashing in Identity.

Improve later:

- Login endpoint.
- Token issuing.
- Refresh tokens.
- Role-based authorization policies.
- Gateway-level authentication.
- Service-to-service auth.
- Secret manager integration.

## API Improvements

Current:

- Minimal APIs.
- Gateway routes.
- Health checks.

Improve later:

- OpenAPI/Swagger per service.
- API versioning policy.
- Request validation library.
- ProblemDetails consistency.
- Rate limiting at gateway.

## Deployment Improvements

Current:

- Dockerfile.
- Docker Compose.
- Local scripts.
- Secretless GitHub Actions build/test/validation pipeline.
- Gated BuildKit/Compose build for all application container images.

Improve later:

- Protected container publish pipeline.
- Container registry.
- Environment-specific deployment manifests.
- Kubernetes or cloud app platform config.
- Automated migration job.
- Rollback strategy.

## Documentation Improvements

This documentation is the first complete pass.

Future docs to add:

- API request/response examples.
- Full ER diagrams.
- RabbitMQ topology screenshots after Docker runtime works.
- Sequence diagrams per flow.
- Troubleshooting guide.
- Local setup guide from zero.
