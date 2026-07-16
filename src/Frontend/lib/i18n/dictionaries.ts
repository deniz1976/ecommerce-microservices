import type { ProductStatusName } from "@/types"

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
    totalProducts: string
    activeProducts: string
    draftProducts: string
    recentProducts: string
    recentProductsDescription: string
    catalogUnavailable: string
    noProducts: string
    status: Record<ProductStatusName, string>
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
    totalProducts: "Total products",
    activeProducts: "Active products on this page",
    draftProducts: "Draft products on this page",
    recentProducts: "Recent catalog products",
    recentProductsDescription: "The latest products returned by the Catalog service.",
    catalogUnavailable: "Catalog data is currently unavailable. Check that the gateway and Catalog service are running.",
    noProducts: "No products have been created in the catalog yet.",
    status: {
      Active: "Active",
      Draft: "Draft",
      Inactive: "Inactive",
      Archived: "Archived",
    },
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
    totalProducts: "Toplam urun",
    activeProducts: "Bu sayfadaki aktif urunler",
    draftProducts: "Bu sayfadaki taslak urunler",
    recentProducts: "Son katalog urunleri",
    recentProductsDescription: "Katalog servisinin dondurdugu en yeni urunler.",
    catalogUnavailable: "Katalog verisi su anda kullanilamiyor. Gateway ve Catalog servisinin calistigini kontrol edin.",
    noProducts: "Katalogda henuz urun olusturulmadi.",
    status: {
      Active: "Aktif",
      Draft: "Taslak",
      Inactive: "Pasif",
      Archived: "Arsivlenmis",
    },
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
