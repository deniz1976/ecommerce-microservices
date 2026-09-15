"use client"

import { useEffect, useState } from "react"
import { Pencil, Store } from "lucide-react"

import { EmptyState, ErrorState } from "@/components/patterns/states"
import { SellerShell } from "@/components/seller/seller-shell"
import { StoreCreateForm } from "@/components/seller/store-create-form"
import { StoreEditForm } from "@/components/seller/store-edit-form"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { getMyCatalogStores } from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import type { CatalogStore } from "@/types"

type StoreState =
  | { status: "loading" }
  | { status: "ready"; stores: CatalogStore[] }
  | { status: "unavailable" }

export function SellerStoreManagementPage() {
  const { t } = useI18n()
  const [stores, setStores] = useState<StoreState>({ status: "loading" })
  const [editingStore, setEditingStore] = useState<CatalogStore | null>(null)

  useEffect(() => {
    const controller = new AbortController()
    getMyCatalogStores(controller.signal)
      .then((data) => setStores({ status: "ready", stores: data }))
      .catch((error: unknown) => {
        if (!isAbortError(error)) setStores({ status: "unavailable" })
      })

    return () => controller.abort()
  }, [])

  function handleStoreCreated(store: CatalogStore) {
    setStores((current) => ({
      status: "ready",
      stores: current.status === "ready" ? [...current.stores, store] : [store],
    }))
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

  return (
    <SellerShell
      section="stores"
      title={t.seller.stores}
      description={t.seller.storePageDescription}
    >
      <div className="mt-5 grid gap-6 lg:grid-cols-[minmax(0,1fr)_20rem]">
        <section className="min-w-0" aria-labelledby="seller-store-list-title">
          <div className="flex items-center justify-between gap-3">
            <h2 id="seller-store-list-title" className="font-heading text-lg font-semibold">
              {t.seller.stores}
            </h2>
            {stores.status === "ready" ? (
              <span className="text-sm text-muted-foreground">
                {t.seller.storeCount.replace("{count}", String(stores.stores.length))}
              </span>
            ) : null}
          </div>

          <div className="mt-3 overflow-hidden rounded-sm border border-border bg-card">
            {stores.status === "loading" ? (
              <div className="flex flex-col gap-2 p-3">
                {Array.from({ length: 3 }, (_, index) => (
                  <Skeleton key={index} className="h-14 rounded-sm" />
                ))}
              </div>
            ) : stores.status === "unavailable" ? (
              <ErrorState title={t.seller.storeLoadFailed} />
            ) : stores.stores.length === 0 ? (
              <EmptyState title={t.seller.noStores} description={t.seller.noStoresDescription} />
            ) : (
              <div className="divide-y divide-border">
                {stores.stores.map((store) => (
                  <div key={store.id} className="flex items-center gap-2 px-5 py-4">
                    <Store className="size-4 shrink-0 text-muted-foreground" />
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium">{store.name}</p>
                      <p className="mt-0.5 truncate text-xs text-muted-foreground">/{store.slug}</p>
                    </div>
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => setEditingStore(store)}
                      aria-label={`${t.seller.editStore}: ${store.name}`}
                    >
                      <Pencil />
                      <span className="hidden sm:inline">{t.seller.edit}</span>
                    </Button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </section>

        <aside>
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
        </aside>
      </div>
    </SellerShell>
  )
}

function isAbortError(error: unknown) {
  return error instanceof DOMException && error.name === "AbortError"
}
