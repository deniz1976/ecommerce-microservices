"use client"

import { useEffect, useState } from "react"
import { Edit3, FolderSearch, Loader2, Plus } from "lucide-react"

import { AdminCategoryForm } from "@/components/admin/admin-category-form"
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
import { getManagedCatalogCategories } from "@/lib/api/catalog"
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value"
import { useI18n } from "@/lib/i18n/provider"
import type { ManagedCatalogCategoryReference, PagedResult } from "@/types"

type CategorySortKey = "englishName" | "turkishName" | "slug" | "isActive"
type CategoryState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<ManagedCatalogCategoryReference> }
  | { status: "unavailable" }

export function AdminCategoryManagementPage() {
  const { t } = useI18n()
  const [categories, setCategories] = useState<CategoryState>({ status: "loading" })
  const [search, setSearch] = useState("")
  const [status, setStatus] = useState<ReferenceStatusFilter>("all")
  const [sortKey, setSortKey] = useState<CategorySortKey>("englishName")
  const [sortDirection, setSortDirection] = useState<ReferenceSortDirection>("ascending")
  const [pageSize, setPageSize] = useState(10)
  const [page, setPage] = useState(1)
  const [refreshVersion, setRefreshVersion] = useState(0)
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [editingCategory, setEditingCategory] = useState<ManagedCatalogCategoryReference | null>(null)

  const debouncedSearch = useDebouncedValue(search, 300)

  useEffect(() => {
    const controller = new AbortController()
    getManagedCatalogCategories({
      pageNumber: page,
      pageSize,
      search: debouncedSearch,
      isActive: status === "all" ? undefined : status === "active",
      sortBy: sortKey,
      sortDescending: sortDirection === "descending",
    }, controller.signal)
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
    if (nextKey === sortKey) {
      setSortDirection((current) => current === "ascending" ? "descending" : "ascending")
      setPage(1)
      return
    }
    setSortKey(nextKey)
    setSortDirection("ascending")
  }

  function handleSaved(saved: ManagedCatalogCategoryReference) {
    setEditingCategory((current) => current?.id === saved.id ? saved : current)
    setRefreshVersion((current) => current + 1)
  }

  return (
    <AdminReferencePageLayout
      activePage="categories"
      title={t.admin.categories}
      description={t.admin.manageCategoriesDescription}
    >
      <section className="mt-6" aria-labelledby="category-list-title">
        <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
          <div>
            <h2 id="category-list-title" className="font-heading text-xl font-semibold text-foreground">
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
            <Plus />
            {t.admin.createCategory}
          </Button>
        </div>

        {showCreateForm ? (
          <div className="mt-4 rounded-lg border border-border bg-background p-5">
            <h3 className="mb-4 font-heading text-lg font-semibold text-foreground">
              {t.admin.newCategory}
            </h3>
            <AdminCategoryForm
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
            searchLabel={t.admin.searchCategories}
            searchPlaceholder={t.admin.searchCategoriesPlaceholder}
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
          {categories.status === "loading" ? (
            <ReferenceMessage loading message={t.common.loading} />
          ) : categories.status === "unavailable" ? (
            <ReferenceMessage message={t.admin.referencesUnavailable} />
          ) : categories.data.items.length === 0 ? (
            <ReferenceMessage message={t.admin.noMatchingCategories} />
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full min-w-[52rem] border-collapse text-left text-sm">
                  <thead className="bg-muted/55 text-xs text-muted-foreground">
                    <tr>
                      <CategoryHeader
                        label={t.admin.categoryEnglishName}
                        column="englishName"
                        activeColumn={sortKey}
                        direction={sortDirection}
                        onSort={updateSort}
                      />
                      <CategoryHeader
                        label={t.admin.categoryTurkishName}
                        column="turkishName"
                        activeColumn={sortKey}
                        direction={sortDirection}
                        onSort={updateSort}
                      />
                      <CategoryHeader
                        label={t.admin.referenceSlug}
                        column="slug"
                        activeColumn={sortKey}
                        direction={sortDirection}
                        onSort={updateSort}
                      />
                      <CategoryHeader
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
                    {categories.data.items.map((category) => (
                      <tr key={category.id} className="transition-colors hover:bg-muted/25">
                        <td className="px-4 py-3 font-medium text-foreground">{category.englishName}</td>
                        <td className="px-4 py-3 text-foreground">{category.turkishName}</td>
                        <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{category.slug}</td>
                        <td className="px-4 py-3">
                          <ReferenceStatusBadge
                            isActive={category.isActive}
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
                              setEditingCategory(category)
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
                page={categories.data.pageNumber}
                totalPages={Math.max(1, categories.data.totalPages)}
                totalCount={categories.data.totalCount}
                pageLabel={t.admin.referencePageStatus}
                previousLabel={t.admin.previousPage}
                nextLabel={t.admin.nextPage}
                onPageChange={setPage}
              />
            </>
          )}
        </div>

        {editingCategory ? (
          <div className="mt-5 rounded-lg border border-border bg-background p-5">
            <h3 className="mb-4 font-heading text-lg font-semibold text-foreground">
              {t.admin.editCategory}
            </h3>
            <AdminCategoryForm
              key={editingCategory.id}
              category={editingCategory}
              onSaved={handleSaved}
              onCancel={() => setEditingCategory(null)}
            />
          </div>
        ) : null}
      </section>
    </AdminReferencePageLayout>
  )
}

function CategoryHeader({
  label,
  column,
  activeColumn,
  direction,
  onSort,
}: {
  label: string
  column: CategorySortKey
  activeColumn: CategorySortKey
  direction: ReferenceSortDirection
  onSort: (column: CategorySortKey) => void
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
