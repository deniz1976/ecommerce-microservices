"use client"

import { type FormEvent, useEffect, useState } from "react"
import {
  ChevronLeft,
  ChevronRight,
  Loader2,
  Package,
  Pencil,
  Search,
} from "lucide-react"

import { ProductEditForm } from "@/components/seller/product-edit-form"
import { Button } from "@/components/ui/button"
import { getManagedCatalogProducts } from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import {
  productStatusNames,
  type CatalogProduct,
  type PagedResult,
  type ProductStatus,
} from "@/types"

type CatalogState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<CatalogProduct> }
  | { status: "unavailable" }

type StatusFilter = "all" | `${ProductStatus}`

const pageSize = 10

export function AdminCatalogWorkspace() {
  const { locale, t } = useI18n()
  const [catalog, setCatalog] = useState<CatalogState>({ status: "loading" })
  const [searchInput, setSearchInput] = useState("")
  const [search, setSearch] = useState("")
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all")
  const [pageNumber, setPageNumber] = useState(1)
  const [editingProduct, setEditingProduct] = useState<CatalogProduct | null>(null)

  useEffect(() => {
    let active = true

    getManagedCatalogProducts({
      pageNumber,
      pageSize,
      search,
      status: statusFilter === "all" ? undefined : Number(statusFilter) as ProductStatus,
      sortBy: "createdAt",
      sortDescending: true,
    })
      .then((data) => {
        if (active) setCatalog({ status: "ready", data })
      })
      .catch(() => {
        if (active) setCatalog({ status: "unavailable" })
      })

    return () => {
      active = false
    }
  }, [pageNumber, search, statusFilter])

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setPageNumber(1)
    setSearch(searchInput.trim())
    setEditingProduct(null)
  }

  function handleStatusChange(value: StatusFilter) {
    setStatusFilter(value)
    setPageNumber(1)
    setEditingProduct(null)
  }

  function handleProductUpdated(product: CatalogProduct) {
    setCatalog((current) => {
      if (current.status !== "ready") return current

      return {
        status: "ready",
        data: {
          ...current.data,
          items: current.data.items.map((item) => item.id === product.id ? product : item),
        },
      }
    })
    setEditingProduct(product)
  }

  return (
    <section id="admin-catalog" className="mt-7" aria-labelledby="admin-catalog-title">
      <div>
        <h2 id="admin-catalog-title" className="font-heading text-xl font-semibold text-foreground">
          {t.admin.manageCatalog}
        </h2>
        <p className="mt-1 text-sm text-muted-foreground">{t.admin.manageCatalogDescription}</p>
      </div>

      <form onSubmit={handleSearch} className="mt-6 grid gap-3 rounded-lg border border-border bg-background p-4 md:grid-cols-[minmax(0,1fr)_12rem_auto]">
        <label className="grid gap-2 text-sm font-medium text-foreground">
          {t.admin.searchProducts}
          <input
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
            placeholder={t.admin.searchProductsPlaceholder}
            className="h-10 rounded-md border border-input bg-background px-3 font-normal"
          />
        </label>
        <label className="grid gap-2 text-sm font-medium text-foreground">
          {t.admin.filterStatus}
          <select
            value={statusFilter}
            onChange={(event) => handleStatusChange(event.target.value as StatusFilter)}
            className="h-10 rounded-md border border-input bg-background px-3 font-normal"
          >
            <option value="all">{t.admin.allStatuses}</option>
            <option value="0">{t.admin.status.Draft}</option>
            <option value="1">{t.admin.status.Active}</option>
            <option value="2">{t.admin.status.Inactive}</option>
            <option value="3">{t.admin.status.Archived}</option>
          </select>
        </label>
        <Button type="submit" className="self-end">
          <Search />
          {t.admin.searchAction}
        </Button>
      </form>

      <div className="mt-6 grid gap-6 xl:grid-cols-[minmax(0,1fr)_22rem]">
        <div className="min-w-0 overflow-hidden rounded-lg border border-border bg-background">
          {catalog.status === "loading" ? (
            <LoadingState label={t.common.loading} />
          ) : catalog.status === "unavailable" ? (
            <MessageState message={t.admin.catalogUnavailable} />
          ) : catalog.data.items.length === 0 ? (
            <MessageState message={t.admin.noMatchingProducts} />
          ) : (
            <>
              <div className="divide-y divide-border">
                {catalog.data.items.map((product) => (
                  <article key={product.id} className="grid gap-3 px-5 py-4 md:grid-cols-[minmax(0,1fr)_auto_auto_auto] md:items-center">
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium text-foreground">{product.name}</p>
                      <p className="mt-1 truncate text-xs text-muted-foreground">
                        {product.sku}{product.storeId ? ` · ${t.admin.sellerProduct}` : ` · ${t.admin.platformProduct}`}
                      </p>
                    </div>
                    <p className="text-sm font-medium text-foreground">
                      {formatPrice(product.price, product.currency, locale)}
                    </p>
                    <StatusBadge product={product} label={t.admin.status[productStatusNames[product.status]]} />
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => setEditingProduct(product)}
                      aria-label={`${t.admin.editProduct}: ${product.name}`}
                    >
                      <Pencil />
                      {t.admin.edit}
                    </Button>
                  </article>
                ))}
              </div>
              <div className="flex items-center justify-between gap-3 border-t border-border px-4 py-3">
                <p className="text-xs text-muted-foreground">
                  {t.admin.pageStatus
                    .replace("{page}", String(catalog.data.pageNumber))
                    .replace("{total}", String(Math.max(catalog.data.totalPages, 1)))}
                </p>
                <div className="flex gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="icon-sm"
                    disabled={catalog.data.pageNumber <= 1}
                    onClick={() => setPageNumber((current) => Math.max(1, current - 1))}
                    aria-label={t.admin.previousPage}
                  >
                    <ChevronLeft />
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    size="icon-sm"
                    disabled={catalog.data.pageNumber >= catalog.data.totalPages}
                    onClick={() => setPageNumber((current) => current + 1)}
                    aria-label={t.admin.nextPage}
                  >
                    <ChevronRight />
                  </Button>
                </div>
              </div>
            </>
          )}
        </div>

        <aside>
          {editingProduct ? (
            <ProductEditForm
              key={editingProduct.id}
              product={editingProduct}
              onCancel={() => setEditingProduct(null)}
              onUpdated={handleProductUpdated}
            />
          ) : (
            <div className="rounded-lg border border-dashed border-border bg-background p-5 text-sm leading-6 text-muted-foreground">
              {t.admin.selectProductToEdit}
            </div>
          )}
        </aside>
      </div>
    </section>
  )
}

function StatusBadge({ product, label }: { product: CatalogProduct; label: string }) {
  const classes = {
    0: "bg-highlight/15 text-highlight-foreground",
    1: "bg-primary/10 text-primary",
    2: "bg-muted text-muted-foreground",
    3: "bg-muted text-muted-foreground",
  } satisfies Record<ProductStatus, string>

  return <span className={`w-fit rounded-md px-2 py-1 text-xs font-medium ${classes[product.status]}`}>{label}</span>
}

function LoadingState({ label }: { label: string }) {
  return (
    <div className="flex min-h-52 items-center justify-center">
      <Loader2 className="size-5 animate-spin text-muted-foreground" />
      <span className="sr-only">{label}</span>
    </div>
  )
}

function MessageState({ message }: { message: string }) {
  return (
    <div className="flex min-h-52 items-center gap-3 px-5 text-sm text-muted-foreground">
      <Package className="size-4 shrink-0" />
      {message}
    </div>
  )
}

function formatPrice(price: number, currency: string, locale: string) {
  return new Intl.NumberFormat(locale, {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(price)
}
