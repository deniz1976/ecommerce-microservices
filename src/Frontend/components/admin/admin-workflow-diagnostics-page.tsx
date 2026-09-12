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
import { isOptionalGuid } from "@/lib/admin/filters"
import { getWorkflowDiagnostics } from "@/lib/api/workflows"
import { formatDateTime } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { workflowStatusLabel, workflowStatusTone } from "@/lib/i18n/status"
import type { OrderWorkflowDiagnostics, OrderWorkflowStatus, PagedResult } from "@/types"

type State =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<OrderWorkflowDiagnostics> }
  | { status: "unavailable" }

const workflowStatuses: OrderWorkflowStatus[] = [1, 2, 3, 4, 5, 6]

export function AdminWorkflowDiagnosticsPage() {
  const { locale, t } = useI18n()
  const [state, setState] = useState<State>({ status: "loading" })
  const [orderInput, setOrderInput] = useState("")
  const [orderId, setOrderId] = useState("")
  const [workflowStatus, setWorkflowStatus] = useState<OrderWorkflowStatus | undefined>()
  const [overdueOnly, setOverdueOnly] = useState(false)
  const [page, setPage] = useState(1)
  const [reloadToken, setReloadToken] = useState(0)
  const orderValid = isOptionalGuid(orderInput.trim())

  useEffect(() => {
    const controller = new AbortController()
    getWorkflowDiagnostics(
      { pageNumber: page, pageSize: 20, orderId: orderId || undefined, status: workflowStatus, overdueOnly },
      controller.signal,
    )
      .then((data) => setState({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) setState({ status: "unavailable" })
      })
    return () => controller.abort()
  }, [orderId, overdueOnly, page, reloadToken, workflowStatus])

  const apply = () => {
    if (!orderValid) return
    setState({ status: "loading" })
    setOrderId(orderInput.trim())
    setPage(1)
  }

  const clear = () => {
    setState({ status: "loading" })
    setOrderInput("")
    setOrderId("")
    setWorkflowStatus(undefined)
    setOverdueOnly(false)
    setPage(1)
  }

  const columns: DataTableColumn<OrderWorkflowDiagnostics>[] = [
    {
      id: "orderId",
      header: t.admin.workflowOrderId,
      cell: (row) => <span className="font-mono text-xs">{row.orderId}</span>,
    },
    {
      id: "status",
      header: t.admin.workflowStatus,
      cell: (row) => (
        <StatusBadge
          label={workflowStatusLabel(row.status, t.status)}
          tone={workflowStatusTone(row.status)}
        />
      ),
    },
    {
      id: "deadline",
      header: t.admin.workflowDeadline,
      cell: (row) => formatDateTime(row.stepDeadlineAt, locale),
      className: "tabular-nums",
    },
    {
      id: "timeoutHandled",
      header: t.admin.workflowTimeoutHandled,
      cell: (row) => formatDateTime(row.timeoutHandledAt, locale),
      className: "tabular-nums",
    },
    {
      id: "createdAt",
      header: t.admin.workflowCreatedAt,
      cell: (row) => formatDateTime(row.createdAt, locale),
      className: "tabular-nums",
    },
    {
      id: "updatedAt",
      header: t.admin.workflowUpdatedAt,
      cell: (row) => formatDateTime(row.updatedAt, locale),
      className: "tabular-nums",
    },
  ]

  const hasActiveFilters = orderId !== "" || workflowStatus !== undefined || overdueOnly

  return (
    <AdminShell section="workflows" title={t.admin.workflows} description={t.admin.manageWorkflowsDescription}>
      <section className="mt-6 flex flex-col gap-4">
        <h2 className="font-heading text-xl font-semibold">{t.admin.workflowList}</h2>

        <FilterBar onClear={clear} hasActiveFilters={hasActiveFilters}>
          <FilterField label={t.admin.workflowOrderId} className="min-w-72 flex-1">
            <Input
              value={orderInput}
              onChange={(event) => setOrderInput(event.target.value)}
              aria-invalid={!orderValid}
              className="font-mono text-xs"
            />
            {!orderValid ? (
              <span className="text-xs text-destructive">{t.admin.invalidOrderId}</span>
            ) : null}
          </FilterField>

          <FilterField label={t.admin.workflowStatus}>
            <SelectNative
              value={workflowStatus ?? ""}
              onChange={(event) => {
                setState({ status: "loading" })
                setWorkflowStatus(
                  event.target.value ? (Number(event.target.value) as OrderWorkflowStatus) : undefined,
                )
                setPage(1)
              }}
            >
              <option value="">{t.admin.allWorkflowStatuses}</option>
              {workflowStatuses.map((value) => (
                <option key={value} value={value}>
                  {workflowStatusLabel(value, t.status)}
                </option>
              ))}
            </SelectNative>
          </FilterField>

          <label className="flex h-8 items-center gap-2 self-end rounded-lg border border-border px-2.5 text-sm">
            <input
              type="checkbox"
              checked={overdueOnly}
              onChange={(event) => {
                setState({ status: "loading" })
                setOverdueOnly(event.target.checked)
                setPage(1)
              }}
            />
            {t.admin.workflowOverdueOnly}
          </label>

          <Button type="button" className="self-end" onClick={apply} disabled={!orderValid}>
            <SearchIcon />
            {t.admin.applyWorkflowFilters}
          </Button>
        </FilterBar>

        <DataTable
          columns={columns}
          page={state.status === "ready" ? state.data : null}
          rowKey={(row) => row.orderId}
          isLoading={state.status === "loading"}
          error={state.status === "unavailable"}
          onRetry={() => {
            setState({ status: "loading" })
            setReloadToken((token) => token + 1)
          }}
          onPageChange={(value) => {
            setState({ status: "loading" })
            setPage(value)
          }}
          rowClassName={(row) => (row.isOverdue ? "bg-destructive/5" : undefined)}
          emptyTitle={t.admin.noMatchingWorkflows}
          minWidthClassName="min-w-[70rem]"
        />
      </section>
    </AdminShell>
  )
}
