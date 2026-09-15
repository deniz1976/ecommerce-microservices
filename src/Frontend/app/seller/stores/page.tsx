import { SellerRouteGuard } from "@/components/seller/seller-route-guard"
import { SellerStoreManagementPage } from "@/components/seller/seller-store-management-page"

export default function SellerStoresPage() {
  return (
    <SellerRouteGuard>
      <SellerStoreManagementPage />
    </SellerRouteGuard>
  )
}
