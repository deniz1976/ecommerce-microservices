"use client"

import { useEffect, useState } from "react"
import { Boxes, ClipboardList, Loader2, LogOut, Pencil, ShieldCheck, Store } from "lucide-react"
import Link from "next/link"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { StoreCreateForm } from "@/components/seller/store-create-form"
import { StoreEditForm } from "@/components/seller/store-edit-form"
import { ProductCreateForm } from "@/components/seller/product-create-form"
import { ProductEditForm } from "@/components/seller/product-edit-form"
import { ProductInventoryForm } from "@/components/seller/product-inventory-form"
import { EmptyState, ErrorState } from "@/components/patterns/states"
import { StatusBadge } from "@/components/patterns/status-badge"
import { Button, buttonVariants } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { getMyCatalogStores, getStoreCatalogProducts } from "@/lib/api/catalog"
import { logoutFromAuth0 } from "@/lib/auth/auth0"
import { formatMoney } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { productStatusLabel, productStatusTone } from "@/lib/i18n/status"
import { cn } from "@/lib/utils"
import type { CatalogProduct, CatalogStore, PagedResult, UserProfile } from "@/types"

interface SellerDashboardProps {
  profile: UserProfile
  onOpenAdminWorkspace?: () => void
}

type StoreState =
  | { status: "loading" }
  | { status: "ready"; stores: CatalogStore[] }
  | { status: "unavailable" }

type ProductState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "ready"; products: PagedResult<CatalogProduct> }
  | { status: "unavailable" }

export function SellerDashboard({ profile, onOpenAdminWorkspace }: SellerDashboardProps) {
  const { locale, t } = useI18n()
  const [stores, setStores] = useState<StoreState>({ status: "loading" })
  const [selectedStoreId, setSelectedStoreId] = useState<string | null>(null)
  const [products, setProducts] = useState<ProductState>({ status: "idle" })
  const [editingStore, setEditingStore] = useState<CatalogStore | null>(null)
  const [editingProduct, setEditingProduct] = useState<CatalogProduct | null>(null)
  const [inventoryProduct, setInventoryProduct] = useState<CatalogProduct | null>(null)
  const [signingOut, setSigningOut] = useState(false)

  useEffect(() => {
    let active = true
    getMyCatalogStores()
      .then((data) => {
        if (!active) return
        setStores({ status: "ready", stores: data })
        const firstStoreId = data[0]?.id ?? null
        if (firstStoreId) {
          setProducts({ status: "loading" })
          setSelectedStoreId(firstStoreId)
        }
      })
      .catch(() => {
        if (active) setStores({ status: "unavailable" })
      })

    return () => {
      active = false
    }
  }, [])

  useEffect(() => {
    if (!selectedStoreId) {
      return
    }

    let active = true
    getStoreCatalogProducts(selectedStoreId)
      .then((data) => {
        if (active) setProducts({ status: "ready", products: data })
      })
      .catch(() => {
        if (active) setProducts({ status: "unavailable" })
      })

    return () => {
      active = false
    }
  }, [locale, selectedStoreId])

  function handleStoreCreated(store: CatalogStore) {
    setStores((current) => ({
      status: "ready",
      stores: current.status === "ready" ? [...current.stores, store] : [store],
    }))
    selectStore(store.id)
  }

  function selectStore(storeId: string) {
    setProducts({ status: "loading" })
    setEditingStore(null)
    setEditingProduct(null)
    setInventoryProduct(null)
    setSelectedStoreId(storeId)
  }

  function handleStoreUpdated(store: CatalogStore) {
    setStores((current) => current.status === "ready"
      ? {
          status: "ready",
          stores: current.stores.map((item) => item.id === store.id ? store : item),
        }
      : current)
    setEditingStore(null)
  }

  function handleProductCreated(product: CatalogProduct) {
    setProducts((current) => {
      if (current.status !== "ready") {
        return { status: "ready", products: { items: [product], pageNumber: 1, pageSize: 20, totalCount: 1, totalPages: 1 } }
      }

      return {
        status: "ready",
        products: {
          ...current.products,
          items: [product, ...current.products.items],
          totalCount: current.products.totalCount + 1,
        },
      }
    })
  }

  function handleProductUpdated(product: CatalogProduct) {
    setProducts((current) => {
      if (current.status !== "ready") return current
      return {
        status: "ready",
        products: {
          ...current.products,
          items: current.products.items.map((item) => item.id === product.id ? product : item),
        },
      }
    })
    setEditingProduct(product)
  }

  async function handleSignOut() {
    setSigningOut(true)
    await logoutFromAuth0("/login")
  }

  const selectedStore = stores.status === "ready"
    ? stores.stores.find((store) => store.id === selectedStoreId)
    : undefined

  return (
    <div className="min-h-svh bg-muted/35">
      <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-border bg-background px-4 sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
          <Link
            href="/seller/orders"
            className={cn(buttonVariants({ variant: "outline", size: "sm" }))}
          >
            <ClipboardList />
            <span className="hidden sm:inline">{t.seller.openOrders}</span>
          </Link>
          {onOpenAdminWorkspace ? (
            <Button type="button" variant="outline" size="sm" onClick={onOpenAdminWorkspace}>
              <ShieldCheck />
              <span className="hidden sm:inline">{t.seller.openAdminWorkspace}</span>
            </Button>
          ) : null}
          <LanguageSwitcher />
          <ThemeToggle />
          <Button type="button" variant="outline" size="sm" onClick={handleSignOut} disabled={signingOut}>
            {signingOut ? <Loader2 className="animate-spin" /> : <LogOut />}
            <span className="hidden sm:inline">{t.home.signOut}</span>
          </Button>
        </div>
      </header>

      <main className="mx-auto w-full max-w-6xl px-4 py-7 sm:px-6 lg:px-8">
        <div className="flex flex-col justify-between gap-4 border-b border-border pb-6 sm:flex-row sm:items-end">
          <div>
            <div className="flex items-center gap-2 text-sm text-primary">
              <Store className="size-4" />
              <span className="font-medium">{t.seller.roleLabel}</span>
            </div>
            <h1 className="mt-2 font-heading text-2xl font-semibold text-foreground sm:text-3xl">
              {t.seller.welcome.replace("{name}", profile.displayName || profile.email)}
            </h1>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground">{t.seller.description}</p>
          </div>
          <p className="text-sm text-muted-foreground">{profile.email}</p>
        </div>

        <div className="mt-7 grid gap-7 lg:grid-cols-[minmax(0,1fr)_20rem]">
          <section className="min-w-0">
            <h2 className="font-heading text-lg font-semibold text-foreground">{t.seller.stores}</h2>
            <div className="mt-4 overflow-hidden rounded-xl border border-border bg-card">
              {stores.status === "loading" ? (
                <div className="flex flex-col gap-2 p-3">
                  {Array.from({ length: 2 }, (_, index) => (
                    <Skeleton key={index} className="h-14 rounded-lg" />
                  ))}
                </div>
              ) : stores.status === "unavailable" ? (
                <ErrorState title={t.seller.storeLoadFailed} />
              ) : stores.stores.length === 0 ? (
                <EmptyState
                  title={t.seller.noStores}
                  description={t.seller.noStoresDescription}
                />
              ) : (
                <div className="divide-y divide-border">
                  {stores.stores.map((store) => (
                    <div
                      key={store.id}
                      className={`flex items-center gap-2 px-2 transition-colors ${selectedStoreId === store.id ? "bg-accent" : "hover:bg-muted/60"}`}
                    >
                      <button
                        type="button"
                        onClick={() => selectStore(store.id)}
                        className="flex min-w-0 flex-1 items-center justify-between gap-4 px-3 py-4 text-left"
                      >
                        <span className="min-w-0">
                          <span className="block truncate text-sm font-medium text-foreground">{store.name}</span>
                          <span className="mt-1 block truncate text-xs text-muted-foreground">/{store.slug}</span>
                        </span>
                        <Store className="size-4 shrink-0 text-muted-foreground" />
                      </button>
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon-sm"
                        onClick={() => setEditingStore(store)}
                        aria-label={`${t.seller.editStore}: ${store.name}`}
                      >
                        <Pencil />
                      </Button>
                    </div>
                  ))}
                </div>
              )}
            </div>

            <div className="mt-7 flex items-center justify-between gap-3">
              <h2 className="font-heading text-lg font-semibold text-foreground">{t.seller.products}</h2>
              {products.status === "ready" ? (
                <span className="text-sm text-muted-foreground">
                  {t.seller.productCount.replace("{count}", String(products.products.totalCount))}
                </span>
              ) : null}
            </div>
            <div className="mt-4 overflow-hidden rounded-xl border border-border bg-card">
              {products.status === "loading" ? (
                <div className="flex flex-col gap-2 p-3">
                  {Array.from({ length: 3 }, (_, index) => (
                    <Skeleton key={index} className="h-14 rounded-lg" />
                  ))}
                </div>
              ) : products.status === "unavailable" ? (
                <ErrorState title={t.seller.productLoadFailed} />
              ) : products.status === "ready" && products.products.items.length > 0 ? (
                <div className="divide-y divide-border">
                  {products.products.items.map((product) => (
                    <article key={product.id} className="flex items-center justify-between gap-4 px-5 py-4">
                      <div className="min-w-0">
                        <p className="truncate text-sm font-medium text-foreground">{product.name}</p>
                        <p className="mt-1 truncate text-xs text-muted-foreground">{product.sku}</p>
                      </div>
                      <div className="flex shrink-0 items-center gap-3">
                        <StatusBadge
                          label={productStatusLabel(product.status, t.status)}
                          tone={productStatusTone(product.status)}
                        />
                        <p className="text-sm font-medium tabular-nums">
                          {formatMoney(product.price, product.currency, locale)}
                        </p>
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => {
                            setInventoryProduct(null)
                            setEditingProduct(product)
                          }}
                          aria-label={`${t.seller.editProduct}: ${product.name}`}
                        >
                          <Pencil />
                          <span className="hidden sm:inline">{t.seller.edit}</span>
                        </Button>
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => {
                            setEditingProduct(null)
                            setInventoryProduct(product)
                          }}
                          aria-label={`${t.seller.manageStock}: ${product.name}`}
                        >
                          <Boxes />
                          <span className="hidden sm:inline">{t.seller.manageStock}</span>
                        </Button>
                      </div>
                    </article>
                  ))}
                </div>
              ) : (
                <EmptyState
                  title={selectedStoreId ? t.seller.noProducts : t.seller.selectStore}
                  description=""
                />
              )}
            </div>
          </section>

          <aside className="space-y-6">
            {editingStore ? (
              <StoreEditForm
                key={editingStore.id}
                store={editingStore}
                onCancel={() => setEditingStore(null)}
                onUpdated={handleStoreUpdated}
              />
            ) : (
              <StoreCreateForm onCreated={handleStoreCreated} />
            )}
            {inventoryProduct ? (
              <ProductInventoryForm
                key={inventoryProduct.id}
                product={inventoryProduct}
                onCancel={() => setInventoryProduct(null)}
              />
            ) : editingProduct ? (
              <ProductEditForm
                key={editingProduct.id}
                product={editingProduct}
                onCancel={() => setEditingProduct(null)}
                onUpdated={handleProductUpdated}
              />
            ) : selectedStore ? (
              <ProductCreateForm
                store={selectedStore}
                onCreated={handleProductCreated}
              />
            ) : null}
          </aside>
        </div>
      </main>
    </div>
  )
}
