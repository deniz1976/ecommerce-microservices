---
id: ai-services
type: service-catalog
version: 1
status: active
tags:
- services
- catalog
- graph
related:
- ai-architecture
- ai-databases
- ai-events
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Catalog all runtime services in this repository.

# Service Catalog

## ApiGateway

- id: `service-api-gateway`
- path: `src/ApiGateways/ECommerce.ApiGateway`
- type: Ocelot gateway
- database: none
- details: [[../../src/ApiGateways/ECommerce.ApiGateway/AI#Purpose]]

## Catalog

- id: `service-catalog`
- path: `src/Services/Catalog`
- database: [[03_DATABASES#CatalogDb]]
- APIs: [[05_APIS#Catalog API]]
- details: [[../../src/Services/Catalog/AI#Purpose]]

## Basket

- id: `service-basket`
- path: `src/Services/Basket`
- database: [[03_DATABASES#BasketDb]]
- cache: [[03_DATABASES#Redis]]
- APIs: [[05_APIS#Basket API]]
- details: [[../../src/Services/Basket/AI#Purpose]]

## Ordering

- id: `service-ordering`
- path: `src/Services/Ordering`
- database: [[03_DATABASES#OrderingDb]]
- publishes: [[04_EVENTS#OrderSubmitted]], [[04_EVENTS#OrderConfirmed]], [[04_EVENTS#OrderCancelled]]
- consumes: [[04_EVENTS#OrderConfirmed]], [[04_EVENTS#OrderCancelled]]
- APIs: [[05_APIS#Ordering API]]
- details: [[../../src/Services/Ordering/AI#Purpose]]

## Inventory

- id: `service-inventory`
- path: `src/Services/Inventory`
- database: [[03_DATABASES#InventoryDb]]
- consumes: [[04_EVENTS#ReserveInventory]], [[04_EVENTS#ReleaseInventory]]
- publishes: [[04_EVENTS#InventoryReserved]], [[04_EVENTS#InventoryReservationFailed]]
- APIs: [[05_APIS#Inventory API]]
- details: [[../../src/Services/Inventory/AI#Purpose]]

## Payment

- id: `service-payment`
- path: `src/Services/Payment`
- database: [[03_DATABASES#PaymentDb]]
- consumes: [[04_EVENTS#AuthorizePayment]], [[04_EVENTS#RefundPayment]]
- publishes: [[04_EVENTS#PaymentAuthorized]], [[04_EVENTS#PaymentFailed]]
- details: [[../../src/Services/Payment/AI#Purpose]]

## Shipping

- id: `service-shipping`
- path: `src/Services/Shipping`
- database: [[03_DATABASES#ShippingDb]]
- consumes: [[04_EVENTS#CreateShipment]]
- publishes: [[04_EVENTS#ShipmentCreated]], [[04_EVENTS#ShipmentFailed]]
- details: [[../../src/Services/Shipping/AI#Purpose]]

## Notification

- id: `service-notification`
- path: `src/Services/Notification`
- database: [[03_DATABASES#NotificationDb]]
- consumes: order, payment, and shipment events from [[04_EVENTS#Message Contracts]]
- APIs: [[05_APIS#Notification SignalR]]
- details: [[../../src/Services/Notification/AI#Purpose]]

## Identity

- id: `service-identity`
- path: `src/Services/Identity`
- database: [[03_DATABASES#IdentityDb]]
- APIs: [[05_APIS#Identity API]]
- details: [[../../src/Services/Identity/AI#Purpose]]

## OrderingSaga

- id: `service-ordering-saga`
- path: `src/Services/OrderingSaga`
- database: [[03_DATABASES#OrderingSagaDb]]
- consumes and publishes workflow messages from [[04_EVENTS#Order Workflow]]
- details: [[../../src/Services/OrderingSaga/AI#Purpose]]
