import type { Dictionary } from "@/lib/i18n/dictionaries"
import { getOrderStatusName } from "@/lib/orders/status"
import type {
  OrderStatus,
  OrderWorkflowStatus,
  PaymentStatus,
  ProductStatus,
  ShipmentStatus,
  StockMovementKind,
  UserStatus,
} from "@/types"

/**
 * Visual tone shared by every status surface. Keeps enum values from every
 * service mapped to a single, bounded set of colours.
 */
export type StatusTone = "neutral" | "progress" | "success" | "warning" | "danger"

type StatusDictionary = Dictionary["status"]

/**
 * Order status labels already live under `orders.status`, keyed by
 * `OrderStatusName`. This wrapper keeps every status surface on one call shape
 * without duplicating those translations.
 */
export function orderStatusLabel(
  status: OrderStatus,
  labels: Dictionary["orders"]["status"],
): string {
  return labels[getOrderStatusName(status)]
}

export function orderStatusTone(status: OrderStatus): StatusTone {
  const tones: Record<OrderStatus, StatusTone> = {
    0: "neutral",
    1: "progress",
    2: "progress",
    3: "progress",
    4: "success",
    5: "danger",
    6: "warning",
  }
  return tones[status]
}

export function paymentStatusLabel(status: PaymentStatus, t: StatusDictionary): string {
  const labels: Record<PaymentStatus, string> = {
    1: t.payment.authorized,
    2: t.payment.failed,
    3: t.payment.refunded,
  }
  return labels[status]
}

export function paymentStatusTone(status: PaymentStatus): StatusTone {
  const tones: Record<PaymentStatus, StatusTone> = {
    1: "success",
    2: "danger",
    3: "warning",
  }
  return tones[status]
}

export function shipmentStatusLabel(status: ShipmentStatus, t: StatusDictionary): string {
  const labels: Record<ShipmentStatus, string> = {
    1: t.shipment.created,
    2: t.shipment.failed,
    3: t.shipment.inTransit,
    4: t.shipment.delivered,
  }
  return labels[status]
}

export function shipmentStatusTone(status: ShipmentStatus): StatusTone {
  const tones: Record<ShipmentStatus, StatusTone> = {
    1: "neutral",
    2: "danger",
    3: "progress",
    4: "success",
  }
  return tones[status]
}

export function productStatusLabel(status: ProductStatus, t: StatusDictionary): string {
  const labels: Record<ProductStatus, string> = {
    0: t.product.draft,
    1: t.product.active,
    2: t.product.inactive,
    3: t.product.archived,
  }
  return labels[status]
}

export function productStatusTone(status: ProductStatus): StatusTone {
  const tones: Record<ProductStatus, StatusTone> = {
    0: "neutral",
    1: "success",
    2: "warning",
    3: "danger",
  }
  return tones[status]
}

export function userStatusLabel(status: UserStatus, t: StatusDictionary): string {
  const labels: Record<UserStatus, string> = {
    1: t.user.active,
    2: t.user.disabled,
  }
  return labels[status]
}

export function userStatusTone(status: UserStatus): StatusTone {
  const tones: Record<UserStatus, StatusTone> = {
    1: "success",
    2: "danger",
  }
  return tones[status]
}

export function workflowStatusLabel(status: OrderWorkflowStatus, t: StatusDictionary): string {
  const labels: Record<OrderWorkflowStatus, string> = {
    1: t.workflow.submitted,
    2: t.workflow.inventoryReserved,
    3: t.workflow.paymentAuthorized,
    4: t.workflow.shipmentCreated,
    5: t.workflow.completed,
    6: t.workflow.cancelled,
  }
  return labels[status]
}

export function workflowStatusTone(status: OrderWorkflowStatus): StatusTone {
  const tones: Record<OrderWorkflowStatus, StatusTone> = {
    1: "neutral",
    2: "progress",
    3: "progress",
    4: "progress",
    5: "success",
    6: "danger",
  }
  return tones[status]
}

export function stockMovementLabel(kind: StockMovementKind, t: StatusDictionary): string {
  const labels: Record<StockMovementKind, string> = {
    1: t.stockMovement.stockInitialized,
    2: t.stockMovement.stockIncreased,
    3: t.stockMovement.stockDecreased,
    4: t.stockMovement.stockReserved,
    5: t.stockMovement.stockReleased,
    6: t.stockMovement.auditBaseline,
  }
  return labels[kind]
}

export function stockMovementTone(kind: StockMovementKind): StatusTone {
  const tones: Record<StockMovementKind, StatusTone> = {
    1: "neutral",
    2: "success",
    3: "warning",
    4: "progress",
    5: "neutral",
    6: "neutral",
  }
  return tones[kind]
}

/** Ordered saga steps rendered by the order timeline. Cancellation is not a step. */
export const ORDER_TIMELINE_STEPS: readonly OrderStatus[] = [0, 1, 2, 3, 4] as const
