import { AdminPaymentManagementPage } from "@/components/admin/admin-payment-management-page"
import { AdminRouteGuard } from "@/components/admin/admin-route-guard"

export default function AdminPaymentsPage() {
  return (
    <AdminRouteGuard>
      <AdminPaymentManagementPage />
    </AdminRouteGuard>
  )
}
