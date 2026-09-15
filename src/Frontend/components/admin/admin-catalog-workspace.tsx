"use client"

import { type FormEvent, useEffect, useState } from "react"
import { PencilIcon, SearchIcon } from "lucide-react"

import { DataTable, type DataTableColumn } from "@/components/patterns/data-table"
import { FilterBar, FilterField } from "@/components/patterns/filter-bar"
import { StatusBadge } from "@/components/patterns/status-badge"
import { ProductEditForm } from "@/components/seller/product-edit-form"
import { Button } from "@/components/ui/button"
import { SelectNative } from "@/components/ui/select-native"
import { getManagedCatalogProducts } from "@/lib/api/catalog"
import { formatMoney } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { productStatusLabel, productStatusTone } from "@/lib/i18n/status"
import type { CatalogProduct, PagedResult, ProductStatus } from "@/types"

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
  const [reloadToken, setReloadToken] = useState(0)
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
  }, [locale, pageNumber, reloadToken, search, statusFilter])

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

  const columns: DataTableColumn<CatalogProduct>[] = [
    {
      id: "product",
      header: t.admin.searchProducts,
      cell: (product) => (
        <div className="flex min-w-0 flex-col gap-0.5">
          <span className="truncate font-medium">{product.name}</span>
          <span className="truncate text-xs text-muted-foreground">
            {product.sku}
            {product.storeId ? ` · ${t.admin.sellerProduct}` : ` · ${t.admin.platformProduct}`}
          </span>
        </div>
      ),
    },
    {
      id: "price",
      header: t.seller.price,
      headerClassName: "text-right",
      className: "text-right font-medium tabular-nums",
      cell: (product) => formatMoney(product.price, product.currency, locale),
    },
    {
      id: "status",
      header: t.admin.filterStatus,
      cell: (product) => (
        <StatusBadge
          label={productStatusLabel(product.status, t.status)}
          tone={productStatusTone(product.status)}
        />
      ),
    },
    {
      id: "actions",
      header: "",
      className: "text-right",
      cell: (product) => (
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => setEditingProduct(product)}
          aria-label={`${t.admin.editProduct}: ${product.name}`}
        >
          <PencilIcon />
          {t.admin.edit}
        </Button>
      ),
    },
  ]

  return (
    <section id="admin-catalog" className="mt-7" aria-labelledby="admin-catalog-title">
      <div>
        <h2 id="admin-catalog-title" className="font-heading text-xl font-semibold text-foreground">
          {t.admin.manageCatalog}
        </h2>
        <p className="mt-1 text-sm text-muted-foreground">{t.admin.manageCatalogDescription}</p>
      </div>

      <form onSubmit={handleSearch}>
        <FilterBar
          search={{
            value: searchInput,
            onChange: setSearchInput,
            placeholder: t.admin.searchProductsPlaceholder,
          }}
        >
          <FilterField label={t.admin.filterStatus}>
            <SelectNative
              value={statusFilter}
              onChange={(event) => handleStatusChange(event.target.value as StatusFilter)}
            >
              <option value="all">{t.admin.allStatuses}</option>
              <option value="0">{t.admin.status.Draft}</option>
              <option value="1">{t.admin.status.Active}</option>
              <option value="2">{t.admin.status.Inactive}</option>
              <option value="3">{t.admin.status.Archived}</option>
            </SelectNative>
          </FilterField>
          <Button type="submit" className="self-end">
            <SearchIcon />
            {t.admin.searchAction}
          </Button>
        </FilterBar>
      </form>

      <div className="mt-6 grid gap-6 xl:grid-cols-[minmax(0,1fr)_22rem]">
        <div className="min-w-0">
          <DataTable
            columns={columns}
            page={catalog.status === "ready" ? catalog.data : null}
            rowKey={(row) => row.id}
            isLoading={catalog.status === "loading"}
            error={catalog.status === "unavailable"}
            onRetry={() => {
              setCatalog({ status: "loading" })
              setReloadToken((token) => token + 1)
            }}
            onPageChange={(value) => {
              setCatalog({ status: "loading" })
              setPageNumber(value)
            }}
            emptyTitle={t.admin.noMatchingProducts}
            minWidthClassName="min-w-[40rem]"
          />
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
            <div className="border border-dashed border-border bg-card p-5 text-sm leading-6 text-muted-foreground">
              {t.admin.selectProductToEdit}
            </div>
          )}
        </aside>
      </div>
    </section>
  )
}
