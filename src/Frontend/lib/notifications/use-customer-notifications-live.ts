import { useEffect, useRef } from "react"

import {
  createCustomerNotificationConnection,
  startCustomerNotificationConnection,
} from "@/lib/notifications/live"
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

    let active = true
    const connection = createCustomerNotificationConnection(
      customerId,
      (notification) => {
        if (active) {
          notificationHandler.current(notification)
        }
      },
    )

    startCustomerNotificationConnection(connection, customerId).catch(() => {
      // Persisted history remains available when the live channel is offline.
    })

    return () => {
      active = false
      void connection.stop().catch(() => undefined)
    }
  }, [customerId])
}
