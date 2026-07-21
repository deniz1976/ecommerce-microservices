export type PublicRole = "Customer" | "Seller"

export type Role = PublicRole | "Admin"

export interface UserProfile {
  id: string
  email: string
  displayName: string
  roles: Role[]
  isOnboardingComplete: boolean
}

export interface UpdateRolePayload {
  role: PublicRole
}

export type ProductStatus = 0 | 1 | 2 | 3

export type ProductStatusName = "Draft" | "Active" | "Inactive" | "Archived"

export const productStatusNames: Record<ProductStatus, ProductStatusName> = {
  0: "Draft",
  1: "Active",
  2: "Inactive",
  3: "Archived",
}

export interface CatalogProduct {
  id: string
  sku: string
  name: string
  description: string
  categoryId: string
  categoryName: string | null
  brandId: string
  brandName: string | null
  storeId: string | null
  price: number
  currency: string
  status: ProductStatus
  images: CatalogProductImage[]
}

export interface CatalogStore {
  id: string
  name: string
  slug: string
  createdAt: string
  updatedAt: string
}

export interface CatalogProductImage {
  id: string
  url: string
  altText: string | null
  sortOrder: number
}

export interface PagedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export const PUBLIC_ROLES: readonly PublicRole[] = ["Customer", "Seller"] as const
