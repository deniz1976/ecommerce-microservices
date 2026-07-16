---
id: ai-instructions
type: ai-guidance
version: 1
status: active
tags:
- ai
- documentation
- knowledge-graph
related:
- ai-project-index
- ai-architecture
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

This file defines how AI agents must work in this repository.

# Required Workflow

1. Read [[docs/ai/README#Purpose]] first.
2. Read [[docs/ai/00_PROJECT_INDEX#Repository Map]] before changing code.
3. Prefer documentation-guided exploration before broad source-code scanning.
4. Read only source code relevant to the requested change.
5. Keep documentation synchronized with code changes.
6. Update the nearest service `AI.md` whenever service architecture changes.
7. Update [[docs/ai/09_DECISIONS#Decision Records]] whenever an architectural decision changes.
8. Update [[docs/ai/04_EVENTS#Message Contracts]] whenever command/event contracts change.
9. Update [[docs/ai/03_DATABASES#Database Catalog]] whenever tables, migrations, or database ownership change.
10. Update [[docs/ai/05_APIS#API Catalog]] whenever endpoint behavior changes.

# Documentation Rules

- Every AI documentation file must start with YAML front matter.
- Do not remove stable headings from service documents.
- Use internal references like `[[03_DATABASES#OrderingDb]]`.
- Leave `TODO` when information is missing instead of guessing.
- Do not write secrets into documentation.
- Keep code identifiers in English.

# Code Exploration Rules

- Start from `ECommerce.sln`, then service-level `AI.md`.
- Use `rg` for targeted source search.
- Do not infer architecture from naming alone when code is available.
- Treat generated `bin`, `obj`, and `.vs` content as non-source.

# Synchronization Rules

- New service: add service `AI.md`, update [[docs/ai/02_SERVICES#Service Catalog]], [[docs/ai/03_DATABASES#Database Catalog]], [[docs/ai/05_APIS#API Catalog]], and [[docs/ai/06_DEPLOYMENT#Runtime Units]].
- New event: add to [[docs/ai/04_EVENTS#Message Contracts]] and relevant service `AI.md`.
- New database/table: add to [[docs/ai/03_DATABASES#Database Catalog]].
- New architectural choice: add to [[docs/ai/09_DECISIONS#Decision Records]].
