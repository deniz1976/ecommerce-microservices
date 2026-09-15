import type { Dictionary } from "@/lib/i18n/dictionaries"
import type { CustomerNotification } from "@/types"

type NotificationDictionary = Dictionary["notifications"]

type NotificationTypeKey = keyof NotificationDictionary["types"]

const knownTypes: Record<string, NotificationTypeKey> = {
  "order.submitted": "orderSubmitted",
  "order.confirmed": "orderConfirmed",
  "order.cancelled": "orderCancelled",
  "order.cancellation_requested": "orderCancellationRequested",
  "order.cancellation_rejected": "orderCancellationRejected",
  "payment.authorized": "paymentAuthorized",
  "payment.failed": "paymentFailed",
  "shipment.created": "shipmentCreated",
  "shipment.failed": "shipmentFailed",
}

export function notificationTitle(
  notification: CustomerNotification,
  labels: NotificationDictionary,
): string {
  if (!notification.orderId) {
    return notification.title
  }

  return labels.orderTitle.replace("{code}", orderCode(notification.orderId))
}

export function notificationMessage(
  notification: CustomerNotification,
  labels: NotificationDictionary,
  reasons: Dictionary["orders"]["cancellationReasons"],
): string {
  const key = knownTypes[notification.type]
  if (!key) {
    return notification.message
  }

  const template = labels.types[key]

  if (key === "shipmentCreated") {
    return notification.trackingNumber
      ? template.replace("{trackingNumber}", notification.trackingNumber)
      : labels.types.shipmentCreatedWithoutTracking
  }

  if (key === "orderCancelled") {
    const reason = notification.reasonCode
      ? reasons[notification.reasonCode as keyof typeof reasons]
      : undefined
    return reason ? `${template} ${reason}` : template
  }

  return template
}

function orderCode(orderId: string): string {
  return orderId.replace(/-/g, "").slice(0, 8).toUpperCase()
}
