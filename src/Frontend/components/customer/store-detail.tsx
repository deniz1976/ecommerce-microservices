"use client"

import { ArrowLeft, Loader2, Store } from "lucide-react"
import Link from "next/link"
import { useParams } from "next/navigation"
import { useEffect, useState } from "react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { CatalogProductCard } from "@/components/customer/catalog-product-card"
import { Button } from "@/components/ui/button"
import { getCatalogStore, getPublicCatalogProducts } from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import type { CatalogProduct, CatalogStore, PagedResult } from "@/types"

type State = { status: "loading" } | { status: "ready"; store: CatalogStore; products: PagedResult<CatalogProduct> } | { status: "unavailable" }

export function StoreDetail() {
  const params = useParams<{ id: string }>()
  const { locale, t } = useI18n()
  const [state, setState] = useState<State>({ status: "loading" })
  const [page, setPage] = useState(1)

  useEffect(() => {
    let active = true
    Promise.all([getCatalogStore(params.id), getPublicCatalogProducts({ storeId: params.id, pageNumber: page, pageSize: 12, sortBy: "createdAt", sortDescending: true })])
      .then(([store, products]) => { if (active) setState({ status: "ready", store, products }) })
      .catch(() => { if (active) setState({ status: "unavailable" }) })
    return () => { active = false }
  }, [locale, page, params.id])

  return <div className="min-h-svh bg-muted/30"><header className="flex h-16 items-center justify-between border-b bg-background px-4 sm:px-6"><Logo /><div className="flex items-center gap-2"><LanguageSwitcher /><ThemeToggle /></div></header><main className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 lg:px-8"><Link href="/" className="inline-flex items-center gap-2 text-sm font-medium text-primary hover:underline"><ArrowLeft className="size-4" />{t.customer.backToCatalog}</Link>{state.status === "loading" ? <div className="flex min-h-96 items-center justify-center"><Loader2 className="size-6 animate-spin" /></div> : state.status === "unavailable" ? <div className="mt-8 flex min-h-72 flex-col items-center justify-center rounded-xl border border-dashed"><Store className="size-9 text-muted-foreground" /><p className="mt-3 text-sm text-muted-foreground">{t.customer.storeUnavailable}</p></div> : <><section className="mt-8 rounded-2xl border bg-card p-6 sm:p-8"><p className="text-sm font-medium text-primary">{t.customer.storeDetails}</p><h1 className="mt-2 font-heading text-3xl font-semibold">{state.store.name}</h1><p className="mt-2 text-sm text-muted-foreground">/{state.store.slug}</p></section><section className="mt-8"><h2 className="font-heading text-xl font-semibold">{t.customer.storeProducts}</h2>{state.products.items.length === 0 ? <p className="mt-4 rounded-xl border border-dashed p-8 text-center text-sm text-muted-foreground">{t.customer.noProducts}</p> : <div className="mt-4 grid gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">{state.products.items.map((product) => <CatalogProductCard key={product.id} product={product} locale={locale} viewLabel={t.customer.viewProduct} />)}</div>}{state.products.totalPages > 1 ? <div className="mt-8 flex justify-center gap-3"><Button type="button" variant="outline" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}>{t.customer.previousPage}</Button><Button type="button" variant="outline" disabled={page >= state.products.totalPages} onClick={() => setPage((value) => value + 1)}>{t.customer.nextPage}</Button></div> : null}</section></>}</main></div>
}
