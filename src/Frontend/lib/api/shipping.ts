import { apiRequest } from "@/lib/api/client"
import type { PagedResult, ShipmentStatus, ShipmentSummary } from "@/types"

export interface ManagedShipmentsQuery {
  customerId?: string
  orderId?: string
  status?: ShipmentStatus
  createdFrom?: string
  createdTo?: string
  pageNumber?: number
  pageSize?: number
  sortBy?: "status" | "createdAt" | "updatedAt"
  sortDescending?: boolean
}

export function getManagedShipments(
  query: ManagedShipmentsQuery,
  signal?: AbortSignal,
): Promise<PagedResult<ShipmentSummary>> {
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

  return apiRequest<PagedResult<ShipmentSummary>>(
    `/gateway/shipments/manage?${parameters.toString()}`,
    { authenticated: true, signal },
  )
}
