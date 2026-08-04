"use client"

import { Loader2, ReceiptText, Search } from "lucide-react"
import type { ReactNode } from "react"
import { useEffect, useState } from "react"

import { AdminPageLayout } from "@/components/admin/admin-page-layout"
import { ReferencePagination } from "@/components/admin/admin-reference-list-controls"
import { Button } from "@/components/ui/button"
import { getManagedPayments, type ManagedPaymentsQuery } from "@/lib/api/payments"
import { useI18n } from "@/lib/i18n/provider"
import type { PagedResult, PaymentStatus, PaymentSummary } from "@/types"

type PaymentState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<PaymentSummary> }
  | { status: "unavailable" }

interface AppliedFilters {
  customerId?: string
  orderId?: string
  createdFrom?: string
  createdTo?: string
}

const paymentStatuses: readonly PaymentStatus[] = [1, 2, 3]
const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

export function AdminPaymentManagementPage() {
  const { locale, t } = useI18n()
  const [payments, setPayments] = useState<PaymentState>({ status: "loading" })
  const [customerInput, setCustomerInput] = useState("")
  const [orderInput, setOrderInput] = useState("")
  const [createdFromInput, setCreatedFromInput] = useState("")
  const [createdToInput, setCreatedToInput] = useState("")
  const [filters, setFilters] = useState<AppliedFilters>({})
  const [status, setStatus] = useState<"all" | PaymentStatus>("all")
  const [sortBy, setSortBy] = useState<NonNullable<ManagedPaymentsQuery["sortBy"]>>("createdAt")
  const [sortDescending, setSortDescending] = useState(true)
  const [pageSize, setPageSize] = useState(20)
  const [page, setPage] = useState(1)

  const customerId = customerInput.trim()
  const orderId = orderInput.trim()
  const customerValid = customerId === "" || guidPattern.test(customerId)
  const orderValid = orderId === "" || guidPattern.test(orderId)
  const createdFrom = toUtc(createdFromInput)
  const createdTo = toUtc(createdToInput)
  const dateRangeValid = !createdFrom || !createdTo || createdFrom <= createdTo
  const filtersValid = customerValid && orderValid && dateRangeValid

  useEffect(() => {
    const controller = new AbortController()
    getManagedPayments({
      ...filters,
      status: status === "all" ? undefined : status,
      pageNumber: page,
      pageSize,
      sortBy,
      sortDescending,
    }, controller.signal)
      .then((data) => setPayments({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setPayments({ status: "unavailable" })
        }
      })
    return () => controller.abort()
  }, [filters, page, pageSize, sortBy, sortDescending, status])

  const applyFilters = () => {
    if (!filtersValid) return
    setPayments({ status: "loading" })
    setFilters({
      customerId: customerId || undefined,
      orderId: orderId || undefined,
      createdFrom,
      createdTo,
    })
    setPage(1)
  }

  return (
    <AdminPageLayout title={t.admin.payments} description={t.admin.managePaymentsDescription}>
      <section className="mt-6" aria-labelledby="admin-payment-list-title">
        <h2 id="admin-payment-list-title" className="font-heading text-xl font-semibold text-foreground">{t.admin.paymentList}</h2>
        <div className="mt-4 grid gap-3 rounded-lg border border-border bg-background p-4 md:grid-cols-2 xl:grid-cols-4">
          <PaymentInput label={t.admin.paymentCustomerId} value={customerInput} onChange={setCustomerInput} placeholder={t.admin.guidFilterPlaceholder} invalid={!customerValid} error={t.admin.invalidCustomerId} />
          <PaymentInput label={t.admin.paymentOrderId} value={orderInput} onChange={setOrderInput} placeholder={t.admin.guidFilterPlaceholder} invalid={!orderValid} error={t.admin.invalidOrderId} />
          <PaymentInput label={t.admin.createdFrom} value={createdFromInput} onChange={setCreatedFromInput} type="datetime-local" />
          <PaymentInput label={t.admin.createdTo} value={createdToInput} onChange={setCreatedToInput} type="datetime-local" invalid={!dateRangeValid} error={t.admin.invalidDateRange} />
          <PaymentSelect label={t.admin.paymentStatus} value={String(status)} onChange={(value) => updateQuery(() => setStatus(value === "all" ? "all" : Number(value) as PaymentStatus))}>
            <option value="all">{t.admin.allPaymentStatuses}</option>
            {paymentStatuses.map((value) => <option key={value} value={value}>{paymentStatusLabel(value, t.orders.payment.status)}</option>)}
          </PaymentSelect>
          <PaymentSelect label={t.admin.paymentSort} value={sortBy} onChange={(value) => updateQuery(() => setSortBy(value as NonNullable<ManagedPaymentsQuery["sortBy"]>))}>
            <option value="createdAt">{t.admin.paymentCreatedAt}</option>
            <option value="updatedAt">{t.admin.paymentUpdatedAt}</option>
            <option value="amount">{t.admin.paymentAmount}</option>
            <option value="status">{t.admin.paymentStatus}</option>
          </PaymentSelect>
          <PaymentSelect label={t.admin.inventoryDirection} value={sortDescending ? "descending" : "ascending"} onChange={(value) => updateQuery(() => setSortDescending(value === "descending"))}>
            <option value="descending">{t.admin.descending}</option>
            <option value="ascending">{t.admin.ascending}</option>
          </PaymentSelect>
          <div className="grid content-start gap-2">
            <span className="text-sm font-medium text-foreground">{t.admin.rowsPerPage}</span>
            <div className="flex gap-2">
              <select value={pageSize} onChange={(event) => updateQuery(() => setPageSize(Number(event.target.value)))} className="h-10 min-w-0 flex-1 rounded-md border border-input bg-background px-3">
                {[10, 20, 50].map((value) => <option key={value} value={value}>{value}</option>)}
              </select>
              <Button type="button" size="icon" onClick={applyFilters} disabled={!filtersValid} aria-label={t.admin.applyPaymentFilters}><Search /></Button>
            </div>
          </div>
        </div>

        <div className="mt-4 overflow-hidden rounded-lg border border-border bg-background">
          {payments.status === "loading" ? <PaymentMessage loading message={t.common.loading} />
            : payments.status === "unavailable" ? <PaymentMessage message={t.admin.paymentsUnavailable} />
              : payments.data.items.length === 0 ? <PaymentMessage message={t.admin.noMatchingPayments} />
                : <>
                  <div className="overflow-x-auto">
                    <table className="w-full min-w-[78rem] border-collapse text-left text-sm">
                      <thead className="bg-muted/55 text-xs text-muted-foreground"><tr>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentId}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentOrderId}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentCustomerId}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentStatus}</th>
                        <th scope="col" className="px-4 py-3 text-right font-medium">{t.admin.paymentAmount}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentCreatedAt}</th>
                        <th scope="col" className="px-4 py-3 font-medium">{t.admin.paymentUpdatedAt}</th>
                      </tr></thead>
                      <tbody className="divide-y divide-border">{payments.data.items.map((payment) => <tr key={payment.id} className="hover:bg-muted/25">
                        <td className="px-4 py-3 font-mono text-xs text-foreground">{payment.id}</td>
                        <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{payment.orderId}</td>
                        <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{payment.customerId}</td>
                        <td className="px-4 py-3 font-medium text-primary">{paymentStatusLabel(payment.status, t.orders.payment.status)}</td>
                        <td className="px-4 py-3 text-right font-medium text-foreground">{formatMoney(payment.amount, payment.currency, locale)}</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatDate(payment.createdAt, locale)}</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatDate(payment.updatedAt, locale)}</td>
                      </tr>)}</tbody>
                    </table>
                  </div>
                  <ReferencePagination page={payments.data.pageNumber} totalPages={Math.max(1, payments.data.totalPages)} totalCount={payments.data.totalCount} pageLabel={t.admin.paymentPageStatus} previousLabel={t.admin.previousPage} nextLabel={t.admin.nextPage} onPageChange={(value) => { setPayments({ status: "loading" }); setPage(value) }} />
                </>}
        </div>
      </section>
    </AdminPageLayout>
  )

  function updateQuery(update: () => void) {
    setPayments({ status: "loading" })
    update()
    setPage(1)
  }
}

function PaymentInput({ label, value, onChange, placeholder, type = "text", invalid = false, error }: { label: string; value: string; onChange: (value: string) => void; placeholder?: string; type?: string; invalid?: boolean; error?: string }) {
  return <label className="grid content-start gap-2 text-sm font-medium text-foreground">{label}<input type={type} value={value} onChange={(event) => onChange(event.target.value)} placeholder={placeholder} aria-invalid={invalid} className="h-10 rounded-md border border-input bg-background px-3" />{invalid && error ? <span className="text-xs text-destructive">{error}</span> : null}</label>
}

function PaymentSelect({ label, value, onChange, children }: { label: string; value: string; onChange: (value: string) => void; children: ReactNode }) {
  return <label className="grid content-start gap-2 text-sm font-medium text-foreground">{label}<select value={value} onChange={(event) => onChange(event.target.value)} className="h-10 rounded-md border border-input bg-background px-3 font-normal">{children}</select></label>
}

function PaymentMessage({ message, loading = false }: { message: string; loading?: boolean }) {
  return <div className="flex min-h-44 items-center justify-center gap-3 px-5 text-sm text-muted-foreground">{loading ? <Loader2 className="size-5 animate-spin" /> : <ReceiptText className="size-5" />}{message}</div>
}

function paymentStatusLabel(status: PaymentStatus, labels: { authorized: string; failed: string; refunded: string }) {
  return status === 1 ? labels.authorized : status === 2 ? labels.failed : labels.refunded
}

function toUtc(value: string) {
  return value ? new Date(value).toISOString() : undefined
}

function formatMoney(amount: number, currency: string, locale: "en" | "tr") {
  return new Intl.NumberFormat(locale === "tr" ? "tr-TR" : "en-US", { style: "currency", currency }).format(amount)
}

function formatDate(value: string, locale: "en" | "tr") {
  return new Intl.DateTimeFormat(locale === "tr" ? "tr-TR" : "en-US", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value))
}
