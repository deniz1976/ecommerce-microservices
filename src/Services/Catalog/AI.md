---
id: service-catalog
type: service
version: 1
status: active
tags:
- microservice
- catalog
- products
related:
- database-catalog
- api-catalog
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Own product, category, brand, image, and translation data.

# Responsibilities

- Create, update, and query products.
- Store localized product/category text.
- Keep catalog data separate from stock and orders.

# Dependencies

- [[../../../docs/ai/12_DEPENDENCIES#BuildingBlocks]]
- [[../../../docs/ai/03_DATABASES#CatalogDb]]
- Cloudinary configuration for image provider abstraction.

# Database

See [[../../../docs/ai/03_DATABASES#CatalogDb]].

# APIs

See [[../../../docs/ai/05_APIS#Catalog API]].

Product queries are public. Product creation and update require the shared `Admin` authorization policy.

# Events Published

None.

# Events Consumed

None.

# Important Classes

- `ProductEndpoints`
- `ProductService`
- `ProductRepository`
- `CatalogDbContext`
- `Product`, `Category`, `Brand`

# Folder Structure

- `ECommerce.Catalog.Api`
- `ECommerce.Catalog.Application`
- `ECommerce.Catalog.Domain`
- `ECommerce.Catalog.Infrastructure`

# Configuration

- `ConnectionStrings__CatalogDb`
- `Cloudinary__CloudName`
- `Cloudinary__ApiKey`
- `Cloudinary__ApiSecret`
- `Auth__RoleClaimType`

# Design Decisions

Catalog does not own inventory. See [[../../../docs/ai/09_DECISIONS#decision-database-per-service]]. Catalog mutations remain admin-only until seller/store ownership is modeled.

# Future Improvements

- Complete real image upload workflow.
- Add richer search and filtering.
