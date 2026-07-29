import { apiRequest } from "@/lib/api/client"
import type { Payment } from "@/types"

export function getOrderPayment(orderId: string): Promise<Payment> {
  return apiRequest<Payment>(`/gateway/payments/order/${orderId}`, {
    authenticated: true,
  })
}
