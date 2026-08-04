import { AdminUserDetailPage } from "@/components/admin/admin-user-detail-page"
import { AdminRouteGuard } from "@/components/admin/admin-route-guard"

export default function AdminUserPage() {
  return (
    <AdminRouteGuard>
      <AdminUserDetailPage />
    </AdminRouteGuard>
  )
}
