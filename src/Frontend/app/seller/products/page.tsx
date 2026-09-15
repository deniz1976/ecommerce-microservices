import { SellerProductManagementPage } from "@/components/seller/seller-product-management-page"
import { SellerRouteGuard } from "@/components/seller/seller-route-guard"

export default function SellerProductsPage() {
  return (
    <SellerRouteGuard>
      <SellerProductManagementPage />
    </SellerRouteGuard>
  )
}
