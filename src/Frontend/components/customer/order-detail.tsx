"use client"

import { ArrowLeft, Loader2, MapPin, X } from "lucide-react"
import Link from "next/link"
import { useParams } from "next/navigation"
import { useEffect, useState } from "react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { PaymentSummary } from "@/components/customer/payment-summary"
import { ShipmentSummary } from "@/components/customer/shipment-summary"
import { OrderTimeline } from "@/components/patterns/order-timeline"
import { StatusBadge } from "@/components/patterns/status-badge"
import { Button, buttonVariants } from "@/components/ui/button"
import { ApiError } from "@/lib/api/client"
import { getOrder, requestOrderCancellation } from "@/lib/api/orders"
import { formatDateTime, formatMoney } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { orderStatusLabel, orderStatusTone } from "@/lib/i18n/status"
import { getOrderStatusName } from "@/lib/orders/status"
import { cn } from "@/lib/utils"
import type { Order } from "@/types"

type DetailState =
  | { status: "loading" }
  | { status: "ready"; order: Order }
  | { status: "unavailable" }

export function OrderDetail() {
  const { locale, t } = useI18n()
  const params = useParams<{ id: string }>()
  const [state, setState] = useState<DetailState>({ status: "loading" })
  const [cancelling, setCancelling] = useState(false)
  const [cancellationMessage, setCancellationMessage] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    let timer: ReturnType<typeof setTimeout> | undefined

    async function loadOrder() {
      try {
        const order = await getOrder(params.id)
        if (active) {
          setState({ status: "ready", order })
          const status = getOrderStatusName(order.status)
          if (status !== "Confirmed" && status !== "Cancelled") {
            timer = setTimeout(loadOrder, 2500)
          }
        }
      } catch (error) {
        if (active && error instanceof ApiError && error.status === 404) {
          timer = setTimeout(loadOrder, 2000)
        } else if (active) {
          setState({ status: "unavailable" })
        }
      }
    }

    loadOrder()
    return () => {
      active = false
      if (timer) clearTimeout(timer)
    }
  }, [params.id])

  async function handleCancellation() {
    if (state.status !== "ready") return

    setCancelling(true)
    setCancellationMessage(null)
    try {
      const order = await requestOrderCancellation(state.order.id)
      setState({ status: "ready", order })
      setCancellationMessage(t.orders.cancellationRequested)
    } catch (error) {
      setCancellationMessage(
        error instanceof ApiError && error.status === 409
          ? t.orders.orderNotCancellable
          : t.orders.cancellationFailed,
      )
    } finally {
      setCancelling(false)
    }
  }

  return (
    <div className="min-h-svh bg-muted/30">
      <header className="flex h-16 items-center justify-between border-b border-border bg-background px-4 sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
          <LanguageSwitcher />
          <ThemeToggle />
        </div>
      </header>
      <main className="mx-auto w-full max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
        <Link href="/orders" className={cn(buttonVariants({ variant: "ghost" }), "-ml-2")}>
          <ArrowLeft />
          {t.orders.backToOrders}
        </Link>

        {state.status === "loading" ? (
          <div className="mt-8 flex min-h-72 flex-col items-center justify-center rounded-xl border border-dashed border-border bg-card">
            <Loader2 className="size-7 animate-spin text-primary" />
            <p className="mt-4 font-medium">{t.orders.processing}</p>
            <p className="mt-2 max-w-md text-center text-sm text-muted-foreground">{t.orders.processingNote}</p>
          </div>
        ) : state.status === "unavailable" ? (
          <div className="mt-8 rounded-xl border border-border bg-card p-8 text-center text-sm text-muted-foreground">
            {t.orders.loadFailed}
          </div>
        ) : (
          <article className="mt-8 overflow-hidden rounded-xl border border-border bg-card">
            <div className="flex flex-wrap items-start justify-between gap-4 border-b border-border p-6">
              <div>
                <p className="text-xs text-muted-foreground">{t.orders.orderNumber}</p>
                <h1 className="mt-1 font-mono text-lg font-semibold">{state.order.id.toUpperCase()}</h1>
                <p className="mt-2 text-sm text-muted-foreground">{formatDateTime(state.order.createdAt, locale)}</p>
              </div>
              <div className="flex flex-col items-end gap-2">
                <StatusBadge
                  label={orderStatusLabel(state.order.status, t.orders.status)}
                  tone={orderStatusTone(state.order.status)}
                />
                {state.order.status <= 1 ? (
                  <Button
                    type="button"
                    variant="destructive"
                    size="sm"
                    disabled={cancelling}
                    onClick={handleCancellation}
                  >
                    {cancelling ? <Loader2 className="animate-spin" /> : <X />}
                    {cancelling ? t.orders.cancellingOrder : t.orders.cancelOrder}
                  </Button>
                ) : null}
                {cancellationMessage ? (
                  <p className="max-w-xs text-right text-xs text-muted-foreground">
                    {cancellationMessage}
                  </p>
                ) : null}
              </div>
            </div>
            <div className="border-b border-border p-6">
              <OrderTimeline
                status={state.order.status}
                history={state.order.statusHistory}
                cancellationReasonLabel={t.orders.cancellationReason}
                cancellationReasons={t.orders.cancellationReasons}
              />
            </div>
            <PaymentSummary
              orderId={state.order.id}
              locale={locale}
              labels={t.orders.payment}
              retryWhenMissing={shouldRetryMissingPayment(state.order)}
            />
            {shouldShowShipment(state.order) ? (
              <ShipmentSummary
                orderId={state.order.id}
                locale={locale}
                labels={t.orders.shipment}
                retryWhenMissing={getOrderStatusName(state.order.status) !== "Cancelled"}
              />
            ) : null}
            <div className="divide-y divide-border">
              {state.order.items.map((item) => (
                <div key={item.id} className="flex items-center justify-between gap-4 p-6">
                  <div>
                    <h2 className="font-medium">{item.productName}</h2>
                    <p className="mt-1 text-sm text-muted-foreground">{t.basket.quantity}: {item.quantity}</p>
                  </div>
                  <p className="font-semibold">{formatMoney(item.totalPrice, item.currency, locale)}</p>
                </div>
              ))}
            </div>
            <div className="border-t border-border p-6">
              <div className="flex items-center gap-2 text-sm font-medium">
                <MapPin className="size-4 text-primary" />
                {t.orders.shippingAddress}
              </div>
              <address className="mt-3 text-sm not-italic leading-6 text-muted-foreground">
                <span className="font-medium text-foreground">{state.order.recipientName}</span><br />
                {state.order.addressLine}<br />
                {state.order.postalCode} {state.order.city}, {state.order.countryCode}
              </address>
            </div>
            <div className="flex items-center justify-between border-t border-border bg-muted/30 p-6">
              <span className="text-sm text-muted-foreground">{t.basket.total}</span>
              <strong className="font-heading text-2xl">{formatMoney(state.order.totalAmount, state.order.currency, locale)}</strong>
            </div>
          </article>
        )}
      </main>
    </div>
  )
}

function shouldRetryMissingPayment(order: Order) {
  if (getOrderStatusName(order.status) !== "Cancelled") return true

  const cancellation = order.statusHistory.find(
    (entry) => getOrderStatusName(entry.status) === "Cancelled",
  )
  return cancellation?.reasonCode !== "INSUFFICIENT_STOCK"
}

function shouldShowShipment(order: Order) {
  if (order.status >= 2 && order.status <= 4) return true
  if (getOrderStatusName(order.status) !== "Cancelled") return false

  return order.statusHistory.some(
    (entry) => getOrderStatusName(entry.status) === "Cancelled" && entry.reasonCode === "SHIPMENT_FAILED",
  )
}
