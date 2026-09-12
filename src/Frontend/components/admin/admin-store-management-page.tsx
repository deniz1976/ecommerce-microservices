"use client"

import { SearchIcon } from "lucide-react"
import { useEffect, useState } from "react"

import { AdminPageLayout } from "@/components/admin/admin-page-layout"
import { DataTable, type DataTableColumn } from "@/components/patterns/data-table"
import { FilterBar, FilterField } from "@/components/patterns/filter-bar"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { SelectNative } from "@/components/ui/select-native"
import { isOptionalGuid } from "@/lib/admin/filters"
import { getManagedCatalogStores, type ManagedCatalogStoreQuery } from "@/lib/api/catalog"
import { formatDateTime } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import type { ManagedCatalogStore, PagedResult } from "@/types"

type StoreState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<ManagedCatalogStore> }
  | { status: "unavailable" }

interface AppliedFilters {
  search?: string
  ownerUserId?: string
}

export function AdminStoreManagementPage() {
  const { locale, t } = useI18n()
  const [stores, setStores] = useState<StoreState>({ status: "loading" })
  const [searchInput, setSearchInput] = useState("")
  const [ownerInput, setOwnerInput] = useState("")
  const [filters, setFilters] = useState<AppliedFilters>({})
  const [sortBy, setSortBy] = useState<NonNullable<ManagedCatalogStoreQuery["sortBy"]>>("name")
  const [sortDescending, setSortDescending] = useState(false)
  const [pageSize, setPageSize] = useState(20)
  const [page, setPage] = useState(1)
  const [reloadToken, setReloadToken] = useState(0)

  const ownerUserId = ownerInput.trim()
  const ownerValid = isOptionalGuid(ownerUserId)

  useEffect(() => {
    const controller = new AbortController()
    getManagedCatalogStores(
      {
        ...filters,
        pageNumber: page,
        pageSize,
        sortBy,
        sortDescending,
      },
      controller.signal,
    )
      .then((data) => setStores({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setStores({ status: "unavailable" })
        }
      })
    return () => controller.abort()
  }, [filters, page, pageSize, reloadToken, sortBy, sortDescending])

  const updateQuery = (update: () => void) => {
    setStores({ status: "loading" })
    update()
    setPage(1)
  }

  const applyFilters = () => {
    if (!ownerValid) return
    updateQuery(() =>
      setFilters({
        search: searchInput.trim() || undefined,
        ownerUserId: ownerUserId || undefined,
      }),
    )
  }

  const clearFilters = () => {
    setSearchInput("")
    setOwnerInput("")
    updateQuery(() => setFilters({}))
  }

  const columns: DataTableColumn<ManagedCatalogStore>[] = [
    {
      id: "name",
      header: t.admin.storeName,
      className: "font-medium",
      cell: (row) => row.name,
    },
    {
      id: "slug",
      header: t.admin.storeSlug,
      cell: (row) => <span className="font-mono text-xs text-primary">/{row.slug}</span>,
    },
    {
      id: "ownerUserId",
      header: t.admin.storeOwnerId,
      cell: (row) => (
        <span className="font-mono text-xs text-muted-foreground">{row.ownerUserId}</span>
      ),
    },
    {
      id: "createdAt",
      header: t.admin.paymentCreatedAt,
      className: "text-muted-foreground tabular-nums",
      cell: (row) => formatDateTime(row.createdAt, locale),
    },
    {
      id: "updatedAt",
      header: t.admin.paymentUpdatedAt,
      className: "text-muted-foreground tabular-nums",
      cell: (row) => formatDateTime(row.updatedAt, locale),
    },
  ]

  return (
    <AdminPageLayout title={t.admin.stores} description={t.admin.manageStoresDescription}>
      <section className="mt-6 flex flex-col gap-4" aria-labelledby="admin-store-list-title">
        <h2 id="admin-store-list-title" className="font-heading text-xl font-semibold">
          {t.admin.storeList}
        </h2>

        <FilterBar
          onClear={clearFilters}
          hasActiveFilters={filters.search !== undefined || filters.ownerUserId !== undefined}
        >
          <FilterField label={t.admin.storeSearch} className="min-w-56 flex-1">
            <Input
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === "Enter") applyFilters()
              }}
              placeholder={t.admin.storeSearchPlaceholder}
            />
          </FilterField>

          <FilterField label={t.admin.storeOwnerId} className="min-w-64">
            <Input
              value={ownerInput}
              onChange={(event) => setOwnerInput(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === "Enter") applyFilters()
              }}
              placeholder={t.admin.guidFilterPlaceholder}
              aria-invalid={!ownerValid}
              className="font-mono text-xs"
            />
            {!ownerValid ? (
              <span className="text-xs text-destructive">{t.admin.invalidOwnerId}</span>
            ) : null}
          </FilterField>

          <FilterField label={t.admin.storeSort}>
            <SelectNative
              value={sortBy}
              onChange={(event) =>
                updateQuery(() =>
                  setSortBy(event.target.value as NonNullable<ManagedCatalogStoreQuery["sortBy"]>),
                )
              }
            >
              <option value="name">{t.admin.storeName}</option>
              <option value="slug">{t.admin.storeSlug}</option>
              <option value="createdAt">{t.admin.paymentCreatedAt}</option>
              <option value="updatedAt">{t.admin.paymentUpdatedAt}</option>
            </SelectNative>
          </FilterField>

          <FilterField label={t.admin.inventoryDirection}>
            <SelectNative
              value={sortDescending ? "descending" : "ascending"}
              onChange={(event) =>
                updateQuery(() => setSortDescending(event.target.value === "descending"))
              }
            >
              <option value="ascending">{t.admin.ascending}</option>
              <option value="descending">{t.admin.descending}</option>
            </SelectNative>
          </FilterField>

          <FilterField label={t.table.rowsPerPage} className="min-w-24">
            <SelectNative
              value={pageSize}
              onChange={(event) => updateQuery(() => setPageSize(Number(event.target.value)))}
            >
              {[10, 20, 50].map((value) => (
                <option key={value} value={value}>
                  {value}
                </option>
              ))}
            </SelectNative>
          </FilterField>

          <Button
            type="button"
            className="self-end"
            onClick={applyFilters}
            disabled={!ownerValid}
            aria-label={t.admin.applyStoreFilters}
          >
            <SearchIcon />
            {t.admin.applyStoreFilters}
          </Button>
        </FilterBar>

        <DataTable
          columns={columns}
          page={stores.status === "ready" ? stores.data : null}
          rowKey={(row) => row.id}
          isLoading={stores.status === "loading"}
          error={stores.status === "unavailable"}
          onRetry={() => {
            setStores({ status: "loading" })
            setReloadToken((token) => token + 1)
          }}
          onPageChange={(value) => {
            setStores({ status: "loading" })
            setPage(value)
          }}
          emptyTitle={t.admin.noMatchingStores}
          minWidthClassName="min-w-[68rem]"
        />
      </section>
    </AdminPageLayout>
  )
}
