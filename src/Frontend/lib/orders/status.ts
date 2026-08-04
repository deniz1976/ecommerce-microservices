import type { OrderStatus, OrderStatusName } from "@/types"

export const orderProgress: readonly OrderStatusName[] = [
  "Submitted",
  "InventoryReserved",
  "PaymentAuthorized",
  "ShipmentCreated",
  "Confirmed",
]

export function getOrderStatusName(status: OrderStatus): OrderStatusName {
  switch (status) {
    case 1:
      return "InventoryReserved"
    case 2:
      return "PaymentAuthorized"
    case 3:
      return "ShipmentCreated"
    case 4:
      return "Confirmed"
    case 5:
      return "Cancelled"
    case 6:
      return "CancellationRequested"
    default:
      return "Submitted"
  }
}
