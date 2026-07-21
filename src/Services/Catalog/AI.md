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

Own store, product, category, brand, image, and translation data.

# Responsibilities

- Create, update, and query products.
- Create seller-owned stores and enforce store ownership on seller product writes.
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

Product and store-by-id queries are public. Product creation/update and store creation/list-mine require `SellerOrAdmin`. A seller must target a store whose `owner_user_id` matches the local Identity `Guid` resolved from the caller's Bearer token. Admin may manage any store product and legacy platform products with no store.

Public product queries explicitly use `AllowAnonymous`; the shared fallback policy protects any new Catalog route unless it is deliberately opened.

# Events Published

None.

# Events Consumed

None.

# Important Classes

- `ProductEndpoints`
- `StoreEndpoints`
- `ProductService`
- `ProductStoreAccessValidator`
- `ProductReferenceValidator`
- `ProductImageAttacher`
- `StoreService`
- `ProductRepository`
- `CatalogDbContext`
- `Product`, `Store`, `Category`, `Brand`

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
- `IdentityClient__BaseUrl`
- `IdentityClient__TimeoutSeconds`

# Design Decisions

Catalog does not own inventory. See [[../../../docs/ai/09_DECISIONS#decision-database-per-service]]. Seller ownership uses the local Identity user `Guid`, never a request-body owner or external Auth0 `sub`. Existing products remain valid with nullable `store_id`; sellers cannot mutate those platform products.

`ProductService` coordinates the use case. Store authorization, category/brand/translation validation, and image verification/attachment are separate injected responsibilities so each policy can change and be tested independently.

# Future Improvements

- Complete real image upload workflow.
- Add richer search and filtering.
