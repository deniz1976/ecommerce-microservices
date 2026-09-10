import type { Locale } from "@/lib/i18n/dictionaries"

const intlLocales: Record<Locale, string> = {
  en: "en-US",
  tr: "tr-TR",
}

export function intlLocale(locale: Locale): string {
  return intlLocales[locale]
}

export function formatDateTime(value: string | null | undefined, locale: Locale): string {
  if (!value) {
    return "-"
  }
  return new Intl.DateTimeFormat(intlLocales[locale], {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value))
}

export function formatDate(value: string | null | undefined, locale: Locale): string {
  if (!value) {
    return "-"
  }
  return new Intl.DateTimeFormat(intlLocales[locale], { dateStyle: "medium" }).format(
    new Date(value),
  )
}

export function formatMoney(amount: number, currency: string, locale: Locale): string {
  return new Intl.NumberFormat(intlLocales[locale], {
    style: "currency",
    currency,
  }).format(amount)
}

export function formatNumber(value: number, locale: Locale): string {
  return new Intl.NumberFormat(intlLocales[locale]).format(value)
}
