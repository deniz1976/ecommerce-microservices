import type { ProductStatusName, Role } from "@/types"

export const locales = ["en", "tr"] as const
export type Locale = (typeof locales)[number]
export const defaultLocale: Locale = "en"

export const localeNames: Record<Locale, string> = {
  en: "English",
  tr: "Türkçe",
}

export interface Dictionary {
  brand: {
    name: string
    tagline: string
  }
  common: {
    email: string
    emailPlaceholder: string
    password: string
    passwordPlaceholder: string
    displayName: string
    displayNamePlaceholder: string
    continue: string
    back: string
    or: string
    loading: string
    showPassword: string
    hidePassword: string
    toggleTheme: string
    lightMode: string
    darkMode: string
    changeLanguage: string
  }
  aside: {
    trustTitle: string
    trustBody: string
    point1: string
    point2: string
    point3: string
  }
  login: {
    title: string
    subtitle: string
    continueWithAuth0: string
    signInCta: string
    noAccount: string
    registerLink: string
  }
  register: {
    title: string
    subtitle: string
    chooseRole: string
    createAccount: string
    haveAccount: string
    loginLink: string
    auth0Note: string
    continueWithAuth0: string
  }
  onboarding: {
    title: string
    subtitle: string
    confirmRole: string
    savingRole: string
    note: string
  }
  roles: {
    customer: {
      name: string
      description: string
      feature1: string
      feature2: string
      feature3: string
    }
    seller: {
      name: string
      description: string
      feature1: string
      feature2: string
      feature3: string
    }
  }
  home: {
    signedInAs: string
    welcome: string
    placeholder: string
    signOut: string
    role: string
  }
  admin: {
    workspace: string
    navigation: string
    overview: string
    catalog: string
    users: string
    orders: string
    inventory: string
    payments: string
    shipments: string
    workflows: string
    openSellerWorkspace: string
    roleLabel: string
    welcome: string
    description: string
    catalogSummary: string
    catalogSummaryDescription: string
    metricsUnavailable: string
    totalProducts: string
    activeProducts: string
    draftProducts: string
    inactiveProducts: string
    archivedProducts: string
    totalStores: string
    totalCategories: string
    totalBrands: string
    recentProducts: string
    recentProductsDescription: string
    catalogUnavailable: string
    noProducts: string
    manageCatalog: string
    manageCatalogDescription: string
    manageReferences: string
    manageReferencesDescription: string
    backToOverview: string
    referenceNavigation: string
    categories: string
    brands: string
    stores: string
    manageCategoriesDescription: string
    manageBrandsDescription: string
    openCategoryManagement: string
    openBrandManagement: string
    categoryList: string
    categoryListDescription: string
    brandList: string
    brandListDescription: string
    newCategory: string
    newBrand: string
    categoryEnglishName: string
    categoryTurkishName: string
    brandName: string
    referenceSlug: string
    referenceSlugHint: string
    createCategory: string
    creatingCategory: string
    categoryCreated: string
    categoryCreateFailed: string
    createBrand: string
    creatingBrand: string
    brandCreated: string
    brandCreateFailed: string
    updateCategory: string
    updateBrand: string
    updatingReference: string
    categoryUpdated: string
    brandUpdated: string
    referenceUpdateFailed: string
    activeReference: string
    inactiveReference: string
    searchCategories: string
    searchCategoriesPlaceholder: string
    searchBrands: string
    searchBrandsPlaceholder: string
    referenceStatus: string
    allReferenceStatuses: string
    rowsPerPage: string
    referenceActions: string
    referencePageStatus: string
    noMatchingCategories: string
    noMatchingBrands: string
    editCategory: string
    editBrand: string
    cancelReferenceForm: string
    referencesUnavailable: string
    noCategories: string
    noBrands: string
    searchProducts: string
    searchProductsPlaceholder: string
    filterStatus: string
    allStatuses: string
    searchAction: string
    noMatchingProducts: string
    sellerProduct: string
    platformProduct: string
    editProduct: string
    edit: string
    pageStatus: string
    previousPage: string
    nextPage: string
    selectProductToEdit: string
    manageUsers: string
    manageUsersDescription: string
    searchUsers: string
    searchUsersPlaceholder: string
    filterRole: string
    allRoles: string
    filterUserStatus: string
    allUserStatuses: string
    userActive: string
    userDisabled: string
    usersUnavailable: string
    noMatchingUsers: string
    onboardingPending: string
    joinedAt: string
    userPageStatus: string
    usersReadOnlyNote: string
    viewUserDetails: string
    userDetails: string
    userDetailsDescription: string
    userDetailsUnavailable: string
    userNotFound: string
    userId: string
    userRoles: string
    onboardingStatus: string
    onboardingComplete: string
    backToUsers: string
    manageOrdersDescription: string
    orderList: string
    orderCustomerId: string
    orderCustomerIdPlaceholder: string
    applyOrderFilter: string
    invalidCustomerId: string
    orderStatus: string
    allOrderStatuses: string
    orderSort: string
    newestOrders: string
    oldestOrders: string
    ordersUnavailable: string
    noMatchingOrders: string
    orderNumber: string
    orderTotal: string
    orderCreatedAt: string
    orderUpdatedAt: string
    orderPageStatus: string
    manageInventoryDescription: string
    inventoryList: string
    inventoryProductId: string
    inventoryProductIdPlaceholder: string
    invalidProductId: string
    maximumAvailable: string
    maximumAvailablePlaceholder: string
    invalidMaximumAvailable: string
    inventorySort: string
    inventoryDirection: string
    descending: string
    ascending: string
    quantityOnHand: string
    reservedQuantity: string
    availableQuantity: string
    inventoryUpdatedAt: string
    applyInventoryFilters: string
    inventoryUnavailable: string
    noMatchingInventory: string
    inventoryPageStatus: string
    viewStockMovements: string
    stockMovements: string
    stockMovementsDescription: string
    stockMovementsUnavailable: string
    noStockMovements: string
    movementType: string
    movementQuantity: string
    movementOnHand: string
    movementReserved: string
    movementOrder: string
    movementOccurredAt: string
    movementPageStatus: string
    close: string
    managePaymentsDescription: string
    paymentList: string
    paymentCustomerId: string
    paymentOrderId: string
    guidFilterPlaceholder: string
    invalidOrderId: string
    createdFrom: string
    createdTo: string
    invalidDateRange: string
    paymentStatus: string
    allPaymentStatuses: string
    paymentSort: string
    paymentAmount: string
    paymentId: string
    paymentCreatedAt: string
    paymentUpdatedAt: string
    applyPaymentFilters: string
    paymentsUnavailable: string
    noMatchingPayments: string
    paymentPageStatus: string
    manageShipmentsDescription: string
    shipmentList: string
    shipmentStatus: string
    allShipmentStatuses: string
    shipmentSort: string
    shipmentId: string
    trackingNumber: string
    shipmentCreated: string
    shipmentFailed: string
    shipmentInTransit: string
    shipmentDelivered: string
    notAvailable: string
    applyShipmentFilters: string
    shipmentsUnavailable: string
    noMatchingShipments: string
    shipmentPageStatus: string
    manageStoresDescription: string
    storeList: string
    storeSearch: string
    storeSearchPlaceholder: string
    storeOwnerId: string
    invalidOwnerId: string
    storeSort: string
    storeName: string
    storeSlug: string
    applyStoreFilters: string
    storesUnavailable: string
    noMatchingStores: string
    storePageStatus: string
    manageWorkflowsDescription: string
    workflowList: string
    workflowOrderId: string
    workflowStatus: string
    allWorkflowStatuses: string
    workflowOverdueOnly: string
    overdueWorkflowsTitle: string
    overdueWorkflowsBody: string
    reviewOverdueWorkflows: string
    metricsRetry: string
    workflowDeadline: string
    workflowTimeoutHandled: string
    workflowCreatedAt: string
    workflowUpdatedAt: string
    workflowsUnavailable: string
    noMatchingWorkflows: string
    workflowPageStatus: string
    applyWorkflowFilters: string
    userRole: Record<Role, string>
    status: Record<ProductStatusName, string>
  }
  seller: {
    workspace: string
    navigation: string
    overview: string
    storePageDescription: string
    productPageDescription: string
    storeCount: string
    manageStoresDescription: string
    manageProductsDescription: string
    manageOrdersDescription: string
    openAdminWorkspace: string
    roleLabel: string
    welcome: string
    description: string
    stores: string
    products: string
    createStore: string
    storeName: string
    storeNamePlaceholder: string
    storeSlug: string
    storeSlugPlaceholder: string
    slugHint: string
    saveStore: string
    savingStore: string
    editStore: string
    updateStore: string
    updatingStore: string
    storeUpdateFailed: string
    noStores: string
    noStoresDescription: string
    storeLoadFailed: string
    storeCreateFailed: string
    slugConflict: string
    selectStore: string
    productCount: string
    noProducts: string
    productLoadFailed: string
    createProduct: string
    sku: string
    productName: string
    productDescription: string
    productNameEnglish: string
    productDescriptionEnglish: string
    productNameTurkish: string
    productDescriptionTurkish: string
    category: string
    brand: string
    price: string
    currency: string
    statusLabel: string
    draft: string
    active: string
    inactive: string
    archived: string
    saveProduct: string
    savingProduct: string
    productCreateFailed: string
    referencesUnavailable: string
    referencesEmpty: string
    refreshReferences: string
    refreshingReferences: string
    productCreated: string
    productCreatedAddImages: string
    edit: string
    editProduct: string
    cancelEditing: string
    saveChanges: string
    savingChanges: string
    productUpdated: string
    productUpdateFailed: string
    currentImages: string
    noImages: string
    uploadImage: string
    uploadingImage: string
    imageUploadHint: string
    imageInvalid: string
    imageUploaded: string
    imageUploadFailed: string
    mainImage: string
    setMainImage: string
    mainImageUpdated: string
    imageUpdateFailed: string
    deleteImage: string
    imageDeleted: string
    imageDeleteFailed: string
    manageStock: string
    stockTitle: string
    quantityOnHand: string
    reservedQuantity: string
    availableQuantity: string
    stockNotCreated: string
    saveStock: string
    savingStock: string
    stockUpdated: string
    stockLoadFailed: string
    stockUpdateFailed: string
    stockInvalid: string
    closeStockEditor: string
    openOrders: string
    orderPageTitle: string
    orderPageDescription: string
    orderStore: string
    orderStatus: string
    allOrderStatuses: string
    orderSort: string
    newestOrders: string
    oldestOrders: string
    orderRowsPerPage: string
    ordersLoadFailed: string
    noOrders: string
    orderCount: string
    orderNumber: string
    placedAt: string
    lastUpdated: string
    storeTotal: string
    orderItems: string
    showOrderItems: string
    hideOrderItems: string
    orderItemsLoadFailed: string
    orderProduct: string
    orderQuantity: string
    orderUnitPrice: string
    orderLineTotal: string
    orderPageStatus: string
    previousOrderPage: string
    nextOrderPage: string
  }
  customer: {
    roleLabel: string
    welcome: string
    description: string
    searchLabel: string
    searchPlaceholder: string
    searchAction: string
    category: string
    allCategories: string
    brand: string
    allBrands: string
    sort: string
    filters: string
    showFilters: string
    hideFilters: string
    priceRange: string
    minPrice: string
    maxPrice: string
    applyFilters: string
    clearFilters: string
    priceRangeInvalid: string
    newest: string
    priceLowToHigh: string
    priceHighToLow: string
    results: string
    noProducts: string
    catalogUnavailable: string
    filtersUnavailable: string
    previousPage: string
    nextPage: string
    page: string
    viewProduct: string
    inStock: string
    lowStock: string
    outOfStock: string
    productUnavailable: string
    backToCatalog: string
    browseTitle: string
    catalog: string
    navigation: string
    productDetails: string
    purchase: string
    storeDetails: string
    viewStore: string
    storeUnavailable: string
    storeProducts: string
    backToStore: string
  }
  basket: {
    title: string
    description: string
    openBasket: string
    addToBasket: string
    adding: string
    added: string
    addFailed: string
    outOfStock: string
    stockLimited: string
    signInToAdd: string
    empty: string
    continueShopping: string
    quantity: string
    unitPrice: string
    total: string
    remove: string
    clear: string
    updateFailed: string
    loadFailed: string
    temporarilyUnavailable: string
    checkout: string
    checkingOut: string
    checkoutRecorded: string
    checkoutNote: string
    demoPayment: string
    demoPaymentNote: string
    recipientName: string
    addressLine: string
    city: string
    countryCode: string
    postalCode: string
    addressRequired: string
    viewOrder: string
  }
  orders: {
    title: string
    openOrders: string
    description: string
    empty: string
    loadFailed: string
    orderNumber: string
    placedAt: string
    pageStatus: string
    previousPage: string
    nextPage: string
    status: Record<
      "Submitted" | "InventoryReserved" | "PaymentAuthorized" | "ShipmentCreated" | "Confirmed" | "Cancelled" | "CancellationRequested",
      string
    >
    processing: string
    processingNote: string
    backToOrders: string
    shippingAddress: string
    cancellationReason: string
    cancelOrder: string
    cancellingOrder: string
    cancellationRequested: string
    cancellationFailed: string
    orderNotCancellable: string
    cancellationReasons: Record<
      "INSUFFICIENT_STOCK" | "PAYMENT_FAILED" | "SHIPMENT_FAILED" | "UNEXPECTED_ERROR" | "ORDER_CANCELLED_BY_CUSTOMER",
      string
    >
    payment: {
      title: string
      pending: string
      unavailable: string
      authorization: string
      refund: string
      status: Record<"authorized" | "failed" | "refunded", string>
    }
    shipment: {
      title: string
      pending: string
      unavailable: string
      created: string
      failed: string
      inTransit: string
      delivered: string
      trackingNumber: string
      notAvailable: string
      createdAt: string
    }
  }
  notifications: {
    title: string
    openNotifications: string
    openNotificationsWithUnread: string
    description: string
    all: string
    unread: string
    unreadLabel: string
    readLabel: string
    empty: string
    emptyUnread: string
    loadFailed: string
    markRead: string
    markingRead: string
    markAllRead: string
    markingAllRead: string
    viewOrder: string
    previousPage: string
    nextPage: string
    pageStatus: string
    orderTitle: string
    types: {
      orderSubmitted: string
      orderConfirmed: string
      orderCancelled: string
      orderCancellationRequested: string
      orderCancellationRejected: string
      paymentAuthorized: string
      paymentFailed: string
      shipmentCreated: string
      shipmentCreatedWithoutTracking: string
      shipmentFailed: string
    }
  }
  validation: {
    required: string
    invalidEmail: string
    passwordTooShort: string
    displayNameTooShort: string
    selectRole: string
  }
  status: {
    payment: {
      authorized: string
      failed: string
      refunded: string
    }
    shipment: {
      created: string
      failed: string
      inTransit: string
      delivered: string
    }
    product: {
      draft: string
      active: string
      inactive: string
      archived: string
    }
    user: {
      active: string
      disabled: string
    }
    workflow: {
      submitted: string
      inventoryReserved: string
      paymentAuthorized: string
      shipmentCreated: string
      completed: string
      cancelled: string
    }
    stockMovement: {
      stockInitialized: string
      stockIncreased: string
      stockDecreased: string
      stockReserved: string
      stockReleased: string
      auditBaseline: string
      stockShipped: string
    }
  }
  table: {
    empty: string
    emptyDescription: string
    loadFailed: string
    loadFailedDescription: string
    retry: string
    previousPage: string
    nextPage: string
    pageStatus: string
    rowsPerPage: string
    totalItems: string
    clearFilters: string
    filters: string
  }
  errors: {
    generic: string
    roleUpdateFailed: string
  }
}

const en: Dictionary = {
  brand: {
    name: "Marketplace",
    tagline: "Commerce, refined.",
  },
  common: {
    email: "Email",
    emailPlaceholder: "you@example.com",
    password: "Password",
    passwordPlaceholder: "Enter your password",
    displayName: "Display name",
    displayNamePlaceholder: "How should we call you?",
    continue: "Continue",
    back: "Back",
    or: "or",
    loading: "Please wait",
    showPassword: "Show password",
    hidePassword: "Hide password",
    toggleTheme: "Toggle theme",
    lightMode: "Light",
    darkMode: "Dark",
    changeLanguage: "Change language",
  },
  aside: {
    trustTitle: "Shopping, made simpler",
    trustBody:
      "A premium platform connecting customers and sellers with speed, security, and clarity at every step.",
    point1: "Secure access for every account",
    point2: "Powerful tools for sellers and customers",
    point3: "Fast, reliable, and ready to scale",
  },
  login: {
    title: "Welcome back",
    subtitle: "Sign in to continue to your account.",
    continueWithAuth0: "Continue with Auth0",
    signInCta: "Sign in",
    noAccount: "Don't have an account?",
    registerLink: "Create one",
  },
  register: {
    title: "Create your account",
    subtitle: "Choose how you want to use the platform.",
    chooseRole: "I want to join as",
    createAccount: "Create account",
    haveAccount: "Already have an account?",
    loginLink: "Sign in",
    auth0Note:
      "Secure account creation is handled by Auth0. You will pick your role after authentication.",
    continueWithAuth0: "Sign up with Auth0",
  },
  onboarding: {
    title: "One last step",
    subtitle: "Tell us how you will use the platform.",
    confirmRole: "Confirm and continue",
    savingRole: "Saving your choice",
    note: "Public onboarding supports Customer and Seller only. Admin access is assigned operationally.",
  },
  roles: {
    customer: {
      name: "Customer",
      description: "Shop across the catalog and manage your orders.",
      feature1: "Browse and discover products",
      feature2: "Track orders and deliveries",
      feature3: "Manage your basket",
    },
    seller: {
      name: "Seller",
      description: "Sell products and manage your business.",
      feature1: "List and manage products",
      feature2: "Control stock and inventory",
      feature3: "Track seller workflows",
    },
  },
  home: {
    signedInAs: "Signed in as",
    welcome: "You are signed in",
    placeholder:
      "This is the first authenticated shell. Catalog, basket, seller, and admin experiences will be added here.",
    signOut: "Sign out",
    role: "Roles",
  },
  admin: {
    workspace: "Workspace",
    navigation: "Administration navigation",
    overview: "Overview",
    catalog: "Catalog",
    users: "Users",
    orders: "Orders",
    inventory: "Inventory",
    payments: "Payments",
    shipments: "Shipments",
    workflows: "Workflows",
    openSellerWorkspace: "Seller workspace",
    roleLabel: "Administrator",
    welcome: "Welcome back, {name}",
    description: "Monitor the catalog and begin managing the marketplace from one operational workspace.",
    catalogSummary: "Catalog summary",
    catalogSummaryDescription: "Current marketplace totals from the protected Catalog metrics API.",
    metricsUnavailable: "Catalog metrics are currently unavailable.",
    totalProducts: "Total products",
    activeProducts: "Active products",
    draftProducts: "Draft products",
    inactiveProducts: "Inactive products",
    archivedProducts: "Archived products",
    totalStores: "Stores",
    totalCategories: "Categories",
    totalBrands: "Brands",
    recentProducts: "Recent catalog products",
    recentProductsDescription: "The latest products returned by the Catalog service.",
    catalogUnavailable: "Catalog data is currently unavailable. Check that the gateway and Catalog service are running.",
    noProducts: "No products have been created in the catalog yet.",
    manageCatalog: "Manage catalog",
    manageCatalogDescription: "Search, filter, page through, and update marketplace products across every store.",
    manageReferences: "Categories and brands",
    manageReferencesDescription: "Create the active catalog references sellers need before adding products.",
    backToOverview: "Back to overview",
    referenceNavigation: "Catalog reference navigation",
    categories: "Categories",
    brands: "Brands",
    stores: "Stores",
    manageCategoriesDescription: "Search, sort, page through, create, and update every English/Turkish category pair.",
    manageBrandsDescription: "Search, sort, page through, create, and update every marketplace brand.",
    openCategoryManagement: "Manage categories",
    openBrandManagement: "Manage brands",
    categoryList: "All categories",
    categoryListDescription: "Active and inactive categories are listed in one searchable table.",
    brandList: "All brands",
    brandListDescription: "Active and inactive brands are listed in one searchable table.",
    newCategory: "New category",
    newBrand: "New brand",
    categoryEnglishName: "English category name",
    categoryTurkishName: "Turkish category name",
    brandName: "Brand name",
    referenceSlug: "Slug",
    referenceSlugHint: "Use lowercase English letters, numbers, and single hyphens.",
    createCategory: "Create category",
    creatingCategory: "Creating category",
    categoryCreated: "Category created successfully.",
    categoryCreateFailed: "The category could not be created.",
    createBrand: "Create brand",
    creatingBrand: "Creating brand",
    brandCreated: "Brand created successfully.",
    brandCreateFailed: "The brand could not be created.",
    updateCategory: "Update category",
    updateBrand: "Update brand",
    updatingReference: "Saving changes",
    categoryUpdated: "Category updated successfully.",
    brandUpdated: "Brand updated successfully.",
    referenceUpdateFailed: "The catalog reference could not be updated.",
    activeReference: "Active",
    inactiveReference: "Inactive",
    searchCategories: "Search categories",
    searchCategoriesPlaceholder: "English name, Turkish name, or address",
    searchBrands: "Search brands",
    searchBrandsPlaceholder: "Brand name or address",
    referenceStatus: "Status",
    allReferenceStatuses: "All statuses",
    rowsPerPage: "Rows per page",
    referenceActions: "Actions",
    referencePageStatus: "Page {page} of {total} · {count} results",
    noMatchingCategories: "No categories match the current search and status filter.",
    noMatchingBrands: "No brands match the current search and status filter.",
    editCategory: "Edit category",
    editBrand: "Edit brand",
    cancelReferenceForm: "Close form",
    referencesUnavailable: "Categories and brands could not be loaded.",
    noCategories: "No active categories have been created.",
    noBrands: "No active brands have been created.",
    searchProducts: "Search products",
    searchProductsPlaceholder: "Product name or SKU",
    filterStatus: "Status",
    allStatuses: "All statuses",
    searchAction: "Search",
    noMatchingProducts: "No products match the current filters.",
    sellerProduct: "Seller product",
    platformProduct: "Platform product",
    editProduct: "Edit product",
    edit: "Edit",
    pageStatus: "Page {page} of {total}",
    previousPage: "Previous page",
    nextPage: "Next page",
    selectProductToEdit: "Select a product to update its catalog details, lifecycle status, or images.",
    manageUsers: "Users",
    manageUsersDescription: "Search and review locally registered marketplace identities.",
    searchUsers: "Search users",
    searchUsersPlaceholder: "Name or email address",
    filterRole: "Role",
    allRoles: "All roles",
    filterUserStatus: "Account status",
    allUserStatuses: "All statuses",
    userActive: "Active",
    userDisabled: "Disabled",
    usersUnavailable: "User data is currently unavailable.",
    noMatchingUsers: "No users match the current filters.",
    onboardingPending: "Onboarding pending",
    joinedAt: "Joined",
    userPageStatus: "Page {page} of {total} · {count} users",
    usersReadOnlyNote: "This view is read-only. Roles and Auth0 accounts cannot be changed here.",
    viewUserDetails: "View details",
    userDetails: "User details",
    userDetailsDescription: "Review the safe local profile and account state. Identity-provider identifiers and credentials are excluded.",
    userDetailsUnavailable: "User details could not be loaded.",
    userNotFound: "The requested user could not be found.",
    userId: "User ID",
    userRoles: "Roles",
    onboardingStatus: "Onboarding",
    onboardingComplete: "Complete",
    backToUsers: "Back to users",
    manageOrdersDescription: "Search and review marketplace orders across customers with bounded server pagination.",
    orderList: "Marketplace orders",
    orderCustomerId: "Customer ID",
    orderCustomerIdPlaceholder: "Customer GUID or leave empty",
    applyOrderFilter: "Apply customer filter",
    invalidCustomerId: "Enter a valid customer GUID.",
    orderStatus: "Order status",
    allOrderStatuses: "All statuses",
    orderSort: "Order",
    newestOrders: "Newest first",
    oldestOrders: "Oldest first",
    ordersUnavailable: "Orders could not be loaded.",
    noMatchingOrders: "No orders match the selected filters.",
    orderNumber: "Order ID",
    orderTotal: "Total",
    orderCreatedAt: "Created",
    orderUpdatedAt: "Updated",
    orderPageStatus: "Page {page} of {total} · {count} orders",
    manageInventoryDescription: "Review stock levels across products with bounded server-side filters and sorting.",
    inventoryList: "Inventory operations",
    inventoryProductId: "Product ID",
    inventoryProductIdPlaceholder: "Product GUID or leave empty",
    invalidProductId: "Enter a valid product GUID.",
    maximumAvailable: "Maximum available",
    maximumAvailablePlaceholder: "Optional whole number",
    invalidMaximumAvailable: "Enter zero or a positive whole number.",
    inventorySort: "Sort field",
    inventoryDirection: "Direction",
    descending: "Descending",
    ascending: "Ascending",
    quantityOnHand: "On hand",
    reservedQuantity: "Reserved",
    availableQuantity: "Available",
    inventoryUpdatedAt: "Last updated",
    applyInventoryFilters: "Apply inventory filters",
    inventoryUnavailable: "Inventory data could not be loaded.",
    noMatchingInventory: "No inventory items match the selected filters.",
    inventoryPageStatus: "Page {page} of {total} · {count} inventory items",
    viewStockMovements: "View movements",
    stockMovements: "Stock movements",
    stockMovementsDescription: "Immutable stock changes recorded for product {productId}.",
    stockMovementsUnavailable: "Stock movements could not be loaded.",
    noStockMovements: "No stock movements were recorded for this product.",
    movementType: "Type",
    movementQuantity: "Quantity",
    movementOnHand: "On hand before → after",
    movementReserved: "Reserved before → after",
    movementOrder: "Order",
    movementOccurredAt: "Occurred",
    movementPageStatus: "Page {page} of {total} · {count} movements",
    close: "Close",
    managePaymentsDescription: "Review safe payment summaries across customers and orders with bounded server-side filters.",
    paymentList: "Payment operations",
    paymentCustomerId: "Customer ID",
    paymentOrderId: "Order ID",
    guidFilterPlaceholder: "GUID or leave empty",
    invalidOrderId: "Enter a valid order GUID.",
    createdFrom: "Created from",
    createdTo: "Created to",
    invalidDateRange: "The end date must be on or after the start date.",
    paymentStatus: "Payment status",
    allPaymentStatuses: "All statuses",
    paymentSort: "Sort field",
    paymentAmount: "Amount",
    paymentId: "Payment ID",
    paymentCreatedAt: "Created",
    paymentUpdatedAt: "Updated",
    applyPaymentFilters: "Apply payment filters",
    paymentsUnavailable: "Payment data could not be loaded.",
    noMatchingPayments: "No payments match the selected filters.",
    paymentPageStatus: "Page {page} of {total} · {count} payments",
    manageShipmentsDescription: "Review safe shipment summaries across customers and orders with bounded server-side filters.",
    shipmentList: "Shipment operations",
    shipmentStatus: "Shipment status",
    allShipmentStatuses: "All statuses",
    shipmentSort: "Sort field",
    shipmentId: "Shipment ID",
    trackingNumber: "Tracking number",
    shipmentCreated: "Created",
    shipmentFailed: "Failed",
    shipmentInTransit: "In transit",
    shipmentDelivered: "Delivered",
    notAvailable: "Not available",
    applyShipmentFilters: "Apply shipment filters",
    shipmentsUnavailable: "Shipment data could not be loaded.",
    noMatchingShipments: "No shipments match the selected filters.",
    shipmentPageStatus: "Page {page} of {total} · {count} shipments",
    manageStoresDescription: "Search and review stores across owners with bounded server-side sorting and pagination.",
    storeList: "Marketplace stores",
    storeSearch: "Search",
    storeSearchPlaceholder: "Store name or address",
    storeOwnerId: "Owner user ID",
    invalidOwnerId: "Enter a valid owner GUID.",
    storeSort: "Sort field",
    storeName: "Store name",
    storeSlug: "Store slug",
    applyStoreFilters: "Apply store filters",
    storesUnavailable: "Store data could not be loaded.",
    noMatchingStores: "No stores match the selected filters.",
    storePageStatus: "Page {page} of {total} · {count} stores",
    manageWorkflowsDescription: "Inspect bounded order-saga workflow state and overdue processing deadlines.",
    workflowList: "Order workflow diagnostics",
    workflowOrderId: "Order ID",
    workflowStatus: "Workflow status",
    allWorkflowStatuses: "All statuses",
    workflowOverdueOnly: "Overdue only",
    overdueWorkflowsTitle: "Order workflows need attention",
    overdueWorkflowsBody: "{count} order workflows are past their step deadline.",
    reviewOverdueWorkflows: "Review workflows",
    metricsRetry: "Try again",
    workflowDeadline: "Step deadline",
    workflowTimeoutHandled: "Timeout handled",
    workflowCreatedAt: "Created",
    workflowUpdatedAt: "Updated",
    workflowsUnavailable: "Workflow diagnostics could not be loaded.",
    noMatchingWorkflows: "No workflows match the selected filters.",
    workflowPageStatus: "Page {page} of {total} · {count} workflows",
    applyWorkflowFilters: "Apply workflow filters",
    userRole: {
      Customer: "Customer",
      Seller: "Seller",
      Admin: "Administrator",
    },
    status: {
      Active: "Active",
      Draft: "Draft",
      Inactive: "Inactive",
      Archived: "Archived",
    },
  },
  seller: {
    workspace: "Seller workspace",
    navigation: "Seller navigation",
    overview: "Overview",
    storePageDescription: "Create the stores you sell from and keep their names and addresses current.",
    productPageDescription: "Pick a store, then add products, edit their catalog details, and adjust stock.",
    storeCount: "{count} stores",
    manageStoresDescription: "Create a store or update the ones you already own.",
    manageProductsDescription: "Add products to a store and keep their details and stock current.",
    manageOrdersDescription: "Review the order lines and totals attributed to each store.",
    openAdminWorkspace: "Administrator workspace",
    roleLabel: "Seller",
    welcome: "Welcome back, {name}",
    description: "Manage stores you own and review the products assigned to each store.",
    stores: "Your stores",
    products: "Store products",
    createStore: "Create a store",
    storeName: "Store name",
    storeNamePlaceholder: "Example Store",
    storeSlug: "Store slug",
    storeSlugPlaceholder: "example-store",
    slugHint: "Use lowercase letters, numbers, and single hyphens.",
    saveStore: "Create store",
    savingStore: "Creating store",
    editStore: "Edit store",
    updateStore: "Save changes",
    updatingStore: "Saving changes",
    storeUpdateFailed: "The store could not be updated. Check the fields and try again.",
    noStores: "You do not have a store yet.",
    noStoresDescription: "Create your first store before adding products.",
    storeLoadFailed: "Your stores could not be loaded.",
    storeCreateFailed: "The store could not be created. Check the fields and try again.",
    slugConflict: "This store address is already in use.",
    selectStore: "Select a store",
    productCount: "{count} products",
    noProducts: "This store does not have any products yet.",
    productLoadFailed: "Products for this store could not be loaded.",
    createProduct: "Add a product",
    sku: "SKU",
    productName: "Product name",
    productDescription: "Description",
    productNameEnglish: "English product name",
    productDescriptionEnglish: "English description",
    productNameTurkish: "Turkish product name",
    productDescriptionTurkish: "Turkish description",
    category: "Category",
    brand: "Brand",
    price: "Price",
    currency: "Currency",
    statusLabel: "Status",
    draft: "Draft",
    active: "Active",
    inactive: "Inactive",
    archived: "Archived",
    saveProduct: "Create product",
    savingProduct: "Creating product",
    productCreateFailed: "The product could not be created. Check the fields and try again.",
    referencesUnavailable: "Category and brand options could not be loaded.",
    referencesEmpty: "Products require an active category and brand. Only an Administrator can create them from Category and brand management.",
    refreshReferences: "Reload categories and brands",
    refreshingReferences: "Reloading categories and brands",
    productCreated: "Product created successfully.",
    productCreatedAddImages: "Product created. You can add its images below.",
    edit: "Edit",
    editProduct: "Edit product",
    cancelEditing: "Close product editor",
    saveChanges: "Save changes",
    savingChanges: "Saving changes",
    productUpdated: "Product updated successfully.",
    productUpdateFailed: "The product could not be updated. Check the fields and your store access.",
    currentImages: "Current images",
    noImages: "This product does not have an image yet.",
    uploadImage: "Upload image",
    uploadingImage: "Uploading image",
    imageUploadHint: "JPEG, PNG, GIF, or WebP. Maximum 5 MB and 8 images per product.",
    imageInvalid: "Choose a supported image up to 5 MB.",
    imageUploaded: "Image uploaded successfully.",
    imageUploadFailed: "The image could not be uploaded. Check the file or media configuration.",
    mainImage: "Main",
    setMainImage: "Set as main image",
    mainImageUpdated: "Main image updated.",
    imageUpdateFailed: "The image could not be updated.",
    deleteImage: "Delete image",
    imageDeleted: "Image deleted.",
    imageDeleteFailed: "The image could not be deleted.",
    manageStock: "Stock",
    stockTitle: "Manage stock",
    quantityOnHand: "Quantity on hand",
    reservedQuantity: "Reserved",
    availableQuantity: "Available",
    stockNotCreated: "No stock record exists yet. Saving will create it.",
    saveStock: "Save stock",
    savingStock: "Saving stock",
    stockUpdated: "Stock updated successfully.",
    stockLoadFailed: "Stock information could not be loaded.",
    stockUpdateFailed: "Stock could not be updated. Check your product access and try again.",
    stockInvalid: "Enter a whole number of zero or greater.",
    closeStockEditor: "Close stock editor",
    openOrders: "Store orders",
    orderPageTitle: "Store orders",
    orderPageDescription: "Review the order lines and totals attributed to each store you own.",
    orderStore: "Store",
    orderStatus: "Order status",
    allOrderStatuses: "All statuses",
    orderSort: "Order",
    newestOrders: "Newest first",
    oldestOrders: "Oldest first",
    orderRowsPerPage: "Orders per page",
    ordersLoadFailed: "Orders for this store could not be loaded.",
    noOrders: "No attributed orders match the selected filters.",
    orderCount: "{count} orders",
    orderNumber: "Order",
    placedAt: "Placed",
    lastUpdated: "Last updated",
    storeTotal: "Store total",
    orderItems: "Store items",
    showOrderItems: "Show {count} items",
    hideOrderItems: "Hide items",
    orderItemsLoadFailed: "The items for this order could not be loaded.",
    orderProduct: "Product",
    orderQuantity: "Quantity",
    orderUnitPrice: "Unit price",
    orderLineTotal: "Line total",
    orderPageStatus: "Page {page} of {total} · {count} orders",
    previousOrderPage: "Previous page",
    nextOrderPage: "Next page",
  },
  customer: {
    roleLabel: "Customer",
    welcome: "Find something worth keeping, {name}",
    description: "Explore active products from marketplace sellers. Search by name or SKU, then narrow the catalog by category and brand.",
    searchLabel: "Search products",
    searchPlaceholder: "Product name or SKU",
    searchAction: "Search",
    category: "Category",
    allCategories: "All categories",
    brand: "Brand",
    allBrands: "All brands",
    sort: "Sort",
    filters: "Filters",
    showFilters: "Show filters",
    hideFilters: "Hide filters",
    priceRange: "Price range",
    minPrice: "Min",
    maxPrice: "Max",
    applyFilters: "Apply",
    clearFilters: "Clear all filters",
    priceRangeInvalid: "Enter a valid price range.",
    newest: "Newest",
    priceLowToHigh: "Price: low to high",
    priceHighToLow: "Price: high to low",
    results: "{count} active products",
    noProducts: "No active products match these filters.",
    catalogUnavailable: "The catalog is temporarily unavailable.",
    filtersUnavailable: "Category and brand filters are temporarily unavailable.",
    previousPage: "Previous",
    nextPage: "Next",
    page: "Page {current} of {total}",
    viewProduct: "View product",
    inStock: "In stock",
    lowStock: "Only {count} left",
    outOfStock: "Out of stock",
    productUnavailable: "This product is unavailable or no longer active.",
    backToCatalog: "Back to catalog",
    browseTitle: "Browse the marketplace",
    catalog: "Catalog",
    navigation: "Customer navigation",
    productDetails: "Product details",
    purchase: "Purchase",
    storeDetails: "Store details",
    viewStore: "View store",
    storeUnavailable: "This store is unavailable.",
    storeProducts: "Products from this store",
    backToStore: "Back to store",
  },
  basket: {
    title: "Your basket",
    description: "Review quantities and totals before continuing to checkout.",
    openBasket: "Basket",
    addToBasket: "Add to basket",
    adding: "Adding",
    added: "Added",
    addFailed: "The product could not be added to your basket.",
    outOfStock: "This product is out of stock; remove it to continue.",
    stockLimited: "Only {count} left in stock; lower the quantity to continue.",
    signInToAdd: "Sign in to add this product",
    empty: "Your basket is empty.",
    continueShopping: "Continue shopping",
    quantity: "Quantity",
    unitPrice: "Unit price",
    total: "Total",
    remove: "Remove",
    clear: "Clear basket",
    updateFailed: "The basket could not be updated.",
    loadFailed: "Your basket could not be loaded.",
    temporarilyUnavailable: "The basket service is temporarily unavailable. Your basket is preserved, please try again shortly.",
    checkout: "Confirm basket",
    checkingOut: "Confirming basket",
    checkoutRecorded: "Basket confirmed",
    checkoutNote: "Your order was accepted and is now being processed.",
    demoPayment: "Demo payment",
    demoPaymentNote: "No real charge is made and no card information is requested.",
    recipientName: "Recipient name",
    addressLine: "Address",
    city: "City",
    countryCode: "Country code",
    postalCode: "Postal code",
    addressRequired: "Complete the shipping address before confirming your basket.",
    viewOrder: "View order",
  },
  orders: {
    title: "Your orders",
    openOrders: "Orders",
    description: "Follow order processing, payment, and shipment status.",
    empty: "You have not placed an order yet.",
    loadFailed: "Your orders could not be loaded.",
    orderNumber: "Order",
    placedAt: "Placed",
    pageStatus: "Page {page} of {total}",
    previousPage: "Previous page",
    nextPage: "Next page",
    status: {
      Submitted: "Processing",
      InventoryReserved: "Stock reserved",
      PaymentAuthorized: "Payment authorized",
      ShipmentCreated: "Shipment created",
      Confirmed: "Confirmed",
      Cancelled: "Cancelled",
      CancellationRequested: "Cancellation requested",
    },
    processing: "Order is being prepared",
    processingNote: "The order handoff is durable. This page updates when processing completes.",
    backToOrders: "Back to orders",
    shippingAddress: "Shipping address",
    cancellationReason: "Cancellation reason",
    cancelOrder: "Cancel order",
    cancellingOrder: "Requesting cancellation",
    cancellationRequested: "Cancellation requested. Compensation is being completed.",
    cancellationFailed: "The cancellation request could not be completed.",
    orderNotCancellable: "This order can no longer be cancelled.",
    cancellationReasons: {
      INSUFFICIENT_STOCK: "One or more products are out of stock.",
      PAYMENT_FAILED: "The payment could not be authorized.",
      SHIPMENT_FAILED: "The shipment could not be created.",
      UNEXPECTED_ERROR: "The order could not be completed.",
      ORDER_CANCELLED_BY_CUSTOMER: "You cancelled this order.",
    },
    payment: {
      title: "Payment",
      pending: "Payment details are being prepared.",
      unavailable: "Payment details are temporarily unavailable.",
      authorization: "Authorization",
      refund: "Refund",
      status: {
        authorized: "Authorized",
        failed: "Failed",
        refunded: "Refunded",
      },
    },
    shipment: {
      title: "Shipment",
      pending: "Shipment details are being prepared.",
      unavailable: "Shipment details are temporarily unavailable.",
      created: "Created",
      failed: "Failed",
      inTransit: "In transit",
      delivered: "Delivered",
      trackingNumber: "Tracking number",
      notAvailable: "Not available",
      createdAt: "Created",
    },
  },
  notifications: {
    title: "Notifications",
    openNotifications: "Notifications",
    openNotificationsWithUnread: "Notifications, {count} unread",
    description: "Review order, payment, and shipment updates.",
    all: "All",
    unread: "Unread",
    unreadLabel: "Unread",
    readLabel: "Read",
    empty: "You do not have any notifications yet.",
    emptyUnread: "You do not have any unread notifications.",
    loadFailed: "Your notifications could not be loaded.",
    markRead: "Mark as read",
    markingRead: "Marking as read",
    markAllRead: "Mark all as read",
    markingAllRead: "Marking all as read",
    viewOrder: "View order",
    previousPage: "Previous",
    nextPage: "Next",
    pageStatus: "Page {page} of {total}",
    orderTitle: "Order {code}",
    types: {
      orderSubmitted: "Your order has been submitted.",
      orderConfirmed: "Your order is confirmed.",
      orderCancelled: "Your order was cancelled.",
      orderCancellationRequested: "Your cancellation request was received.",
      orderCancellationRejected: "Your cancellation request was rejected; the order already progressed.",
      paymentAuthorized: "Payment has been authorized.",
      paymentFailed: "Payment failed.",
      shipmentCreated: "Shipment created. Tracking number: {trackingNumber}",
      shipmentCreatedWithoutTracking: "Shipment created.",
      shipmentFailed: "Shipment could not be created.",
    },
  },
  validation: {
    required: "This field is required.",
    invalidEmail: "Please enter a valid email address.",
    passwordTooShort: "Password must be at least 8 characters.",
    displayNameTooShort: "Display name must be at least 2 characters.",
    selectRole: "Please select a role to continue.",
  },
  status: {
    payment: {
      authorized: "Authorized",
      failed: "Failed",
      refunded: "Refunded",
    },
    shipment: {
      created: "Created",
      failed: "Failed",
      inTransit: "In transit",
      delivered: "Delivered",
    },
    product: {
      draft: "Draft",
      active: "Active",
      inactive: "Inactive",
      archived: "Archived",
    },
    user: {
      active: "Active",
      disabled: "Disabled",
    },
    workflow: {
      submitted: "Submitted",
      inventoryReserved: "Inventory reserved",
      paymentAuthorized: "Payment authorized",
      shipmentCreated: "Shipment created",
      completed: "Completed",
      cancelled: "Cancelled",
    },
    stockMovement: {
      stockInitialized: "Stock initialized",
      stockIncreased: "Stock increased",
      stockDecreased: "Stock decreased",
      stockReserved: "Stock reserved",
      stockReleased: "Stock released",
      auditBaseline: "Audit baseline",
      stockShipped: "Shipped",
    },
  },
  table: {
    empty: "No records found",
    emptyDescription: "Try changing the filters or check back later.",
    loadFailed: "Could not load data",
    loadFailedDescription: "The request failed. Please try again.",
    retry: "Try again",
    previousPage: "Previous",
    nextPage: "Next",
    pageStatus: "Page {page} of {totalPages}",
    rowsPerPage: "Rows per page",
    totalItems: "{count} records",
    clearFilters: "Clear filters",
    filters: "Filters",
  },
  errors: {
    generic: "Something went wrong. Please try again.",
    roleUpdateFailed: "We could not save your role. Please try again.",
  },
}

const tr: Dictionary = {
  brand: {
    name: "Marketplace",
    tagline: "Ticaretin rafine hali.",
  },
  common: {
    email: "E-posta",
    emailPlaceholder: "siz@ornek.com",
    password: "Parola",
    passwordPlaceholder: "Parolanızı girin",
    displayName: "Görünen ad",
    displayNamePlaceholder: "Size nasıl hitap edelim?",
    continue: "Devam et",
    back: "Geri",
    or: "veya",
    loading: "Lütfen bekleyin",
    showPassword: "Parolayı göster",
    hidePassword: "Parolayı gizle",
    toggleTheme: "Temayı değiştir",
    lightMode: "Açık",
    darkMode: "Koyu",
    changeLanguage: "Dili değiştir",
  },
  aside: {
    trustTitle: "Alışveriş, daha sade.",
    trustBody:
      "Müşterileri ve satıcıları hız, güvenlik ve netlikle buluşturan modern bir e-ticaret platformu.",
    point1: "Her hesap için güvenli erişim",
    point2: "Satıcılar ve müşteriler için güçlü araçlar",
    point3: "Hızlı, güvenilir ve büyümeye hazır",
  },
  login: {
    title: "Tekrar hoş geldiniz",
    subtitle: "Hesabınıza devam etmek için giriş yapın.",
    continueWithAuth0: "Auth0 ile devam et",
    signInCta: "Giriş yap",
    noAccount: "Hesabınız yok mu?",
    registerLink: "Hesap oluşturun",
  },
  register: {
    title: "Hesabınızı oluşturun",
    subtitle: "Platformu nasıl kullanmak istediğinizi seçin.",
    chooseRole: "Şu rolle katılmak istiyorum",
    createAccount: "Hesap oluştur",
    haveAccount: "Zaten hesabınız var mı?",
    loginLink: "Giriş yap",
    auth0Note:
      "Güvenli hesap oluşturma Auth0 tarafından yönetilir. Kimlik doğrulamadan sonra rolünüzü seçeceksiniz.",
    continueWithAuth0: "Auth0 ile kaydol",
  },
  onboarding: {
    title: "Son bir adım",
    subtitle: "Platformu nasıl kullanacağınızı seçin.",
    confirmRole: "Onayla ve devam et",
    savingRole: "Seçiminiz kaydediliyor",
    note: "Herkese açık kayıt akışında yalnızca Müşteri ve Satıcı rolleri seçilebilir. Admin erişimi operasyonel olarak atanır.",
  },
  roles: {
    customer: {
      name: "Müşteri",
      description: "Katalogda gezinin ve siparişlerinizi yönetin.",
      feature1: "Ürünleri keşfedin ve inceleyin",
      feature2: "Siparişleri ve teslimatları takip edin",
      feature3: "Sepetinizi yönetin",
    },
    seller: {
      name: "Satıcı",
      description: "Ürünlerinizi satın ve işinizi yönetin.",
      feature1: "Ürünleri listeleyin ve yönetin",
      feature2: "Stok ve envanteri kontrol edin",
      feature3: "Satıcı iş akışlarını takip edin",
    },
  },
  home: {
    signedInAs: "Giriş yapan",
    welcome: "Giriş yaptınız",
    placeholder:
      "Bu ilk kimliği doğrulanmış ekran. Katalog, sepet, satıcı ve admin deneyimleri buraya eklenecek.",
    signOut: "Çıkış yap",
    role: "Roller",
  },
  admin: {
    workspace: "Çalışma alanı",
    navigation: "Yönetim gezinmesi",
    overview: "Genel bakış",
    catalog: "Katalog",
    users: "Kullanıcılar",
    orders: "Siparişler",
    inventory: "Stok",
    payments: "Ödemeler",
    shipments: "Gönderiler",
    workflows: "İş akışları",
    openSellerWorkspace: "Satıcı paneli",
    roleLabel: "Yönetici",
    welcome: "Tekrar hoş geldiniz, {name}",
    description: "Kataloğu izleyin ve pazaryerini tek bir operasyon alanından yönetmeye başlayın.",
    catalogSummary: "Katalog özeti",
    catalogSummaryDescription: "Korumalı Katalog metrik API'sinden güncel pazaryeri toplamları.",
    metricsUnavailable: "Katalog metrikleri şu anda kullanılamıyor.",
    totalProducts: "Toplam ürün",
    activeProducts: "Aktif ürünler",
    draftProducts: "Taslak ürünler",
    inactiveProducts: "Pasif ürünler",
    archivedProducts: "Arşivlenmiş ürünler",
    totalStores: "Mağazalar",
    totalCategories: "Kategoriler",
    totalBrands: "Markalar",
    recentProducts: "Son katalog ürünleri",
    recentProductsDescription: "Katalog servisinin döndürdüğü en yeni ürünler.",
    catalogUnavailable: "Katalog verisi şu anda kullanılamıyor. Gateway ve Catalog servisinin çalıştığını kontrol edin.",
    noProducts: "Katalogda henüz ürün oluşturulmadı.",
    manageCatalog: "Kataloğu yönet",
    manageCatalogDescription: "Tüm mağazalardaki ürünleri arayın, filtreleyin, sayfalayın ve güncelleyin.",
    manageReferences: "Kategoriler ve markalar",
    manageReferencesDescription: "Satıcıların ürün ekleyebilmesi için gereken aktif katalog referanslarını oluşturun.",
    backToOverview: "Genel bakışa dön",
    referenceNavigation: "Katalog referansı gezinmesi",
    categories: "Kategoriler",
    brands: "Markalar",
    stores: "Mağazalar",
    manageCategoriesDescription: "Tüm İngilizce/Türkçe kategori eşleşmelerini arayın, sıralayın, sayfalayın, oluşturun ve güncelleyin.",
    manageBrandsDescription: "Tüm pazaryeri markalarını arayın, sıralayın, sayfalayın, oluşturun ve güncelleyin.",
    openCategoryManagement: "Kategorileri yönet",
    openBrandManagement: "Markaları yönet",
    categoryList: "Tüm kategoriler",
    categoryListDescription: "Aktif ve pasif kategoriler aranabilir tek tabloda listelenir.",
    brandList: "Tüm markalar",
    brandListDescription: "Aktif ve pasif markalar aranabilir tek tabloda listelenir.",
    newCategory: "Yeni kategori",
    newBrand: "Yeni marka",
    categoryEnglishName: "İngilizce kategori adı",
    categoryTurkishName: "Türkçe kategori adı",
    brandName: "Marka adı",
    referenceSlug: "URL adı",
    referenceSlugHint: "Küçük İngilizce harfler, rakamlar ve tek tire kullanın.",
    createCategory: "Kategori oluştur",
    creatingCategory: "Kategori oluşturuluyor",
    categoryCreated: "Kategori başarıyla oluşturuldu.",
    categoryCreateFailed: "Kategori oluşturulamadı.",
    createBrand: "Marka oluştur",
    creatingBrand: "Marka oluşturuluyor",
    brandCreated: "Marka başarıyla oluşturuldu.",
    brandCreateFailed: "Marka oluşturulamadı.",
    updateCategory: "Kategoriyi güncelle",
    updateBrand: "Markayı güncelle",
    updatingReference: "Değişiklikler kaydediliyor",
    categoryUpdated: "Kategori başarıyla güncellendi.",
    brandUpdated: "Marka başarıyla güncellendi.",
    referenceUpdateFailed: "Katalog referansı güncellenemedi.",
    activeReference: "Aktif",
    inactiveReference: "Pasif",
    searchCategories: "Kategori ara",
    searchCategoriesPlaceholder: "İngilizce ad, Türkçe ad veya adres",
    searchBrands: "Marka ara",
    searchBrandsPlaceholder: "Marka adı veya adres",
    referenceStatus: "Durum",
    allReferenceStatuses: "Tüm durumlar",
    rowsPerPage: "Sayfa başına satır",
    referenceActions: "İşlemler",
    referencePageStatus: "Sayfa {page} / {total} · {count} sonuç",
    noMatchingCategories: "Arama ve durum filtresiyle eşleşen kategori bulunamadı.",
    noMatchingBrands: "Arama ve durum filtresiyle eşleşen marka bulunamadı.",
    editCategory: "Kategoriyi düzenle",
    editBrand: "Markayı düzenle",
    cancelReferenceForm: "Formu kapat",
    referencesUnavailable: "Kategoriler ve markalar yüklenemedi.",
    noCategories: "Henüz aktif kategori oluşturulmadı.",
    noBrands: "Henüz aktif marka oluşturulmadı.",
    searchProducts: "Ürün ara",
    searchProductsPlaceholder: "Ürün adı veya stok kodu",
    filterStatus: "Durum",
    allStatuses: "Tüm durumlar",
    searchAction: "Ara",
    noMatchingProducts: "Seçili filtrelerle eşleşen ürün bulunamadı.",
    sellerProduct: "Satıcı ürünü",
    platformProduct: "Platform ürünü",
    editProduct: "Ürünü düzenle",
    edit: "Düzenle",
    pageStatus: "Sayfa {page} / {total}",
    previousPage: "Önceki sayfa",
    nextPage: "Sonraki sayfa",
    selectProductToEdit: "Katalog bilgilerini, yaşam döngüsü durumunu veya görsellerini güncellemek için bir ürün seçin.",
    manageUsers: "Kullanıcılar",
    manageUsersDescription: "Yerel olarak kayıtlı pazaryeri kimliklerini arayın ve inceleyin.",
    searchUsers: "Kullanıcı ara",
    searchUsersPlaceholder: "Ad veya e-posta adresi",
    filterRole: "Rol",
    allRoles: "Tüm roller",
    filterUserStatus: "Hesap durumu",
    allUserStatuses: "Tüm durumlar",
    userActive: "Aktif",
    userDisabled: "Devre dışı",
    usersUnavailable: "Kullanıcı verisi şu anda kullanılamıyor.",
    noMatchingUsers: "Seçili filtrelerle eşleşen kullanıcı bulunamadı.",
    onboardingPending: "Rol seçimi bekleniyor",
    joinedAt: "Katılma tarihi",
    userPageStatus: "Sayfa {page} / {total} · {count} kullanıcı",
    usersReadOnlyNote: "Bu ekran salt okunurdur. Roller ve Auth0 hesapları buradan değiştirilemez.",
    viewUserDetails: "Ayrıntıları görüntüle",
    userDetails: "Kullanıcı ayrıntıları",
    userDetailsDescription: "Güvenli yerel profili ve hesap durumunu inceleyin. Kimlik sağlayıcı tanımlayıcıları ve kimlik bilgileri gösterilmez.",
    userDetailsUnavailable: "Kullanıcı ayrıntıları yüklenemedi.",
    userNotFound: "İstenen kullanıcı bulunamadı.",
    userId: "Kullanıcı kimliği",
    userRoles: "Roller",
    onboardingStatus: "İlk kurulum",
    onboardingComplete: "Tamamlandı",
    backToUsers: "Kullanıcılara dön",
    manageOrdersDescription: "Tüm müşterilerin pazaryeri siparişlerini sınırlı sunucu sayfalamasıyla arayın ve inceleyin.",
    orderList: "Pazaryeri siparişleri",
    orderCustomerId: "Müşteri kimliği",
    orderCustomerIdPlaceholder: "Müşteri GUID değeri veya boş bırakın",
    applyOrderFilter: "Müşteri filtresini uygula",
    invalidCustomerId: "Geçerli bir müşteri GUID değeri girin.",
    orderStatus: "Sipariş durumu",
    allOrderStatuses: "Tüm durumlar",
    orderSort: "Sıralama",
    newestOrders: "En yeni önce",
    oldestOrders: "En eski önce",
    ordersUnavailable: "Siparişler yüklenemedi.",
    noMatchingOrders: "Seçili filtrelerle eşleşen sipariş bulunamadı.",
    orderNumber: "Sipariş kimliği",
    orderTotal: "Toplam",
    orderCreatedAt: "Oluşturulma",
    orderUpdatedAt: "Güncellenme",
    orderPageStatus: "Sayfa {page} / {total} · {count} sipariş",
    manageInventoryDescription: "Tüm ürünlerin stok seviyelerini sınırlı sunucu filtreleri ve sıralamayla inceleyin.",
    inventoryList: "Stok operasyonları",
    inventoryProductId: "Ürün kimliği",
    inventoryProductIdPlaceholder: "Ürün GUID değeri veya boş bırakın",
    invalidProductId: "Geçerli bir ürün GUID değeri girin.",
    maximumAvailable: "En fazla kullanılabilir",
    maximumAvailablePlaceholder: "İsteğe bağlı tam sayı",
    invalidMaximumAvailable: "Sıfır veya pozitif bir tam sayı girin.",
    inventorySort: "Sıralama alanı",
    inventoryDirection: "Yön",
    descending: "Azalan",
    ascending: "Artan",
    quantityOnHand: "Eldeki",
    reservedQuantity: "Ayrılmış",
    availableQuantity: "Kullanılabilir",
    inventoryUpdatedAt: "Son güncelleme",
    applyInventoryFilters: "Stok filtrelerini uygula",
    inventoryUnavailable: "Stok verisi yüklenemedi.",
    noMatchingInventory: "Seçili filtrelerle eşleşen stok kaydı bulunamadı.",
    inventoryPageStatus: "Sayfa {page} / {total} · {count} stok kaydı",
    viewStockMovements: "Hareketleri görüntüle",
    stockMovements: "Stok hareketleri",
    stockMovementsDescription: "{productId} ürünü için değiştirilemez stok hareketleri.",
    stockMovementsUnavailable: "Stok hareketleri yüklenemedi.",
    noStockMovements: "Bu ürün için stok hareketi bulunmuyor.",
    movementType: "Tür",
    movementQuantity: "Miktar",
    movementOnHand: "Eldeki önce → sonra",
    movementReserved: "Ayrılmış önce → sonra",
    movementOrder: "Sipariş",
    movementOccurredAt: "Gerçekleşme",
    movementPageStatus: "Sayfa {page} / {total} · {count} hareket",
    close: "Kapat",
    managePaymentsDescription: "Müşteri ve siparişler arasındaki güvenli ödeme özetlerini sınırlı sunucu filtreleriyle inceleyin.",
    paymentList: "Ödeme operasyonları",
    paymentCustomerId: "Müşteri kimliği",
    paymentOrderId: "Sipariş kimliği",
    guidFilterPlaceholder: "GUID değeri veya boş bırakın",
    invalidOrderId: "Geçerli bir sipariş GUID değeri girin.",
    createdFrom: "Başlangıç tarihi",
    createdTo: "Bitiş tarihi",
    invalidDateRange: "Bitiş tarihi başlangıç tarihinden önce olamaz.",
    paymentStatus: "Ödeme durumu",
    allPaymentStatuses: "Tüm durumlar",
    paymentSort: "Sıralama alanı",
    paymentAmount: "Tutar",
    paymentId: "Ödeme kimliği",
    paymentCreatedAt: "Oluşturulma",
    paymentUpdatedAt: "Güncellenme",
    applyPaymentFilters: "Ödeme filtrelerini uygula",
    paymentsUnavailable: "Ödeme verisi yüklenemedi.",
    noMatchingPayments: "Seçili filtrelerle eşleşen ödeme bulunamadı.",
    paymentPageStatus: "Sayfa {page} / {total} · {count} ödeme",
    manageShipmentsDescription: "Müşteri ve siparişler arasındaki güvenli gönderi özetlerini sınırlı sunucu filtreleriyle inceleyin.",
    shipmentList: "Gönderi operasyonları",
    shipmentStatus: "Gönderi durumu",
    allShipmentStatuses: "Tüm durumlar",
    shipmentSort: "Sıralama alanı",
    shipmentId: "Gönderi kimliği",
    trackingNumber: "Takip numarası",
    shipmentCreated: "Oluşturuldu",
    shipmentFailed: "Başarısız",
    shipmentInTransit: "Yolda",
    shipmentDelivered: "Teslim edildi",
    notAvailable: "Bulunmuyor",
    applyShipmentFilters: "Gönderi filtrelerini uygula",
    shipmentsUnavailable: "Gönderi verisi yüklenemedi.",
    noMatchingShipments: "Seçili filtrelerle eşleşen gönderi bulunamadı.",
    shipmentPageStatus: "Sayfa {page} / {total} · {count} gönderi",
    manageStoresDescription: "Tüm mağazaları sahipleri genelinde sunucu sıralaması ve sayfalama ile arayıp inceleyin.",
    storeList: "Pazaryeri mağazaları",
    storeSearch: "Ara",
    storeSearchPlaceholder: "Mağaza adı veya adresi",
    storeOwnerId: "Sahip kullanıcı kimliği",
    invalidOwnerId: "Geçerli bir sahip GUID değeri girin.",
    storeSort: "Sıralama alanı",
    storeName: "Mağaza adı",
    storeSlug: "Mağaza URL adı",
    applyStoreFilters: "Mağaza filtrelerini uygula",
    storesUnavailable: "Mağaza verisi yüklenemedi.",
    noMatchingStores: "Seçili filtrelerle eşleşen mağaza bulunamadı.",
    storePageStatus: "Sayfa {page} / {total} · {count} mağaza",
    manageWorkflowsDescription: "Sipariş saga durumlarını ve gecikmiş işlem son tarihlerini sınırlı sorgularla inceleyin.",
    workflowList: "Sipariş iş akışı tanıları",
    workflowOrderId: "Sipariş kimliği",
    workflowStatus: "İş akışı durumu",
    allWorkflowStatuses: "Tüm durumlar",
    workflowOverdueOnly: "Yalnız gecikenler",
    overdueWorkflowsTitle: "Sipariş iş akışları dikkat istiyor",
    overdueWorkflowsBody: "{count} sipariş iş akışı adım süresini aştı.",
    reviewOverdueWorkflows: "İş akışlarını incele",
    metricsRetry: "Tekrar dene",
    workflowDeadline: "Adım son tarihi",
    workflowTimeoutHandled: "Zaman aşımı işlendi",
    workflowCreatedAt: "Oluşturulma",
    workflowUpdatedAt: "Güncellenme",
    workflowsUnavailable: "İş akışı tanıları yüklenemedi.",
    noMatchingWorkflows: "Seçili filtrelerle eşleşen iş akışı bulunamadı.",
    workflowPageStatus: "Sayfa {page} / {total} · {count} iş akışı",
    applyWorkflowFilters: "İş akışı filtrelerini uygula",
    userRole: {
      Customer: "Müşteri",
      Seller: "Satıcı",
      Admin: "Yönetici",
    },
    status: {
      Active: "Aktif",
      Draft: "Taslak",
      Inactive: "Pasif",
      Archived: "Arşivlenmiş",
    },
  },
  seller: {
    workspace: "Satıcı çalışma alanı",
    navigation: "Satıcı gezinmesi",
    overview: "Genel bakış",
    storePageDescription: "Satış yaptığınız mağazaları oluşturun; adlarını ve adreslerini güncel tutun.",
    productPageDescription: "Bir mağaza seçin; ürün ekleyin, katalog bilgilerini düzenleyin ve stoğu güncelleyin.",
    storeCount: "{count} mağaza",
    manageStoresDescription: "Yeni mağaza oluşturun veya mevcut mağazalarınızı güncelleyin.",
    manageProductsDescription: "Mağazaya ürün ekleyin; bilgilerini ve stoklarını güncel tutun.",
    manageOrdersDescription: "Her mağazaya ait sipariş kalemlerini ve toplamları inceleyin.",
    openAdminWorkspace: "Yönetici paneli",
    roleLabel: "Satıcı",
    welcome: "Tekrar hoş geldiniz, {name}",
    description: "Sahibi olduğunuz mağazaları yönetin ve her mağazaya bağlı ürünleri inceleyin.",
    stores: "Mağazalarınız",
    products: "Mağaza ürünleri",
    createStore: "Mağaza oluştur",
    storeName: "Mağaza adı",
    storeNamePlaceholder: "Örnek Mağaza",
    storeSlug: "Mağaza URL adı",
    storeSlugPlaceholder: "ornek-magaza",
    slugHint: "Küçük harf, rakam ve tek tire kullanın.",
    saveStore: "Mağazayı oluştur",
    savingStore: "Mağaza oluşturuluyor",
    editStore: "Mağazayı düzenle",
    updateStore: "Değişiklikleri kaydet",
    updatingStore: "Değişiklikler kaydediliyor",
    storeUpdateFailed: "Mağaza güncellenemedi. Alanları kontrol edip tekrar deneyin.",
    noStores: "Henüz bir mağazanız yok.",
    noStoresDescription: "Ürün eklemeden önce ilk mağazanızı oluşturun.",
    storeLoadFailed: "Mağazalarınız yüklenemedi.",
    storeCreateFailed: "Mağaza oluşturulamadı. Alanları kontrol edip tekrar deneyin.",
    slugConflict: "Bu mağaza adresi zaten kullanılıyor.",
    selectStore: "Mağaza seçin",
    productCount: "{count} ürün",
    noProducts: "Bu mağazada henüz ürün yok.",
    productLoadFailed: "Bu mağazanın ürünleri yüklenemedi.",
    createProduct: "Ürün ekle",
    sku: "Stok kodu",
    productName: "Ürün adı",
    productDescription: "Açıklama",
    productNameEnglish: "İngilizce ürün adı",
    productDescriptionEnglish: "İngilizce açıklama",
    productNameTurkish: "Türkçe ürün adı",
    productDescriptionTurkish: "Türkçe açıklama",
    category: "Kategori",
    brand: "Marka",
    price: "Fiyat",
    currency: "Para birimi",
    statusLabel: "Durum",
    draft: "Taslak",
    active: "Aktif",
    inactive: "Pasif",
    archived: "Arşivlenmiş",
    saveProduct: "Ürünü oluştur",
    savingProduct: "Ürün oluşturuluyor",
    productCreateFailed: "Ürün oluşturulamadı. Alanları kontrol edip tekrar deneyin.",
    referencesUnavailable: "Kategori ve marka seçenekleri yüklenemedi.",
    referencesEmpty: "Ürün eklemek için aktif bir kategori ve marka gerekir. Bunları yalnızca Yönetici, Kategori ve marka yönetimi bölümünden oluşturabilir.",
    refreshReferences: "Kategori ve markaları yeniden yükle",
    refreshingReferences: "Kategori ve markalar yeniden yükleniyor",
    productCreated: "Ürün başarıyla oluşturuldu.",
    productCreatedAddImages: "Ürün oluşturuldu. Görsellerini aşağıdan ekleyebilirsiniz.",
    edit: "Düzenle",
    editProduct: "Ürünü düzenle",
    cancelEditing: "Ürün düzenleyiciyi kapat",
    saveChanges: "Değişiklikleri kaydet",
    savingChanges: "Değişiklikler kaydediliyor",
    productUpdated: "Ürün başarıyla güncellendi.",
    productUpdateFailed: "Ürün güncellenemedi. Alanları ve mağaza erişiminizi kontrol edin.",
    currentImages: "Mevcut görseller",
    noImages: "Bu ürüne henüz görsel eklenmemiş.",
    uploadImage: "Görsel yükle",
    uploadingImage: "Görsel yükleniyor",
    imageUploadHint: "JPEG, PNG, GIF veya WebP. En fazla 5 MB ve ürün başına 8 görsel.",
    imageInvalid: "En fazla 5 MB boyutunda desteklenen bir görsel seçin.",
    imageUploaded: "Görsel başarıyla yüklendi.",
    imageUploadFailed: "Görsel yüklenemedi. Dosyayı veya medya yapılandırmasını kontrol edin.",
    mainImage: "Ana",
    setMainImage: "Ana görsel yap",
    mainImageUpdated: "Ana görsel güncellendi.",
    imageUpdateFailed: "Görsel güncellenemedi.",
    deleteImage: "Görseli sil",
    imageDeleted: "Görsel silindi.",
    imageDeleteFailed: "Görsel silinemedi.",
    manageStock: "Stok",
    stockTitle: "Stok yönetimi",
    quantityOnHand: "Toplam stok miktarı",
    reservedQuantity: "Rezerve",
    availableQuantity: "Kullanılabilir",
    stockNotCreated: "Henüz stok kaydı yok. Kaydettiğinizde oluşturulacak.",
    saveStock: "Stoku kaydet",
    savingStock: "Stok kaydediliyor",
    stockUpdated: "Stok başarıyla güncellendi.",
    stockLoadFailed: "Stok bilgisi yüklenemedi.",
    stockUpdateFailed: "Stok güncellenemedi. Ürün erişiminizi kontrol edip tekrar deneyin.",
    stockInvalid: "Sıfır veya daha büyük bir tam sayı girin.",
    closeStockEditor: "Stok düzenleyiciyi kapat",
    openOrders: "Mağaza siparişleri",
    orderPageTitle: "Mağaza siparişleri",
    orderPageDescription: "Sahibi olduğunuz her mağazaya ait sipariş kalemlerini ve toplamları inceleyin.",
    orderStore: "Mağaza",
    orderStatus: "Sipariş durumu",
    allOrderStatuses: "Tüm durumlar",
    orderSort: "Sıralama",
    newestOrders: "En yeni önce",
    oldestOrders: "En eski önce",
    orderRowsPerPage: "Sayfa başına sipariş",
    ordersLoadFailed: "Bu mağazanın siparişleri yüklenemedi.",
    noOrders: "Seçili filtrelerle eşleşen mağaza siparişi bulunamadı.",
    orderCount: "{count} sipariş",
    orderNumber: "Sipariş",
    placedAt: "Oluşturulma",
    lastUpdated: "Son güncelleme",
    storeTotal: "Mağaza toplamı",
    orderItems: "Mağaza kalemleri",
    showOrderItems: "{count} kalemi göster",
    hideOrderItems: "Kalemleri gizle",
    orderItemsLoadFailed: "Bu siparişin kalemleri yüklenemedi.",
    orderProduct: "Ürün",
    orderQuantity: "Miktar",
    orderUnitPrice: "Birim fiyat",
    orderLineTotal: "Kalem toplamı",
    orderPageStatus: "Sayfa {page} / {total} · {count} sipariş",
    previousOrderPage: "Önceki sayfa",
    nextOrderPage: "Sonraki sayfa",
  },
  customer: {
    roleLabel: "Müşteri",
    welcome: "Aradığınız ürünü bulun, {name}",
    description: "Pazaryeri satıcılarının aktif ürünlerini keşfedin. Ürün adı veya stok koduyla arayın, kategori ve markaya göre filtreleyin.",
    searchLabel: "Ürün ara",
    searchPlaceholder: "Ürün adı veya stok kodu",
    searchAction: "Ara",
    category: "Kategori",
    allCategories: "Tüm kategoriler",
    brand: "Marka",
    allBrands: "Tüm markalar",
    sort: "Sıralama",
    filters: "Filtreler",
    showFilters: "Filtreleri göster",
    hideFilters: "Filtreleri gizle",
    priceRange: "Fiyat aralığı",
    minPrice: "En az",
    maxPrice: "En çok",
    applyFilters: "Uygula",
    clearFilters: "Tüm filtreleri temizle",
    priceRangeInvalid: "Geçerli bir fiyat aralığı girin.",
    newest: "En yeni",
    priceLowToHigh: "Fiyat: düşükten yükseğe",
    priceHighToLow: "Fiyat: yüksekten düşüğe",
    results: "{count} aktif ürün",
    noProducts: "Bu filtrelerle eşleşen aktif ürün bulunamadı.",
    catalogUnavailable: "Katalog geçici olarak kullanılamıyor.",
    filtersUnavailable: "Kategori ve marka filtreleri geçici olarak kullanılamıyor.",
    previousPage: "Önceki",
    nextPage: "Sonraki",
    page: "Sayfa {current} / {total}",
    viewProduct: "Ürünü incele",
    inStock: "Stokta",
    lowStock: "Son {count} adet",
    outOfStock: "Stokta yok",
    productUnavailable: "Bu ürün kullanılamıyor veya artık aktif değil.",
    backToCatalog: "Kataloğa dön",
    browseTitle: "Pazaryerini keşfedin",
    catalog: "Katalog",
    navigation: "Müşteri gezinmesi",
    productDetails: "Ürün detayları",
    purchase: "Satın alma",
    storeDetails: "Mağaza detayları",
    viewStore: "Mağazayı görüntüle",
    storeUnavailable: "Bu mağaza kullanılamıyor.",
    storeProducts: "Bu mağazanın ürünleri",
    backToStore: "Mağazaya dön",
  },
  basket: {
    title: "Sepetiniz",
    description: "Ödeme adımına geçmeden önce miktarları ve toplamı inceleyin.",
    openBasket: "Sepet",
    addToBasket: "Sepete ekle",
    adding: "Ekleniyor",
    added: "Eklendi",
    addFailed: "Ürün sepetinize eklenemedi.",
    outOfStock: "Bu ürün tükendi; devam etmek için sepetten çıkarın.",
    stockLimited: "Stokta {count} adet kaldı; devam etmek için miktarı düşürün.",
    signInToAdd: "Bu ürünü eklemek için giriş yapın",
    empty: "Sepetiniz boş.",
    continueShopping: "Alışverişe devam et",
    quantity: "Miktar",
    unitPrice: "Birim fiyat",
    total: "Toplam",
    remove: "Kaldır",
    clear: "Sepeti temizle",
    updateFailed: "Sepet güncellenemedi.",
    loadFailed: "Sepetiniz yüklenemedi.",
    temporarilyUnavailable: "Sepet servisine şu anda ulaşılamıyor. Sepetiniz korunuyor, lütfen birazdan yeniden deneyin.",
    checkout: "Sepeti onayla",
    checkingOut: "Sepet onaylanıyor",
    checkoutRecorded: "Sepet onaylandı",
    checkoutNote: "Siparişiniz alındı ve işlenmeye başladı.",
    demoPayment: "Demo ödeme",
    demoPaymentNote: "Gerçek tahsilat yapılmaz ve kart bilgisi istenmez.",
    recipientName: "Alıcı adı",
    addressLine: "Adres",
    city: "Şehir",
    countryCode: "Ülke kodu",
    postalCode: "Posta kodu",
    addressRequired: "Sepetinizi onaylamadan önce teslimat adresini tamamlayın.",
    viewOrder: "Siparişi görüntüle",
  },
  orders: {
    title: "Siparişleriniz",
    openOrders: "Siparişler",
    description: "Sipariş, ödeme ve gönderim durumunu takip edin.",
    empty: "Henüz bir sipariş vermediniz.",
    loadFailed: "Siparişleriniz yüklenemedi.",
    orderNumber: "Sipariş",
    placedAt: "Oluşturulma",
    pageStatus: "Sayfa {page} / {total}",
    previousPage: "Önceki sayfa",
    nextPage: "Sonraki sayfa",
    status: {
      Submitted: "İşleniyor",
      InventoryReserved: "Stok ayrıldı",
      PaymentAuthorized: "Ödeme onaylandı",
      ShipmentCreated: "Gönderi oluşturuldu",
      Confirmed: "Onaylandı",
      Cancelled: "İptal edildi",
      CancellationRequested: "İptal bekleniyor",
    },
    processing: "Sipariş hazırlanıyor",
    processingNote: "Sipariş aktarımı güvenli biçimde kaydedildi. İşlem tamamlandığında bu sayfa güncellenir.",
    backToOrders: "Siparişlere dön",
    shippingAddress: "Teslimat adresi",
    cancellationReason: "İptal nedeni",
    cancelOrder: "Siparişi iptal et",
    cancellingOrder: "İptal isteniyor",
    cancellationRequested: "İptal isteği alındı. Telafi işlemleri tamamlanıyor.",
    cancellationFailed: "İptal isteği tamamlanamadı.",
    orderNotCancellable: "Bu sipariş artık iptal edilemez.",
    cancellationReasons: {
      INSUFFICIENT_STOCK: "Bir veya daha fazla ürün stokta bulunmuyor.",
      PAYMENT_FAILED: "Ödeme onaylanamadı.",
      SHIPMENT_FAILED: "Gönderi oluşturulamadı.",
      UNEXPECTED_ERROR: "Sipariş tamamlanamadı.",
      ORDER_CANCELLED_BY_CUSTOMER: "Bu siparişi siz iptal ettiniz.",
    },
    payment: {
      title: "Ödeme",
      pending: "Ödeme ayrıntıları hazırlanıyor.",
      unavailable: "Ödeme ayrıntılarına geçici olarak ulaşılamıyor.",
      authorization: "Onay",
      refund: "İade",
      status: {
        authorized: "Onaylandı",
        failed: "Başarısız",
        refunded: "İade edildi",
      },
    },
    shipment: {
      title: "Gönderi",
      pending: "Gönderi ayrıntıları hazırlanıyor.",
      unavailable: "Gönderi ayrıntılarına geçici olarak ulaşılamıyor.",
      created: "Oluşturuldu",
      failed: "Başarısız",
      inTransit: "Yolda",
      delivered: "Teslim edildi",
      trackingNumber: "Takip numarası",
      notAvailable: "Bulunmuyor",
      createdAt: "Oluşturulma",
    },
  },
  notifications: {
    title: "Bildirimler",
    openNotifications: "Bildirimler",
    openNotificationsWithUnread: "Bildirimler, {count} okunmamış",
    description: "Sipariş, ödeme ve gönderim güncellemelerini inceleyin.",
    all: "Tümü",
    unread: "Okunmamış",
    unreadLabel: "Okunmamış",
    readLabel: "Okundu",
    empty: "Henüz bir bildiriminiz yok.",
    emptyUnread: "Okunmamış bildiriminiz yok.",
    loadFailed: "Bildirimleriniz yüklenemedi.",
    markRead: "Okundu işaretle",
    markingRead: "İşaretleniyor",
    markAllRead: "Tümünü okundu işaretle",
    markingAllRead: "Tümü işaretleniyor",
    viewOrder: "Siparişi görüntüle",
    previousPage: "Önceki",
    nextPage: "Sonraki",
    pageStatus: "Sayfa {page} / {total}",
    orderTitle: "Sipariş {code}",
    types: {
      orderSubmitted: "Siparişiniz alındı.",
      orderConfirmed: "Siparişiniz onaylandı.",
      orderCancelled: "Siparişiniz iptal edildi.",
      orderCancellationRequested: "İptal talebiniz alındı.",
      orderCancellationRejected: "İptal talebiniz reddedildi; sipariş çoktan ilerlemişti.",
      paymentAuthorized: "Ödemeniz onaylandı.",
      paymentFailed: "Ödeme alınamadı.",
      shipmentCreated: "Gönderi oluşturuldu. Takip numarası: {trackingNumber}",
      shipmentCreatedWithoutTracking: "Gönderi oluşturuldu.",
      shipmentFailed: "Gönderi oluşturulamadı.",
    },
  },
  validation: {
    required: "Bu alan zorunludur.",
    invalidEmail: "Lütfen geçerli bir e-posta adresi girin.",
    passwordTooShort: "Parola en az 8 karakter olmalıdır.",
    displayNameTooShort: "Görünen ad en az 2 karakter olmalıdır.",
    selectRole: "Devam etmek için lütfen bir rol seçin.",
  },
  status: {
    payment: {
      authorized: "Onaylandı",
      failed: "Başarısız",
      refunded: "İade edildi",
    },
    shipment: {
      created: "Oluşturuldu",
      failed: "Başarısız",
      inTransit: "Yolda",
      delivered: "Teslim edildi",
    },
    product: {
      draft: "Taslak",
      active: "Aktif",
      inactive: "Pasif",
      archived: "Arşivlendi",
    },
    user: {
      active: "Aktif",
      disabled: "Devre dışı",
    },
    workflow: {
      submitted: "Oluşturuldu",
      inventoryReserved: "Stok ayrıldı",
      paymentAuthorized: "Ödeme onaylandı",
      shipmentCreated: "Gönderi oluşturuldu",
      completed: "Tamamlandı",
      cancelled: "İptal edildi",
    },
    stockMovement: {
      stockInitialized: "Stok başlatıldı",
      stockIncreased: "Stok artırıldı",
      stockDecreased: "Stok azaltıldı",
      stockReserved: "Stok ayrıldı",
      stockReleased: "Stok serbest bırakıldı",
      auditBaseline: "Denetim referansı",
      stockShipped: "Sevk edildi",
    },
  },
  table: {
    empty: "Kayıt bulunamadı",
    emptyDescription: "Filtreleri değiştirmeyi deneyin veya daha sonra tekrar bakın.",
    loadFailed: "Veri yüklenemedi",
    loadFailedDescription: "İstek başarısız oldu. Lütfen tekrar deneyin.",
    retry: "Tekrar dene",
    previousPage: "Önceki",
    nextPage: "Sonraki",
    pageStatus: "Sayfa {page} / {totalPages}",
    rowsPerPage: "Sayfa başına satır",
    totalItems: "{count} kayıt",
    clearFilters: "Filtreleri temizle",
    filters: "Filtreler",
  },
  errors: {
    generic: "Bir şeyler ters gitti. Lütfen tekrar deneyin.",
    roleUpdateFailed: "Rolünüzü kaydedemedik. Lütfen tekrar deneyin.",
  },
}

export const dictionaries: Record<Locale, Dictionary> = { en, tr }

export function getDictionary(locale: Locale): Dictionary {
  return dictionaries[locale] ?? dictionaries[defaultLocale]
}
