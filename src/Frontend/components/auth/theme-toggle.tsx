"use client"

import { Moon, Sun } from "lucide-react"

import { Button } from "@/components/ui/button"
import { useI18n } from "@/lib/i18n/provider"
import { useTheme } from "@/lib/theme/provider"

export function ThemeToggle() {
  const { theme, toggleTheme } = useTheme()
  const { t } = useI18n()

  return (
    <Button
      type="button"
      variant="outline"
      size="icon"
      onClick={toggleTheme}
      aria-label={t.common.toggleTheme}
      title={theme === "dark" ? t.common.lightMode : t.common.darkMode}
      className="size-9"
    >
      {theme === "dark" ? (
        <Sun className="size-4" />
      ) : (
        <Moon className="size-4" />
      )}
    </Button>
  )
}
