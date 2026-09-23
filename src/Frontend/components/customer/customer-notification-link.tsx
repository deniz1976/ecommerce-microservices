"use client"

import { Bell } from "lucide-react"
import Link from "next/link"
import { useEffect, useState } from "react"

import { buttonVariants } from "@/components/ui/button"
import { getCustomerNotifications } from "@/lib/api/notifications"
import { useI18n } from "@/lib/i18n/provider"
import { subscribeToNotificationReadState } from "@/lib/notifications/read-state"
import { useCustomerNotificationsLive } from "@/lib/notifications/use-customer-notifications-live"
import { cn } from "@/lib/utils"

interface CustomerNotificationLinkProps {
  customerId: string
}

export function CustomerNotificationLink({
  customerId,
}: CustomerNotificationLinkProps) {
  const { t } = useI18n()
  const [unreadCount, setUnreadCount] = useState<number | null>(null)
  const [refreshVersion, setRefreshVersion] = useState(0)

  useCustomerNotificationsLive(
    customerId,
    () => setRefreshVersion((current) => current + 1),
  )

  useEffect(
    () => subscribeToNotificationReadState(
      () => setRefreshVersion((current) => current + 1),
    ),
    [],
  )

  useEffect(() => {
    let active = true

    getCustomerNotifications(customerId, 1, 1, true)
      .then((data) => {
        if (active) setUnreadCount(data.totalCount)
      })
      .catch(() => {
        if (active) setUnreadCount(null)
      })

    return () => {
      active = false
    }
  }, [customerId, refreshVersion])

  const hasUnread = unreadCount !== null && unreadCount > 0
  const accessibleLabel = hasUnread
    ? t.notifications.openNotificationsWithUnread.replace(
        "{count}",
        unreadCount.toString(),
      )
    : t.notifications.openNotifications

  return (
    <Link
      href="/notifications"
      aria-label={accessibleLabel}
      className={cn(
        buttonVariants({ variant: "outline", size: "sm" }),
        "relative gap-1.5",
      )}
    >
      <Bell />
      <span className="hidden sm:inline">
        {t.notifications.openNotifications}
      </span>
      {hasUnread ? (
        <span
          aria-hidden="true"
          className="flex min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[0.625rem] font-semibold leading-4 text-primary-foreground"
        >
          {unreadCount > 99 ? "99+" : unreadCount}
        </span>
      ) : null}
    </Link>
  )
}
