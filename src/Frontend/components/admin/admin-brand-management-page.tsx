"use client"

import { useEffect, useState } from "react"
import { Edit3Icon, PlusIcon } from "lucide-react"

import { AdminBrandForm } from "@/components/admin/admin-brand-form"
import { AdminShell } from "@/components/admin/admin-shell"
import { DataTable, type DataTableColumn } from "@/components/patterns/data-table"
import { FilterBar, FilterField } from "@/components/patterns/filter-bar"
import { StatusBadge } from "@/components/patterns/status-badge"
import { Button } from "@/components/ui/button"
import { SelectNative } from "@/components/ui/select-native"
import { getManagedCatalogBrands } from "@/lib/api/catalog"
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value"
import { useI18n } from "@/lib/i18n/provider"
import type { ManagedCatalogBrandReference, PagedResult } from "@/types"

type BrandSortKey = "name" | "slug" | "isActive"
type SortDirection = "ascending" | "descending"
type StatusFilter = "all" | "active" | "inactive"

type BrandState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<ManagedCatalogBrandReference> }
  | { status: "unavailable" }

export function AdminBrandManagementPage() {
  const { t } = useI18n()
  const [brands, setBrands] = useState<BrandState>({ status: "loading" })
  const [search, setSearch] = useState("")
  const [status, setStatus] = useState<StatusFilter>("all")
  const [sortKey, setSortKey] = useState<BrandSortKey>("name")
  const [sortDirection, setSortDirection] = useState<SortDirection>("ascending")
  const [pageSize, setPageSize] = useState(10)
  const [page, setPage] = useState(1)
  const [refreshVersion, setRefreshVersion] = useState(0)
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [editingBrand, setEditingBrand] = useState<ManagedCatalogBrandReference | null>(null)

  const debouncedSearch = useDebouncedValue(search, 300)

  useEffect(() => {
    const controller = new AbortController()
    getManagedCatalogBrands(
      {
        pageNumber: page,
        pageSize,
        search: debouncedSearch,
        isActive: status === "all" ? undefined : status === "active",
        sortBy: sortKey,
        sortDescending: sortDirection === "descending",
      },
      controller.signal,
    )
      .then((data) => {
        setBrands({ status: "ready", data })
      })
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setBrands({ status: "unavailable" })
        }
      })

    return () => controller.abort()
  }, [debouncedSearch, page, pageSize, refreshVersion, sortDirection, sortKey, status])

  function updateSort(nextKey: BrandSortKey) {
    setBrands({ status: "loading" })
    if (nextKey === sortKey) {
      setSortDirection((current) => (current === "ascending" ? "descending" : "ascending"))
    } else {
      setSortKey(nextKey)
      setSortDirection("ascending")
    }
    setPage(1)
  }

  function handleSaved(saved: ManagedCatalogBrandReference) {
    setEditingBrand((current) => (current?.id === saved.id ? saved : current))
    setRefreshVersion((current) => current + 1)
  }

  function clearFilters() {
    setBrands({ status: "loading" })
    setSearch("")
    setStatus("all")
    setPage(1)
  }

  const columns: DataTableColumn<ManagedCatalogBrandReference, BrandSortKey>[] = [
    {
      id: "name",
      header: t.admin.brandName,
      sortKey: "name",
      className: "font-medium",
      cell: (row) => row.name,
    },
    {
      id: "slug",
      header: t.admin.referenceSlug,
      sortKey: "slug",
      cell: (row) => <span className="font-mono text-xs text-muted-foreground">{row.slug}</span>,
    },
    {
      id: "isActive",
      header: t.admin.referenceStatus,
      sortKey: "isActive",
      cell: (row) => (
        <StatusBadge
          label={row.isActive ? t.admin.activeReference : t.admin.inactiveReference}
          tone={row.isActive ? "success" : "neutral"}
        />
      ),
    },
    {
      id: "actions",
      header: t.admin.referenceActions,
      className: "text-right",
      headerClassName: "text-right",
      cell: (row) => (
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => {
            setEditingBrand(row)
            setShowCreateForm(false)
          }}
        >
          <Edit3Icon />
          {t.admin.edit}
        </Button>
      ),
    },
  ]

  return (
    <AdminShell
      section="brands"
      title={t.admin.brands}
      description={t.admin.manageBrandsDescription}
    >
      <section className="mt-6 flex flex-col gap-4" aria-labelledby="brand-list-title">
        <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
          <div>
            <h2 id="brand-list-title" className="font-heading text-xl font-semibold">
              {t.admin.brandList}
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">{t.admin.brandListDescription}</p>
          </div>
          <Button
            type="button"
            onClick={() => {
              setShowCreateForm((current) => !current)
              setEditingBrand(null)
            }}
            aria-expanded={showCreateForm}
          >
            <PlusIcon />
            {t.admin.createBrand}
          </Button>
        </div>

        {showCreateForm ? (
          <div className="rounded-xl border border-border bg-card p-5">
            <h3 className="mb-4 font-heading text-lg font-semibold">{t.admin.newBrand}</h3>
            <AdminBrandForm onSaved={handleSaved} onCancel={() => setShowCreateForm(false)} />
          </div>
        ) : null}

        <FilterBar
          search={{
            value: search,
            onChange: (value) => {
              setSearch(value)
              setPage(1)
            },
            placeholder: t.admin.searchBrandsPlaceholder,
          }}
          onClear={clearFilters}
          hasActiveFilters={search !== "" || status !== "all"}
        >
          <FilterField label={t.admin.referenceStatus}>
            <SelectNative
              value={status}
              onChange={(event) => {
                setBrands({ status: "loading" })
                setStatus(event.target.value as StatusFilter)
                setPage(1)
              }}
            >
              <option value="all">{t.admin.allReferenceStatuses}</option>
              <option value="active">{t.admin.activeReference}</option>
              <option value="inactive">{t.admin.inactiveReference}</option>
            </SelectNative>
          </FilterField>

          <FilterField label={t.table.rowsPerPage} className="min-w-24">
            <SelectNative
              value={pageSize}
              onChange={(event) => {
                setBrands({ status: "loading" })
                setPageSize(Number(event.target.value))
                setPage(1)
              }}
            >
              {[10, 25, 50].map((value) => (
                <option key={value} value={value}>
                  {value}
                </option>
              ))}
            </SelectNative>
          </FilterField>
        </FilterBar>

        <DataTable
          columns={columns}
          page={brands.status === "ready" ? brands.data : null}
          rowKey={(row) => row.id}
          isLoading={brands.status === "loading"}
          error={brands.status === "unavailable"}
          onRetry={() => {
            setBrands({ status: "loading" })
            setRefreshVersion((current) => current + 1)
          }}
          onPageChange={(value) => {
            setBrands({ status: "loading" })
            setPage(value)
          }}
          sort={{ key: sortKey, direction: sortDirection, onChange: updateSort }}
          emptyTitle={t.admin.noMatchingBrands}
        />

        {editingBrand ? (
          <div className="rounded-xl border border-border bg-card p-5">
            <h3 className="mb-4 font-heading text-lg font-semibold">{t.admin.editBrand}</h3>
            <AdminBrandForm
              key={editingBrand.id}
              brand={editingBrand}
              onSaved={handleSaved}
              onCancel={() => setEditingBrand(null)}
            />
          </div>
        ) : null}
      </section>
    </AdminShell>
  )
}
