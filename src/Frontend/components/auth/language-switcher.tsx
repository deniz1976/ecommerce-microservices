"use client"

import { locales, type Locale } from "@/lib/i18n/dictionaries"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"

export function LanguageSwitcher() {
  const { locale, setLocale, t } = useI18n()

  return (
    <div
      role="group"
      aria-label={t.common.changeLanguage}
      className="inline-flex h-9 items-center rounded-lg border border-border bg-background p-0.5"
    >
      {locales.map((code: Locale) => {
        const active = code === locale
        return (
          <button
            key={code}
            type="button"
            onClick={() => setLocale(code)}
            aria-pressed={active}
            className={cn(
              "h-8 rounded-md px-2.5 text-xs font-semibold uppercase tracking-wide transition-colors",
              "focus-visible:outline-none focus-visible:ring-3 focus-visible:ring-ring/25",
              active
                ? "bg-primary text-primary-foreground shadow-sm"
                : "text-muted-foreground hover:text-foreground",
            )}
          >
            {code}
          </button>
        )
      })}
    </div>
  )
}
