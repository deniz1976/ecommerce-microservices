import { apiRequest } from "@/lib/api/client"
import type { Locale } from "@/lib/i18n/dictionaries"
import type {
  CatalogProduct,
  CatalogProductImage,
  CatalogBrandReference,
  CatalogCategoryReference,
  CatalogMetrics,
  CatalogStore,
  CreateCatalogProductPayload,
  CreateCatalogBrandPayload,
  CreateCatalogCategoryPayload,
  CreateCatalogStorePayload,
  PagedResult,
  ProductStatus,
  ManagedCatalogBrandReference,
  ManagedCatalogCategoryReference,
  ManagedCatalogStore,
  UpdateCatalogBrandPayload,
  UpdateCatalogCategoryPayload,
  UpdateCatalogProductPayload,
  UpdateCatalogStorePayload,
} from "@/types"

export interface CatalogProductQuery {
  pageNumber?: number
  pageSize?: number
  search?: string
  categoryId?: string
  brandId?: string
  storeId?: string
  minPrice?: number
  maxPrice?: number
  status?: ProductStatus
  sortBy?: "createdAt" | "price" | "sku"
  sortDescending?: boolean
}

export interface ManagedCatalogCategoryQuery {
  pageNumber: number
  pageSize: number
  search?: string
  isActive?: boolean
  sortBy: "englishName" | "turkishName" | "slug" | "isActive"
  sortDescending: boolean
}

export interface ManagedCatalogBrandQuery {
  pageNumber: number
  pageSize: number
  search?: string
  isActive?: boolean
  sortBy: "name" | "slug" | "isActive"
  sortDescending: boolean
}

export interface ManagedCatalogStoreQuery {
  pageNumber?: number
  pageSize?: number
  search?: string
  ownerUserId?: string
  sortBy?: "name" | "slug" | "createdAt" | "updatedAt"
  sortDescending?: boolean
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
  if (query.storeId) parameters.set("storeId", query.storeId)
  if (query.minPrice !== undefined) parameters.set("minPrice", String(query.minPrice))
  if (query.maxPrice !== undefined) parameters.set("maxPrice", String(query.maxPrice))

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
  if (query.minPrice !== undefined) parameters.set("minPrice", String(query.minPrice))
  if (query.maxPrice !== undefined) parameters.set("maxPrice", String(query.maxPrice))
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

export function getManagedCatalogStores(
  query: ManagedCatalogStoreQuery,
  signal?: AbortSignal,
): Promise<PagedResult<ManagedCatalogStore>> {
  const parameters = new URLSearchParams({
    pageNumber: String(query.pageNumber ?? 1),
    pageSize: String(query.pageSize ?? 20),
    sortBy: query.sortBy ?? "name",
    sortDescending: String(query.sortDescending ?? false),
  })
  if (query.search?.trim()) parameters.set("search", query.search.trim())
  if (query.ownerUserId) parameters.set("ownerUserId", query.ownerUserId)

  return apiRequest<PagedResult<ManagedCatalogStore>>(
    `/gateway/catalog/stores/manage?${parameters.toString()}`,
    { authenticated: true, signal },
  )
}

export function getCatalogCategories(locale?: Locale): Promise<CatalogCategoryReference[]> {
  return apiRequest<CatalogCategoryReference[]>("/gateway/catalog/references/categories", {
    locale,
  })
}

export function getCatalogBrands(): Promise<CatalogBrandReference[]> {
  return apiRequest<CatalogBrandReference[]>("/gateway/catalog/references/brands")
}

export function createCatalogCategory(
  payload: CreateCatalogCategoryPayload,
): Promise<CatalogCategoryReference> {
  return apiRequest<CatalogCategoryReference>("/gateway/catalog/references/categories", {
    method: "POST",
    authenticated: true,
    body: payload,
  })
}

export function createCatalogBrand(
  payload: CreateCatalogBrandPayload,
): Promise<CatalogBrandReference> {
  return apiRequest<CatalogBrandReference>("/gateway/catalog/references/brands", {
    method: "POST",
    authenticated: true,
    body: payload,
  })
}

export function getManagedCatalogCategories(
  query: ManagedCatalogCategoryQuery,
  signal?: AbortSignal,
): Promise<PagedResult<ManagedCatalogCategoryReference>> {
  const parameters = createManagedReferenceParameters(query)
  return apiRequest<PagedResult<ManagedCatalogCategoryReference>>(
    `/gateway/catalog/references/manage/categories?${parameters.toString()}`,
    { authenticated: true, signal },
  )
}

export function getManagedCatalogBrands(
  query: ManagedCatalogBrandQuery,
  signal?: AbortSignal,
): Promise<PagedResult<ManagedCatalogBrandReference>> {
  const parameters = createManagedReferenceParameters(query)
  return apiRequest<PagedResult<ManagedCatalogBrandReference>>(
    `/gateway/catalog/references/manage/brands?${parameters.toString()}`,
    { authenticated: true, signal },
  )
}

function createManagedReferenceParameters(query: {
  pageNumber: number
  pageSize: number
  search?: string
  isActive?: boolean
  sortBy: string
  sortDescending: boolean
}): URLSearchParams {
  const parameters = new URLSearchParams({
    pageNumber: String(query.pageNumber),
    pageSize: String(query.pageSize),
    sortBy: query.sortBy,
    sortDescending: String(query.sortDescending),
  })

  if (query.search?.trim()) parameters.set("search", query.search.trim())
  if (query.isActive !== undefined) parameters.set("isActive", String(query.isActive))
  return parameters
}

export function updateCatalogCategory(
  categoryId: string,
  payload: UpdateCatalogCategoryPayload,
): Promise<ManagedCatalogCategoryReference> {
  return apiRequest<ManagedCatalogCategoryReference>(
    `/gateway/catalog/references/categories/${categoryId}`,
    {
      method: "PUT",
      authenticated: true,
      body: payload,
    },
  )
}

export function updateCatalogBrand(
  brandId: string,
  payload: UpdateCatalogBrandPayload,
): Promise<ManagedCatalogBrandReference> {
  return apiRequest<ManagedCatalogBrandReference>(
    `/gateway/catalog/references/brands/${brandId}`,
    {
      method: "PUT",
      authenticated: true,
      body: payload,
    },
  )
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

export function getMyCatalogStores(signal?: AbortSignal): Promise<CatalogStore[]> {
  return apiRequest<CatalogStore[]>("/gateway/catalog/stores/mine", {
    authenticated: true,
    signal,
  })
}

export function getCatalogStore(storeId: string): Promise<CatalogStore> {
  return apiRequest<CatalogStore>(`/gateway/catalog/stores/${storeId}`)
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

export function updateCatalogStore(
  storeId: string,
  payload: UpdateCatalogStorePayload,
): Promise<CatalogStore> {
  return apiRequest<CatalogStore>(`/gateway/catalog/stores/${storeId}`, {
    method: "PUT",
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
