import { AdminInventoryManagementPage } from "@/components/admin/admin-inventory-management-page"
import { AdminRouteGuard } from "@/components/admin/admin-route-guard"

export default function AdminInventoryPage() {
  return (
    <AdminRouteGuard>
      <AdminInventoryManagementPage />
    </AdminRouteGuard>
  )
}
