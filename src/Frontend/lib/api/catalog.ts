import { apiRequest } from "@/lib/api/client"
import type { CatalogProduct, PagedResult } from "@/types"

export function getCatalogProducts(pageSize = 5): Promise<PagedResult<CatalogProduct>> {
  const parameters = new URLSearchParams({
    pageNumber: "1",
    pageSize: String(pageSize),
    sortBy: "createdAt",
    sortDescending: "true",
  })

  return apiRequest<PagedResult<CatalogProduct>>(
    `/gateway/catalog/products?${parameters.toString()}`,
  )
}
