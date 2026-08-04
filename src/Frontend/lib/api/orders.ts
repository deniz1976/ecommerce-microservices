import { apiRequest } from "@/lib/api/client"
import type {
  Order,
  OrderStatus,
  OrderSummary,
  PagedResult,
  SellerOrderDetail,
  SellerOrderSummary,
} from "@/types"

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

export interface SellerOrderQuery {
  pageNumber?: number
  pageSize?: number
  status?: OrderStatus
  sortDescending?: boolean
}

export function getSellerOrders(
  storeId: string,
  query: SellerOrderQuery,
  signal?: AbortSignal,
): Promise<PagedResult<SellerOrderSummary>> {
  const parameters = new URLSearchParams({
    pageNumber: String(query.pageNumber ?? 1),
    pageSize: String(query.pageSize ?? 20),
    sortDescending: String(query.sortDescending ?? true),
  })
  if (query.status !== undefined) parameters.set("status", String(query.status))

  return apiRequest<PagedResult<SellerOrderSummary>>(
    `/gateway/orders/store/${storeId}?${parameters.toString()}`,
    {
      authenticated: true,
      signal,
    },
  )
}

export function getSellerOrder(
  storeId: string,
  orderId: string,
  signal?: AbortSignal,
): Promise<SellerOrderDetail> {
  return apiRequest<SellerOrderDetail>(
    `/gateway/orders/store/${storeId}/${orderId}`,
    {
      authenticated: true,
      signal,
    },
  )
}

export function requestOrderCancellation(orderId: string): Promise<Order> {
  return apiRequest<Order>(`/gateway/orders/${orderId}/cancellation`, {
    method: "PUT",
    authenticated: true,
  })
}
