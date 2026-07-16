---
id: ai-coding-rules
type: coding-rules
version: 1
status: active
tags:
- coding
- dotnet
- conventions
related:
- ai-architecture
- ai-instructions
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Define repository coding conventions for AI agents and developers.

# Coding Rules

- Code identifiers are English.
- Chat and explanatory documentation may be Turkish or English depending on audience.
- Service business logic stays inside the owning service.
- BuildingBlocks must not contain service-specific domain logic.
- Contracts shared by services belong in `ECommerce.BuildingBlocks.Contracts`.
- Persistence implementations stay in Infrastructure projects.
- Application projects define use cases and interfaces.
- Domain projects define entities and domain state.
- API projects define HTTP endpoints and host startup.
- `Program.cs` files are composition roots only: register dependencies, compose middleware, map endpoints, and start the host.
- Do not declare classes, records, interfaces, or reusable local functions in `Program.cs`; place them in responsibility-based files and folders.
- Repeated host registration and middleware behavior belongs in focused extension classes.
- Application services use `ILogger<T>` for operational logging. Command-line tools report success or failure through process exit codes and must not emit routine `Console` output.

# Testing Rules

- Add focused unit tests for domain/application changes.
- Use contract tests for shared message contracts.
- Use `tools/ECommerce.RuntimeChecks` for runtime workflow verification.

# Configuration Rules

- No real secrets in tracked files.
- Runtime secrets come from Infisical or process environment.
- `.env.example` may list keys only.

# Documentation Rules

- Update `/docs/ai` when architecture changes.
- Update service `AI.md` when service responsibilities, APIs, events, or database ownership changes.
- Keep YAML front matter valid.

# TODO

- Add analyzer rules beyond current project defaults.
