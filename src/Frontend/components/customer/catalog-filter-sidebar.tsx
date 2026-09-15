"use client"

import { useState } from "react"
import type { FormEvent, ReactNode } from "react"
import { SearchIcon } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Skeleton } from "@/components/ui/skeleton"
import type { CatalogProductQuery } from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"
import type { CatalogBrandReference, CatalogCategoryReference } from "@/types"

interface CatalogFilterSidebarProps {
  query: CatalogProductQuery
  searchInput: string
  onSearchInputChange: (value: string) => void
  onSearchSubmit: () => void
  onChange: (change: Partial<CatalogProductQuery>) => void
  onClear: () => void
  references:
    | { status: "loading" }
    | { status: "ready"; categories: CatalogCategoryReference[]; brands: CatalogBrandReference[] }
    | { status: "unavailable" }
}

export function CatalogFilterSidebar({
  query,
  searchInput,
  onSearchInputChange,
  onSearchSubmit,
  onChange,
  onClear,
  references,
}: CatalogFilterSidebarProps) {
  const { t } = useI18n()
  const [minInput, setMinInput] = useState(query.minPrice === undefined ? "" : String(query.minPrice))
  const [maxInput, setMaxInput] = useState(query.maxPrice === undefined ? "" : String(query.maxPrice))

  const minPrice = parsePrice(minInput)
  const maxPrice = parsePrice(maxInput)
  const priceInvalid =
    (minInput.trim() !== "" && minPrice === undefined) ||
    (maxInput.trim() !== "" && maxPrice === undefined) ||
    (minPrice !== undefined && maxPrice !== undefined && minPrice > maxPrice)

  const hasActiveFilters =
    Boolean(query.search) ||
    Boolean(query.categoryId) ||
    Boolean(query.brandId) ||
    query.minPrice !== undefined ||
    query.maxPrice !== undefined

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    onSearchSubmit()
  }

  function applyPrice(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (priceInvalid) return
    onChange({ minPrice, maxPrice })
  }

  function clearAll() {
    setMinInput("")
    setMaxInput("")
    onClear()
  }

  return (
    <div className="flex flex-col divide-y divide-border border-border lg:border-r">
      <form onSubmit={handleSearch} className="flex gap-2 pb-4 lg:pr-4">
        <Input
          type="search"
          value={searchInput}
          aria-label={t.customer.searchLabel}
          placeholder={t.customer.searchPlaceholder}
          onChange={(event) => onSearchInputChange(event.target.value)}
        />
        <Button type="submit" size="icon" aria-label={t.customer.searchAction}>
          <SearchIcon />
        </Button>
      </form>

      <FilterGroup title={t.customer.category}>
        {references.status === "loading" ? (
          <SidebarSkeleton />
        ) : references.status === "unavailable" ? (
          <p className="text-sm text-muted-foreground">{t.customer.filtersUnavailable}</p>
        ) : (
          <FilterOptions
            allLabel={t.customer.allCategories}
            selectedId={query.categoryId}
            options={references.categories}
            onSelect={(value) => onChange({ categoryId: value })}
          />
        )}
      </FilterGroup>

      <FilterGroup title={t.customer.brand}>
        {references.status === "loading" ? (
          <SidebarSkeleton />
        ) : references.status === "unavailable" ? (
          <p className="text-sm text-muted-foreground">{t.customer.filtersUnavailable}</p>
        ) : (
          <FilterOptions
            allLabel={t.customer.allBrands}
            selectedId={query.brandId}
            options={references.brands}
            onSelect={(value) => onChange({ brandId: value })}
          />
        )}
      </FilterGroup>

      <FilterGroup title={t.customer.priceRange}>
        <form onSubmit={applyPrice} className="flex flex-col gap-2">
          <div className="flex items-center gap-2">
            <Input
              type="number"
              inputMode="decimal"
              min="0"
              step="0.01"
              value={minInput}
              aria-label={t.customer.minPrice}
              placeholder={t.customer.minPrice}
              onChange={(event) => setMinInput(event.target.value)}
            />
            <span className="text-sm text-muted-foreground">-</span>
            <Input
              type="number"
              inputMode="decimal"
              min="0"
              step="0.01"
              value={maxInput}
              aria-label={t.customer.maxPrice}
              placeholder={t.customer.maxPrice}
              onChange={(event) => setMaxInput(event.target.value)}
            />
          </div>
          {priceInvalid ? (
            <p className="text-sm text-destructive">{t.customer.priceRangeInvalid}</p>
          ) : null}
          <Button type="submit" variant="outline" size="sm" disabled={priceInvalid}>
            {t.customer.applyFilters}
          </Button>
        </form>
      </FilterGroup>

      {hasActiveFilters ? (
        <div className="py-4 lg:pr-4">
          <Button type="button" variant="ghost" size="sm" onClick={clearAll}>
            {t.customer.clearFilters}
          </Button>
        </div>
      ) : null}
    </div>
  )
}

function FilterGroup({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="flex flex-col gap-2 py-4 lg:pr-4">
      <h2 className="text-xs font-medium tracking-wide text-muted-foreground uppercase">{title}</h2>
      {children}
    </section>
  )
}

function FilterOptions({
  allLabel,
  selectedId,
  options,
  onSelect,
}: {
  allLabel: string
  selectedId?: string
  options: Array<{ id: string; name: string }>
  onSelect: (value: string | undefined) => void
}) {
  return (
    <div className="flex max-h-64 flex-col overflow-y-auto">
      <FilterOption label={allLabel} selected={!selectedId} onSelect={() => onSelect(undefined)} />
      {options.map((option) => (
        <FilterOption
          key={option.id}
          label={option.name}
          selected={selectedId === option.id}
          onSelect={() => onSelect(option.id)}
        />
      ))}
    </div>
  )
}

function FilterOption({
  label,
  selected,
  onSelect,
}: {
  label: string
  selected: boolean
  onSelect: () => void
}) {
  return (
    <button
      type="button"
      aria-pressed={selected}
      onClick={onSelect}
      className={cn(
        "flex h-8 items-center rounded-sm px-2 text-left text-sm transition-colors",
        selected
          ? "bg-accent font-medium text-accent-foreground"
          : "text-muted-foreground hover:bg-muted hover:text-foreground",
      )}
    >
      <span className="truncate">{label}</span>
    </button>
  )
}

function SidebarSkeleton() {
  return (
    <div className="flex flex-col gap-1.5">
      {Array.from({ length: 4 }, (_, index) => (
        <Skeleton key={index} className="h-8 rounded-sm" />
      ))}
    </div>
  )
}

function parsePrice(value: string): number | undefined {
  const trimmed = value.trim()
  if (trimmed === "") return undefined
  const parsed = Number(trimmed)
  return Number.isFinite(parsed) && parsed >= 0 ? parsed : undefined
}
