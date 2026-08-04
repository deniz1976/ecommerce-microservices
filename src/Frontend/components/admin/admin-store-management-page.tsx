"use client"

import { Loader2, Search, Store } from "lucide-react"
import type { ReactNode } from "react"
import { useEffect, useState } from "react"

import { AdminPageLayout } from "@/components/admin/admin-page-layout"
import { ReferencePagination } from "@/components/admin/admin-reference-list-controls"
import { Button } from "@/components/ui/button"
import { isOptionalGuid } from "@/lib/admin/filters"
import { getManagedCatalogStores, type ManagedCatalogStoreQuery } from "@/lib/api/catalog"
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

  const ownerUserId = ownerInput.trim()
  const ownerValid = isOptionalGuid(ownerUserId)

  useEffect(() => {
    const controller = new AbortController()
    getManagedCatalogStores({
      ...filters,
      pageNumber: page,
      pageSize,
      sortBy,
      sortDescending,
    }, controller.signal)
      .then((data) => setStores({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setStores({ status: "unavailable" })
        }
      })
    return () => controller.abort()
  }, [filters, page, pageSize, sortBy, sortDescending])

  const applyFilters = () => {
    if (!ownerValid) return
    setStores({ status: "loading" })
    setFilters({
      search: searchInput.trim() || undefined,
      ownerUserId: ownerUserId || undefined,
    })
    setPage(1)
  }

  const updateQuery = (update: () => void) => {
    setStores({ status: "loading" })
    update()
    setPage(1)
  }

  return (
    <AdminPageLayout title={t.admin.stores} description={t.admin.manageStoresDescription}>
      <section className="mt-6" aria-labelledby="admin-store-list-title">
        <h2 id="admin-store-list-title" className="font-heading text-xl font-semibold text-foreground">{t.admin.storeList}</h2>
        <div className="mt-4 grid gap-3 rounded-lg border border-border bg-background p-4 md:grid-cols-2 xl:grid-cols-[minmax(15rem,1fr)_minmax(18rem,1fr)_13rem_12rem_10rem]">
          <label className="grid content-start gap-2 text-sm font-medium text-foreground">
            {t.admin.storeSearch}
            <input value={searchInput} onChange={(event) => setSearchInput(event.target.value)} onKeyDown={(event) => { if (event.key === "Enter") applyFilters() }} placeholder={t.admin.storeSearchPlaceholder} className="h-10 rounded-md border border-input bg-background px-3" />
          </label>
          <label className="grid content-start gap-2 text-sm font-medium text-foreground">
            {t.admin.storeOwnerId}
            <input value={ownerInput} onChange={(event) => setOwnerInput(event.target.value)} onKeyDown={(event) => { if (event.key === "Enter") applyFilters() }} placeholder={t.admin.guidFilterPlaceholder} aria-invalid={!ownerValid} className="h-10 rounded-md border border-input bg-background px-3 font-mono text-xs" />
            {!ownerValid ? <span className="text-xs text-destructive">{t.admin.invalidOwnerId}</span> : null}
          </label>
          <StoreSelect label={t.admin.storeSort} value={sortBy} onChange={(value) => updateQuery(() => setSortBy(value as NonNullable<ManagedCatalogStoreQuery["sortBy"]>))}>
            <option value="name">{t.admin.storeName}</option>
            <option value="slug">{t.admin.storeSlug}</option>
            <option value="createdAt">{t.admin.paymentCreatedAt}</option>
            <option value="updatedAt">{t.admin.paymentUpdatedAt}</option>
          </StoreSelect>
          <StoreSelect label={t.admin.inventoryDirection} value={sortDescending ? "descending" : "ascending"} onChange={(value) => updateQuery(() => setSortDescending(value === "descending"))}>
            <option value="ascending">{t.admin.ascending}</option>
            <option value="descending">{t.admin.descending}</option>
          </StoreSelect>
          <div className="grid content-start gap-2">
            <span className="text-sm font-medium text-foreground">{t.admin.rowsPerPage}</span>
            <div className="flex gap-2">
              <select value={pageSize} onChange={(event) => updateQuery(() => setPageSize(Number(event.target.value)))} className="h-10 min-w-0 flex-1 rounded-md border border-input bg-background px-3">
                {[10, 20, 50].map((value) => <option key={value} value={value}>{value}</option>)}
              </select>
              <Button type="button" size="icon" onClick={applyFilters} disabled={!ownerValid} aria-label={t.admin.applyStoreFilters}><Search /></Button>
            </div>
          </div>
        </div>

        <div className="mt-4 overflow-hidden rounded-lg border border-border bg-background">
          {stores.status === "loading" ? <StoreMessage loading message={t.common.loading} />
            : stores.status === "unavailable" ? <StoreMessage message={t.admin.storesUnavailable} />
              : stores.data.items.length === 0 ? <StoreMessage message={t.admin.noMatchingStores} />
                : <>
                  <div className="overflow-x-auto">
                    <table className="w-full min-w-[68rem] border-collapse text-left text-sm">
                      <thead className="bg-muted/55 text-xs text-muted-foreground"><tr>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.storeName}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.storeSlug}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.storeOwnerId}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentCreatedAt}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentUpdatedAt}</th>
                      </tr></thead>
                      <tbody className="divide-y divide-border">{stores.data.items.map((store) => <tr key={store.id} className="hover:bg-muted/25">
                        <td className="px-4 py-3 font-medium text-foreground">{store.name}</td>
                        <td className="px-4 py-3 font-mono text-xs text-primary">/{store.slug}</td>
                        <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{store.ownerUserId}</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatDate(store.createdAt, locale)}</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatDate(store.updatedAt, locale)}</td>
                      </tr>)}</tbody>
                    </table>
                  </div>
                  <ReferencePagination page={stores.data.pageNumber} totalPages={Math.max(1, stores.data.totalPages)} totalCount={stores.data.totalCount} pageLabel={t.admin.storePageStatus} previousLabel={t.admin.previousPage} nextLabel={t.admin.nextPage} onPageChange={(value) => { setStores({ status: "loading" }); setPage(value) }} />
                </>}
        </div>
      </section>
    </AdminPageLayout>
  )
}

function StoreSelect({ label, value, onChange, children }: { label: string; value: string; onChange: (value: string) => void; children: ReactNode }) {
  return <label className="grid content-start gap-2 text-sm font-medium text-foreground">{label}<select value={value} onChange={(event) => onChange(event.target.value)} className="h-10 rounded-md border border-input bg-background px-3 font-normal">{children}</select></label>
}

function StoreMessage({ message, loading = false }: { message: string; loading?: boolean }) {
  return <div className="flex min-h-44 items-center justify-center gap-3 px-5 text-sm text-muted-foreground">{loading ? <Loader2 className="size-5 animate-spin" /> : <Store className="size-5" />}{message}</div>
}

function formatDate(value: string, locale: "en" | "tr") {
  return new Intl.DateTimeFormat(locale === "tr" ? "tr-TR" : "en-US", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value))
}
