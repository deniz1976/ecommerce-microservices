"use client"

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react"

import {
  defaultLocale,
  getDictionary,
  locales,
  type Dictionary,
  type Locale,
} from "@/lib/i18n/dictionaries"

const STORAGE_KEY = "app.locale"

interface I18nContextValue {
  locale: Locale
  setLocale: (locale: Locale) => void
  t: Dictionary
}

const I18nContext = createContext<I18nContextValue | null>(null)

function isLocale(value: string | null): value is Locale {
  return value !== null && (locales as readonly string[]).includes(value)
}

function getPreferredLocale(): Locale {
  const stored = localStorage.getItem(STORAGE_KEY)
  if (isLocale(stored)) {
    return stored
  }

  const browser = navigator.language.slice(0, 2).toLowerCase()
  if (isLocale(browser)) {
    return browser
  }

  return defaultLocale
}

export function I18nProvider({ children }: { children: React.ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(defaultLocale)

  useEffect(() => {
    const preferredLocale = getPreferredLocale()
    queueMicrotask(() => setLocaleState(preferredLocale))
  }, [])

  useEffect(() => {
    document.documentElement.lang = locale
  }, [locale])

  const setLocale = useCallback((next: Locale) => {
    setLocaleState(next)
    localStorage.setItem(STORAGE_KEY, next)
  }, [])

  const value = useMemo<I18nContextValue>(
    () => ({ locale, setLocale, t: getDictionary(locale) }),
    [locale, setLocale],
  )

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>
}

export function useI18n(): I18nContextValue {
  const context = useContext(I18nContext)
  if (!context) {
    throw new Error("useI18n must be used within an I18nProvider")
  }
  return context
}
