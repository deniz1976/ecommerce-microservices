import { apiRequest } from "@/lib/api/client"
import type { Order, OrderStatus, OrderSummary, PagedResult } from "@/types"

export function getCustomerOrders(
  customerId: string,
  pageNumber = 1,
  pageSize = 10,
  status?: OrderStatus,
): Promise<PagedResult<OrderSummary>> {
  const parameters = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(pageSize),
    sortDescending: "true",
  })
  if (status !== undefined) parameters.set("status", String(status))

  return apiRequest<PagedResult<OrderSummary>>(
    `/gateway/orders/customer/${customerId}?${parameters.toString()}`,
    {
    authenticated: true,
    },
  )
}

export function getOrder(orderId: string): Promise<Order> {
  return apiRequest<Order>(`/gateway/orders/${orderId}`, {
    authenticated: true,
  })
}

export function requestOrderCancellation(orderId: string): Promise<Order> {
  return apiRequest<Order>(`/gateway/orders/${orderId}/cancellation`, {
    method: "PUT",
    authenticated: true,
  })
}
