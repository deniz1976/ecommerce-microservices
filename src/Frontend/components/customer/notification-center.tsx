"use client"

import { Bell, Check, Loader2 } from "lucide-react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { useEffect, useState } from "react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { EmptyState, ErrorState } from "@/components/patterns/states"
import { Button, buttonVariants } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { getProfile } from "@/lib/api/auth"
import {
  getCustomerNotifications,
  markAllCustomerNotificationsRead,
  markCustomerNotificationRead,
} from "@/lib/api/notifications"
import { formatDateTime } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { useCustomerNotificationsLive } from "@/lib/notifications/use-customer-notifications-live"
import { cn } from "@/lib/utils"
import type { CustomerNotification, PagedResult } from "@/types"

type NotificationState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<CustomerNotification>; customerId: string }
  | { status: "unavailable" }

const pageSize = 20

export function NotificationCenter() {
  const { locale, t } = useI18n()
  const router = useRouter()
  const [state, setState] = useState<NotificationState>({ status: "loading" })
  const [pageNumber, setPageNumber] = useState(1)
  const [unreadOnly, setUnreadOnly] = useState(false)
  const [markingId, setMarkingId] = useState<string | null>(null)
  const [markingAll, setMarkingAll] = useState(false)
  const [refreshVersion, setRefreshVersion] = useState(0)

  useEffect(() => {
    let active = true

    getProfile()
      .then(async (profile) => {
        if (!profile.roles.includes("Customer")) {
          router.replace("/")
          return
        }

        const data = await getCustomerNotifications(
          profile.id,
          pageNumber,
          pageSize,
          unreadOnly,
        )
        if (active) {
          setState({ status: "ready", data, customerId: profile.id })
        }
      })
      .catch(() => {
        if (active) setState({ status: "unavailable" })
      })

    return () => {
      active = false
    }
  }, [pageNumber, refreshVersion, router, unreadOnly])

  function changeFilter(nextUnreadOnly: boolean) {
    setState({ status: "loading" })
    setUnreadOnly(nextUnreadOnly)
    setPageNumber(1)
  }

  function changePage(nextPageNumber: number) {
    setState({ status: "loading" })
    setPageNumber(nextPageNumber)
  }

  async function markRead(notificationId: string) {
    if (state.status !== "ready") return

    setMarkingId(notificationId)
    try {
      await markCustomerNotificationRead(state.customerId, notificationId)
      setRefreshVersion((current) => current + 1)
    } catch {
      setState({ status: "unavailable" })
    } finally {
      setMarkingId(null)
    }
  }

  async function markAllRead() {
    if (state.status !== "ready") return

    setMarkingAll(true)
    try {
      await markAllCustomerNotificationsRead(state.customerId)
      setPageNumber(1)
      setRefreshVersion((current) => current + 1)
    } catch {
      setState({ status: "unavailable" })
    } finally {
      setMarkingAll(false)
    }
  }

  const totalPages = state.status === "ready"
    ? Math.max(1, state.data.totalPages)
    : 1
  const customerId = state.status === "ready" ? state.customerId : null

  useCustomerNotificationsLive(
    customerId,
    () => setRefreshVersion((current) => current + 1),
  )

  return (
    <div className="min-h-svh bg-muted/30">
      <header className="flex h-16 items-center justify-between border-b border-border bg-background px-4 sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
          <LanguageSwitcher />
          <ThemeToggle />
          <Link href="/orders" className={buttonVariants({ variant: "outline", size: "sm" })}>
            {t.orders.openOrders}
          </Link>
        </div>
      </header>

      <main className="mx-auto w-full max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
        <Link href="/" className={cn(buttonVariants({ variant: "ghost" }), "-ml-2")}>
          {t.basket.continueShopping}
        </Link>

        <div className="mt-6 flex flex-col gap-5 sm:flex-row sm:items-start sm:justify-between">
          <div className="flex items-start gap-3">
            <span className="flex size-11 items-center justify-center rounded-xl bg-primary text-primary-foreground">
              <Bell className="size-5" />
            </span>
            <div>
              <h1 className="font-heading text-3xl font-semibold tracking-tight">
                {t.notifications.title}
              </h1>
              <p className="mt-1 text-sm text-muted-foreground">
                {t.notifications.description}
              </p>
            </div>
          </div>

          <div className="flex flex-wrap items-center justify-end gap-2">
            {state.status === "ready" &&
            state.data.items.some((notification) => notification.readAt === null) ? (
              <Button
                type="button"
                size="sm"
                variant="outline"
                onClick={markAllRead}
                disabled={markingAll}
              >
                {markingAll ? <Loader2 className="animate-spin" /> : <Check />}
                {markingAll
                  ? t.notifications.markingAllRead
                  : t.notifications.markAllRead}
              </Button>
            ) : null}
            <div className="flex rounded-lg border border-border bg-background p-1">
              <Button
                type="button"
                size="sm"
                variant={unreadOnly ? "ghost" : "secondary"}
                onClick={() => changeFilter(false)}
              >
                {t.notifications.all}
              </Button>
              <Button
                type="button"
                size="sm"
                variant={unreadOnly ? "secondary" : "ghost"}
                onClick={() => changeFilter(true)}
              >
                {t.notifications.unread}
              </Button>
            </div>
          </div>
        </div>

        {state.status === "loading" ? (
          <div className="mt-8 flex flex-col gap-3">
            {Array.from({ length: 4 }, (_, index) => (
              <Skeleton key={index} className="h-32 rounded-xl" />
            ))}
          </div>
        ) : state.status === "unavailable" ? (
          <ErrorState className="mt-8" title={t.notifications.loadFailed} />
        ) : state.data.items.length === 0 ? (
          <EmptyState
            className="mt-8"
            title={unreadOnly ? t.notifications.emptyUnread : t.notifications.empty}
            description=""
          />
        ) : (
          <>
            <div className="mt-8 grid gap-3">
              {state.data.items.map((notification) => {
                const unread = notification.readAt === null
                const isMarking = markingId === notification.id

                return (
                  <article
                    key={notification.id}
                    className={cn(
                      "rounded-xl border bg-card p-5",
                      unread ? "border-primary/35 shadow-sm" : "border-border",
                    )}
                  >
                    <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                      <div className="min-w-0">
                        <div className="flex flex-wrap items-center gap-2">
                          <h2 className="font-heading text-lg font-semibold">
                            {notification.title}
                          </h2>
                          <span
                            className={cn(
                              "rounded-full px-2 py-0.5 text-xs font-medium",
                              unread
                                ? "bg-primary/10 text-primary"
                                : "bg-muted text-muted-foreground",
                            )}
                          >
                            {unread ? t.notifications.unreadLabel : t.notifications.readLabel}
                          </span>
                        </div>
                        <p className="mt-2 text-sm leading-6 text-muted-foreground">
                          {notification.message}
                        </p>
                        <time
                          dateTime={notification.createdAt}
                          className="mt-3 block text-xs text-muted-foreground"
                        >
                          {formatDateTime(notification.createdAt, locale)}
                        </time>
                      </div>

                      <div className="flex shrink-0 flex-wrap gap-2">
                        {notification.orderId ? (
                          <Link
                            href={`/orders/${notification.orderId}`}
                            className={buttonVariants({ variant: "outline", size: "sm" })}
                          >
                            {t.notifications.viewOrder}
                          </Link>
                        ) : null}
                        {unread ? (
                          <Button
                            type="button"
                            size="sm"
                            onClick={() => markRead(notification.id)}
                            disabled={isMarking}
                          >
                            {isMarking
                              ? <Loader2 className="animate-spin" />
                              : <Check />}
                            {isMarking
                              ? t.notifications.markingRead
                              : t.notifications.markRead}
                          </Button>
                        ) : null}
                      </div>
                    </div>
                  </article>
                )
              })}
            </div>

            <div className="mt-6 flex items-center justify-between gap-4">
              <Button
                type="button"
                variant="outline"
                disabled={state.data.pageNumber <= 1}
                onClick={() => changePage(Math.max(1, state.data.pageNumber - 1))}
              >
                {t.notifications.previousPage}
              </Button>
              <p className="text-sm text-muted-foreground">
                {t.notifications.pageStatus
                  .replace("{page}", state.data.pageNumber.toString())
                  .replace("{total}", totalPages.toString())}
              </p>
              <Button
                type="button"
                variant="outline"
                disabled={state.data.pageNumber >= totalPages}
                onClick={() => changePage(state.data.pageNumber + 1)}
              >
                {t.notifications.nextPage}
              </Button>
            </div>
          </>
        )}
      </main>
    </div>
  )
}
