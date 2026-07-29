import { ApiError, apiRequest } from "@/lib/api/client"
import type {
  AddBasketItemPayload,
  Basket,
  CheckoutBasketPayload,
  CheckoutBasketResult,
} from "@/types"

export function getBasket(customerId: string): Promise<Basket> {
  return apiRequest<Basket>(`/gateway/baskets/${customerId}`, { authenticated: true })
}

export function addBasketItem(customerId: string, payload: AddBasketItemPayload): Promise<Basket> {
  return apiRequest<Basket>(`/gateway/baskets/${customerId}/items`, {
    method: "PUT",
    authenticated: true,
    body: payload,
  })
}

export async function incrementBasketItem(customerId: string, productId: string): Promise<Basket> {
  let quantity = 1

  try {
    const basket = await getBasket(customerId)
    quantity = (basket.items.find((item) => item.productId === productId)?.quantity ?? 0) + 1
  } catch (error) {
    if (!(error instanceof ApiError) || error.status !== 404) throw error
  }

  return addBasketItem(customerId, { productId, quantity })
}

export function removeBasketItem(customerId: string, productId: string): Promise<Basket> {
  return apiRequest<Basket>(`/gateway/baskets/${customerId}/items/${productId}`, {
    method: "DELETE",
    authenticated: true,
  })
}

export function clearBasket(customerId: string): Promise<void> {
  return apiRequest<void>(`/gateway/baskets/${customerId}`, {
    method: "DELETE",
    authenticated: true,
  })
}

export function checkoutBasket(
  customerId: string,
  payload: CheckoutBasketPayload,
): Promise<CheckoutBasketResult> {
  return apiRequest<CheckoutBasketResult>(`/gateway/baskets/${customerId}/checkout`, {
    method: "POST",
    authenticated: true,
    body: payload,
  })
}
