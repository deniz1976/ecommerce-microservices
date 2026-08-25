import { AdminRouteGuard } from "@/components/admin/admin-route-guard"
import { AdminWorkflowDiagnosticsPage } from "@/components/admin/admin-workflow-diagnostics-page"

export default function AdminWorkflowsPage() {
  return <AdminRouteGuard><AdminWorkflowDiagnosticsPage /></AdminRouteGuard>
}
