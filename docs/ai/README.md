---
id: ai-docs-readme
type: documentation-index
version: 1
status: active
tags:
- ai
- docs
- knowledge-graph
related:
- ai-project-index
- ai-instructions
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

This folder is the AI-first memory system for this repository. It is designed so future tooling can generate graph JSON, Neo4j nodes, Mermaid diagrams, PlantUML diagrams, and dependency graphs without rewriting the documentation.

# Reading Order

1. [[00_PROJECT_INDEX#Purpose]]
2. [[01_ARCHITECTURE#Purpose]]
3. [[02_SERVICES#Service Catalog]]
4. [[03_DATABASES#Database Catalog]]
5. [[04_EVENTS#Message Contracts]]
6. [[05_APIS#API Catalog]]
7. [[06_DEPLOYMENT#Runtime Units]]
8. [[07_SECURITY#Security Model]]
9. [[08_CODING_RULES#Coding Rules]]
10. [[09_DECISIONS#Decision Records]]
11. [[10_ROADMAP#Roadmap]]
12. [[11_GLOSSARY#Glossary]]
13. [[12_DEPENDENCIES#Dependency Catalog]]

# Graph Conventions

- `id` values are stable graph node identifiers.
- `related` values are graph edges.
- `type` values describe future node labels.
- Headings are stable parser anchors.
- Cross references use `[[file#Heading]]`.

# Source of Truth

This documentation describes the current repository structure under `C:\Users\deniz\Desktop\microservice`. If code and documentation disagree, update the documentation or code in the same change.
