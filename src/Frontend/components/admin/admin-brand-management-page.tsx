"use client"

import { useEffect, useState } from "react"
import { Edit3, FolderSearch, Loader2, Plus } from "lucide-react"

import { AdminBrandForm } from "@/components/admin/admin-brand-form"
import { AdminReferencePageLayout } from "@/components/admin/admin-reference-page-layout"
import {
  ReferenceListToolbar,
  ReferencePagination,
  ReferenceSortButton,
  ReferenceStatusBadge,
  type ReferenceSortDirection,
  type ReferenceStatusFilter,
} from "@/components/admin/admin-reference-list-controls"
import { Button } from "@/components/ui/button"
import { getManagedCatalogBrands } from "@/lib/api/catalog"
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value"
import { useI18n } from "@/lib/i18n/provider"
import type { ManagedCatalogBrandReference, PagedResult } from "@/types"

type BrandSortKey = "name" | "slug" | "isActive"
type BrandState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<ManagedCatalogBrandReference> }
  | { status: "unavailable" }

export function AdminBrandManagementPage() {
  const { t } = useI18n()
  const [brands, setBrands] = useState<BrandState>({ status: "loading" })
  const [search, setSearch] = useState("")
  const [status, setStatus] = useState<ReferenceStatusFilter>("all")
  const [sortKey, setSortKey] = useState<BrandSortKey>("name")
  const [sortDirection, setSortDirection] = useState<ReferenceSortDirection>("ascending")
  const [pageSize, setPageSize] = useState(10)
  const [page, setPage] = useState(1)
  const [refreshVersion, setRefreshVersion] = useState(0)
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [editingBrand, setEditingBrand] = useState<ManagedCatalogBrandReference | null>(null)

  const debouncedSearch = useDebouncedValue(search, 300)

  useEffect(() => {
    const controller = new AbortController()
    getManagedCatalogBrands({
      pageNumber: page,
      pageSize,
      search: debouncedSearch,
      isActive: status === "all" ? undefined : status === "active",
      sortBy: sortKey,
      sortDescending: sortDirection === "descending",
    }, controller.signal)
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
    if (nextKey === sortKey) {
      setSortDirection((current) => current === "ascending" ? "descending" : "ascending")
      setPage(1)
      return
    }
    setSortKey(nextKey)
    setSortDirection("ascending")
  }

  function handleSaved(saved: ManagedCatalogBrandReference) {
    setEditingBrand((current) => current?.id === saved.id ? saved : current)
    setRefreshVersion((current) => current + 1)
  }

  return (
    <AdminReferencePageLayout
      activePage="brands"
      title={t.admin.brands}
      description={t.admin.manageBrandsDescription}
    >
      <section className="mt-6" aria-labelledby="brand-list-title">
        <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
          <div>
            <h2 id="brand-list-title" className="font-heading text-xl font-semibold text-foreground">
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
            <Plus />
            {t.admin.createBrand}
          </Button>
        </div>

        {showCreateForm ? (
          <div className="mt-4 rounded-lg border border-border bg-background p-5">
            <h3 className="mb-4 font-heading text-lg font-semibold text-foreground">
              {t.admin.newBrand}
            </h3>
            <AdminBrandForm
              onSaved={handleSaved}
              onCancel={() => setShowCreateForm(false)}
            />
          </div>
        ) : null}

        <div className="mt-5">
          <ReferenceListToolbar
            search={search}
            onSearchChange={(value) => {
              setSearch(value)
              setPage(1)
            }}
            searchLabel={t.admin.searchBrands}
            searchPlaceholder={t.admin.searchBrandsPlaceholder}
            status={status}
            onStatusChange={(value) => {
              setStatus(value)
              setPage(1)
            }}
            statusLabel={t.admin.referenceStatus}
            allStatusesLabel={t.admin.allReferenceStatuses}
            activeLabel={t.admin.activeReference}
            inactiveLabel={t.admin.inactiveReference}
            pageSize={pageSize}
            onPageSizeChange={(value) => {
              setPageSize(value)
              setPage(1)
            }}
            rowsPerPageLabel={t.admin.rowsPerPage}
          />
        </div>

        <div className="mt-4 overflow-hidden rounded-lg border border-border bg-background">
          {brands.status === "loading" ? (
            <ReferenceMessage loading message={t.common.loading} />
          ) : brands.status === "unavailable" ? (
            <ReferenceMessage message={t.admin.referencesUnavailable} />
          ) : brands.data.items.length === 0 ? (
            <ReferenceMessage message={t.admin.noMatchingBrands} />
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full min-w-[42rem] border-collapse text-left text-sm">
                  <thead className="bg-muted/55 text-xs text-muted-foreground">
                    <tr>
                      <BrandHeader
                        label={t.admin.brandName}
                        column="name"
                        activeColumn={sortKey}
                        direction={sortDirection}
                        onSort={updateSort}
                      />
                      <BrandHeader
                        label={t.admin.referenceSlug}
                        column="slug"
                        activeColumn={sortKey}
                        direction={sortDirection}
                        onSort={updateSort}
                      />
                      <BrandHeader
                        label={t.admin.referenceStatus}
                        column="isActive"
                        activeColumn={sortKey}
                        direction={sortDirection}
                        onSort={updateSort}
                      />
                      <th scope="col" className="px-4 py-3 font-medium">{t.admin.referenceActions}</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border">
                    {brands.data.items.map((brand) => (
                      <tr key={brand.id} className="transition-colors hover:bg-muted/25">
                        <td className="px-4 py-3 font-medium text-foreground">{brand.name}</td>
                        <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{brand.slug}</td>
                        <td className="px-4 py-3">
                          <ReferenceStatusBadge
                            isActive={brand.isActive}
                            activeLabel={t.admin.activeReference}
                            inactiveLabel={t.admin.inactiveReference}
                          />
                        </td>
                        <td className="px-4 py-3">
                          <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            onClick={() => {
                              setEditingBrand(brand)
                              setShowCreateForm(false)
                            }}
                          >
                            <Edit3 />
                            {t.admin.edit}
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <ReferencePagination
                page={brands.data.pageNumber}
                totalPages={Math.max(1, brands.data.totalPages)}
                totalCount={brands.data.totalCount}
                pageLabel={t.admin.referencePageStatus}
                previousLabel={t.admin.previousPage}
                nextLabel={t.admin.nextPage}
                onPageChange={setPage}
              />
            </>
          )}
        </div>

        {editingBrand ? (
          <div className="mt-5 rounded-lg border border-border bg-background p-5">
            <h3 className="mb-4 font-heading text-lg font-semibold text-foreground">
              {t.admin.editBrand}
            </h3>
            <AdminBrandForm
              key={editingBrand.id}
              brand={editingBrand}
              onSaved={handleSaved}
              onCancel={() => setEditingBrand(null)}
            />
          </div>
        ) : null}
      </section>
    </AdminReferencePageLayout>
  )
}

function BrandHeader({
  label,
  column,
  activeColumn,
  direction,
  onSort,
}: {
  label: string
  column: BrandSortKey
  activeColumn: BrandSortKey
  direction: ReferenceSortDirection
  onSort: (column: BrandSortKey) => void
}) {
  const active = column === activeColumn
  return (
    <th
      scope="col"
      className="px-4 py-3 font-medium"
      aria-sort={active ? direction : "none"}
    >
      <ReferenceSortButton
        label={label}
        active={active}
        direction={direction}
        onClick={() => onSort(column)}
      />
    </th>
  )
}

function ReferenceMessage({
  message,
  loading = false,
}: {
  message: string
  loading?: boolean
}) {
  return (
    <div className="flex min-h-44 items-center justify-center gap-3 px-5 text-sm text-muted-foreground">
      {loading ? <Loader2 className="size-5 animate-spin" /> : <FolderSearch className="size-5" />}
      {message}
    </div>
  )
}
