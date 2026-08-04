import { AdminBrandManagementPage } from "@/components/admin/admin-brand-management-page"
import { AdminRouteGuard } from "@/components/admin/admin-route-guard"

export default function BrandsPage() {
  return (
    <AdminRouteGuard>
      <AdminBrandManagementPage />
    </AdminRouteGuard>
  )
}
