import { apiRequest } from "@/lib/api/client"
import type { OrderWorkflowDiagnostics, OrderWorkflowStatus, PagedResult } from "@/types"

export interface WorkflowDiagnosticsQuery {
  pageNumber?: number
  pageSize?: number
  orderId?: string
  status?: OrderWorkflowStatus
  overdueOnly?: boolean
}

export function getWorkflowDiagnostics(
  query: WorkflowDiagnosticsQuery,
  signal?: AbortSignal,
): Promise<PagedResult<OrderWorkflowDiagnostics>> {
  const parameters = new URLSearchParams({
    pageNumber: String(query.pageNumber ?? 1),
    pageSize: String(query.pageSize ?? 20),
    overdueOnly: String(query.overdueOnly ?? false),
  })
  if (query.orderId) parameters.set("orderId", query.orderId)
  if (query.status !== undefined) parameters.set("status", String(query.status))
  return apiRequest<PagedResult<OrderWorkflowDiagnostics>>(
    `/gateway/workflows/diagnostics?${parameters.toString()}`,
    { authenticated: true, signal },
  )
}
