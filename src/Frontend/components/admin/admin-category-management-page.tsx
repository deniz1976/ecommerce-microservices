"use client"

import { useEffect, useState } from "react"
import { Edit3Icon, PlusIcon } from "lucide-react"

import { AdminCategoryForm } from "@/components/admin/admin-category-form"
import { AdminShell } from "@/components/admin/admin-shell"
import { DataTable, type DataTableColumn } from "@/components/patterns/data-table"
import { FilterBar, FilterField } from "@/components/patterns/filter-bar"
import { StatusBadge } from "@/components/patterns/status-badge"
import { Button } from "@/components/ui/button"
import { SelectNative } from "@/components/ui/select-native"
import { getManagedCatalogCategories } from "@/lib/api/catalog"
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value"
import { useI18n } from "@/lib/i18n/provider"
import type { ManagedCatalogCategoryReference, PagedResult } from "@/types"

type CategorySortKey = "englishName" | "turkishName" | "slug" | "isActive"
type SortDirection = "ascending" | "descending"
type StatusFilter = "all" | "active" | "inactive"

type CategoryState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<ManagedCatalogCategoryReference> }
  | { status: "unavailable" }

export function AdminCategoryManagementPage() {
  const { t } = useI18n()
  const [categories, setCategories] = useState<CategoryState>({ status: "loading" })
  const [search, setSearch] = useState("")
  const [status, setStatus] = useState<StatusFilter>("all")
  const [sortKey, setSortKey] = useState<CategorySortKey>("englishName")
  const [sortDirection, setSortDirection] = useState<SortDirection>("ascending")
  const [pageSize, setPageSize] = useState(10)
  const [page, setPage] = useState(1)
  const [refreshVersion, setRefreshVersion] = useState(0)
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [editingCategory, setEditingCategory] = useState<ManagedCatalogCategoryReference | null>(null)

  const debouncedSearch = useDebouncedValue(search, 300)

  useEffect(() => {
    const controller = new AbortController()
    getManagedCatalogCategories(
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
        setCategories({ status: "ready", data })
      })
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setCategories({ status: "unavailable" })
        }
      })

    return () => controller.abort()
  }, [debouncedSearch, page, pageSize, refreshVersion, sortDirection, sortKey, status])

  function updateSort(nextKey: CategorySortKey) {
    setCategories({ status: "loading" })
    if (nextKey === sortKey) {
      setSortDirection((current) => (current === "ascending" ? "descending" : "ascending"))
    } else {
      setSortKey(nextKey)
      setSortDirection("ascending")
    }
    setPage(1)
  }

  function handleSaved(saved: ManagedCatalogCategoryReference) {
    setEditingCategory((current) => (current?.id === saved.id ? saved : current))
    setRefreshVersion((current) => current + 1)
  }

  function clearFilters() {
    setCategories({ status: "loading" })
    setSearch("")
    setStatus("all")
    setPage(1)
  }

  const columns: DataTableColumn<ManagedCatalogCategoryReference, CategorySortKey>[] = [
    {
      id: "englishName",
      header: t.admin.categoryEnglishName,
      sortKey: "englishName",
      className: "font-medium",
      cell: (row) => row.englishName,
    },
    {
      id: "turkishName",
      header: t.admin.categoryTurkishName,
      sortKey: "turkishName",
      cell: (row) => row.turkishName,
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
            setEditingCategory(row)
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
      section="categories"
      title={t.admin.categories}
      description={t.admin.manageCategoriesDescription}
    >
      <section className="mt-6 flex flex-col gap-4" aria-labelledby="category-list-title">
        <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
          <div>
            <h2 id="category-list-title" className="font-heading text-xl font-semibold">
              {t.admin.categoryList}
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">{t.admin.categoryListDescription}</p>
          </div>
          <Button
            type="button"
            onClick={() => {
              setShowCreateForm((current) => !current)
              setEditingCategory(null)
            }}
            aria-expanded={showCreateForm}
          >
            <PlusIcon />
            {t.admin.createCategory}
          </Button>
        </div>

        {showCreateForm ? (
          <div className="border border-border bg-card p-5">
            <h3 className="mb-4 font-heading text-lg font-semibold">{t.admin.newCategory}</h3>
            <AdminCategoryForm onSaved={handleSaved} onCancel={() => setShowCreateForm(false)} />
          </div>
        ) : null}

        <FilterBar
          search={{
            value: search,
            onChange: (value) => {
              setSearch(value)
              setPage(1)
            },
            placeholder: t.admin.searchCategoriesPlaceholder,
          }}
          onClear={clearFilters}
          hasActiveFilters={search !== "" || status !== "all"}
        >
          <FilterField label={t.admin.referenceStatus}>
            <SelectNative
              value={status}
              onChange={(event) => {
                setCategories({ status: "loading" })
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
                setCategories({ status: "loading" })
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
          page={categories.status === "ready" ? categories.data : null}
          rowKey={(row) => row.id}
          isLoading={categories.status === "loading"}
          error={categories.status === "unavailable"}
          onRetry={() => {
            setCategories({ status: "loading" })
            setRefreshVersion((current) => current + 1)
          }}
          onPageChange={(value) => {
            setCategories({ status: "loading" })
            setPage(value)
          }}
          sort={{ key: sortKey, direction: sortDirection, onChange: updateSort }}
          emptyTitle={t.admin.noMatchingCategories}
        />

        {editingCategory ? (
          <div className="border border-border bg-card p-5">
            <h3 className="mb-4 font-heading text-lg font-semibold">{t.admin.editCategory}</h3>
            <AdminCategoryForm
              key={editingCategory.id}
              category={editingCategory}
              onSaved={handleSaved}
              onCancel={() => setEditingCategory(null)}
            />
          </div>
        ) : null}
      </section>
    </AdminShell>
  )
}
