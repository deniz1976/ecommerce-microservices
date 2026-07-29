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
    userRole: Record<Role, string>
    status: Record<ProductStatusName, string>
  }
  seller: {
    workspace: string
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
    productCreated: string
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
    productUnavailable: string
    backToCatalog: string
    productDetails: string
  }
  basket: {
    title: string
    description: string
    openBasket: string
    addToBasket: string
    adding: string
    added: string
    addFailed: string
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
    status: Record<
      "Submitted" | "InventoryReserved" | "PaymentAuthorized" | "ShipmentCreated" | "Confirmed" | "Cancelled",
      string
    >
    processing: string
    processingNote: string
    backToOrders: string
    shippingAddress: string
    cancellationReason: string
    cancellationReasons: Record<
      "INSUFFICIENT_STOCK" | "PAYMENT_FAILED" | "SHIPMENT_FAILED" | "UNEXPECTED_ERROR",
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
    viewOrder: string
    previousPage: string
    nextPage: string
    pageStatus: string
  }
  validation: {
    required: string
    invalidEmail: string
    passwordTooShort: string
    displayNameTooShort: string
    selectRole: string
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
    trustTitle: "Built for serious commerce",
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
    roleLabel: "Seller",
    welcome: "Welcome back, {name}",
    description: "Manage stores you own and review the products assigned to each store.",
    stores: "Your stores",
    products: "Store products",
    createStore: "Create a store",
    storeName: "Store name",
    storeNamePlaceholder: "Example Store",
    storeSlug: "Store address",
    storeSlugPlaceholder: "example-store",
    slugHint: "Use lowercase letters, numbers, and single hyphens.",
    saveStore: "Create store",
    savingStore: "Creating store",
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
    referencesEmpty: "At least one active category and brand must exist before a product can be created.",
    productCreated: "Product created successfully.",
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
    productUnavailable: "This product is unavailable or no longer active.",
    backToCatalog: "Back to catalog",
    productDetails: "Product details",
  },
  basket: {
    title: "Your basket",
    description: "Review quantities and totals before continuing to checkout.",
    openBasket: "Basket",
    addToBasket: "Add to basket",
    adding: "Adding",
    added: "Added",
    addFailed: "The product could not be added to your basket.",
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
    status: {
      Submitted: "Processing",
      InventoryReserved: "Stock reserved",
      PaymentAuthorized: "Payment authorized",
      ShipmentCreated: "Shipment created",
      Confirmed: "Confirmed",
      Cancelled: "Cancelled",
    },
    processing: "Order is being prepared",
    processingNote: "The order handoff is durable. This page updates when processing completes.",
    backToOrders: "Back to orders",
    shippingAddress: "Shipping address",
    cancellationReason: "Cancellation reason",
    cancellationReasons: {
      INSUFFICIENT_STOCK: "One or more products are out of stock.",
      PAYMENT_FAILED: "The payment could not be authorized.",
      SHIPMENT_FAILED: "The shipment could not be created.",
      UNEXPECTED_ERROR: "The order could not be completed.",
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
    viewOrder: "View order",
    previousPage: "Previous",
    nextPage: "Next",
    pageStatus: "Page {page} of {total}",
  },
  validation: {
    required: "This field is required.",
    invalidEmail: "Please enter a valid email address.",
    passwordTooShort: "Password must be at least 8 characters.",
    displayNameTooShort: "Display name must be at least 2 characters.",
    selectRole: "Please select a role to continue.",
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
    trustTitle: "Ciddi ticaret için tasarlandı",
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
    workspace: "Calisma alani",
    navigation: "Yonetim gezinmesi",
    overview: "Genel bakis",
    catalog: "Katalog",
    users: "Kullanicilar",
    roleLabel: "Yonetici",
    welcome: "Tekrar hos geldiniz, {name}",
    description: "Katalogu izleyin ve pazaryerini tek bir operasyon alanindan yonetmeye baslayin.",
    catalogSummary: "Katalog ozeti",
    catalogSummaryDescription: "Korumali Katalog metrik API'sinden guncel pazaryeri toplamları.",
    metricsUnavailable: "Katalog metrikleri su anda kullanilamiyor.",
    totalProducts: "Toplam urun",
    activeProducts: "Aktif urunler",
    draftProducts: "Taslak urunler",
    inactiveProducts: "Pasif urunler",
    archivedProducts: "Arsivlenmis urunler",
    totalStores: "Magazalar",
    totalCategories: "Kategoriler",
    totalBrands: "Markalar",
    recentProducts: "Son katalog urunleri",
    recentProductsDescription: "Katalog servisinin dondurdugu en yeni urunler.",
    catalogUnavailable: "Katalog verisi su anda kullanilamiyor. Gateway ve Catalog servisinin calistigini kontrol edin.",
    noProducts: "Katalogda henuz urun olusturulmadi.",
    manageCatalog: "Katalogu yonet",
    manageCatalogDescription: "Tum magazalardaki urunleri arayin, filtreleyin, sayfalayin ve guncelleyin.",
    searchProducts: "Urun ara",
    searchProductsPlaceholder: "Urun adi veya stok kodu",
    filterStatus: "Durum",
    allStatuses: "Tum durumlar",
    searchAction: "Ara",
    noMatchingProducts: "Secili filtrelerle eslesen urun bulunamadi.",
    sellerProduct: "Satici urunu",
    platformProduct: "Platform urunu",
    editProduct: "Urunu duzenle",
    edit: "Duzenle",
    pageStatus: "Sayfa {page} / {total}",
    previousPage: "Onceki sayfa",
    nextPage: "Sonraki sayfa",
    selectProductToEdit: "Katalog bilgilerini, yasam dongusu durumunu veya gorsellerini guncellemek icin bir urun secin.",
    manageUsers: "Kullanicilar",
    manageUsersDescription: "Yerel olarak kayitli pazaryeri kimliklerini arayin ve inceleyin.",
    searchUsers: "Kullanici ara",
    searchUsersPlaceholder: "Ad veya e-posta adresi",
    filterRole: "Rol",
    allRoles: "Tum roller",
    filterUserStatus: "Hesap durumu",
    allUserStatuses: "Tum durumlar",
    userActive: "Aktif",
    userDisabled: "Devre disi",
    usersUnavailable: "Kullanici verisi su anda kullanilamiyor.",
    noMatchingUsers: "Secili filtrelerle eslesen kullanici bulunamadi.",
    onboardingPending: "Rol secimi bekleniyor",
    joinedAt: "Katilma tarihi",
    userPageStatus: "Sayfa {page} / {total} · {count} kullanici",
    usersReadOnlyNote: "Bu ekran salt okunurdur. Roller ve Auth0 hesaplari buradan degistirilemez.",
    userRole: {
      Customer: "Musteri",
      Seller: "Satici",
      Admin: "Yonetici",
    },
    status: {
      Active: "Aktif",
      Draft: "Taslak",
      Inactive: "Pasif",
      Archived: "Arsivlenmis",
    },
  },
  seller: {
    workspace: "Satici calisma alani",
    roleLabel: "Satici",
    welcome: "Tekrar hos geldiniz, {name}",
    description: "Sahibi oldugunuz magazalari yonetin ve her magazaya bagli urunleri inceleyin.",
    stores: "Magazalariniz",
    products: "Magaza urunleri",
    createStore: "Magaza olustur",
    storeName: "Magaza adi",
    storeNamePlaceholder: "Ornek Magaza",
    storeSlug: "Magaza adresi",
    storeSlugPlaceholder: "ornek-magaza",
    slugHint: "Kucuk harf, rakam ve tek tire kullanin.",
    saveStore: "Magazayi olustur",
    savingStore: "Magaza olusturuluyor",
    noStores: "Henuz bir magazaniz yok.",
    noStoresDescription: "Urun eklemeden once ilk magazanizi olusturun.",
    storeLoadFailed: "Magazalariniz yuklenemedi.",
    storeCreateFailed: "Magaza olusturulamadi. Alanlari kontrol edip tekrar deneyin.",
    slugConflict: "Bu magaza adresi zaten kullaniliyor.",
    selectStore: "Magaza secin",
    productCount: "{count} urun",
    noProducts: "Bu magazada henuz urun yok.",
    productLoadFailed: "Bu magazanin urunleri yuklenemedi.",
    createProduct: "Urun ekle",
    sku: "Stok kodu",
    productName: "Urun adi",
    productDescription: "Aciklama",
    category: "Kategori",
    brand: "Marka",
    price: "Fiyat",
    currency: "Para birimi",
    statusLabel: "Durum",
    draft: "Taslak",
    active: "Aktif",
    inactive: "Pasif",
    archived: "Arsivlenmis",
    saveProduct: "Urunu olustur",
    savingProduct: "Urun olusturuluyor",
    productCreateFailed: "Urun olusturulamadi. Alanlari kontrol edip tekrar deneyin.",
    referencesUnavailable: "Kategori ve marka secenekleri yuklenemedi.",
    referencesEmpty: "Urun olusturmadan once en az bir aktif kategori ve marka bulunmalidir.",
    productCreated: "Urun basariyla olusturuldu.",
    edit: "Duzenle",
    editProduct: "Urunu duzenle",
    cancelEditing: "Urun duzenleyiciyi kapat",
    saveChanges: "Degisiklikleri kaydet",
    savingChanges: "Degisiklikler kaydediliyor",
    productUpdated: "Urun basariyla guncellendi.",
    productUpdateFailed: "Urun guncellenemedi. Alanlari ve magaza erisiminizi kontrol edin.",
    currentImages: "Mevcut gorseller",
    noImages: "Bu urune henuz gorsel eklenmemis.",
    uploadImage: "Gorsel yukle",
    uploadingImage: "Gorsel yukleniyor",
    imageUploadHint: "JPEG, PNG, GIF veya WebP. En fazla 5 MB ve urun basina 8 gorsel.",
    imageInvalid: "En fazla 5 MB boyutunda desteklenen bir gorsel secin.",
    imageUploaded: "Gorsel basariyla yuklendi.",
    imageUploadFailed: "Gorsel yuklenemedi. Dosyayi veya medya yapilandirmasini kontrol edin.",
    mainImage: "Ana",
    setMainImage: "Ana gorsel yap",
    mainImageUpdated: "Ana gorsel guncellendi.",
    imageUpdateFailed: "Gorsel guncellenemedi.",
    deleteImage: "Gorseli sil",
    imageDeleted: "Gorsel silindi.",
    imageDeleteFailed: "Gorsel silinemedi.",
  },
  customer: {
    roleLabel: "Musteri",
    welcome: "Aradiginiz urunu bulun, {name}",
    description: "Pazaryeri saticilarinin aktif urunlerini kesfedin. Urun adi veya stok koduyla arayin, kategori ve markaya gore filtreleyin.",
    searchLabel: "Urun ara",
    searchPlaceholder: "Urun adi veya stok kodu",
    searchAction: "Ara",
    category: "Kategori",
    allCategories: "Tum kategoriler",
    brand: "Marka",
    allBrands: "Tum markalar",
    sort: "Siralama",
    newest: "En yeni",
    priceLowToHigh: "Fiyat: dusukten yuksege",
    priceHighToLow: "Fiyat: yuksekten dusuge",
    results: "{count} aktif urun",
    noProducts: "Bu filtrelerle eslesen aktif urun bulunamadi.",
    catalogUnavailable: "Katalog gecici olarak kullanilamiyor.",
    filtersUnavailable: "Kategori ve marka filtreleri gecici olarak kullanilamiyor.",
    previousPage: "Onceki",
    nextPage: "Sonraki",
    page: "Sayfa {current} / {total}",
    viewProduct: "Urunu incele",
    productUnavailable: "Bu urun kullanilamiyor veya artik aktif degil.",
    backToCatalog: "Kataloga don",
    productDetails: "Urun detaylari",
  },
  basket: {
    title: "Sepetiniz",
    description: "Ödeme adımına geçmeden önce miktarları ve toplamı inceleyin.",
    openBasket: "Sepet",
    addToBasket: "Sepete ekle",
    adding: "Ekleniyor",
    added: "Eklendi",
    addFailed: "Ürün sepetinize eklenemedi.",
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
    status: {
      Submitted: "İşleniyor",
      InventoryReserved: "Stok ayrıldı",
      PaymentAuthorized: "Ödeme onaylandı",
      ShipmentCreated: "Gönderi oluşturuldu",
      Confirmed: "Onaylandı",
      Cancelled: "İptal edildi",
    },
    processing: "Sipariş hazırlanıyor",
    processingNote: "Sipariş aktarımı güvenli biçimde kaydedildi. İşlem tamamlandığında bu sayfa güncellenir.",
    backToOrders: "Siparişlere dön",
    shippingAddress: "Teslimat adresi",
    cancellationReason: "İptal nedeni",
    cancellationReasons: {
      INSUFFICIENT_STOCK: "Bir veya daha fazla ürün stokta bulunmuyor.",
      PAYMENT_FAILED: "Ödeme onaylanamadı.",
      SHIPMENT_FAILED: "Gönderi oluşturulamadı.",
      UNEXPECTED_ERROR: "Sipariş tamamlanamadı.",
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
    viewOrder: "Siparişi görüntüle",
    previousPage: "Önceki",
    nextPage: "Sonraki",
    pageStatus: "Sayfa {page} / {total}",
  },
  validation: {
    required: "Bu alan zorunludur.",
    invalidEmail: "Lütfen geçerli bir e-posta adresi girin.",
    passwordTooShort: "Parola en az 8 karakter olmalıdır.",
    displayNameTooShort: "Görünen ad en az 2 karakter olmalıdır.",
    selectRole: "Devam etmek için lütfen bir rol seçin.",
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
