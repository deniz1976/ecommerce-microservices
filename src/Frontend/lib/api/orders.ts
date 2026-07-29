import { apiRequest } from "@/lib/api/client"
import type { Order } from "@/types"

export function getCustomerOrders(customerId: string): Promise<Order[]> {
  return apiRequest<Order[]>(`/gateway/orders/customer/${customerId}`, {
    authenticated: true,
  })
}

export function getOrder(orderId: string): Promise<Order> {
  return apiRequest<Order>(`/gateway/orders/${orderId}`, {
    authenticated: true,
  })
}
