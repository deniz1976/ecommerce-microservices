"use client"

import { SearchIcon } from "lucide-react"
import { useEffect, useState } from "react"

import { AdminShell } from "@/components/admin/admin-shell"
import { DataTable, type DataTableColumn } from "@/components/patterns/data-table"
import { FilterBar, FilterField } from "@/components/patterns/filter-bar"
import { StatusBadge } from "@/components/patterns/status-badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { SelectNative } from "@/components/ui/select-native"
import { isOptionalGuid, isValidUtcRange, toOptionalUtcIso } from "@/lib/admin/filters"
import { getManagedPayments, type ManagedPaymentsQuery } from "@/lib/api/payments"
import { formatDateTime, formatMoney } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { paymentStatusLabel, paymentStatusTone } from "@/lib/i18n/status"
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
  const [reloadToken, setReloadToken] = useState(0)

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
    getManagedPayments(
      {
        ...filters,
        status: status === "all" ? undefined : status,
        pageNumber: page,
        pageSize,
        sortBy,
        sortDescending,
      },
      controller.signal,
    )
      .then((data) => setPayments({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setPayments({ status: "unavailable" })
        }
      })
    return () => controller.abort()
  }, [filters, page, pageSize, reloadToken, sortBy, sortDescending, status])

  const updateQuery = (update: () => void) => {
    setPayments({ status: "loading" })
    update()
    setPage(1)
  }

  const applyFilters = () => {
    if (!filtersValid) return
    updateQuery(() =>
      setFilters({
        customerId: customerId || undefined,
        orderId: orderId || undefined,
        createdFrom,
        createdTo,
      }),
    )
  }

  const clearFilters = () => {
    setCustomerInput("")
    setOrderInput("")
    setCreatedFromInput("")
    setCreatedToInput("")
    updateQuery(() => {
      setFilters({})
      setStatus("all")
    })
  }

  const hasActiveFilters =
    status !== "all" ||
    filters.customerId !== undefined ||
    filters.orderId !== undefined ||
    filters.createdFrom !== undefined ||
    filters.createdTo !== undefined

  const columns: DataTableColumn<PaymentSummary>[] = [
    {
      id: "id",
      header: t.admin.paymentId,
      cell: (row) => <span className="font-mono text-xs">{row.id}</span>,
    },
    {
      id: "orderId",
      header: t.admin.paymentOrderId,
      cell: (row) => <span className="font-mono text-xs text-muted-foreground">{row.orderId}</span>,
    },
    {
      id: "customerId",
      header: t.admin.paymentCustomerId,
      cell: (row) => (
        <span className="font-mono text-xs text-muted-foreground">{row.customerId}</span>
      ),
    },
    {
      id: "status",
      header: t.admin.paymentStatus,
      cell: (row) => (
        <StatusBadge
          label={paymentStatusLabel(row.status, t.status)}
          tone={paymentStatusTone(row.status)}
        />
      ),
    },
    {
      id: "amount",
      header: t.admin.paymentAmount,
      headerClassName: "text-right",
      className: "text-right font-medium tabular-nums",
      cell: (row) => formatMoney(row.amount, row.currency, locale),
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
    <AdminShell section="payments" title={t.admin.payments} description={t.admin.managePaymentsDescription}>
      <section className="mt-6 flex flex-col gap-4" aria-labelledby="admin-payment-list-title">
        <h2 id="admin-payment-list-title" className="font-heading text-xl font-semibold">
          {t.admin.paymentList}
        </h2>

        <FilterBar onClear={clearFilters} hasActiveFilters={hasActiveFilters}>
          <FilterField label={t.admin.paymentCustomerId} className="min-w-64">
            <Input
              value={customerInput}
              onChange={(event) => setCustomerInput(event.target.value)}
              placeholder={t.admin.guidFilterPlaceholder}
              aria-invalid={!customerValid}
              className="font-mono text-xs"
            />
            {!customerValid ? (
              <span className="text-xs text-destructive">{t.admin.invalidCustomerId}</span>
            ) : null}
          </FilterField>

          <FilterField label={t.admin.paymentOrderId} className="min-w-64">
            <Input
              value={orderInput}
              onChange={(event) => setOrderInput(event.target.value)}
              placeholder={t.admin.guidFilterPlaceholder}
              aria-invalid={!orderValid}
              className="font-mono text-xs"
            />
            {!orderValid ? (
              <span className="text-xs text-destructive">{t.admin.invalidOrderId}</span>
            ) : null}
          </FilterField>

          <FilterField label={t.admin.createdFrom}>
            <Input
              type="datetime-local"
              value={createdFromInput}
              onChange={(event) => setCreatedFromInput(event.target.value)}
            />
          </FilterField>

          <FilterField label={t.admin.createdTo}>
            <Input
              type="datetime-local"
              value={createdToInput}
              onChange={(event) => setCreatedToInput(event.target.value)}
              aria-invalid={!dateRangeValid}
            />
            {!dateRangeValid ? (
              <span className="text-xs text-destructive">{t.admin.invalidDateRange}</span>
            ) : null}
          </FilterField>

          <FilterField label={t.admin.paymentStatus}>
            <SelectNative
              value={String(status)}
              onChange={(event) =>
                updateQuery(() =>
                  setStatus(
                    event.target.value === "all"
                      ? "all"
                      : (Number(event.target.value) as PaymentStatus),
                  ),
                )
              }
            >
              <option value="all">{t.admin.allPaymentStatuses}</option>
              {paymentStatuses.map((value) => (
                <option key={value} value={value}>
                  {paymentStatusLabel(value, t.status)}
                </option>
              ))}
            </SelectNative>
          </FilterField>

          <FilterField label={t.admin.paymentSort}>
            <SelectNative
              value={sortBy}
              onChange={(event) =>
                updateQuery(() =>
                  setSortBy(event.target.value as NonNullable<ManagedPaymentsQuery["sortBy"]>),
                )
              }
            >
              <option value="createdAt">{t.admin.paymentCreatedAt}</option>
              <option value="updatedAt">{t.admin.paymentUpdatedAt}</option>
              <option value="amount">{t.admin.paymentAmount}</option>
              <option value="status">{t.admin.paymentStatus}</option>
            </SelectNative>
          </FilterField>

          <FilterField label={t.admin.inventoryDirection}>
            <SelectNative
              value={sortDescending ? "descending" : "ascending"}
              onChange={(event) =>
                updateQuery(() => setSortDescending(event.target.value === "descending"))
              }
            >
              <option value="descending">{t.admin.descending}</option>
              <option value="ascending">{t.admin.ascending}</option>
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
            disabled={!filtersValid}
            aria-label={t.admin.applyPaymentFilters}
          >
            <SearchIcon />
            {t.admin.applyPaymentFilters}
          </Button>
        </FilterBar>

        <DataTable
          columns={columns}
          page={payments.status === "ready" ? payments.data : null}
          rowKey={(row) => row.id}
          isLoading={payments.status === "loading"}
          error={payments.status === "unavailable"}
          onRetry={() => {
            setPayments({ status: "loading" })
            setReloadToken((token) => token + 1)
          }}
          onPageChange={(value) => {
            setPayments({ status: "loading" })
            setPage(value)
          }}
          emptyTitle={t.admin.noMatchingPayments}
          minWidthClassName="min-w-[78rem]"
        />
      </section>
    </AdminShell>
  )
}
