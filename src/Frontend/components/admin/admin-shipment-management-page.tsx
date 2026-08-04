"use client"

import { Loader2, Search, Truck } from "lucide-react"
import type { ReactNode } from "react"
import { useEffect, useState } from "react"

import { AdminPageLayout } from "@/components/admin/admin-page-layout"
import { ReferencePagination } from "@/components/admin/admin-reference-list-controls"
import { Button } from "@/components/ui/button"
import { isOptionalGuid, isValidUtcRange, toOptionalUtcIso } from "@/lib/admin/filters"
import { getManagedShipments, type ManagedShipmentsQuery } from "@/lib/api/shipping"
import { useI18n } from "@/lib/i18n/provider"
import type { PagedResult, ShipmentStatus, ShipmentSummary } from "@/types"

type ShipmentState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<ShipmentSummary> }
  | { status: "unavailable" }

interface AppliedFilters {
  customerId?: string
  orderId?: string
  createdFrom?: string
  createdTo?: string
}

const shipmentStatuses: readonly ShipmentStatus[] = [1, 2]

export function AdminShipmentManagementPage() {
  const { locale, t } = useI18n()
  const [shipments, setShipments] = useState<ShipmentState>({ status: "loading" })
  const [customerInput, setCustomerInput] = useState("")
  const [orderInput, setOrderInput] = useState("")
  const [createdFromInput, setCreatedFromInput] = useState("")
  const [createdToInput, setCreatedToInput] = useState("")
  const [filters, setFilters] = useState<AppliedFilters>({})
  const [status, setStatus] = useState<"all" | ShipmentStatus>("all")
  const [sortBy, setSortBy] = useState<NonNullable<ManagedShipmentsQuery["sortBy"]>>("createdAt")
  const [sortDescending, setSortDescending] = useState(true)
  const [pageSize, setPageSize] = useState(20)
  const [page, setPage] = useState(1)

  const customerId = customerInput.trim()
  const orderId = orderInput.trim()
  const customerValid = isOptionalGuid(customerId)
  const orderValid = isOptionalGuid(orderId)
  const createdFrom = toOptionalUtcIso(createdFromInput)
  const createdTo = toOptionalUtcIso(createdToInput)
  const dateRangeValid = isValidUtcRange(createdFrom, createdTo)
  const filtersValid = customerValid && orderValid && dateRangeValid

  useEffect(() => {
    const controller = new AbortController()
    getManagedShipments({
      ...filters,
      status: status === "all" ? undefined : status,
      pageNumber: page,
      pageSize,
      sortBy,
      sortDescending,
    }, controller.signal)
      .then((data) => setShipments({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setShipments({ status: "unavailable" })
        }
      })
    return () => controller.abort()
  }, [filters, page, pageSize, sortBy, sortDescending, status])

  const applyFilters = () => {
    if (!filtersValid) return
    setShipments({ status: "loading" })
    setFilters({ customerId: customerId || undefined, orderId: orderId || undefined, createdFrom, createdTo })
    setPage(1)
  }

  const updateQuery = (update: () => void) => {
    setShipments({ status: "loading" })
    update()
    setPage(1)
  }

  return (
    <AdminPageLayout title={t.admin.shipments} description={t.admin.manageShipmentsDescription}>
      <section className="mt-6" aria-labelledby="admin-shipment-list-title">
        <h2 id="admin-shipment-list-title" className="font-heading text-xl font-semibold text-foreground">{t.admin.shipmentList}</h2>
        <div className="mt-4 grid gap-3 rounded-lg border border-border bg-background p-4 md:grid-cols-2 xl:grid-cols-4">
          <ShipmentInput label={t.admin.paymentCustomerId} value={customerInput} onChange={setCustomerInput} placeholder={t.admin.guidFilterPlaceholder} invalid={!customerValid} error={t.admin.invalidCustomerId} />
          <ShipmentInput label={t.admin.paymentOrderId} value={orderInput} onChange={setOrderInput} placeholder={t.admin.guidFilterPlaceholder} invalid={!orderValid} error={t.admin.invalidOrderId} />
          <ShipmentInput label={t.admin.createdFrom} value={createdFromInput} onChange={setCreatedFromInput} type="datetime-local" />
          <ShipmentInput label={t.admin.createdTo} value={createdToInput} onChange={setCreatedToInput} type="datetime-local" invalid={!dateRangeValid} error={t.admin.invalidDateRange} />
          <ShipmentSelect label={t.admin.shipmentStatus} value={String(status)} onChange={(value) => updateQuery(() => setStatus(value === "all" ? "all" : Number(value) as ShipmentStatus))}>
            <option value="all">{t.admin.allShipmentStatuses}</option>
            {shipmentStatuses.map((value) => <option key={value} value={value}>{shipmentStatusLabel(value, t.admin)}</option>)}
          </ShipmentSelect>
          <ShipmentSelect label={t.admin.shipmentSort} value={sortBy} onChange={(value) => updateQuery(() => setSortBy(value as NonNullable<ManagedShipmentsQuery["sortBy"]>))}>
            <option value="createdAt">{t.admin.paymentCreatedAt}</option>
            <option value="updatedAt">{t.admin.paymentUpdatedAt}</option>
            <option value="status">{t.admin.shipmentStatus}</option>
          </ShipmentSelect>
          <ShipmentSelect label={t.admin.inventoryDirection} value={sortDescending ? "descending" : "ascending"} onChange={(value) => updateQuery(() => setSortDescending(value === "descending"))}>
            <option value="descending">{t.admin.descending}</option>
            <option value="ascending">{t.admin.ascending}</option>
          </ShipmentSelect>
          <div className="grid content-start gap-2">
            <span className="text-sm font-medium text-foreground">{t.admin.rowsPerPage}</span>
            <div className="flex gap-2">
              <select value={pageSize} onChange={(event) => updateQuery(() => setPageSize(Number(event.target.value)))} className="h-10 min-w-0 flex-1 rounded-md border border-input bg-background px-3">
                {[10, 20, 50].map((value) => <option key={value} value={value}>{value}</option>)}
              </select>
              <Button type="button" size="icon" onClick={applyFilters} disabled={!filtersValid} aria-label={t.admin.applyShipmentFilters}><Search /></Button>
            </div>
          </div>
        </div>

        <div className="mt-4 overflow-hidden rounded-lg border border-border bg-background">
          {shipments.status === "loading" ? <ShipmentMessage loading message={t.common.loading} />
            : shipments.status === "unavailable" ? <ShipmentMessage message={t.admin.shipmentsUnavailable} />
              : shipments.data.items.length === 0 ? <ShipmentMessage message={t.admin.noMatchingShipments} />
                : <>
                  <div className="overflow-x-auto">
                    <table className="w-full min-w-[72rem] border-collapse text-left text-sm">
                      <thead className="bg-muted/55 text-xs text-muted-foreground"><tr>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.shipmentId}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentOrderId}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentCustomerId}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.trackingNumber}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.shipmentStatus}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentCreatedAt}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentUpdatedAt}</th>
                      </tr></thead>
                      <tbody className="divide-y divide-border">{shipments.data.items.map((shipment) => <tr key={shipment.id} className="hover:bg-muted/25">
                        <td className="px-4 py-3 font-mono text-xs text-foreground">{shipment.id}</td>
                        <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{shipment.orderId}</td>
                        <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{shipment.customerId}</td>
                        <td className="px-4 py-3 font-mono text-xs text-foreground">{shipment.trackingNumber ?? t.admin.notAvailable}</td>
                        <td className="px-4 py-3 font-medium text-primary">{shipmentStatusLabel(shipment.status, t.admin)}</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatDate(shipment.createdAt, locale)}</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatDate(shipment.updatedAt, locale)}</td>
                      </tr>)}</tbody>
                    </table>
                  </div>
                  <ReferencePagination page={shipments.data.pageNumber} totalPages={Math.max(1, shipments.data.totalPages)} totalCount={shipments.data.totalCount} pageLabel={t.admin.shipmentPageStatus} previousLabel={t.admin.previousPage} nextLabel={t.admin.nextPage} onPageChange={(value) => { setShipments({ status: "loading" }); setPage(value) }} />
                </>}
        </div>
      </section>
    </AdminPageLayout>
  )
}

function ShipmentInput({ label, value, onChange, placeholder, type = "text", invalid = false, error }: { label: string; value: string; onChange: (value: string) => void; placeholder?: string; type?: string; invalid?: boolean; error?: string }) {
  return <label className="grid content-start gap-2 text-sm font-medium text-foreground">{label}<input type={type} value={value} onChange={(event) => onChange(event.target.value)} placeholder={placeholder} aria-invalid={invalid} className="h-10 rounded-md border border-input bg-background px-3" />{invalid && error ? <span className="text-xs text-destructive">{error}</span> : null}</label>
}

function ShipmentSelect({ label, value, onChange, children }: { label: string; value: string; onChange: (value: string) => void; children: ReactNode }) {
  return <label className="grid content-start gap-2 text-sm font-medium text-foreground">{label}<select value={value} onChange={(event) => onChange(event.target.value)} className="h-10 rounded-md border border-input bg-background px-3 font-normal">{children}</select></label>
}

function ShipmentMessage({ message, loading = false }: { message: string; loading?: boolean }) {
  return <div className="flex min-h-44 items-center justify-center gap-3 px-5 text-sm text-muted-foreground">{loading ? <Loader2 className="size-5 animate-spin" /> : <Truck className="size-5" />}{message}</div>
}

function shipmentStatusLabel(status: ShipmentStatus, labels: { shipmentCreated: string; shipmentFailed: string }) {
  return status === 1 ? labels.shipmentCreated : labels.shipmentFailed
}

function formatDate(value: string, locale: "en" | "tr") {
  return new Intl.DateTimeFormat(locale === "tr" ? "tr-TR" : "en-US", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value))
}
