"use client"

import { useEffect, useState } from "react"
import { Boxes, Pencil } from "lucide-react"

import { FilterBar, FilterField } from "@/components/patterns/filter-bar"
import { EmptyState, ErrorState } from "@/components/patterns/states"
import { StatusBadge } from "@/components/patterns/status-badge"
import { ProductCreateForm } from "@/components/seller/product-create-form"
import { ProductEditForm } from "@/components/seller/product-edit-form"
import { ProductInventoryForm } from "@/components/seller/product-inventory-form"
import { SellerShell } from "@/components/seller/seller-shell"
import { Button } from "@/components/ui/button"
import { SelectNative } from "@/components/ui/select-native"
import { Skeleton } from "@/components/ui/skeleton"
import { getMyCatalogStores, getStoreCatalogProducts } from "@/lib/api/catalog"
import { formatMoney } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { productStatusLabel, productStatusTone } from "@/lib/i18n/status"
import type { CatalogProduct, CatalogStore, PagedResult } from "@/types"

type StoreState =
  | { status: "loading" }
  | { status: "ready"; stores: CatalogStore[] }
  | { status: "unavailable" }

type ProductState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "ready"; products: PagedResult<CatalogProduct> }
  | { status: "unavailable" }

export function SellerProductManagementPage() {
  const { locale, t } = useI18n()
  const [stores, setStores] = useState<StoreState>({ status: "loading" })
  const [selectedStoreId, setSelectedStoreId] = useState<string | null>(null)
  const [products, setProducts] = useState<ProductState>({ status: "idle" })
  const [editingProduct, setEditingProduct] = useState<CatalogProduct | null>(null)
  const [createdProductId, setCreatedProductId] = useState<string | null>(null)
  const [inventoryProduct, setInventoryProduct] = useState<CatalogProduct | null>(null)

  useEffect(() => {
    const controller = new AbortController()
    getMyCatalogStores(controller.signal)
      .then((data) => {
        setStores({ status: "ready", stores: data })
        const firstStoreId = data[0]?.id ?? null
        if (firstStoreId) {
          setProducts({ status: "loading" })
          setSelectedStoreId(firstStoreId)
        }
      })
      .catch((error: unknown) => {
        if (!isAbortError(error)) setStores({ status: "unavailable" })
      })

    return () => controller.abort()
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

  function selectStore(storeId: string) {
    setProducts({ status: "loading" })
    setEditingProduct(null)
    setInventoryProduct(null)
    setSelectedStoreId(storeId)
  }

  function handleProductCreated(product: CatalogProduct) {
    setProducts((current) => {
      if (current.status !== "ready") {
        return {
          status: "ready",
          products: { items: [product], pageNumber: 1, pageSize: 20, totalCount: 1, totalPages: 1 },
        }
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
    setInventoryProduct(null)
    setEditingProduct(product)
    setCreatedProductId(product.id)
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

  const selectedStore = stores.status === "ready"
    ? stores.stores.find((store) => store.id === selectedStoreId)
    : undefined

  return (
    <SellerShell
      section="products"
      title={t.seller.products}
      description={t.seller.productPageDescription}
    >
      <div className="mt-5 flex flex-col gap-4">
        <FilterBar>
          <FilterField label={t.seller.orderStore} className="min-w-56 flex-1">
            <SelectNative
              value={selectedStoreId ?? ""}
              disabled={stores.status !== "ready" || stores.stores.length === 0}
              onChange={(event) => selectStore(event.target.value)}
            >
              {stores.status === "ready" && stores.stores.length > 0 ? (
                stores.stores.map((store) => (
                  <option key={store.id} value={store.id}>
                    {store.name}
                  </option>
                ))
              ) : (
                <option value="">{t.seller.selectStore}</option>
              )}
            </SelectNative>
          </FilterField>
        </FilterBar>

        <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_20rem]">
          <section className="min-w-0" aria-labelledby="seller-product-list-title">
            <div className="flex items-center justify-between gap-3">
              <div>
                <h2 id="seller-product-list-title" className="font-heading text-lg font-semibold">
                  {selectedStore?.name ?? t.seller.products}
                </h2>
                {selectedStore ? (
                  <p className="mt-0.5 text-xs text-muted-foreground">/{selectedStore.slug}</p>
                ) : null}
              </div>
              {products.status === "ready" ? (
                <span className="text-sm text-muted-foreground">
                  {t.seller.productCount.replace("{count}", String(products.products.totalCount))}
                </span>
              ) : null}
            </div>

            <div className="mt-3 overflow-hidden rounded-sm border border-border bg-card">
              {stores.status === "loading" || products.status === "loading" ? (
                <div className="flex flex-col gap-2 p-3">
                  {Array.from({ length: 3 }, (_, index) => (
                    <Skeleton key={index} className="h-14 rounded-sm" />
                  ))}
                </div>
              ) : stores.status === "unavailable" ? (
                <ErrorState title={t.seller.storeLoadFailed} />
              ) : stores.status === "ready" && stores.stores.length === 0 ? (
                <EmptyState title={t.seller.noStores} description={t.seller.noStoresDescription} />
              ) : products.status === "unavailable" ? (
                <ErrorState title={t.seller.productLoadFailed} />
              ) : products.status === "ready" && products.products.items.length > 0 ? (
                <div className="divide-y divide-border">
                  {products.products.items.map((product) => (
                    <article
                      key={product.id}
                      className="flex flex-wrap items-center justify-between gap-4 px-5 py-4"
                    >
                      <div className="min-w-0">
                        <p className="truncate text-sm font-medium">{product.name}</p>
                        <p className="mt-0.5 truncate text-xs text-muted-foreground">{product.sku}</p>
                      </div>
                      <div className="flex shrink-0 items-center gap-3">
                        <StatusBadge
                          label={productStatusLabel(product.status, t.status)}
                          tone={productStatusTone(product.status)}
                        />
                        <p className="text-sm font-medium text-primary tabular-nums">
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

          <aside>
            {inventoryProduct ? (
              <ProductInventoryForm
                key={inventoryProduct.id}
                product={inventoryProduct}
                onCancel={() => setInventoryProduct(null)}
              />
            ) : editingProduct ? (
              <div className="flex flex-col gap-3">
                {createdProductId === editingProduct.id ? (
                  <p className="border border-primary/30 bg-primary/5 p-3 text-sm text-foreground" role="status">
                    {t.seller.productCreatedAddImages}
                  </p>
                ) : null}
                <ProductEditForm
                  key={editingProduct.id}
                  product={editingProduct}
                  onCancel={() => setEditingProduct(null)}
                  onUpdated={handleProductUpdated}
                />
              </div>
            ) : selectedStore ? (
              <ProductCreateForm store={selectedStore} onCreated={handleProductCreated} />
            ) : null}
          </aside>
        </div>
      </div>
    </SellerShell>
  )
}

function isAbortError(error: unknown) {
  return error instanceof DOMException && error.name === "AbortError"
}
