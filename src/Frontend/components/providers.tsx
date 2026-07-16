"use client"

import { I18nProvider } from "@/lib/i18n/provider"
import { ThemeProvider } from "@/lib/theme/provider"

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <ThemeProvider>
      <I18nProvider>{children}</I18nProvider>
    </ThemeProvider>
  )
}
