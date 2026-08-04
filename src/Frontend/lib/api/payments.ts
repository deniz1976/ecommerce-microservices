import { apiRequest } from "@/lib/api/client"
import type { PagedResult, Payment, PaymentStatus, PaymentSummary } from "@/types"

export interface ManagedPaymentsQuery {
  customerId?: string
  orderId?: string
  status?: PaymentStatus
  createdFrom?: string
  createdTo?: string
  pageNumber?: number
  pageSize?: number
  sortBy?: "amount" | "status" | "createdAt" | "updatedAt"
  sortDescending?: boolean
}

export function getManagedPayments(
  query: ManagedPaymentsQuery,
  signal?: AbortSignal,
): Promise<PagedResult<PaymentSummary>> {
  const parameters = new URLSearchParams({
    pageNumber: String(query.pageNumber ?? 1),
    pageSize: String(query.pageSize ?? 20),
    sortBy: query.sortBy ?? "createdAt",
    sortDescending: String(query.sortDescending ?? true),
  })
  if (query.customerId) parameters.set("customerId", query.customerId)
  if (query.orderId) parameters.set("orderId", query.orderId)
  if (query.status !== undefined) parameters.set("status", String(query.status))
  if (query.createdFrom) parameters.set("createdFrom", query.createdFrom)
  if (query.createdTo) parameters.set("createdTo", query.createdTo)

  return apiRequest<PagedResult<PaymentSummary>>(
    `/gateway/payments/manage?${parameters.toString()}`,
    { authenticated: true, signal },
  )
}

export function getOrderPayment(orderId: string): Promise<Payment> {
  return apiRequest<Payment>(`/gateway/payments/order/${orderId}`, {
    authenticated: true,
  })
}
