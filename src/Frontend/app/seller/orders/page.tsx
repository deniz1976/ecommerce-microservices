import { SellerOrderManagementPage } from "@/components/seller/seller-order-management-page"
import { SellerRouteGuard } from "@/components/seller/seller-route-guard"

export default function SellerOrdersPage() {
  return (
    <SellerRouteGuard>
      <SellerOrderManagementPage />
    </SellerRouteGuard>
  )
}
