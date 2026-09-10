"use client"

import { SearchIcon } from "lucide-react"
import { useEffect, useState } from "react"

import { AdminPageLayout } from "@/components/admin/admin-page-layout"
import { DataTable, type DataTableColumn } from "@/components/patterns/data-table"
import { FilterBar, FilterField } from "@/components/patterns/filter-bar"
import { StatusBadge } from "@/components/patterns/status-badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { SelectNative } from "@/components/ui/select-native"
import { isOptionalGuid } from "@/lib/admin/filters"
import { getManagedOrders } from "@/lib/api/orders"
import { formatDateTime, formatMoney } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { orderStatusLabel, orderStatusTone } from "@/lib/i18n/status"
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
  const [reloadToken, setReloadToken] = useState(0)

  const normalizedCustomerInput = customerInput.trim()
  const customerInputValid = isOptionalGuid(normalizedCustomerInput)

  useEffect(() => {
    const controller = new AbortController()
    getManagedOrders(
      {
        customerId: customerId || undefined,
        pageNumber: page,
        pageSize,
        status: status === "all" ? undefined : status,
        sortDescending,
      },
      controller.signal,
    )
      .then((data) => setOrders({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setOrders({ status: "unavailable" })
        }
      })
    return () => controller.abort()
  }, [customerId, page, pageSize, reloadToken, sortDescending, status])

  const updateQuery = (update: () => void) => {
    setOrders({ status: "loading" })
    update()
    setPage(1)
  }

  const applyCustomerFilter = () => {
    if (!customerInputValid) return
    updateQuery(() => setCustomerId(normalizedCustomerInput))
  }

  const clearFilters = () => {
    setCustomerInput("")
    updateQuery(() => {
      setCustomerId("")
      setStatus("all")
    })
  }

  const columns: DataTableColumn<OrderSummary>[] = [
    {
      id: "id",
      header: t.admin.orderNumber,
      cell: (row) => <span className="font-mono text-xs">{row.id}</span>,
    },
    {
      id: "customerId",
      header: t.admin.orderCustomerId,
      cell: (row) => (
        <span className="font-mono text-xs text-muted-foreground">{row.customerId}</span>
      ),
    },
    {
      id: "status",
      header: t.admin.orderStatus,
      cell: (row) => (
        <StatusBadge
          label={orderStatusLabel(row.status, t.orders.status)}
          tone={orderStatusTone(row.status)}
        />
      ),
    },
    {
      id: "total",
      header: t.admin.orderTotal,
      headerClassName: "text-right",
      className: "text-right font-medium tabular-nums",
      cell: (row) => formatMoney(row.totalAmount, row.currency, locale),
    },
    {
      id: "createdAt",
      header: t.admin.orderCreatedAt,
      className: "text-muted-foreground tabular-nums",
      cell: (row) => formatDateTime(row.createdAt, locale),
    },
    {
      id: "updatedAt",
      header: t.admin.orderUpdatedAt,
      className: "text-muted-foreground tabular-nums",
      cell: (row) => formatDateTime(row.updatedAt, locale),
    },
  ]

  return (
    <AdminPageLayout title={t.admin.orders} description={t.admin.manageOrdersDescription}>
      <section className="mt-6 flex flex-col gap-4" aria-labelledby="admin-order-list-title">
        <h2 id="admin-order-list-title" className="font-heading text-xl font-semibold">
          {t.admin.orderList}
        </h2>

        <FilterBar
          onClear={clearFilters}
          hasActiveFilters={customerId !== "" || status !== "all"}
        >
          <FilterField label={t.admin.orderCustomerId} className="min-w-72 flex-1">
            <Input
              value={customerInput}
              onChange={(event) => setCustomerInput(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === "Enter") applyCustomerFilter()
              }}
              placeholder={t.admin.orderCustomerIdPlaceholder}
              aria-invalid={!customerInputValid}
              className="font-mono text-xs"
            />
            {!customerInputValid ? (
              <span className="text-xs text-destructive">{t.admin.invalidCustomerId}</span>
            ) : null}
          </FilterField>

          <FilterField label={t.admin.orderStatus}>
            <SelectNative
              value={String(status)}
              onChange={(event) =>
                updateQuery(() =>
                  setStatus(
                    event.target.value === "all"
                      ? "all"
                      : (Number(event.target.value) as OrderStatus),
                  ),
                )
              }
            >
              <option value="all">{t.admin.allOrderStatuses}</option>
              {orderStatuses.map((value) => (
                <option key={value} value={value}>
                  {orderStatusLabel(value, t.orders.status)}
                </option>
              ))}
            </SelectNative>
          </FilterField>

          <FilterField label={t.admin.orderSort}>
            <SelectNative
              value={sortDescending ? "newest" : "oldest"}
              onChange={(event) =>
                updateQuery(() => setSortDescending(event.target.value === "newest"))
              }
            >
              <option value="newest">{t.admin.newestOrders}</option>
              <option value="oldest">{t.admin.oldestOrders}</option>
            </SelectNative>
          </FilterField>

          <FilterField label={t.table.rowsPerPage} className="min-w-24">
            <SelectNative
              value={String(pageSize)}
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
            onClick={applyCustomerFilter}
            disabled={!customerInputValid}
            aria-label={t.admin.applyOrderFilter}
          >
            <SearchIcon />
            {t.admin.applyOrderFilter}
          </Button>
        </FilterBar>

        <DataTable
          columns={columns}
          page={orders.status === "ready" ? orders.data : null}
          rowKey={(row) => row.id}
          isLoading={orders.status === "loading"}
          error={orders.status === "unavailable"}
          onRetry={() => {
            setOrders({ status: "loading" })
            setReloadToken((token) => token + 1)
          }}
          onPageChange={(value) => {
            setOrders({ status: "loading" })
            setPage(value)
          }}
          emptyTitle={t.admin.noMatchingOrders}
          minWidthClassName="min-w-[64rem]"
        />
      </section>
    </AdminPageLayout>
  )
}
