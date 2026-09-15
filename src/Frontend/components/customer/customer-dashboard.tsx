"use client"

import { useEffect, useState } from "react"
import { Check, SlidersHorizontal, ShoppingBag } from "lucide-react"

import { CatalogFilterSidebar } from "@/components/customer/catalog-filter-sidebar"
import { CatalogProductCard } from "@/components/customer/catalog-product-card"
import { EmptyState, ErrorState } from "@/components/patterns/states"
import { CustomerShell } from "@/components/customer/customer-shell"
import { Button } from "@/components/ui/button"
import { SelectNative } from "@/components/ui/select-native"
import { Skeleton } from "@/components/ui/skeleton"
import { incrementBasketItem } from "@/lib/api/basket"
import { getInventoryItems } from "@/lib/api/inventory"
import {
  getCatalogBrands,
  getCatalogCategories,
  getPublicCatalogProducts,
  type CatalogProductQuery,
} from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"
import type {
  CatalogBrandReference,
  CatalogCategoryReference,
  CatalogProduct,
  PagedResult,
  UserProfile,
} from "@/types"

interface CustomerDashboardProps {
  profile?: UserProfile
}

type CatalogState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<CatalogProduct> }
  | { status: "unavailable" }

type ReferenceState =
  | { status: "loading" }
  | { status: "ready"; categories: CatalogCategoryReference[]; brands: CatalogBrandReference[] }
  | { status: "unavailable" }

const PAGE_SIZE = 24

export function CustomerDashboard({ profile }: CustomerDashboardProps) {
  const { locale, t } = useI18n()
  const [searchInput, setSearchInput] = useState("")
  const [filtersOpen, setFiltersOpen] = useState(false)
  const [query, setQuery] = useState<CatalogProductQuery>({ pageNumber: 1, pageSize: PAGE_SIZE })
  const [catalog, setCatalog] = useState<CatalogState>({ status: "loading" })
  const [references, setReferences] = useState<ReferenceState>({ status: "loading" })
  const [addingProductId, setAddingProductId] = useState<string | null>(null)
  const [addedProductId, setAddedProductId] = useState<string | null>(null)
  const [basketError, setBasketError] = useState(false)
  const [stockByProductId, setStockByProductId] = useState<Record<string, number>>({})

  useEffect(() => {
    let active = true
    Promise.all([getCatalogCategories(locale), getCatalogBrands()])
      .then(([categories, brands]) => {
        if (active) setReferences({ status: "ready", categories, brands })
      })
      .catch(() => {
        if (active) setReferences({ status: "unavailable" })
      })
    return () => {
      active = false
    }
  }, [locale])

  useEffect(() => {
    let active = true
    getPublicCatalogProducts(query)
      .then((data) => {
        if (active) setCatalog({ status: "ready", data })
      })
      .catch(() => {
        if (active) setCatalog({ status: "unavailable" })
      })
    return () => {
      active = false
    }
  }, [locale, query])

  useEffect(() => {
    if (catalog.status !== "ready" || catalog.data.items.length === 0) {
      return
    }

    const controller = new AbortController()
    getInventoryItems(
      catalog.data.items.map((product) => product.id),
      controller.signal,
    )
      .then((items) => {
        setStockByProductId(
          Object.fromEntries(items.map((item) => [item.productId, item.availableQuantity])),
        )
      })
      .catch(() => undefined)

    return () => controller.abort()
  }, [catalog])

  function updateQuery(change: Partial<CatalogProductQuery>) {
    setCatalog({ status: "loading" })
    setQuery((current) => ({ ...current, ...change, pageNumber: change.pageNumber ?? 1 }))
  }

  function clearFilters() {
    setSearchInput("")
    setCatalog({ status: "loading" })
    setQuery({ pageNumber: 1, pageSize: PAGE_SIZE })
  }

  function handleSort(value: string) {
    if (value === "price-asc") updateQuery({ sortBy: "price", sortDescending: false })
    else if (value === "price-desc") updateQuery({ sortBy: "price", sortDescending: true })
    else updateQuery({ sortBy: "createdAt", sortDescending: true })
  }

  async function handleAddToBasket(product: CatalogProduct) {
    if (!profile) {
      return
    }

    setAddingProductId(product.id)
    setAddedProductId(null)
    setBasketError(false)

    try {
      await incrementBasketItem(profile.id, product.id)
      setAddedProductId(product.id)
    } catch {
      setBasketError(true)
    } finally {
      setAddingProductId(null)
    }
  }

  const page = catalog.status === "ready" ? catalog.data.pageNumber : query.pageNumber ?? 1
  const totalPages = catalog.status === "ready" ? Math.max(1, catalog.data.totalPages) : 1

  return (
    <CustomerShell customerId={profile?.id ?? null}>
      <main>
        <section className="border-b border-border bg-background">
          <div className="mx-auto w-full max-w-7xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8">
            <div className="flex items-center gap-2 text-sm font-medium text-primary">
              <ShoppingBag className="size-4" aria-hidden="true" />
              {t.customer.roleLabel}
            </div>
            <h1 className="mt-3 max-w-3xl font-heading text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
              {profile
                ? t.customer.welcome.replace("{name}", profile.displayName || profile.email)
                : t.customer.browseTitle}
            </h1>
            <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
              {t.customer.description}
            </p>
          </div>
        </section>

        <div className="mx-auto grid w-full max-w-7xl gap-6 px-4 py-7 sm:px-6 lg:grid-cols-[15rem_minmax(0,1fr)] lg:px-8">
          <div>
            <Button
              type="button"
              variant="outline"
              size="sm"
              aria-expanded={filtersOpen}
              onClick={() => setFiltersOpen((open) => !open)}
              className="lg:hidden"
            >
              <SlidersHorizontal />
              {filtersOpen ? t.customer.hideFilters : t.customer.showFilters}
            </Button>

            <aside
              aria-label={t.customer.filters}
              className={cn("mt-3 lg:mt-0 lg:block", filtersOpen ? "block" : "hidden")}
            >
              <CatalogFilterSidebar
                query={query}
                searchInput={searchInput}
                onSearchInputChange={setSearchInput}
                onSearchSubmit={() => updateQuery({ search: searchInput })}
                onChange={updateQuery}
                onClear={clearFilters}
                references={references}
              />
            </aside>
          </div>

          <div className="min-w-0">
            {basketError ? (
              <p className="mt-3 text-sm text-destructive">{t.basket.addFailed}</p>
            ) : addedProductId ? (
              <p className="mt-3 inline-flex items-center gap-1.5 text-sm text-primary">
                <Check className="size-4" />
                {t.basket.added}
              </p>
            ) : null}

            <div className="flex flex-wrap items-center justify-between gap-3 border-b border-border pb-3">
              <p className="text-sm text-muted-foreground">
                {catalog.status === "ready" ? t.customer.results.replace("{count}", String(catalog.data.totalCount)) : t.common.loading}
              </p>
              <div className="flex items-center gap-3">
                <p className="text-sm text-muted-foreground">
                  {t.customer.page.replace("{current}", String(page)).replace("{total}", String(totalPages))}
                </p>
                <label className="flex items-center gap-2 text-sm text-muted-foreground">
                  {t.customer.sort}
                  <SelectNative
                    value={sortValue(query)}
                    onChange={(event) => handleSort(event.target.value)}
                    className="w-44"
                  >
                    <option value="newest">{t.customer.newest}</option>
                    <option value="price-asc">{t.customer.priceLowToHigh}</option>
                    <option value="price-desc">{t.customer.priceHighToLow}</option>
                  </SelectNative>
                </label>
              </div>
            </div>

            {catalog.status === "loading" ? (
              <div className="mt-4 grid grid-cols-2 gap-x-3 gap-y-6 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-6">
                {Array.from({ length: 12 }, (_, index) => (
                  <Skeleton key={index} className="aspect-[3/4] rounded-sm" />
                ))}
              </div>
            ) : catalog.status === "unavailable" ? (
              <ErrorState
                className="mt-4"
                title={t.customer.catalogUnavailable}
                onRetry={() => updateQuery({ pageNumber: query.pageNumber ?? 1 })}
              />
            ) : catalog.data.items.length === 0 ? (
              <EmptyState className="mt-4" title={t.customer.noProducts} />
            ) : (
              <div className="mt-4 grid grid-cols-2 gap-x-3 gap-y-6 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-6">
                {catalog.data.items.map((product) => (
                  <CatalogProductCard
                    key={product.id}
                    product={product}
                    locale={locale}
                    viewLabel={t.customer.viewProduct}
                    addLabel={profile ? t.basket.addToBasket : undefined}
                    addingLabel={t.basket.adding}
                    addedLabel={t.basket.added}
                    adding={addingProductId === product.id}
                    added={addedProductId === product.id}
                    onAdd={profile ? handleAddToBasket : undefined}
                    availableQuantity={stockByProductId[product.id]}
                    stockLabels={{
                      inStock: t.customer.inStock,
                      lowStock: t.customer.lowStock,
                      outOfStock: t.customer.outOfStock,
                    }}
                  />
                ))}
              </div>
            )}

            {catalog.status === "ready" && catalog.data.totalPages > 1 ? (
              <nav className="mt-8 flex items-center justify-center gap-3" aria-label={t.customer.page.replace("{current}", String(page)).replace("{total}", String(totalPages))}>
                <Button type="button" variant="outline" onClick={() => updateQuery({ pageNumber: page - 1 })} disabled={page <= 1}>
                  {t.customer.previousPage}
                </Button>
                <Button type="button" variant="outline" onClick={() => updateQuery({ pageNumber: page + 1 })} disabled={page >= totalPages}>
                  {t.customer.nextPage}
                </Button>
              </nav>
            ) : null}
          </div>
        </div>
      </main>
    </CustomerShell>
  )
}

function sortValue(query: CatalogProductQuery): string {
  if (query.sortBy === "price") return query.sortDescending ? "price-desc" : "price-asc"
  return "newest"
}
