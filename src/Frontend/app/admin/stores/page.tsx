import { AdminStoreManagementPage } from "@/components/admin/admin-store-management-page"
import { AdminRouteGuard } from "@/components/admin/admin-route-guard"

export default function AdminStoresPage() {
  return (
    <AdminRouteGuard>
      <AdminStoreManagementPage />
    </AdminRouteGuard>
  )
}
