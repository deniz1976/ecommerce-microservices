"use client"

import { SearchIcon, XIcon } from "lucide-react"
import type { ReactNode } from "react"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"

interface FilterBarProps {
  children?: ReactNode
  search?: {
    value: string
    onChange: (value: string) => void
    placeholder: string
  }
  onClear?: () => void
  hasActiveFilters?: boolean
  className?: string
}

/**
 * Shared filter row for every list surface. Individual filters are passed as
 * children so each page keeps ownership of its own query parameters.
 */
export function FilterBar({
  children,
  search,
  onClear,
  hasActiveFilters = false,
  className,
}: FilterBarProps) {
  const { t } = useI18n()

  return (
    <div
      role="search"
      aria-label={t.table.filters}
      className={cn(
        "flex flex-wrap items-end gap-3 rounded-xl border border-border bg-card p-3",
        className,
      )}
    >
      {search ? (
        <div className="relative min-w-56 flex-1">
          <SearchIcon
            className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
            aria-hidden="true"
          />
          <Input
            type="search"
            value={search.value}
            placeholder={search.placeholder}
            aria-label={search.placeholder}
            onChange={(event) => search.onChange(event.target.value)}
            className="pl-8"
          />
        </div>
      ) : null}

      {children}

      {onClear && hasActiveFilters ? (
        <Button variant="ghost" size="sm" onClick={onClear}>
          <XIcon />
          {t.table.clearFilters}
        </Button>
      ) : null}
    </div>
  )
}

interface FilterFieldProps {
  label: string
  children: ReactNode
  className?: string
}

export function FilterField({ label, children, className }: FilterFieldProps) {
  return (
    <label className={cn("grid min-w-40 gap-1.5 text-xs font-medium text-muted-foreground", className)}>
      {label}
      {children}
    </label>
  )
}
