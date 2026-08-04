"use client"

import { ArrowLeft, Check, Loader2, MapPin, PackageCheck, X } from "lucide-react"
import Link from "next/link"
import { useParams } from "next/navigation"
import { useEffect, useState } from "react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { PaymentSummary } from "@/components/customer/payment-summary"
import { Button, buttonVariants } from "@/components/ui/button"
import { ApiError } from "@/lib/api/client"
import { getOrder, requestOrderCancellation } from "@/lib/api/orders"
import { useI18n } from "@/lib/i18n/provider"
import { getOrderStatusName, orderProgress } from "@/lib/orders/status"
import { cn } from "@/lib/utils"
import type { Order, OrderCancellationReasonCode, OrderStatusHistory, OrderStatusName } from "@/types"

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
                <p className="mt-2 text-sm text-muted-foreground">{formatDate(state.order.createdAt, locale)}</p>
              </div>
              <div className="flex flex-col items-end gap-2">
                <span className="inline-flex items-center gap-2 rounded-full bg-primary/10 px-3 py-1.5 text-sm font-medium text-primary">
                  <PackageCheck className="size-4" />
                  {t.orders.status[getOrderStatusName(state.order.status)]}
                </span>
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
            <OrderProgress
              status={getOrderStatusName(state.order.status)}
              history={state.order.statusHistory}
              labels={t.orders.status}
              cancellationReasonLabel={t.orders.cancellationReason}
              cancellationReasons={t.orders.cancellationReasons}
              locale={locale}
            />
            <PaymentSummary
              orderId={state.order.id}
              locale={locale}
              labels={t.orders.payment}
              retryWhenMissing={shouldRetryMissingPayment(state.order)}
            />
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

function OrderProgress({
  status,
  history,
  labels,
  cancellationReasonLabel,
  cancellationReasons,
  locale,
}: {
  status: OrderStatusName
  history: OrderStatusHistory[]
  labels: Record<OrderStatusName, string>
  cancellationReasonLabel: string
  cancellationReasons: Record<OrderCancellationReasonCode, string>
  locale: "en" | "tr"
}) {
  const cancelled = status === "Cancelled"
  const cancellationPending = status === "CancellationRequested"
  const currentIndex = orderProgress.indexOf(status)
  const steps = cancelled
    ? ["Submitted", "Cancelled"] as const
    : cancellationPending
      ? ["Submitted", "CancellationRequested"] as const
      : orderProgress
  const cancellation = history.find((entry) => getOrderStatusName(entry.status) === "Cancelled")

  return (
    <div className="border-b border-border p-6">
      <ol className="grid gap-3 sm:grid-cols-5">
        {steps.map((step, index) => {
          const complete = cancelled || cancellationPending ? index === 0 : index <= currentIndex
          const active = step === status
          const entry = history.find((item) => getOrderStatusName(item.status) === step)
          return (
            <li key={step} className="flex items-start gap-2 text-xs">
              <span className={cn(
                "flex size-6 shrink-0 items-center justify-center rounded-full border",
                complete || active
                  ? "border-primary bg-primary text-primary-foreground"
                  : "border-border text-muted-foreground",
                (step === "Cancelled" || step === "CancellationRequested") && "border-destructive bg-destructive text-destructive-foreground",
              )}>
                {step === "Cancelled" || step === "CancellationRequested" ? <X className="size-3.5" /> : complete ? <Check className="size-3.5" /> : index + 1}
              </span>
              <span>
                <span className={active ? "font-medium text-foreground" : "text-muted-foreground"}>
                  {labels[step]}
                </span>
                {entry && (
                  <span className="mt-1 block text-[11px] text-muted-foreground">
                    {formatDate(entry.occurredAt, locale)}
                  </span>
                )}
              </span>
            </li>
          )
        })}
      </ol>
      {cancellation?.reasonCode && (
        <p className="mt-4 rounded-lg bg-destructive/10 px-3 py-2 text-sm text-destructive">
          <span className="font-medium">{cancellationReasonLabel}:</span>{" "}
          {cancellationReasons[cancellation.reasonCode]}
        </p>
      )}
    </div>
  )
}

function formatMoney(amount: number, currency: string, locale: "en" | "tr") {
  return new Intl.NumberFormat(locale === "tr" ? "tr-TR" : "en-US", {
    style: "currency",
    currency,
  }).format(amount)
}

function formatDate(value: string, locale: "en" | "tr") {
  return new Intl.DateTimeFormat(locale === "tr" ? "tr-TR" : "en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value))
}

function shouldRetryMissingPayment(order: Order) {
  if (getOrderStatusName(order.status) !== "Cancelled") return true

  const cancellation = order.statusHistory.find(
    (entry) => getOrderStatusName(entry.status) === "Cancelled",
  )
  return cancellation?.reasonCode !== "INSUFFICIENT_STOCK"
}
