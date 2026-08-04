"use client"

import { Loader2, PackageSearch, Search } from "lucide-react"
import type { ReactNode } from "react"
import { useEffect, useState } from "react"

import { AdminPageLayout } from "@/components/admin/admin-page-layout"
import { ReferencePagination } from "@/components/admin/admin-reference-list-controls"
import { Button } from "@/components/ui/button"
import { getManagedInventory, type ManagedInventoryQuery } from "@/lib/api/inventory"
import { useI18n } from "@/lib/i18n/provider"
import type { InventoryItem, PagedResult } from "@/types"

type InventoryState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<InventoryItem> }
  | { status: "unavailable" }

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

export function AdminInventoryManagementPage() {
  const { locale, t } = useI18n()
  const [inventory, setInventory] = useState<InventoryState>({ status: "loading" })
  const [productInput, setProductInput] = useState("")
  const [maximumInput, setMaximumInput] = useState("")
  const [productId, setProductId] = useState("")
  const [maximumAvailableQuantity, setMaximumAvailableQuantity] = useState<number | undefined>()
  const [sortBy, setSortBy] = useState<NonNullable<ManagedInventoryQuery["sortBy"]>>("updatedAt")
  const [sortDescending, setSortDescending] = useState(true)
  const [pageSize, setPageSize] = useState(20)
  const [page, setPage] = useState(1)

  const normalizedProduct = productInput.trim()
  const productValid = normalizedProduct === "" || guidPattern.test(normalizedProduct)
  const normalizedMaximum = maximumInput.trim()
  const maximumValid = normalizedMaximum === "" || /^\d+$/.test(normalizedMaximum)

  useEffect(() => {
    const controller = new AbortController()
    getManagedInventory({
      productId: productId || undefined,
      maximumAvailableQuantity,
      pageNumber: page,
      pageSize,
      sortBy,
      sortDescending,
    }, controller.signal)
      .then((data) => setInventory({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setInventory({ status: "unavailable" })
        }
      })
    return () => controller.abort()
  }, [maximumAvailableQuantity, page, pageSize, productId, sortBy, sortDescending])

  const applyFilters = () => {
    if (!productValid || !maximumValid) return
    setInventory({ status: "loading" })
    setProductId(normalizedProduct)
    setMaximumAvailableQuantity(normalizedMaximum === "" ? undefined : Number(normalizedMaximum))
    setPage(1)
  }

  return (
    <AdminPageLayout title={t.admin.inventory} description={t.admin.manageInventoryDescription}>
      <section className="mt-6" aria-labelledby="admin-inventory-list-title">
        <h2 id="admin-inventory-list-title" className="font-heading text-xl font-semibold text-foreground">{t.admin.inventoryList}</h2>
        <div className="mt-4 grid gap-3 rounded-lg border border-border bg-background p-4 lg:grid-cols-[minmax(18rem,1fr)_14rem_13rem_12rem_10rem]">
          <label className="grid content-start gap-2 text-sm font-medium text-foreground">
            {t.admin.inventoryProductId}
            <input value={productInput} onChange={(event) => setProductInput(event.target.value)} placeholder={t.admin.inventoryProductIdPlaceholder} aria-invalid={!productValid} className="h-10 rounded-md border border-input bg-background px-3 font-mono text-xs" />
            {!productValid ? <span className="text-xs text-destructive">{t.admin.invalidProductId}</span> : null}
          </label>
          <label className="grid content-start gap-2 text-sm font-medium text-foreground">
            {t.admin.maximumAvailable}
            <input inputMode="numeric" value={maximumInput} onChange={(event) => setMaximumInput(event.target.value)} placeholder={t.admin.maximumAvailablePlaceholder} aria-invalid={!maximumValid} className="h-10 rounded-md border border-input bg-background px-3" />
            {!maximumValid ? <span className="text-xs text-destructive">{t.admin.invalidMaximumAvailable}</span> : null}
          </label>
          <InventorySelect label={t.admin.inventorySort} value={sortBy} onChange={(value) => {
            setInventory({ status: "loading" })
            setSortBy(value as NonNullable<ManagedInventoryQuery["sortBy"]>)
            setPage(1)
          }}>
            <option value="updatedAt">{t.admin.inventoryUpdatedAt}</option>
            <option value="availableQuantity">{t.admin.availableQuantity}</option>
            <option value="quantityOnHand">{t.admin.quantityOnHand}</option>
            <option value="reservedQuantity">{t.admin.reservedQuantity}</option>
          </InventorySelect>
          <InventorySelect label={t.admin.inventoryDirection} value={sortDescending ? "descending" : "ascending"} onChange={(value) => {
            setInventory({ status: "loading" })
            setSortDescending(value === "descending")
            setPage(1)
          }}>
            <option value="descending">{t.admin.descending}</option>
            <option value="ascending">{t.admin.ascending}</option>
          </InventorySelect>
          <div className="grid content-start gap-2">
            <span className="text-sm font-medium text-foreground">{t.admin.rowsPerPage}</span>
            <div className="flex gap-2">
              <select value={pageSize} onChange={(event) => { setInventory({ status: "loading" }); setPageSize(Number(event.target.value)); setPage(1) }} className="h-10 min-w-0 flex-1 rounded-md border border-input bg-background px-3">
                {[10, 20, 50].map((value) => <option key={value} value={value}>{value}</option>)}
              </select>
              <Button type="button" size="icon" onClick={applyFilters} disabled={!productValid || !maximumValid} aria-label={t.admin.applyInventoryFilters}><Search /></Button>
            </div>
          </div>
        </div>

        <div className="mt-4 overflow-hidden rounded-lg border border-border bg-background">
          {inventory.status === "loading" ? <InventoryMessage loading message={t.common.loading} />
            : inventory.status === "unavailable" ? <InventoryMessage message={t.admin.inventoryUnavailable} />
              : inventory.data.items.length === 0 ? <InventoryMessage message={t.admin.noMatchingInventory} />
                : <>
                  <div className="overflow-x-auto">
                    <table className="w-full min-w-[55rem] border-collapse text-left text-sm">
                      <thead className="bg-muted/55 text-xs text-muted-foreground"><tr>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.inventoryProductId}</th>
                        <th scope="col" className="px-4 py-3 text-right font-medium">{t.admin.quantityOnHand}</th>
                        <th scope="col" className="px-4 py-3 text-right font-medium">{t.admin.reservedQuantity}</th>
                        <th scope="col" className="px-4 py-3 text-right font-medium">{t.admin.availableQuantity}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.inventoryUpdatedAt}</th>
                      </tr></thead>
                      <tbody className="divide-y divide-border">{inventory.data.items.map((item) => <tr key={item.productId} className="hover:bg-muted/25">
                        <td className="px-4 py-3 font-mono text-xs text-foreground">{item.productId}</td>
                        <td className="px-4 py-3 text-right text-foreground">{item.quantityOnHand}</td>
                        <td className="px-4 py-3 text-right text-muted-foreground">{item.reservedQuantity}</td>
                        <td className="px-4 py-3 text-right font-medium text-primary">{item.availableQuantity}</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatDate(item.updatedAt, locale)}</td>
                      </tr>)}</tbody>
                    </table>
                  </div>
                  <ReferencePagination page={inventory.data.pageNumber} totalPages={Math.max(1, inventory.data.totalPages)} totalCount={inventory.data.totalCount} pageLabel={t.admin.inventoryPageStatus} previousLabel={t.admin.previousPage} nextLabel={t.admin.nextPage} onPageChange={(value) => { setInventory({ status: "loading" }); setPage(value) }} />
                </>}
        </div>
      </section>
    </AdminPageLayout>
  )
}

function InventorySelect({ label, value, onChange, children }: { label: string; value: string; onChange: (value: string) => void; children: ReactNode }) {
  return <label className="grid content-start gap-2 text-sm font-medium text-foreground">{label}<select value={value} onChange={(event) => onChange(event.target.value)} className="h-10 rounded-md border border-input bg-background px-3 font-normal">{children}</select></label>
}

function InventoryMessage({ message, loading = false }: { message: string; loading?: boolean }) {
  return <div className="flex min-h-44 items-center justify-center gap-3 px-5 text-sm text-muted-foreground">{loading ? <Loader2 className="size-5 animate-spin" /> : <PackageSearch className="size-5" />}{message}</div>
}

function formatDate(value: string, locale: "en" | "tr") {
  return new Intl.DateTimeFormat(locale === "tr" ? "tr-TR" : "en-US", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value))
}
