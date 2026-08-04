import { AdminOrderManagementPage } from "@/components/admin/admin-order-management-page"
import { AdminRouteGuard } from "@/components/admin/admin-route-guard"

export default function AdminOrdersPage() {
  return (
    <AdminRouteGuard>
      <AdminOrderManagementPage />
    </AdminRouteGuard>
  )
}
