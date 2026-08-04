import { apiRequest } from "@/lib/api/client"
import type { CustomerNotification, PagedResult } from "@/types"

export function getCustomerNotifications(
  customerId: string,
  pageNumber = 1,
  pageSize = 20,
  unreadOnly = false,
): Promise<PagedResult<CustomerNotification>> {
  const query = new URLSearchParams({
    pageNumber: pageNumber.toString(),
    pageSize: pageSize.toString(),
    unreadOnly: unreadOnly.toString(),
  })

  return apiRequest<PagedResult<CustomerNotification>>(
    `/gateway/notifications/customer/${customerId}?${query.toString()}`,
    { authenticated: true },
  )
}

export function markCustomerNotificationRead(
  customerId: string,
  notificationId: string,
): Promise<CustomerNotification> {
  return apiRequest<CustomerNotification>(
    `/gateway/notifications/customer/${customerId}/${notificationId}/read`,
    {
      method: "PUT",
      authenticated: true,
    },
  )
}

export function markAllCustomerNotificationsRead(
  customerId: string,
): Promise<{ changedCount: number }> {
  return apiRequest<{ changedCount: number }>(
    `/gateway/notifications/customer/${customerId}/read`,
    {
      method: "PUT",
      authenticated: true,
    },
  )
}
