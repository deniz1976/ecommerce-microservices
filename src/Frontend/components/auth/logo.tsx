"use client"

import { ShoppingBag } from "lucide-react"

import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"

interface LogoProps {
  className?: string
  showTagline?: boolean
}

export function Logo({ className, showTagline = false }: LogoProps) {
  const { t } = useI18n()

  return (
    <div className={cn("flex items-center gap-2.5", className)}>
      <span className="flex size-9 items-center justify-center rounded-sm bg-primary text-primary-foreground">
        <ShoppingBag className="size-5" />
      </span>
      <div className="flex flex-col leading-none">
        <span className="font-heading text-base font-semibold tracking-tight text-foreground">
          {t.brand.name}
        </span>
        {showTagline ? (
          <span className="text-xs text-muted-foreground">
            {t.brand.tagline}
          </span>
        ) : null}
      </div>
    </div>
  )
}
