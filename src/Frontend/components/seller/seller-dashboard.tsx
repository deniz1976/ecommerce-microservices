"use client"

import { useEffect, useState } from "react"
import { Loader2, LogOut, Package, Store } from "lucide-react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { StoreCreateForm } from "@/components/seller/store-create-form"
import { ProductCreateForm } from "@/components/seller/product-create-form"
import { Button } from "@/components/ui/button"
import { getMyCatalogStores, getStoreCatalogProducts } from "@/lib/api/catalog"
import { logoutFromAuth0 } from "@/lib/auth/auth0"
import { useI18n } from "@/lib/i18n/provider"
import type { CatalogProduct, CatalogStore, PagedResult, UserProfile } from "@/types"

interface SellerDashboardProps {
  profile: UserProfile
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

export function SellerDashboard({ profile }: SellerDashboardProps) {
  const { t } = useI18n()
  const [stores, setStores] = useState<StoreState>({ status: "loading" })
  const [selectedStoreId, setSelectedStoreId] = useState<string | null>(null)
  const [products, setProducts] = useState<ProductState>({ status: "idle" })
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
  }, [selectedStoreId])

  function handleStoreCreated(store: CatalogStore) {
    setStores((current) => ({
      status: "ready",
      stores: current.status === "ready" ? [...current.stores, store] : [store],
    }))
    selectStore(store.id)
  }

  function selectStore(storeId: string) {
    setProducts({ status: "loading" })
    setSelectedStoreId(storeId)
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
            <div className="mt-4 overflow-hidden rounded-lg border border-border bg-background">
              {stores.status === "loading" ? (
                <LoadingState label={t.common.loading} />
              ) : stores.status === "unavailable" ? (
                <MessageState message={t.seller.storeLoadFailed} />
              ) : stores.stores.length === 0 ? (
                <MessageState message={`${t.seller.noStores} ${t.seller.noStoresDescription}`} />
              ) : (
                <div className="divide-y divide-border">
                  {stores.stores.map((store) => (
                    <button
                      key={store.id}
                      type="button"
                      onClick={() => selectStore(store.id)}
                      className={`flex w-full items-center justify-between gap-4 px-5 py-4 text-left transition-colors ${selectedStoreId === store.id ? "bg-accent" : "hover:bg-muted/60"}`}
                    >
                      <span>
                        <span className="block text-sm font-medium text-foreground">{store.name}</span>
                        <span className="mt-1 block text-xs text-muted-foreground">/{store.slug}</span>
                      </span>
                      <Store className="size-4 text-muted-foreground" />
                    </button>
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
            <div className="mt-4 overflow-hidden rounded-lg border border-border bg-background">
              {products.status === "loading" ? (
                <LoadingState label={t.common.loading} />
              ) : products.status === "unavailable" ? (
                <MessageState message={t.seller.productLoadFailed} />
              ) : products.status === "ready" && products.products.items.length > 0 ? (
                <div className="divide-y divide-border">
                  {products.products.items.map((product) => (
                    <article key={product.id} className="flex items-center justify-between gap-4 px-5 py-4">
                      <div className="min-w-0">
                        <p className="truncate text-sm font-medium text-foreground">{product.name}</p>
                        <p className="mt-1 truncate text-xs text-muted-foreground">{product.sku}</p>
                      </div>
                      <p className="text-sm font-medium text-foreground">{product.price} {product.currency}</p>
                    </article>
                  ))}
                </div>
              ) : (
                <MessageState message={selectedStoreId ? t.seller.noProducts : t.seller.selectStore} />
              )}
            </div>
          </section>

          <aside className="space-y-6">
            <StoreCreateForm onCreated={handleStoreCreated} />
            {selectedStore ? (
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

function LoadingState({ label }: { label: string }) {
  return (
    <div className="flex min-h-32 items-center justify-center">
      <Loader2 className="size-5 animate-spin text-muted-foreground" />
      <span className="sr-only">{label}</span>
    </div>
  )
}

function MessageState({ message }: { message: string }) {
  return (
    <div className="flex min-h-32 items-center gap-3 px-5 text-sm text-muted-foreground">
      <Package className="size-4 shrink-0" />
      {message}
    </div>
  )
}
