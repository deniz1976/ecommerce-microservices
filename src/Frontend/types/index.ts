export type PublicRole = "Customer" | "Seller"

export type Role = PublicRole | "Admin"

export interface UserProfile {
  id: string
  email: string
  displayName: string
  roles: Role[]
  isOnboardingComplete: boolean
}

export type UserStatus = 1 | 2

export interface AdminUser {
  id: string
  email: string
  displayName: string
  roles: Role[]
  status: UserStatus
  isOnboardingComplete: boolean
  createdAt: string
  updatedAt: string
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

export interface CatalogMetrics {
  totalProducts: number
  activeProducts: number
  draftProducts: number
  inactiveProducts: number
  archivedProducts: number
  totalStores: number
  totalCategories: number
  totalBrands: number
}

export interface CreateCatalogStorePayload {
  name: string
  slug: string
}

export interface CatalogCategoryReference {
  id: string
  name: string
  slug: string
}

export interface CatalogBrandReference {
  id: string
  name: string
  slug: string
}

export interface CreateCatalogProductPayload {
  sku: string
  categoryId: string
  brandId: string
  storeId: string
  price: number
  currency: string
  status: ProductStatus
  translations: CatalogProductTranslationInput[]
}

export interface UpdateCatalogProductPayload {
  categoryId: string
  brandId: string
  price: number
  currency: string
  status: ProductStatus
  translations: CatalogProductTranslationInput[]
}

export interface CatalogProductTranslationInput {
  languageCode: string
  name: string
  description: string
}

export interface CatalogProductImage {
  id: string
  publicId: string
  url: string
  secureUrl: string
  width: number
  height: number
  format: string
  sortOrder: number
  isMain: boolean
}

export interface PagedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface CustomerNotification {
  id: string
  customerId: string
  orderId: string | null
  type: string
  title: string
  message: string
  culture: string
  createdAt: string
  readAt: string | null
}

export interface Basket {
  customerId: string
  currency: string
  totalAmount: number
  updatedAt: string
  items: BasketItem[]
}

export interface BasketItem {
  productId: string
  productName: string
  quantity: number
  unitPrice: number
  totalPrice: number
  currency: string
}

export interface AddBasketItemPayload {
  productId: string
  quantity: number
}

export interface CheckoutBasketResult {
  snapshotId: string
  customerId: string
  totalAmount: number
  currency: string
  createdAt: string
}

export interface CheckoutBasketPayload {
  checkoutId: string
  recipientName: string
  addressLine: string
  city: string
  countryCode: string
  postalCode: string
}

export type OrderStatus = 0 | 1 | 2 | 3 | 4 | 5
export type OrderStatusName =
  | "Submitted"
  | "InventoryReserved"
  | "PaymentAuthorized"
  | "ShipmentCreated"
  | "Confirmed"
  | "Cancelled"

export type OrderCancellationReasonCode =
  | "INSUFFICIENT_STOCK"
  | "PAYMENT_FAILED"
  | "SHIPMENT_FAILED"
  | "UNEXPECTED_ERROR"

export interface OrderStatusHistory {
  status: OrderStatus
  occurredAt: string
  reasonCode: OrderCancellationReasonCode | null
}

export interface Order {
  id: string
  customerId: string
  currency: string
  status: OrderStatus
  totalAmount: number
  recipientName: string
  addressLine: string
  city: string
  countryCode: string
  postalCode: string
  createdAt: string
  updatedAt: string
  items: OrderItem[]
  statusHistory: OrderStatusHistory[]
}

export interface OrderItem {
  id: string
  productId: string
  productName: string
  quantity: number
  unitPrice: number
  totalPrice: number
  currency: string
}

export type PaymentStatus = 1 | 2 | 3
export type PaymentTransactionType = 1 | 2

export interface Payment {
  id: string
  orderId: string
  customerId: string
  amount: number
  currency: string
  status: PaymentStatus
  createdAt: string
  updatedAt: string
  transactions: PaymentTransaction[]
}

export interface PaymentTransaction {
  id: string
  type: PaymentTransactionType
  amount: number
  currency: string
  createdAt: string
}

export const PUBLIC_ROLES: readonly PublicRole[] = ["Customer", "Seller"] as const
