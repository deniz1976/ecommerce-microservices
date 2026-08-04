import { AdminShipmentManagementPage } from "@/components/admin/admin-shipment-management-page"
import { AdminRouteGuard } from "@/components/admin/admin-route-guard"

export default function AdminShipmentsPage() {
  return (
    <AdminRouteGuard>
      <AdminShipmentManagementPage />
    </AdminRouteGuard>
  )
}
