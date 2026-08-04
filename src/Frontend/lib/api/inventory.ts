import { apiRequest } from "@/lib/api/client"
import type { InventoryItem } from "@/types"

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
