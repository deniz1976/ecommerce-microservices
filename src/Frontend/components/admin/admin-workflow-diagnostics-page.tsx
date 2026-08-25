"use client"

import { Activity, Loader2, Search } from "lucide-react"
import { useEffect, useState } from "react"

import { AdminPageLayout } from "@/components/admin/admin-page-layout"
import { ReferencePagination } from "@/components/admin/admin-reference-list-controls"
import { Button } from "@/components/ui/button"
import { isOptionalGuid } from "@/lib/admin/filters"
import { getWorkflowDiagnostics } from "@/lib/api/workflows"
import { useI18n } from "@/lib/i18n/provider"
import type { OrderWorkflowDiagnostics, OrderWorkflowStatus, PagedResult } from "@/types"

type State =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<OrderWorkflowDiagnostics> }
  | { status: "unavailable" }

const statusNames: Record<OrderWorkflowStatus, string> = {
  1: "Submitted",
  2: "InventoryReserved",
  3: "PaymentAuthorized",
  4: "ShipmentCreated",
  5: "Completed",
  6: "Cancelled",
}

export function AdminWorkflowDiagnosticsPage() {
  const { locale, t } = useI18n()
  const [state, setState] = useState<State>({ status: "loading" })
  const [orderInput, setOrderInput] = useState("")
  const [orderId, setOrderId] = useState("")
  const [workflowStatus, setWorkflowStatus] = useState<OrderWorkflowStatus | undefined>()
  const [overdueOnly, setOverdueOnly] = useState(false)
  const [page, setPage] = useState(1)
  const orderValid = isOptionalGuid(orderInput.trim())

  useEffect(() => {
    const controller = new AbortController()
    getWorkflowDiagnostics({ pageNumber: page, pageSize: 20, orderId: orderId || undefined, status: workflowStatus, overdueOnly }, controller.signal)
      .then((data) => setState({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) setState({ status: "unavailable" })
      })
    return () => controller.abort()
  }, [orderId, overdueOnly, page, workflowStatus])

  const apply = () => {
    if (!orderValid) return
    setState({ status: "loading" })
    setOrderId(orderInput.trim())
    setPage(1)
  }

  return (
    <AdminPageLayout title={t.admin.workflows} description={t.admin.manageWorkflowsDescription}>
      <section className="mt-6">
        <h2 className="font-heading text-xl font-semibold">{t.admin.workflowList}</h2>
        <div className="mt-4 grid gap-3 rounded-lg border bg-background p-4 lg:grid-cols-[minmax(18rem,1fr)_14rem_12rem_auto]">
          <label className="grid gap-2 text-sm font-medium">{t.admin.workflowOrderId}<input value={orderInput} onChange={(event) => setOrderInput(event.target.value)} aria-invalid={!orderValid} className="h-10 rounded-md border bg-background px-3 font-mono text-xs" />{!orderValid ? <span className="text-xs text-destructive">{t.admin.invalidOrderId}</span> : null}</label>
          <label className="grid gap-2 text-sm font-medium">{t.admin.workflowStatus}<select value={workflowStatus ?? ""} onChange={(event) => { setState({ status: "loading" }); setWorkflowStatus(event.target.value ? Number(event.target.value) as OrderWorkflowStatus : undefined); setPage(1) }} className="h-10 rounded-md border bg-background px-3 font-normal"><option value="">{t.admin.allWorkflowStatuses}</option>{Object.entries(statusNames).map(([value, name]) => <option key={value} value={value}>{name}</option>)}</select></label>
          <label className="flex h-10 self-end items-center gap-2 rounded-md border px-3 text-sm"><input type="checkbox" checked={overdueOnly} onChange={(event) => { setState({ status: "loading" }); setOverdueOnly(event.target.checked); setPage(1) }} />{t.admin.workflowOverdueOnly}</label>
          <Button type="button" className="self-end" onClick={apply} disabled={!orderValid}><Search />{t.admin.applyWorkflowFilters}</Button>
        </div>
        <div className="mt-4 overflow-hidden rounded-lg border bg-background">
          {state.status === "loading" ? <Message loading text={t.common.loading} /> : state.status === "unavailable" ? <Message text={t.admin.workflowsUnavailable} /> : state.data.items.length === 0 ? <Message text={t.admin.noMatchingWorkflows} /> : <>
            <div className="overflow-x-auto"><table className="w-full min-w-[70rem] text-left text-sm"><thead className="bg-muted/55 text-xs text-muted-foreground"><tr><th className="px-4 py-3">{t.admin.workflowOrderId}</th><th className="px-4 py-3">{t.admin.workflowStatus}</th><th className="px-4 py-3">{t.admin.workflowDeadline}</th><th className="px-4 py-3">{t.admin.workflowTimeoutHandled}</th><th className="px-4 py-3">{t.admin.workflowCreatedAt}</th><th className="px-4 py-3">{t.admin.workflowUpdatedAt}</th></tr></thead><tbody className="divide-y">{state.data.items.map((item) => <tr key={item.orderId} className={item.isOverdue ? "bg-destructive/5" : "hover:bg-muted/25"}><td className="px-4 py-3 font-mono text-xs">{item.orderId}</td><td className="px-4 py-3 font-medium">{statusNames[item.status]}</td><td className="px-4 py-3">{formatDate(item.stepDeadlineAt, locale)}</td><td className="px-4 py-3">{formatDate(item.timeoutHandledAt, locale)}</td><td className="px-4 py-3">{formatDate(item.createdAt, locale)}</td><td className="px-4 py-3">{formatDate(item.updatedAt, locale)}</td></tr>)}</tbody></table></div>
            <ReferencePagination page={state.data.pageNumber} totalPages={Math.max(1, state.data.totalPages)} totalCount={state.data.totalCount} pageLabel={t.admin.workflowPageStatus} previousLabel={t.admin.previousPage} nextLabel={t.admin.nextPage} onPageChange={(value) => { setState({ status: "loading" }); setPage(value) }} />
          </>}
        </div>
      </section>
    </AdminPageLayout>
  )
}

function Message({ text, loading = false }: { text: string; loading?: boolean }) {
  return <div className="flex min-h-44 items-center justify-center gap-3 text-sm text-muted-foreground">{loading ? <Loader2 className="size-5 animate-spin" /> : <Activity className="size-5" />}{text}</div>
}

function formatDate(value: string | null, locale: "en" | "tr") {
  return value ? new Intl.DateTimeFormat(locale === "tr" ? "tr-TR" : "en-US", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value)) : "-"
}
