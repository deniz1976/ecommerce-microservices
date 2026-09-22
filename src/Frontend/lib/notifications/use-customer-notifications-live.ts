import { useEffect, useRef } from "react"

import { subscribeToCustomerNotifications } from "@/lib/notifications/live"
import type { CustomerNotification } from "@/types"

export function useCustomerNotificationsLive(
  customerId: string | null,
  onNotification: (notification: CustomerNotification) => void,
) {
  const notificationHandler = useRef(onNotification)

  useEffect(() => {
    notificationHandler.current = onNotification
  }, [onNotification])

  useEffect(() => {
    if (!customerId) return

    return subscribeToCustomerNotifications(customerId, (notification) => {
      notificationHandler.current(notification)
    })
  }, [customerId])
}
