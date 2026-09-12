import { apiRequest } from "@/lib/api/client"
import type { InventoryItem, PagedResult, StockMovement, StockMovementKind } from "@/types"

export interface ManagedInventoryQuery {
  productId?: string
  maximumAvailableQuantity?: number
  pageNumber?: number
  pageSize?: number
  sortBy?: "quantityOnHand" | "reservedQuantity" | "availableQuantity" | "updatedAt"
  sortDescending?: boolean
}

export function getManagedInventory(
  query: ManagedInventoryQuery,
  signal?: AbortSignal,
): Promise<PagedResult<InventoryItem>> {
  const parameters = new URLSearchParams({
    pageNumber: String(query.pageNumber ?? 1),
    pageSize: String(query.pageSize ?? 20),
    sortBy: query.sortBy ?? "updatedAt",
    sortDescending: String(query.sortDescending ?? true),
  })
  if (query.productId) parameters.set("productId", query.productId)
  if (query.maximumAvailableQuantity !== undefined) {
    parameters.set("maximumAvailableQuantity", String(query.maximumAvailableQuantity))
  }

  return apiRequest<PagedResult<InventoryItem>>(
    `/gateway/inventory/items/manage?${parameters.toString()}`,
    { authenticated: true, signal },
  )
}

export function getInventoryItems(
  productIds: readonly string[],
  signal?: AbortSignal,
): Promise<InventoryItem[]> {
  if (productIds.length === 0) {
    return Promise.resolve([])
  }

  const parameters = new URLSearchParams()
  for (const productId of productIds) {
    parameters.append("productIds", productId)
  }

  return apiRequest<InventoryItem[]>(
    `/gateway/inventory/items?${parameters.toString()}`,
    { signal },
  )
}

export function getInventoryItem(productId: string): Promise<InventoryItem> {
  return apiRequest<InventoryItem>(`/gateway/inventory/items/${productId}`)
}

export function updateInventoryItem(
  productId: string,
  quantityOnHand: number,
): Promise<InventoryItem> {
  return apiRequest<InventoryItem>(`/gateway/inventory/items/${productId}`, {
    method: "PUT",
    authenticated: true,
    body: { quantityOnHand },
  })
}

export interface StockMovementQuery {
  pageNumber?: number
  pageSize?: number
  orderId?: string
  type?: StockMovementKind
}

export function getStockMovements(
  productId: string,
  query: StockMovementQuery,
  signal?: AbortSignal,
): Promise<PagedResult<StockMovement>> {
  const parameters = new URLSearchParams({
    pageNumber: String(query.pageNumber ?? 1),
    pageSize: String(query.pageSize ?? 20),
  })
  if (query.orderId) parameters.set("orderId", query.orderId)
  if (query.type !== undefined) parameters.set("type", String(query.type))
  return apiRequest<PagedResult<StockMovement>>(
    `/gateway/inventory/items/${productId}/movements?${parameters.toString()}`,
    { authenticated: true, signal },
  )
}
