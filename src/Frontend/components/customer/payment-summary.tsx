"use client"

import { CreditCard, Loader2, RotateCcw } from "lucide-react"
import { useEffect, useState } from "react"

import { ApiError } from "@/lib/api/client"
import { getOrderPayment } from "@/lib/api/payments"
import type { Dictionary } from "@/lib/i18n/dictionaries"
import type { Payment } from "@/types"

type PaymentState =
  | { status: "loading" }
  | { status: "ready"; payment: Payment }
  | { status: "unavailable" }

export function PaymentSummary({
  orderId,
  locale,
  labels,
  retryWhenMissing,
}: {
  orderId: string
  locale: "en" | "tr"
  labels: Dictionary["orders"]["payment"]
  retryWhenMissing: boolean
}) {
  const [state, setState] = useState<PaymentState>({ status: "loading" })

  useEffect(() => {
    let active = true
    let timer: ReturnType<typeof setTimeout> | undefined

    async function loadPayment() {
      try {
        const payment = await getOrderPayment(orderId)
        if (active) setState({ status: "ready", payment })
      } catch (error) {
        if (active && error instanceof ApiError && error.status === 404 && retryWhenMissing) {
          timer = setTimeout(loadPayment, 2500)
        } else if (active) {
          setState({ status: "unavailable" })
        }
      }
    }

    loadPayment()
    return () => {
      active = false
      if (timer) clearTimeout(timer)
    }
  }, [orderId, retryWhenMissing])

  if (state.status === "loading") {
    return (
      <section className="flex items-center gap-3 border-t border-border p-6 text-sm text-muted-foreground">
        <Loader2 className="size-4 animate-spin" />
        {labels.pending}
      </section>
    )
  }

  if (state.status === "unavailable") {
    return (
      <section className="border-t border-border p-6 text-sm text-muted-foreground">
        {labels.unavailable}
      </section>
    )
  }

  const payment = state.payment
  const status = payment.status === 1 ? "authorized" : payment.status === 2 ? "failed" : "refunded"

  return (
    <section className="border-t border-border p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex items-center gap-2 text-sm font-medium">
          <CreditCard className="size-4 text-primary" />
          {labels.title}
        </div>
        <span className="rounded-full bg-muted px-3 py-1 text-xs font-medium">
          {labels.status[status]}
        </span>
      </div>
      <p className="mt-3 font-semibold">{formatMoney(payment.amount, payment.currency, locale)}</p>
      <ol className="mt-4 space-y-2">
        {payment.transactions.map((transaction) => (
          <li key={transaction.id} className="flex items-center justify-between gap-3 text-xs text-muted-foreground">
            <span className="inline-flex items-center gap-2">
              {transaction.type === 2 ? <RotateCcw className="size-3.5" /> : <CreditCard className="size-3.5" />}
              {transaction.type === 2 ? labels.refund : labels.authorization}
            </span>
            <time>{formatDate(transaction.createdAt, locale)}</time>
          </li>
        ))}
      </ol>
    </section>
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
