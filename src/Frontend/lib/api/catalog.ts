import { apiRequest } from "@/lib/api/client"
import type {
  CatalogProduct,
  CatalogProductImage,
  CatalogBrandReference,
  CatalogCategoryReference,
  CatalogMetrics,
  CatalogStore,
  CreateCatalogProductPayload,
  CreateCatalogStorePayload,
  PagedResult,
  ProductStatus,
  UpdateCatalogProductPayload,
} from "@/types"

export interface CatalogProductQuery {
  pageNumber?: number
  pageSize?: number
  search?: string
  categoryId?: string
  brandId?: string
  storeId?: string
  status?: ProductStatus
  sortBy?: "createdAt" | "price" | "sku"
  sortDescending?: boolean
}

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

export function getPublicCatalogProducts(
  query: CatalogProductQuery,
): Promise<PagedResult<CatalogProduct>> {
  const parameters = new URLSearchParams({
    pageNumber: String(query.pageNumber ?? 1),
    pageSize: String(query.pageSize ?? 12),
    status: "1",
    sortBy: query.sortBy ?? "createdAt",
    sortDescending: String(query.sortDescending ?? true),
  })

  if (query.search?.trim()) parameters.set("search", query.search.trim())
  if (query.categoryId) parameters.set("categoryId", query.categoryId)
  if (query.brandId) parameters.set("brandId", query.brandId)

  return apiRequest<PagedResult<CatalogProduct>>(
    `/gateway/catalog/products?${parameters.toString()}`,
  )
}

export function getCatalogProduct(productId: string): Promise<CatalogProduct> {
  return apiRequest<CatalogProduct>(`/gateway/catalog/products/${productId}`)
}

export function getManagedCatalogProducts(
  query: CatalogProductQuery,
): Promise<PagedResult<CatalogProduct>> {
  const parameters = new URLSearchParams({
    pageNumber: String(query.pageNumber ?? 1),
    pageSize: String(query.pageSize ?? 20),
    sortBy: query.sortBy ?? "createdAt",
    sortDescending: String(query.sortDescending ?? true),
  })

  if (query.search?.trim()) parameters.set("search", query.search.trim())
  if (query.categoryId) parameters.set("categoryId", query.categoryId)
  if (query.brandId) parameters.set("brandId", query.brandId)
  if (query.storeId) parameters.set("storeId", query.storeId)
  if (query.status !== undefined) parameters.set("status", String(query.status))

  return apiRequest<PagedResult<CatalogProduct>>(
    `/gateway/catalog/manage/products?${parameters.toString()}`,
    { authenticated: true },
  )
}

export function getManagedCatalogProduct(productId: string): Promise<CatalogProduct> {
  return apiRequest<CatalogProduct>(`/gateway/catalog/manage/products/${productId}`, {
    authenticated: true,
  })
}

export function getCatalogMetrics(): Promise<CatalogMetrics> {
  return apiRequest<CatalogMetrics>("/gateway/catalog/metrics", {
    authenticated: true,
  })
}

export function getCatalogCategories(): Promise<CatalogCategoryReference[]> {
  return apiRequest<CatalogCategoryReference[]>("/gateway/catalog/references/categories")
}

export function getCatalogBrands(): Promise<CatalogBrandReference[]> {
  return apiRequest<CatalogBrandReference[]>("/gateway/catalog/references/brands")
}

export function createCatalogProduct(
  payload: CreateCatalogProductPayload,
): Promise<CatalogProduct> {
  return apiRequest<CatalogProduct>("/gateway/catalog/products", {
    method: "POST",
    authenticated: true,
    body: payload,
  })
}

export function updateCatalogProduct(
  productId: string,
  payload: UpdateCatalogProductPayload,
): Promise<CatalogProduct> {
  return apiRequest<CatalogProduct>(`/gateway/catalog/products/${productId}`, {
    method: "PUT",
    authenticated: true,
    body: payload,
  })
}

export function uploadCatalogProductImage(
  productId: string,
  file: File,
): Promise<CatalogProductImage> {
  const form = new FormData()
  form.set("file", file)
  return apiRequest<CatalogProductImage>(`/gateway/catalog/products/${productId}/images`, {
    method: "POST",
    authenticated: true,
    body: form,
  })
}

export function setMainCatalogProductImage(
  productId: string,
  imageId: string,
): Promise<CatalogProductImage> {
  return apiRequest<CatalogProductImage>(
    `/gateway/catalog/products/${productId}/images/${imageId}/main`,
    {
      method: "PUT",
      authenticated: true,
    },
  )
}

export function deleteCatalogProductImage(
  productId: string,
  imageId: string,
): Promise<void> {
  return apiRequest<void>(`/gateway/catalog/products/${productId}/images/${imageId}`, {
    method: "DELETE",
    authenticated: true,
  })
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
  return getManagedCatalogProducts({
    pageNumber: 1,
    pageSize,
    storeId,
    sortBy: "createdAt",
    sortDescending: true,
  })
}
