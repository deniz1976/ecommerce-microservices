import { apiRequest } from "@/lib/api/client"
import type { InventoryItem, PagedResult } from "@/types"

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
