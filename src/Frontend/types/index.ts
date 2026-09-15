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

export interface InventoryItem {
  productId: string
  quantityOnHand: number
  reservedQuantity: number
  availableQuantity: number
  updatedAt: string
}

export type StockMovementKind = 1 | 2 | 3 | 4 | 5 | 6

export interface StockMovement {
  id: string
  productId: string
  type: StockMovementKind
  quantity: number
  quantityOnHandBefore: number
  quantityOnHandAfter: number
  reservedQuantityBefore: number
  reservedQuantityAfter: number
  orderId: string | null
  occurredAt: string
}

export type OrderWorkflowStatus = 1 | 2 | 3 | 4 | 5 | 6

export interface OrderWorkflowDiagnostics {
  orderId: string
  status: OrderWorkflowStatus
  stepDeadlineAt: string | null
  timeoutHandledAt: string | null
  createdAt: string
  updatedAt: string
  isOverdue: boolean
}

export interface CatalogStore {
  id: string
  name: string
  slug: string
  createdAt: string
  updatedAt: string
}

export interface ManagedCatalogStore {
  id: string
  ownerUserId: string
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

export interface UpdateCatalogStorePayload {
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

export interface ManagedCatalogCategoryReference {
  id: string
  englishName: string
  turkishName: string
  slug: string
  isActive: boolean
}

export interface ManagedCatalogBrandReference {
  id: string
  name: string
  slug: string
  isActive: boolean
}

export interface CreateCatalogCategoryPayload {
  slug: string
  englishName: string
  turkishName: string
}

export interface CreateCatalogBrandPayload {
  name: string
  slug: string
}

export interface UpdateCatalogCategoryPayload extends CreateCatalogCategoryPayload {
  isActive: boolean
}

export interface UpdateCatalogBrandPayload extends CreateCatalogBrandPayload {
  isActive: boolean
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
  reasonCode: string | null
  trackingNumber: string | null
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

export type OrderStatus = 0 | 1 | 2 | 3 | 4 | 5 | 6
export type OrderStatusName =
  | "Submitted"
  | "InventoryReserved"
  | "PaymentAuthorized"
  | "ShipmentCreated"
  | "Confirmed"
  | "Cancelled"
  | "CancellationRequested"

export type OrderCancellationReasonCode =
  | "INSUFFICIENT_STOCK"
  | "PAYMENT_FAILED"
  | "SHIPMENT_FAILED"
  | "UNEXPECTED_ERROR"
  | "ORDER_CANCELLED_BY_CUSTOMER"

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

export interface OrderSummary {
  id: string
  customerId: string
  currency: string
  status: OrderStatus
  totalAmount: number
  createdAt: string
  updatedAt: string
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

export interface SellerOrderItem {
  id: string
  productId: string
  productName: string
  quantity: number
  unitPrice: number
  totalPrice: number
  currency: string
}

export interface SellerOrderSummary {
  orderId: string
  storeId: string
  status: OrderStatus
  currency: string
  storeTotalAmount: number
  itemCount: number
  createdAt: string
  updatedAt: string
}

export interface SellerOrderDetail {
  orderId: string
  storeId: string
  status: OrderStatus
  currency: string
  storeTotalAmount: number
  createdAt: string
  updatedAt: string
  items: SellerOrderItem[]
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

export interface PaymentSummary {
  id: string
  orderId: string
  customerId: string
  amount: number
  currency: string
  status: PaymentStatus
  createdAt: string
  updatedAt: string
}

export interface PaymentTransaction {
  id: string
  type: PaymentTransactionType
  amount: number
  currency: string
  createdAt: string
}

export type ShipmentStatus = 1 | 2 | 3 | 4

export interface ShipmentSummary {
  id: string
  orderId: string
  customerId: string
  trackingNumber: string | null
  status: ShipmentStatus
  createdAt: string
  updatedAt: string
}

export const PUBLIC_ROLES: readonly PublicRole[] = ["Customer", "Seller"] as const
