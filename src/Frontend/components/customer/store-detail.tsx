"use client"

import { ArrowLeftIcon, StoreIcon } from "lucide-react"
import Link from "next/link"
import { useParams } from "next/navigation"
import { useEffect, useState } from "react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { CatalogProductCard } from "@/components/customer/catalog-product-card"
import { EmptyState, ErrorState } from "@/components/patterns/states"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { getCatalogStore, getPublicCatalogProducts } from "@/lib/api/catalog"
import { formatNumber } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import type { CatalogProduct, CatalogStore, PagedResult } from "@/types"

const PAGE_SIZE = 12

type State =
  | { status: "loading" }
  | { status: "ready"; store: CatalogStore; products: PagedResult<CatalogProduct> }
  | { status: "unavailable" }

export function StoreDetail() {
  const params = useParams<{ id: string }>()
  const { locale, t } = useI18n()
  const [state, setState] = useState<State>({ status: "loading" })
  const [page, setPage] = useState(1)
  const [reloadToken, setReloadToken] = useState(0)

  useEffect(() => {
    let active = true
    Promise.all([
      getCatalogStore(params.id),
      getPublicCatalogProducts({
        storeId: params.id,
        pageNumber: page,
        pageSize: PAGE_SIZE,
        sortBy: "createdAt",
        sortDescending: true,
      }),
    ])
      .then(([store, products]) => {
        if (active) setState({ status: "ready", store, products })
      })
      .catch(() => {
        if (active) setState({ status: "unavailable" })
      })
    return () => {
      active = false
    }
  }, [locale, page, params.id, reloadToken])

  return (
    <div className="min-h-svh bg-muted/30">
      <header className="flex h-16 items-center justify-between border-b border-border bg-background px-4 sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
          <LanguageSwitcher />
          <ThemeToggle />
        </div>
      </header>

      <main className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <Link
          href="/"
          className="inline-flex items-center gap-2 text-sm font-medium text-primary hover:underline"
        >
          <ArrowLeftIcon className="size-4" />
          {t.customer.backToCatalog}
        </Link>

        {state.status === "loading" ? (
          <div className="mt-8 flex flex-col gap-8">
            <Skeleton className="h-32 rounded-2xl" />
            <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {Array.from({ length: PAGE_SIZE }, (_, index) => (
                <Skeleton key={index} className="h-[22rem] rounded-xl" />
              ))}
            </div>
          </div>
        ) : state.status === "unavailable" ? (
          <ErrorState
            className="mt-8"
            title={t.customer.storeUnavailable}
            onRetry={() => {
              setState({ status: "loading" })
              setReloadToken((token) => token + 1)
            }}
          />
        ) : (
          <>
            <section className="mt-8 flex items-start gap-4 rounded-2xl border border-border bg-card p-6 sm:p-8">
              <span className="flex size-12 shrink-0 items-center justify-center rounded-xl bg-muted">
                <StoreIcon className="size-5 text-muted-foreground" aria-hidden="true" />
              </span>
              <div className="flex min-w-0 flex-col gap-1">
                <span className="text-xs font-medium tracking-wide text-muted-foreground uppercase">
                  {t.customer.storeDetails}
                </span>
                <h1 className="font-heading text-3xl font-semibold tracking-tight">
                  {state.store.name}
                </h1>
                <p className="font-mono text-sm text-muted-foreground">/{state.store.slug}</p>
              </div>
            </section>

            <section className="mt-8 flex flex-col gap-4">
              <div className="flex flex-wrap items-baseline justify-between gap-2">
                <h2 className="font-heading text-xl font-semibold">{t.customer.storeProducts}</h2>
                <p className="text-sm text-muted-foreground">
                  {t.customer.results.replace(
                    "{count}",
                    formatNumber(state.products.totalCount, locale),
                  )}
                </p>
              </div>

              {state.products.items.length === 0 ? (
                <EmptyState title={t.customer.noProducts} description="" />
              ) : (
                <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                  {state.products.items.map((product) => (
                    <CatalogProductCard
                      key={product.id}
                      product={product}
                      locale={locale}
                      viewLabel={t.customer.viewProduct}
                    />
                  ))}
                </div>
              )}

              {state.products.totalPages > 1 ? (
                <nav className="mt-4 flex items-center justify-center gap-3">
                  <Button
                    type="button"
                    variant="outline"
                    disabled={page <= 1}
                    onClick={() => setPage((value) => value - 1)}
                  >
                    {t.customer.previousPage}
                  </Button>
                  <span className="text-sm text-muted-foreground tabular-nums">
                    {t.customer.page
                      .replace("{current}", String(state.products.pageNumber))
                      .replace("{total}", String(state.products.totalPages))}
                  </span>
                  <Button
                    type="button"
                    variant="outline"
                    disabled={page >= state.products.totalPages}
                    onClick={() => setPage((value) => value + 1)}
                  >
                    {t.customer.nextPage}
                  </Button>
                </nav>
              ) : null}
            </section>
          </>
        )}
      </main>
    </div>
  )
}
