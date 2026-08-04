import { AdminCategoryManagementPage } from "@/components/admin/admin-category-management-page"
import { AdminRouteGuard } from "@/components/admin/admin-route-guard"

export default function CategoriesPage() {
  return (
    <AdminRouteGuard>
      <AdminCategoryManagementPage />
    </AdminRouteGuard>
  )
}
