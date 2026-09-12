"use client"

import { useEffect, useState } from "react"
import type { FormEvent } from "react"
import Link from "next/link"
import { Check, ClipboardList, Loader2, LogOut, Search, ShoppingBag, ShoppingCart } from "lucide-react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { CatalogProductCard } from "@/components/customer/catalog-product-card"
import { FilterBar, FilterField } from "@/components/patterns/filter-bar"
import { EmptyState, ErrorState } from "@/components/patterns/states"
import { CustomerNotificationLink } from "@/components/customer/customer-notification-link"
import { Button, buttonVariants } from "@/components/ui/button"
import { SelectNative } from "@/components/ui/select-native"
import { Skeleton } from "@/components/ui/skeleton"
import { incrementBasketItem } from "@/lib/api/basket"
import {
  getCatalogBrands,
  getCatalogCategories,
  getPublicCatalogProducts,
  type CatalogProductQuery,
} from "@/lib/api/catalog"
import { logoutFromAuth0 } from "@/lib/auth/auth0"
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
  profile: UserProfile
}

type CatalogState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<CatalogProduct> }
  | { status: "unavailable" }

type ReferenceState =
  | { status: "loading" }
  | { status: "ready"; categories: CatalogCategoryReference[]; brands: CatalogBrandReference[] }
  | { status: "unavailable" }

const PAGE_SIZE = 12

export function CustomerDashboard({ profile }: CustomerDashboardProps) {
  const { locale, t } = useI18n()
  const [searchInput, setSearchInput] = useState("")
  const [query, setQuery] = useState<CatalogProductQuery>({ pageNumber: 1, pageSize: PAGE_SIZE })
  const [catalog, setCatalog] = useState<CatalogState>({ status: "loading" })
  const [references, setReferences] = useState<ReferenceState>({ status: "loading" })
  const [signingOut, setSigningOut] = useState(false)
  const [addingProductId, setAddingProductId] = useState<string | null>(null)
  const [addedProductId, setAddedProductId] = useState<string | null>(null)
  const [basketError, setBasketError] = useState(false)

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

  function updateQuery(change: Partial<CatalogProductQuery>) {
    setCatalog({ status: "loading" })
    setQuery((current) => ({ ...current, ...change, pageNumber: change.pageNumber ?? 1 }))
  }

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    updateQuery({ search: searchInput })
  }

  function handleSort(value: string) {
    if (value === "price-asc") updateQuery({ sortBy: "price", sortDescending: false })
    else if (value === "price-desc") updateQuery({ sortBy: "price", sortDescending: true })
    else updateQuery({ sortBy: "createdAt", sortDescending: true })
  }

  async function handleSignOut() {
    setSigningOut(true)
    await logoutFromAuth0("/login")
  }

  async function handleAddToBasket(product: CatalogProduct) {
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
    <div className="min-h-svh bg-muted/30">
      <header className="sticky top-0 z-20 flex h-16 items-center justify-between border-b border-border bg-background/95 px-4 backdrop-blur sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
          <LanguageSwitcher />
          <ThemeToggle />
          <CustomerNotificationLink customerId={profile.id} />
          <Link
            href="/orders"
            className={cn(buttonVariants({ variant: "outline", size: "sm" }), "gap-1.5")}
          >
            <ClipboardList />
            <span className="hidden sm:inline">{t.orders.openOrders}</span>
          </Link>
          <Link
            href="/basket"
            className={cn(buttonVariants({ variant: "outline", size: "sm" }), "gap-1.5")}
          >
            <ShoppingCart />
            <span className="hidden sm:inline">{t.basket.openBasket}</span>
          </Link>
          <Button type="button" variant="outline" size="sm" onClick={handleSignOut} disabled={signingOut}>
            {signingOut ? <Loader2 className="animate-spin" /> : <LogOut />}
            <span className="hidden sm:inline">{t.home.signOut}</span>
          </Button>
        </div>
      </header>

      <main>
        <section className="border-b border-border bg-background">
          <div className="mx-auto w-full max-w-7xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8">
            <div className="flex items-center gap-2 text-sm font-medium text-primary">
              <ShoppingBag className="size-4" aria-hidden="true" />
              {t.customer.roleLabel}
            </div>
            <h1 className="mt-3 max-w-3xl font-heading text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
              {t.customer.welcome.replace("{name}", profile.displayName || profile.email)}
            </h1>
            <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
              {t.customer.description}
            </p>
          </div>
        </section>

        <div className="mx-auto w-full max-w-7xl px-4 py-7 sm:px-6 lg:px-8">
          <form onSubmit={handleSearch}>
            <FilterBar
              search={{
                value: searchInput,
                onChange: setSearchInput,
                placeholder: t.customer.searchPlaceholder,
              }}
            >
              <FilterField label={t.customer.category}>
                <SelectNative
                  value={query.categoryId ?? ""}
                  disabled={references.status !== "ready"}
                  onChange={(event) => updateQuery({ categoryId: event.target.value || undefined })}
                >
                  <option value="">{t.customer.allCategories}</option>
                  {references.status === "ready"
                    ? references.categories.map((category) => (
                        <option key={category.id} value={category.id}>
                          {category.name}
                        </option>
                      ))
                    : null}
                </SelectNative>
              </FilterField>

              <FilterField label={t.customer.brand}>
                <SelectNative
                  value={query.brandId ?? ""}
                  disabled={references.status !== "ready"}
                  onChange={(event) => updateQuery({ brandId: event.target.value || undefined })}
                >
                  <option value="">{t.customer.allBrands}</option>
                  {references.status === "ready"
                    ? references.brands.map((brand) => (
                        <option key={brand.id} value={brand.id}>
                          {brand.name}
                        </option>
                      ))
                    : null}
                </SelectNative>
              </FilterField>

              <FilterField label={t.customer.sort}>
                <SelectNative
                  value={sortValue(query)}
                  onChange={(event) => handleSort(event.target.value)}
                >
                  <option value="newest">{t.customer.newest}</option>
                  <option value="price-asc">{t.customer.priceLowToHigh}</option>
                  <option value="price-desc">{t.customer.priceHighToLow}</option>
                </SelectNative>
              </FilterField>

              <Button type="submit" className="self-end">
                <Search />
                {t.customer.searchAction}
              </Button>
            </FilterBar>
          </form>

          {references.status === "unavailable" ? (
            <p className="mt-3 text-sm text-muted-foreground">{t.customer.filtersUnavailable}</p>
          ) : null}

          {basketError ? (
            <p className="mt-3 text-sm text-destructive">{t.basket.addFailed}</p>
          ) : addedProductId ? (
            <p className="mt-3 inline-flex items-center gap-1.5 text-sm text-primary">
              <Check className="size-4" />
              {t.basket.added}
            </p>
          ) : null}

          <div className="mt-8 flex items-center justify-between gap-3">
            <p className="text-sm text-muted-foreground">
              {catalog.status === "ready" ? t.customer.results.replace("{count}", String(catalog.data.totalCount)) : t.common.loading}
            </p>
            <p className="text-sm text-muted-foreground">
              {t.customer.page.replace("{current}", String(page)).replace("{total}", String(totalPages))}
            </p>
          </div>

          {catalog.status === "loading" ? (
            <div className="mt-4 grid gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {Array.from({ length: PAGE_SIZE }, (_, index) => (
                <Skeleton key={index} className="h-[26rem] rounded-xl" />
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
            <div className="mt-4 grid gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {catalog.data.items.map((product) => (
                <CatalogProductCard
                  key={product.id}
                  product={product}
                  locale={locale}
                  viewLabel={t.customer.viewProduct}
                  addLabel={t.basket.addToBasket}
                  addingLabel={t.basket.adding}
                  addedLabel={t.basket.added}
                  adding={addingProductId === product.id}
                  added={addedProductId === product.id}
                  onAdd={handleAddToBasket}
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
      </main>
    </div>
  )
}

function sortValue(query: CatalogProductQuery): string {
  if (query.sortBy === "price") return query.sortDescending ? "price-desc" : "price-asc"
  return "newest"
}
