"use client"

import { ClipboardList, Loader2, Search } from "lucide-react"
import type { ReactNode } from "react"
import { useEffect, useState } from "react"

import { AdminPageLayout } from "@/components/admin/admin-page-layout"
import { ReferencePagination } from "@/components/admin/admin-reference-list-controls"
import { Button } from "@/components/ui/button"
import { isOptionalGuid } from "@/lib/admin/filters"
import { getManagedOrders } from "@/lib/api/orders"
import { useI18n } from "@/lib/i18n/provider"
import { getOrderStatusName } from "@/lib/orders/status"
import type { OrderStatus, OrderSummary, PagedResult } from "@/types"

type OrderState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<OrderSummary> }
  | { status: "unavailable" }

const orderStatuses: readonly OrderStatus[] = [0, 1, 2, 3, 4, 5, 6]
export function AdminOrderManagementPage() {
  const { locale, t } = useI18n()
  const [orders, setOrders] = useState<OrderState>({ status: "loading" })
  const [customerInput, setCustomerInput] = useState("")
  const [customerId, setCustomerId] = useState("")
  const [status, setStatus] = useState<"all" | OrderStatus>("all")
  const [sortDescending, setSortDescending] = useState(true)
  const [pageSize, setPageSize] = useState(20)
  const [page, setPage] = useState(1)

  const normalizedCustomerInput = customerInput.trim()
  const customerInputValid = isOptionalGuid(normalizedCustomerInput)

  useEffect(() => {
    const controller = new AbortController()
    getManagedOrders({
      customerId: customerId || undefined,
      pageNumber: page,
      pageSize,
      status: status === "all" ? undefined : status,
      sortDescending,
    }, controller.signal)
      .then((data) => setOrders({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setOrders({ status: "unavailable" })
        }
      })
    return () => controller.abort()
  }, [customerId, page, pageSize, sortDescending, status])

  const applyCustomerFilter = () => {
    if (!customerInputValid) return
    setOrders({ status: "loading" })
    setCustomerId(normalizedCustomerInput)
    setPage(1)
  }

  return (
    <AdminPageLayout title={t.admin.orders} description={t.admin.manageOrdersDescription}>
      <section className="mt-6" aria-labelledby="admin-order-list-title">
        <h2 id="admin-order-list-title" className="font-heading text-xl font-semibold text-foreground">
          {t.admin.orderList}
        </h2>
        <div className="mt-4 grid gap-3 rounded-lg border border-border bg-background p-4 lg:grid-cols-[minmax(18rem,1fr)_13rem_12rem_10rem]">
          <label className="grid gap-2 text-sm font-medium text-foreground">
            {t.admin.orderCustomerId}
            <span className="flex gap-2">
              <input
                value={customerInput}
                onChange={(event) => setCustomerInput(event.target.value)}
                onKeyDown={(event) => {
                  if (event.key === "Enter") applyCustomerFilter()
                }}
                placeholder={t.admin.orderCustomerIdPlaceholder}
                aria-invalid={!customerInputValid}
                className="h-10 min-w-0 flex-1 rounded-md border border-input bg-background px-3 font-mono text-xs"
              />
              <Button type="button" size="icon" onClick={applyCustomerFilter} disabled={!customerInputValid} aria-label={t.admin.applyOrderFilter}>
                <Search />
              </Button>
            </span>
            {!customerInputValid ? <span className="text-xs text-destructive">{t.admin.invalidCustomerId}</span> : null}
          </label>
          <OrderSelect label={t.admin.orderStatus} value={String(status)} onChange={(value) => {
            setOrders({ status: "loading" })
            setStatus(value === "all" ? "all" : Number(value) as OrderStatus)
            setPage(1)
          }}>
            <option value="all">{t.admin.allOrderStatuses}</option>
            {orderStatuses.map((value) => <option key={value} value={value}>{t.orders.status[getOrderStatusName(value)]}</option>)}
          </OrderSelect>
          <OrderSelect label={t.admin.orderSort} value={sortDescending ? "newest" : "oldest"} onChange={(value) => {
            setOrders({ status: "loading" })
            setSortDescending(value === "newest")
            setPage(1)
          }}>
            <option value="newest">{t.admin.newestOrders}</option>
            <option value="oldest">{t.admin.oldestOrders}</option>
          </OrderSelect>
          <OrderSelect label={t.admin.rowsPerPage} value={String(pageSize)} onChange={(value) => {
            setOrders({ status: "loading" })
            setPageSize(Number(value))
            setPage(1)
          }}>
            {[10, 20, 50].map((value) => <option key={value} value={value}>{value}</option>)}
          </OrderSelect>
        </div>

        <div className="mt-4 overflow-hidden rounded-lg border border-border bg-background">
          {orders.status === "loading" ? (
            <OrderMessage loading message={t.common.loading} />
          ) : orders.status === "unavailable" ? (
            <OrderMessage message={t.admin.ordersUnavailable} />
          ) : orders.data.items.length === 0 ? (
            <OrderMessage message={t.admin.noMatchingOrders} />
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full min-w-[64rem] border-collapse text-left text-sm">
                  <thead className="bg-muted/55 text-xs text-muted-foreground">
                    <tr>
                      <th className="px-4 py-3 font-medium">{t.admin.orderNumber}</th>
                      <th className="px-4 py-3 font-medium">{t.admin.orderCustomerId}</th>
                      <th className="px-4 py-3 font-medium">{t.admin.orderStatus}</th>
                      <th className="px-4 py-3 text-right font-medium">{t.admin.orderTotal}</th>
                      <th className="px-4 py-3 font-medium">{t.admin.orderCreatedAt}</th>
                      <th className="px-4 py-3 font-medium">{t.admin.orderUpdatedAt}</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border">
                    {orders.data.items.map((order) => (
                      <tr key={order.id} className="hover:bg-muted/25">
                        <td className="px-4 py-3 font-mono text-xs text-foreground">{order.id}</td>
                        <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{order.customerId}</td>
                        <td className="px-4 py-3 font-medium text-primary">{t.orders.status[getOrderStatusName(order.status)]}</td>
                        <td className="px-4 py-3 text-right font-medium text-foreground">{formatMoney(order.totalAmount, order.currency, locale)}</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatDate(order.createdAt, locale)}</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatDate(order.updatedAt, locale)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <ReferencePagination
                page={orders.data.pageNumber}
                totalPages={Math.max(1, orders.data.totalPages)}
                totalCount={orders.data.totalCount}
                pageLabel={t.admin.orderPageStatus}
                previousLabel={t.admin.previousPage}
                nextLabel={t.admin.nextPage}
                onPageChange={(value) => {
                  setOrders({ status: "loading" })
                  setPage(value)
                }}
              />
            </>
          )}
        </div>
      </section>
    </AdminPageLayout>
  )
}

function OrderSelect({ label, value, onChange, children }: { label: string; value: string; onChange: (value: string) => void; children: ReactNode }) {
  return (
    <label className="grid content-start gap-2 text-sm font-medium text-foreground">
      {label}
      <select value={value} onChange={(event) => onChange(event.target.value)} className="h-10 rounded-md border border-input bg-background px-3 font-normal">
        {children}
      </select>
    </label>
  )
}

function OrderMessage({ message, loading = false }: { message: string; loading?: boolean }) {
  return <div className="flex min-h-44 items-center justify-center gap-3 px-5 text-sm text-muted-foreground">{loading ? <Loader2 className="size-5 animate-spin" /> : <ClipboardList className="size-5" />}{message}</div>
}

function formatMoney(amount: number, currency: string, locale: "en" | "tr") {
  return new Intl.NumberFormat(locale === "tr" ? "tr-TR" : "en-US", { style: "currency", currency }).format(amount)
}

function formatDate(value: string, locale: "en" | "tr") {
  return new Intl.DateTimeFormat(locale === "tr" ? "tr-TR" : "en-US", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value))
}
