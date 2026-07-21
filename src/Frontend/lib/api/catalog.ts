import { apiRequest } from "@/lib/api/client"
import type {
  CatalogProduct,
  CatalogStore,
  CreateCatalogStorePayload,
  PagedResult,
} from "@/types"

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

export function getMyCatalogStores(): Promise<CatalogStore[]> {
  return apiRequest<CatalogStore[]>("/gateway/catalog/stores/mine", {
    authenticated: true,
  })
}

export function createCatalogStore(
  payload: CreateCatalogStorePayload,
): Promise<CatalogStore> {
  return apiRequest<CatalogStore>("/gateway/catalog/stores", {
    method: "POST",
    authenticated: true,
    body: payload,
  })
}

export function getStoreCatalogProducts(
  storeId: string,
  pageSize = 20,
): Promise<PagedResult<CatalogProduct>> {
  const parameters = new URLSearchParams({
    pageNumber: "1",
    pageSize: String(pageSize),
    storeId,
    sortBy: "createdAt",
    sortDescending: "true",
  })

  return apiRequest<PagedResult<CatalogProduct>>(
    `/gateway/catalog/products?${parameters.toString()}`,
  )
}
