---
id: ai-roadmap
type: roadmap
version: 1
status: active
tags:
- roadmap
- backlog
- improvements
related:
- ai-decisions
- ai-services
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Track known future work without inventing completed behavior.

# Roadmap

## Authentication Completion

- Completed: authenticated `Customer`/`Seller` onboarding synchronizes Auth0 role membership through a dedicated Management API M2M client and refreshes the browser token.
- Add reconciliation/outbox handling for an Auth0-success/local-database-failure edge case.
- Completed: Basket, Ordering, and Notification enforce authenticated local-customer ownership through Identity resolution, with explicit `Admin` and `customer:act` delegation.
- Add seller/store ownership before enabling `SellerOrAdmin` Catalog mutations.

## Runtime Verification

- Completed: the OIDC-protected `all` runtime scenario passed against Infisical `staging` and the isolated Neon `runtime-integration` branch on 2026-07-19.

## Observability

- Choose durable production trace and log storage, retention, TLS, and authentication.
- Extend the active SDK attribute redaction with Collector/Alloy body-content rules and an approved personal-data allow-list for production.
- Add deployment-platform machine identity and Infisical secret injection without committing the Grafana Cloud token; GitHub Actions runtime verification now uses its own OIDC machine identity, but the hosting platform remains pending.
- Re-evaluate Grafana Alloy or another Collector deployment when production reliability, redaction, enrichment, or sampling requires a central pipeline.

## CI/CD

- Add secret-safe deployment process.

## Service Features

- Catalog: image upload integration completion.
- Basket: managed Redis production verification.
- Ordering: richer order status history.
- Payment: real provider integration.
- Notification: email/SMS/push channels.

# TODO

- Prioritize roadmap items.
- Add owners and target milestones.
