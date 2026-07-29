import {
  defaultLocale,
  locales,
  type Locale,
} from "@/lib/i18n/dictionaries"

export const LOCALE_STORAGE_KEY = "app.locale"

export function isLocale(value: string | null): value is Locale {
  return value !== null && (locales as readonly string[]).includes(value)
}

export function getStoredLocale(): Locale {
  if (typeof window === "undefined") {
    return defaultLocale
  }

  const stored = window.localStorage.getItem(LOCALE_STORAGE_KEY)
  if (isLocale(stored)) {
    return stored
  }

  const browserLocale = window.navigator.language.slice(0, 2).toLowerCase()
  return isLocale(browserLocale) ? browserLocale : defaultLocale
}

export function storeLocale(locale: Locale): void {
  window.localStorage.setItem(LOCALE_STORAGE_KEY, locale)
}
